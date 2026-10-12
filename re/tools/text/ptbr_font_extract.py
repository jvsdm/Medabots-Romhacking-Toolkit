"""Glifos da fonte PT-BR (normal e italica) editados a mao, para o Toolkit (src/Medabots.Rom/Text/ptbr_font.bin).

python ptbr_font_extract.py ORIGINAL_E.gba EDITADA.gba SAIDA.bin

Compara as duas fontes do dialogo da Metabee (E) nos enderecos que o jogo usa (0x4BFC64 normal, 0x4C59A4 italica;
o Toolkit localiza 8 bytes adiante), aplica os ajustes de alinhamento da fonte normal (abaixo) e grava so os glifos
que mudaram: 'MBPTFONT', depois para cada fonte (normal, italica): u8 quantidade, e por glifo u8 indice + 64 bytes
(8x16 4bpp; cor 1 = fundo).

Ajustes da fonte normal (linha de base = linha 10, como A, a, n):
- maiusculas, numeros, k, v, ß, Æ, È-Ë: a sombra da linha 11 entra na linha 10 (antes ficavam 1 px abaixo);
- '.' e ',' descem 1 linha (o ponto fica na linha 10);
- 'l' recebe a haste do 'b' (linhas 3-10);
- o corpo do 'í' (linhas 5 em diante) vai 1 coluna para a esquerda: haste na coluna 0, como o 'i' (antes sobrava
  um espaco antes dele); o acento fica onde estava, inclinado sobre a haste;
- letras: no corpo (linhas 4+ nas maiusculas, 6+ nas minusculas; os acentos ficam), a sombra clara (tom < 8) a
  direita da ultima coluna forte sai, e o pe de 1 pixel do 'a' tambem; a letra termina onde a largura do Toolkit
  termina (PtBrFont: ultima coluna forte + 1 livre) e nao encosta na seguinte.
"""
import sys

FONTS = (0x4BFC64, 0x4C59A4)
GLYPH = 64
BACKGROUND = 1

CHARS = (" ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789·.,'-/:?!\"()♥£&%õ"
         "ÄäÁáÂâÈèÉéÊêËëÎîÏïÍíÖöÔôÓóÜüÛûÙùÚúßÇçÑñ¡¿ÀàÆæÃãÕ")
BASELINE_MERGE = "BDEFJKLMSWYkv0123456789ßÆÈÉÊË"
DOWN_ONE = ".,"
SHIFT_LEFT = "í"
ACCENT_ROWS = 5


def get(glyph, x, y):
    b = glyph[(y // 8) * 32 + (y % 8) * 4 + x // 2]
    return b >> 4 if x & 1 else b & 15


def put(glyph, x, y, value):
    i = (y // 8) * 32 + (y % 8) * 4 + x // 2
    glyph[i] = (glyph[i] & 0x0F) | value << 4 if x & 1 else (glyph[i] & 0xF0) | value


def last_strong_column(glyph, lowercase, top, strong=8):
    # mesma regra de PtBrFont.LastInkColumn: tom >= 8; em minuscula, ultima coluna com 1 pixel so (pe do 'a') nao conta
    cols = [sum(get(glyph, x, y) >= strong for y in range(top, 14)) for x in range(8)]
    inked = [x for x in range(8) if cols[x]]
    if not inked:
        return 7
    last = inked[-1]
    return last - 1 if lowercase and cols[last] == 1 and last > 0 else last


def adjust_normal(font):
    glyphs = [bytearray(font[g * GLYPH:(g + 1) * GLYPH]) for g in range(128)]
    for c in BASELINE_MERGE:
        g = glyphs[CHARS.index(c)]
        for x in range(8):
            put(g, x, 10, max(get(g, x, 10), get(g, x, 11)))
            put(g, x, 11, BACKGROUND)
    for c in DOWN_ONE:
        g = glyphs[CHARS.index(c)]
        rows = [[get(g, x, y) for x in range(8)] for y in range(16)]
        for y in range(16):
            for x in range(8):
                put(g, x, y, rows[y - 1][x] if y > 0 else BACKGROUND)
    for c in SHIFT_LEFT:
        g = glyphs[CHARS.index(c)]
        rows = [[get(g, x, y) for x in range(8)] for y in range(16)]
        for y in range(16):
            for x in range(8):
                if y >= ACCENT_ROWS:
                    put(g, x, y, rows[y][x + 1] if x < 7 else BACKGROUND)
    for i, c in enumerate(CHARS):
        if c.isalpha():
            g = glyphs[i]
            top = 6 if c.islower() else 4
            last = last_strong_column(g, c.islower(), top)
            for y in range(top, 16):
                for x in range(last + 1, 8):
                    put(g, x, y, BACKGROUND)
    stem = glyphs[CHARS.index('b')]
    el = glyphs[CHARS.index('l')]
    for y in range(16):
        for x in range(8):
            put(el, x, y, get(stem, 0, y) if x == 0 and 3 <= y <= 10 else BACKGROUND)
    return b''.join(glyphs)


def main():
    orig, edit = (open(p, 'rb').read() for p in sys.argv[1:3])
    out = sys.argv[3]
    data = bytearray(b'MBPTFONT')
    for n, base in enumerate(FONTS):
        before = orig[base:base + 128 * GLYPH]
        after = edit[base:base + 128 * GLYPH]
        if n == 0:
            after = adjust_normal(after)
        changed = [g for g in range(128) if before[g * GLYPH:(g + 1) * GLYPH] != after[g * GLYPH:(g + 1) * GLYPH]]
        data.append(len(changed))
        for g in changed:
            data.append(g)
            data += after[g * GLYPH:(g + 1) * GLYPH]
        print(f'0x{base:X}: {len(changed)} glifos')
    open(out, 'wb').write(data)
    print(f'{len(data)} bytes -> {out}')


if __name__ == '__main__':
    main()
