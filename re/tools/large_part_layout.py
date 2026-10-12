#!/usr/bin/env python3
"""Monta o bot grande (preview) a partir da ROM Metabee (E): posicao de cada pedaco.

Regra extraida de ImageAssetRepository.LargePartDisplay.cs (ReadMedabotLargeDisplayFrame).
Escrito na nuvem SEM ROM: nao testado. Ver re/reference/large_part_layout.md.

  python large_part_layout.py ROM.gba --bot 0 --side 0        # pedacos com posicao relativa aos pes
  python large_part_layout.py ROM.gba --dump-molds            # registros 1-6 e todos os usados
  python large_part_layout.py ROM.gba --histogram             # entradas por linha (moldes por peca)
"""
import argparse
import struct
from collections import Counter

BASE = 0x08000000
MOLD_TABLE = 0x3D1D78            # ponteiros para registros de 0x18 B
HEAD, RARM, LARM, LEGS = 0x7EE70, 0x7F610, 0x7FDB0, 0x80550   # linhas de 16/16/16/32 B
JOINTS = HEAD - 0x60             # 4 registros sinteticos de 0x18 B (juntas 3-6)
ROW = (16, 16, 16, 32)
ANCHOR_X, ANCHOR_Y = 0x3C, 0x32  # origem do desenho (ReadMedabotLargeDisplayFrame)
MIRROR = {0x0E: 0x11, 0x0F: 0x12, 0x10: 0x13, 0x11: 0x0E, 0x12: 0x0F, 0x13: 0x10}


def s8(b):
    return b - 256 if b > 127 else b


class Rom:
    def __init__(self, path):
        self.d = open(path, 'rb').read()

    def u32(self, o):
        return struct.unpack_from('<I', self.d, o)[0]

    def mold(self, n):
        return bytearray(self.d[self.u32(MOLD_TABLE + 4 * n) - BASE:][:0x18])

    def row(self, part, bot):
        o = (HEAD, RARM, LARM, LEGS)[part] + bot * ROW[part]
        out = []
        for i in range(ROW[part] // 4):
            e = self.u32(o + 4 * i)
            if e & 0x3F == 0:
                break
            out.append(e)
        return out


def assemble(rom, bot, side):
    rows = [rom.row(p, bot) for p in range(4)]
    entries = {}
    for r in rows:
        for e in r:
            entries.setdefault(e & 0x3F, e)
    roots = [r[0] & 0x3F for r in rows]            # cabeca, braco dir, braco esq, pernas
    syn = {}
    for jid, tpl, child in ((3, 0, roots[0]), (4, 1, roots[2]), (5, 2, roots[1]), (6, 3, roots[3])):
        b = bytearray(rom.d[JOINTS + 0x18 * tpl:][:0x18])
        b[0x10] = child
        if side:
            b[0x0D] = (-2 - s8(b[0x0D])) & 0xFF
        syn[jid] = b
    out, seen = [], set()

    def walk(m, ax, ay, key):
        if m <= 0 or m in seen:
            return
        seen.add(m)
        logical = syn.get(m) or rom.mold(m)
        render = logical if (m in syn or not side) else rom.mold(MIRROR.get(m, m))
        pay = ay + s8(render[0x0C])
        pax = ax + (s8(render[0x0D]) if not side else -s8(render[0x0D]))
        raw_y, raw_x = struct.unpack_from('<ii', render, 4)
        h, w = logical[0x12], logical[0x13]
        x = pax - raw_x if not side else pax - (w - raw_x)
        y = pay - raw_y
        k = max(0, key + s8(render[0x0E]))
        if m in entries:
            out.append(dict(mold=m, x=x, y=y, w=w, h=h, key=k, bank=logical[0x11],
                            entry=entries[m]))
        walk(logical[0x10], pax, pay, k)      # filho: ancora ja deslocada
        walk(logical[0x0F], ax, ay, key)      # irmao: ancora do pai
        seen.discard(m)

    walk(1, ANCHOR_X, ANCHOR_Y, 0x1C2)
    out.sort(key=lambda p: p['key'])          # menor chave = desenhado por ultimo (por cima)
    return out


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument('rom')
    ap.add_argument('--bot', type=int, default=0)
    ap.add_argument('--side', type=int, default=0)
    ap.add_argument('--dump-molds', action='store_true')
    ap.add_argument('--histogram', action='store_true')
    a = ap.parse_args()
    rom = Rom(a.rom)
    if a.histogram:
        for p, name in enumerate(('cabeca', 'braco_dir', 'braco_esq', 'pernas')):
            print(name, sorted(Counter(len(rom.row(p, b)) for b in range(122)).items()))
        return
    if a.dump_molds:
        for n in list(range(1, 7)) + sorted({e & 0x3F for p in range(4) for e in rom.row(p, a.bot)}):
            m = rom.mold(n)
            ry, rx = struct.unpack_from('<ii', m, 4)
            print(f"m{n:02d} rawY={ry} rawX={rx} off(y,x)=({s8(m[0xC])},{s8(m[0xD])}) sort={s8(m[0xE])} "
                  f"sib={m[0xF]} child={m[0x10]} bank={m[0x11]} h={m[0x12]} w={m[0x13]} div={m[0x14]:02X} "
                  f"b15-17={m[0x15]:02X} {m[0x16]:02X} {m[0x17]:02X}")
        return
    for p in assemble(rom, a.bot, a.side):
        print(f"m{p['mold']:02d} ({p['x'] - ANCHOR_X:+d},{p['y'] - ANCHOR_Y:+d}) {p['w']}x{p['h']} "
              f"key={p['key']:#x} bank={p['bank']} entry={p['entry']:#010x}")


if __name__ == '__main__':
    main()
