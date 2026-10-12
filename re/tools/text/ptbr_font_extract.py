"""Glifos da fonte PT-BR (normal e italica) editados a mao, para o Toolkit (src/Medabots.Rom/Text/ptbr_font.bin).

python ptbr_font_extract.py ORIGINAL_E.gba EDITADA.gba SAIDA.bin

Compara as duas fontes do dialogo da Metabee (E) nos enderecos que o jogo usa (0x4BFC64 normal, 0x4C59A4 italica;
o Toolkit localiza 8 bytes adiante) e grava so os glifos que mudaram:
'MBPTFONT', depois para cada fonte (normal, italica): u8 quantidade, e por glifo u8 indice + 64 bytes (8x16 4bpp).
"""
import sys

FONTS = (0x4BFC64, 0x4C59A4)
GLYPH = 64

orig, edit = (open(p, 'rb').read() for p in sys.argv[1:3])
out = sys.argv[3]
data = bytearray(b'MBPTFONT')
for base in FONTS:
    changed = [g for g in range(128) if orig[base + g * GLYPH:base + (g + 1) * GLYPH] != edit[base + g * GLYPH:base + (g + 1) * GLYPH]]
    data.append(len(changed))
    for g in changed:
        data.append(g)
        data += edit[base + g * GLYPH:base + (g + 1) * GLYPH]
    print(f'0x{base:X}: {len(changed)} glifos')
open(out, 'wb').write(data)
print(f'{len(data)} bytes -> {out}')
