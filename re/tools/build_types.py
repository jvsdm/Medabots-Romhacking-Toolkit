"""Gera re/ghidra/medabots_types.tsv (lido por MedabotsTypes.java) a partir de re/functions/ST_*.csv.

CSV: struct,offset,size,name,confidence,source. size 1/2/4 = inteiro sem sinal; N > 4 = array de bytes.
Uma linha com offset=BASE dá o tamanho total (size) e o endereço/contagem da instância (name "0x0300xxxx x30").
Campos de BattleBot dentro de 0x48..0xA7 viram campos de BattlePart (4 peças de 0x18).
Campo que sobrepõe outro já aceito é descartado (confirmado entra antes de provável) e listado no fim.
"""
import csv, glob, os, re, sys

HERE = os.path.dirname(os.path.abspath(__file__))
FUNCS = os.path.join(HERE, "..", "functions")
OUT = os.path.join(HERE, "..", "ghidra", "medabots_types.tsv")

# struct: (tamanho, [(endereço, qtd, rótulo)])
KNOWN = {
    "BattleBot": (0x100, [(0x03000590, 6, "gBattleBots")]),
    "BattlePart": (0x18, []),
    "FightInfo": (0x200, [(0x03001800, 1, "gFightInfo")]),  # +0x400/+0x840 são outros globais
    "EventVariantBlock": (8, []),
    "EnemyTemplateRec": (0xC, []),  # cópia em fi+0x04: não aplicar por cima do FightInfo
    "EventBlock": (None, [(0x030057E0, 1, "gEventBlock")]),
}
PART_BASE, PART_SIZE, PART_N = 0x48, 0x18, 4
TYPES = {1: "u8", 2: "u16", 4: "u32"}


def ident(s):
    s = re.sub(r"[^0-9A-Za-z_]", "_", s.strip()) or "field"
    return "_" + s if s[0].isdigit() else s


def main():
    rows, bases = [], {}
    for f in sorted(glob.glob(os.path.join(FUNCS, "ST_*.csv"))):
        if f.endswith("_simbolos.csv"):
            continue
        with open(f, encoding="utf-8") as fh:
            for r in csv.DictReader(fh):
                st = ident(r.get("struct") or "")
                off = (r.get("offset") or "").strip()
                try:
                    size = int(str(r.get("size") or "1").strip(), 0)
                except ValueError:
                    size = 1
                nm = (r.get("name") or "").strip()
                m = re.match(r"(0x[0-9A-Fa-f]+) x(\d+)", nm)
                if off.upper() == "BASE" or m:
                    if m and int(m.group(2)) > 1 and size % int(m.group(2)) == 0 and off.upper() != "BASE":
                        size //= int(m.group(2))  # linha-resumo com tamanho total
                    bases.setdefault(st, (size, nm))
                    continue
                if not re.fullmatch(r"0[xX][0-9A-Fa-f]{1,4}", off):
                    continue
                o = int(off, 16)
                conf = (r.get("confidence") or "").strip().lower()
                if st == "BattleBot" and o == PART_BASE and size > PART_SIZE:
                    continue  # o array de peças é posto abaixo
                if st == "BattleBot" and PART_BASE <= o < PART_BASE + PART_SIZE * PART_N:
                    k, o = divmod(o - PART_BASE, PART_SIZE)
                    st = "BattlePart"
                    if o + size > PART_SIZE:
                        size = 1
                cmt = f"{conf}; {(r.get('source') or '').strip()}".replace("\t", " ")
                rows.append((st, o, max(size, 1), ident(r.get("name") or ""), conf, cmt))

    # tamanho e instâncias
    sizes, inst = {}, {}
    for st, (sz, ins) in KNOWN.items():
        sizes[st], inst[st] = sz, list(ins)
    for st, (sz, name) in bases.items():
        sizes[st] = sizes.get(st) or sz
        m = re.match(r"\s*(0x[0-9A-Fa-f]+)(?:\s*x\s*(\d+))?", name)
        if m and st not in KNOWN and not inst.get(st):
            a = int(m.group(1), 16)
            if a in {x[0] for l in inst.values() for x in l}:
                continue  # mesmo endereço de outro struct (ex.: PartStatsLeg)
            inst[st] = [(a, int(m.group(2) or 1), "g" + st)]
    # sem tamanho declarado: até o fim do último campo, alinhado a 4
    for st in {r[0] for r in rows} | set(sizes):
        if sizes.get(st) is None:
            sizes[st] = (max([o + sz for s, o, sz, *_ in rows if s == st] or [4]) + 3) & ~3

    # campos sem sobreposição; confirmado primeiro
    rows.sort(key=lambda r: (r[4] != "confirmado", r[0], r[1]))
    used, fields, dropped, names = {}, [], [], {}
    for st, o, sz, nm, conf, cmt in rows:
        if o + sz > sizes[st]:
            if o < sizes[st]:
                sz = 1  # array de tamanho incerto: fica só o 1º byte
            else:
                dropped.append((st, o, nm, "fora do tamanho")); continue
        occ = used.setdefault(st, set())
        span = set(range(o, o + sz))
        if occ & span:
            dropped.append((st, o, nm, "sobreposto")); continue
        n = names.setdefault(st, {})
        if nm in n:
            nm = f"{nm}_{o:02X}"
        n[nm] = 1
        occ |= span
        fields.append((st, o, TYPES.get(sz, f"bytes:{sz}"), nm, cmt))

    # BattleBot recebe o array de peças
    fields.append(("BattleBot", PART_BASE, f"BattlePart:{PART_N}", "parts", "peças k=0..3 (cabeça, braços, perna)"))
    fields.sort(key=lambda f: (f[0], f[1]))

    with open(OUT, "w", encoding="utf-8", newline="\n") as fh:
        fh.write("# gerado por re/tools/build_types.py a partir de re/functions/ST_*.csv\n")
        for st in sorted(sizes, key=lambda s: (s == "BattleBot", s)):  # BattlePart antes de BattleBot
            fh.write(f"T\t{st}\t0x{sizes[st]:X}\n")
        for st, o, t, nm, cmt in fields:
            fh.write(f"M\t{st}\t0x{o:X}\t{t}\t{nm}\t{cmt}\n")
        for st, lst in inst.items():
            for a, n, lbl in lst:
                fh.write(f"A\t0x{a:08X}\t{st}\t{n}\t{lbl}\n")
    print(f"structs {len(sizes)}, campos {len(fields)}, descartados {len(dropped)} -> {os.path.relpath(OUT)}")
    for d in dropped:
        print("  descartado", d[0], hex(d[1]), d[2], d[3])


if __name__ == "__main__":
    sys.exit(main())
