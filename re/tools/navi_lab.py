"""
navi_lab.py - Um bot do Medarot Navi no Core com a geometria do Navi (moldes novos 50+), sem espremer.

Usa mold_lab.py (tabela de moldes 64, espelho 64, movimento 63 linhas). Para cada pedaco do bot do Navi cria um
molde novo: registro copiado do molde parecido do Core (paleta, convencoes), com tamanho e pontos de encaixe
(+0x04..+0x0E) do molde do Navi; lista de artes propria (seletor 1 = imagem LZ77 + paleta do Navi, copiadas da
ROM do Navi). Movimento do molde novo = linhas do molde parecido do Core.

Uso: python tools/navi_lab.py CORE_IN.gba OUT.gba NAVI_BOT [BOT_CORE=0]
"""
import struct
import sys

import mold_lab as ml

GBA = 0x08000000
NAVI_ROM = "D:/claude-brain/Medabots/0100 - Medarot Navi - Kuwagata Version (J)(Eurasia).gba"
NAVI_MOLDS = 0x658858
NAVI_ROWS = {"head": (0x4BFC40, 16), "right_arm": (0x4C05C0, 16), "left_arm": (0x4C0F40, 16), "legs": (0x4C18C0, 64)}
CORE_ROWS = {"head": (0x7EE70, 16), "right_arm": (0x7F610, 16), "left_arm": (0x7FDB0, 16), "legs": (0x80550, 32)}
# Molde do Core usado como base (paleta e movimento) para cada molde do Navi.
CORE_ANALOG = {"head": 13, "right_arm": 16, "left_arm": 19, "legs": 30}


def main():
    core_in, out, navi_bot = sys.argv[1], sys.argv[2], int(sys.argv[3])
    core_bot = int(sys.argv[4]) if len(sys.argv) > 4 else 0
    rom = bytearray(open(core_in, "rb").read())
    navi = open(NAVI_ROM, "rb").read()
    space = ml.Space(rom)
    n32 = lambda a: struct.unpack_from("<I", navi, a)[0]

    # Tabela de moldes 64 e espelho 64 (moldes novos preenchidos abaixo).
    table = bytearray(rom[ml.MOLDS:ml.MOLDS + 4 * ml.MOLD_COUNT]) + bytearray(4 * (ml.NEW_MOLDS - ml.MOLD_COUNT))
    mirror = bytearray(rom[ml.MIRROR_MAP:ml.MIRROR_MAP + ml.MOLD_COUNT]) + bytes(range(ml.MOLD_COUNT, ml.NEW_MOLDS))
    row_source = {}
    next_mold = 50
    created = {}       # molde do Navi -> molde novo
    records = {}       # molde novo -> registro (bytearray)
    art = {}           # molde novo -> lista de pares (imagem, paleta); o indice + 1 e o seletor
    navi_kids = {}     # molde novo -> filhos no Navi
    rows = {}
    for part, (navi_rows, size) in NAVI_ROWS.items():
        entries = [n32(navi_rows + navi_bot * size + 4 * k) for k in range(size // 4)]
        core_entries = []
        for entry in entries:
            navi_mold = entry & 0x3F
            if not navi_mold:
                continue
            nrec = n32(NAVI_MOLDS + 4 * navi_mold) - GBA
            # Entradas sem arte (seletor apontando para algo que nao e LZ77) nao sao desenhadas: pula.
            first_image = n32(n32(nrec) - GBA + 8 * ((entry >> 6) & 0xFF))
            if not GBA <= first_image < GBA + len(navi) or navi[first_image - GBA] != 0x10:
                print(f"{part}: entrada {entry:#x} sem arte, ignorada")
                continue
            if navi_mold not in created:
                analog = CORE_ANALOG[part]
                rec = bytearray(rom[ml.u32(rom, ml.MOLDS + 4 * analog) - GBA:][:0x18])
                rec[0x04:0x0F] = navi[nrec + 0x04:nrec + 0x0F]          # pontos de encaixe do Navi
                rec[0x12], rec[0x13] = navi[nrec + 0x12], navi[nrec + 0x13]
                mold = next_mold
                next_mold += 1
                created[navi_mold], records[mold], art[mold] = mold, rec, []
                navi_kids[mold] = (navi[nrec + 0x0F], navi[nrec + 0x10])
                row_source[mold] = analog
                print(f"{part}: Navi molde {navi_mold} ({rec[0x12]}x{rec[0x13]}) -> Core molde {mold} (base {analog})")
            mold = created[navi_mold]
            # A arte desta entrada (1 ou 2 poses) vai para o fim da lista do molde novo.
            blobs = n32(nrec) - GBA
            selector = (entry >> 6) & 0xFF
            poses = ((entry >> 15) & 1) + 1
            first = len(art[mold]) + 1
            for pose in range(poses):
                image, palette = n32(blobs + 8 * (selector + pose)), n32(blobs + 8 * (selector + pose) + 4)
                img = navi[image - GBA:]
                length = 4 + (struct.unpack_from("<I", img, 0)[0] >> 8) + 0x400   # >= tamanho comprimido
                new_image = GBA + space.store(bytes(img[:length]))
                new_palette = GBA + space.store(navi[palette - GBA:palette - GBA + 32]) if palette != image else new_image
                art[mold].append((new_image, new_palette))
            core_entries.append((entry & ~0x3F & ~(0xFF << 6)) | mold | (first << 6))
        rows[part] = core_entries
    for mold, rec in records.items():
        rec[0x0F], rec[0x10] = (created.get(k, 0) if k else 0 for k in navi_kids[mold])
        pairs = art[mold]
        blob = b"".join(struct.pack("<II", *pairs[min(slot, len(pairs)) - 1 if slot else 0]) for slot in range(256))
        struct.pack_into("<I", rec, 0, GBA + space.store(blob))
        struct.pack_into("<I", table, 4 * mold, GBA + space.store(bytes(rec)))
    # Espelho: os bracos novos trocam entre si como 14-16 <-> 17-19.
    right = [m for n, m in created.items() if row_source[m] == 16]
    left = [m for n, m in created.items() if row_source[m] == 19]
    for r, l in zip(right, left):
        mirror[r], mirror[l] = l, r

    new_table = GBA + space.store(bytes(table))
    for at in ml.loaded_literals(rom, GBA + ml.MOLDS):
        ml.put32(rom, at, new_table)
    new_mirror = GBA + space.store(bytes(mirror))
    for at in ml.loaded_literals(rom, GBA + ml.MIRROR_MAP):
        ml.put32(rom, at, new_mirror)
    print("movimento: blocos", ml.expand_motion(rom, space, row_source))

    for part, (core_rows, size) in CORE_ROWS.items():
        at = core_rows + core_bot * size
        rom[at:at + size] = bytes(size)
        for k, entry in enumerate(rows[part][:size // 4]):
            ml.put32(rom, at + 4 * k, entry)
        print(part, [hex(ml.u32(rom, at + 4 * k)) for k in range(size // 4) if ml.u32(rom, at + 4 * k)])
    open(out, "wb").write(rom)
    print("->", out)


if __name__ == "__main__":
    main()
