"""Junta tudo numa fonte só e gera o arquivo que o Ghidra aplica.

1. Funde re/functions/*_simbolos.csv em re/symbols/mtbe.csv (camadas mais novas ganham; o nome antigo vai
   para o comentário como "(antes: X)").
2. Gera re/ghidra/medabots_sync.tsv, lido por re/ghidra/MedabotsSync.java:
     F <addr> <nome>   função: cria se não existir, renomeia se o nome for diferente
     C <addr>          só criar função (alvo de tabela ainda sem nome)
     L <addr> <nome>   rótulo de dado na ROM

Uso: python -I re/tools/build_sync.py   (rodar da raiz do Toolkit)
"""
import csv
import glob
import io
import os
import re

ROOT = os.path.join(os.path.dirname(os.path.abspath(__file__)), "..")
FUNC_DIR = os.path.join(ROOT, "functions")
MTBE = os.path.join(ROOT, "symbols", "mtbe.csv")
SYNC = os.path.join(ROOT, "ghidra", "medabots_sync.tsv")

# Camadas, da mais antiga para a mais nova (a última ganha).
TIERS = [
    r"R\d.*|G",
    r"H|HA\d+|HN\d+|HE\d+|HO\d+|HC\d*",
    r"FH|JT|DT|FL\d*",
    r"FR\d+|JT\d+|SD|A6|DT\d+",
    r"N\d+|QA\d+|CH\d+|MS|TX|FN\d+|MD|AS\d+|SV|EV|BT|OW|GF|SN|PS|MN|DM|AI|IN|EN|MF",
    r"CF\d*|NM\d*",
]
# MERGE_NOTAS: aqui o nome do mtbe é o correto.
KEEP_MTBE = {"0x08035754", "0x08035814", "0x08035880", "0x08035740"}
# Listas de endereços que devem virar função (alvos de tabela de ponteiros).
CREATE_LISTS = ["H_lista.txt", "HC_lista.txt", "JT_criar.txt", "AS_criar.txt", "MS_lista.txt", "TX_lista.txt", "EXTRA_criar.txt"]

IDENT = re.compile(r"^[A-Za-z_][A-Za-z0-9_]*$")
GENERIC = re.compile(r"^(FUN|sub|DAT|LAB|PTR|thunk_FUN)_", re.I)
DATA_KINDS = {"data", "ptr", "label", "table"}


def norm(addr, func):
    v = int(addr, 16)
    if func:
        v &= ~1
    return "0x%08X" % v


def tier_of(prefix):
    for i, pat in enumerate(TIERS):
        if re.fullmatch(pat, prefix):
            return i + 1
    if re.fullmatch(r"[A-Z]{2,3}\d*", prefix):
        return len(TIERS) - 1  # agente novo: mesma camada das rodadas recentes
    return None


def read_csv(path):
    rows = []
    for r in csv.reader(open(path, encoding="utf-8")):
        if not r or not r[0].strip().lower().startswith("0x"):
            continue
        rows.append([c.strip() for c in r] + [""] * 4)
    return rows


def main():
    raw = open(MTBE, encoding="utf-8", newline="").read()
    nl = "\r\n" if "\r\n" in raw else "\n"
    table = list(csv.reader(io.StringIO(raw)))
    header, body = table[0], table[1:]
    by_addr = {}
    for r in body:
        kind = r[2] if len(r) > 2 else ""
        by_addr.setdefault(norm(r[0], kind.startswith("func")), r)

    # Candidatos dos CSVs, por camada.
    cand = {}
    for path in glob.glob(os.path.join(FUNC_DIR, "*_simbolos.csv")):
        prefix = os.path.basename(path)[: -len("_simbolos.csv")]
        base_tier = tier_of(prefix)
        if base_tier is None:
            continue
        for a, name, kind, comment, *_ in read_csv(path):
            t = base_tier
            if not IDENT.match(name) or GENERIC.match(name):
                continue
            kind = kind.lower()
            k = "data" if kind in DATA_KINDS else "func_thumb"
            if k == "data" and kind != "data":
                k = kind
            key = norm(a, k == "func_thumb")
            if "revisto CH" in comment:
                t = len(TIERS)  # revisão de chute é a palavra mais nova sobre o endereço
            old = cand.get(key)
            if old is None or t >= old[0]:
                cand[key] = (t, name, k, comment, prefix)

    added = changed = 0
    for key, (t, name, k, comment, prefix) in sorted(cand.items()):
        r = by_addr.get(key)
        if r is None:
            r = [key, name, k, "", "[%s] %s" % (prefix, comment)]
            body.append(r)
            by_addr[key] = r
            added += 1
        elif r[1] != name and key not in KEEP_MTBE:
            while len(r) < 5:
                r.append("")
            r[4] = (r[4] + " (antes: %s)" % r[1]).strip()
            r[1] = name
            changed += 1
        if k != "func_thumb" and len(r) > 2 and r[2].startswith("func"):
            r[2] = k  # camada nova diz que é rótulo/dado, não função

    body.sort(key=lambda r: int(r[0], 16))
    out = io.StringIO()
    csv.writer(out, lineterminator=nl).writerows([header] + body)
    open(MTBE, "w", encoding="utf-8", newline="").write(out.getvalue())

    # Nomes únicos: o segundo endereço com o mesmo nome ganha sufixo.
    seen = {}
    lines, dups = [], []
    named_funcs = set()
    for r in body:
        kind = r[2] if len(r) > 2 else ""
        a = int(r[0], 16)
        if not (0x08000000 <= a < 0x0A000000):
            continue
        name = r[1]
        if not IDENT.match(name) or GENERIC.match(name):
            continue
        func = kind.startswith("func")
        key = norm(r[0], func)
        if name in seen and seen[name] != key:
            dups.append((name, seen[name], key))
            name = "%s_%s" % (name, key[2:])
        seen.setdefault(name, key)
        if func:
            lines.append("F\t%s\t%s" % (key, name))
            named_funcs.add(key)
        elif kind in DATA_KINDS:
            lines.append("L\t%s\t%s" % (key, name))

    creates = set()
    for fn in CREATE_LISTS:
        p = os.path.join(FUNC_DIR, fn)
        if not os.path.exists(p):
            continue
        for line in open(p, encoding="utf-8"):
            tok = re.split(r"[ \t,]+", line.strip())[0]
            if re.fullmatch(r"0[xX][0-9A-Fa-f]+", tok):  # ignora faixas "0x..0x"
                key = norm(tok, True)
                if key not in named_funcs:
                    creates.add(key)
    lines += ["C\t%s" % a for a in sorted(creates)]

    with open(SYNC, "w", encoding="utf-8", newline="\n") as f:
        f.write("# gerado por re/tools/build_sync.py; nao editar a mao\n")
        f.write("\n".join(lines) + "\n")

    print("mtbe.csv: %d linhas, +%d novos, %d trocados" % (len(body), added, changed))
    print("sync: %d funcoes, %d rotulos, %d so criar" % (
        sum(l[0] == "F" for l in lines), sum(l[0] == "L" for l in lines), len(creates)))
    for name, a, b in dups:
        print("nome repetido: %s em %s e %s (o segundo ganhou sufixo)" % (name, a, b))


if __name__ == "__main__":
    main()
