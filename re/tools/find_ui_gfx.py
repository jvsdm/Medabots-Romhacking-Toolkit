#!/usr/bin/env python3
"""Acha e classifica os assets Malias2 de uma funcao de tela (HUD da batalha, caixa de dialogo).

Escrito na nuvem SEM ROM: so a parte Malias2 foi testada (round-trip com o compressor do Toolkit portado).
Ideia: a rotina que monta uma tela (ex.: InitializeBattleSceneScreen 0x0800406C) guarda os ponteiros dos
assets no literal pool; cada ponteiro 0x08xxxxxx que aponta para um bloco 'Le' (Malias2) e um asset da tela.

  python find_ui_gfx.py ROM.gba --func 0x0800406C --len 0x800          # assets citados pela funcao
  python find_ui_gfx.py ROM.gba --range 0x52A000 0x560800              # todos os blocos Malias2 da faixa
  python find_ui_gfx.py ROM.gba --func 0x0800406C --png out_dir        # + folhas PNG (precisa Pillow)

Classificacao heuristica (tudo 'provavel'): palette = 32/512 B com bit 15 zerado; tilemap = pares u16 com
tile < 0x400; tiles = multiplo de 32 B. Conferir no Ghidra/emulador.
"""
import argparse
import os
import struct

BASE = 0x08000000


def malias_len(d, o):
    """Port de Malias2.TryGetEncodedLength: bytes codificados ou None."""
    if o + 6 > len(d) or d[o] != 0x4C or d[o + 1] != 0x65:
        return None
    rem = d[o + 2] | d[o + 3] << 8 | d[o + 4] << 16
    if rem <= 0 or rem >= 0x100000:
        return None
    c = o + 6
    while rem > 0:
        if c >= len(d):
            return None
        cmd = d[c]
        c += 1
        for _ in range(4):
            if rem <= 0:
                break
            k = cmd & 3
            if k == 0:
                if c + 2 > len(d):
                    return None
                v = d[c] | d[c + 1] << 8
                c += 2
                rem -= (v >> 12) + 3
            elif k == 1:
                if c + 1 > len(d):
                    return None
                v = d[c]
                c += 1
                rem -= (v >> 2) + 2
            elif k == 2:
                if c + 1 > len(d):
                    return None
                c += 1
                rem -= 1
            else:
                if c + 3 > len(d):
                    return None
                c += 3
                rem -= 3
            cmd >>= 2
    return c - o


def malias_unpack(d, o):
    """Port de Malias2.DecompressInternal."""
    n = d[o + 2] | d[o + 3] << 8 | d[o + 4] << 16
    out = bytearray()
    s = o + 6
    left = n
    while left > 0:
        cmd = d[s]
        s += 1
        for _ in range(4):
            if left <= 0:
                break
            k = cmd & 3
            if k == 0:
                v = d[s] | d[s + 1] << 8
                s += 2
                src = len(out) - ((v & 0xFFF) + 5)
                cnt = (v >> 12) + 3
                left -= cnt
                for i in range(cnt):
                    out.append(out[src + i])
            elif k == 1:
                v = d[s]
                s += 1
                src = len(out) - ((v & 3) + 1)
                cnt = (v >> 2) + 2
                left -= cnt
                for i in range(cnt):
                    out.append(out[src + i])
            elif k == 2:
                out.append(d[s])
                s += 1
                left -= 1
            else:
                out += d[s:s + 3]
                s += 3
                left -= 3
            cmd >>= 2
    return bytes(out[:n])


def classify(b):
    n = len(b)
    if n in (32, 512) and all(b[i + 1] < 0x80 for i in range(0, n, 2)):
        return 'palette'
    if n % 2 == 0 and n <= 0x1000 and all((b[i] | b[i + 1] << 8) & 0x3FF < 0x400 for i in range(0, n, 2)):
        words = [b[i] | b[i + 1] << 8 for i in range(0, n, 2)]
        if max(w & 0x3FF for w in words) < 0x300 and sum(1 for w in words if w >> 12) <= len(words):
            if n in (0x800, 0x700, 0x500, 0x400, 0x300, 0x280, 0x200, 0x1000) or len(set(words)) < n // 8:
                return 'tilemap?'
    if n % 32 == 0:
        return 'tiles4bpp'
    return 'data'


def scan_range(d, lo, hi):
    o = lo
    while o < hi:
        ln = malias_len(d, o)
        if ln:
            yield o, ln
            o += (ln + 3) & ~3
        else:
            o += 4


def func_pointers(d, start, length):
    """Palavras alinhadas de 4 B dentro da funcao que parecem ponteiros para ROM (literal pool)."""
    base = start - BASE
    out = []
    for o in range(base & ~3, base + length, 4):
        v = struct.unpack_from('<I', d, o)[0]
        if BASE <= v < BASE + len(d):
            out.append((o + BASE, v))
    return out


def describe(d, off):
    ln = malias_len(d, off)
    if not ln:
        return None
    raw = malias_unpack(d, off)
    return ln, raw, classify(raw)


def save_sheet(raw, kind, path):
    try:
        from PIL import Image
    except ImportError:
        print('  (Pillow ausente: sem PNG)')
        return
    if kind == 'palette':
        return
    # tiles 4bpp em escala de cinza, 16 tiles por linha (paleta real so no emulador)
    t = len(raw) // 32
    cols = 16
    rows = (t + cols - 1) // cols
    img = Image.new('L', (cols * 8, rows * 8))
    for i in range(t):
        for y in range(8):
            for x in range(4):
                b = raw[i * 32 + y * 4 + x]
                for k, v in enumerate((b & 15, b >> 4)):
                    img.putpixel(((i % cols) * 8 + x * 2 + k, (i // cols) * 8 + y), v * 17)
    img.resize((img.width * 3, img.height * 3), Image.NEAREST).save(path)


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument('rom')
    ap.add_argument('--func', type=lambda s: int(s, 0))
    ap.add_argument('--len', type=lambda s: int(s, 0), default=0x400)
    ap.add_argument('--range', nargs=2, type=lambda s: int(s, 0))
    ap.add_argument('--png')
    a = ap.parse_args()
    d = open(a.rom, 'rb').read()
    if a.png:
        os.makedirs(a.png, exist_ok=True)
    seen = set()
    if a.func:
        print(f'ponteiros no literal pool de 0x{a.func:08X}..+0x{a.len:X}:')
        for at, v in func_pointers(d, a.func, a.len):
            r = describe(d, v - BASE)
            if r:
                ln, raw, kind = r
                print(f'  lit 0x{at:08X} -> 0x{v:08X} Malias2 {ln} B -> {len(raw)} B  {kind}')
                if a.png and v not in seen:
                    save_sheet(raw, kind, os.path.join(a.png, f'{v:08X}.png'))
                seen.add(v)
            elif v - BASE < len(d) and v >= BASE + 0x80000:
                print(f'  lit 0x{at:08X} -> 0x{v:08X} (nao e Malias2; LZ77? {d[v - BASE]:#04x})')
    if a.range:
        for o, ln in scan_range(d, *a.range):
            raw = malias_unpack(d, o)
            kind = classify(raw)
            print(f'0x{o:06X} enc {ln:5d} B -> {len(raw):5d} B  {kind}')
            if a.png:
                save_sheet(raw, kind, os.path.join(a.png, f'{o:06X}.png'))


if __name__ == '__main__':
    main()
