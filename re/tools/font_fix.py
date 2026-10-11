"""Corrige o alinhamento da fonte do dialogo (Metabee E) sem mudar o desenho dos glifos.

    python font_fix.py font.png font_corrigida.png

Entrada e saida no formato do insert_font.py: 128x128, 16 glifos 8x16 por linha, cores da paleta da fonte
(0x084E43D8). Glifos novos (0x4F, 0x7D-0x7F etc.) passam intactos, exceto pela regra geral da linha 9.

Correcoes (linhas 0-15 de cada glifo; maiusculas ocupam 2-8):
- linha 9 so com tons claros (indice < 9) em letras e algarismos sem descendente: era um borrao abaixo
  da base que faz B, F, K, M, S, Y, k, m, v, 0-8 parecerem mais baixos que A, C, G, O...
- E: barra de baixo dupla (linhas 8 e 9) -> fica so a 8.
- D: borrao escuro na linha 9 -> removido.
- L: pe duplo (8 e 9) e topo apagado (linha 2) -> linha 9 removida, linha 2 = linha 3.
- l: topo apagado (linha 2) e borrao na 9 -> linha 2 = linha 3, linha 9 removida.
"""
import sys

from PIL import Image

PALETTE_RGB555 = [
    0x0000, 0x27DF, 0x237D, 0x1F3B, 0x1EFA, 0x1AB8, 0x1676, 0x1635,
    0x11F3, 0x0DB1, 0x0D70, 0x092E, 0x04EC, 0x04AB, 0x0069, 0x0028,
]
BACKGROUND = 1
DARK_MIN = 9

ENCODING = (" ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789·.,'-/:?!\"()♥£&%"
            "õÄäÁáÂâÈèÉéÊêËëÎîÏïÍíÖöÔôÓóÜüÛûÙùÚúßÇçÑñ¡¿ÀàÆæÃãÕ")
NO_ROW9_RULE = set("Qgjpqy,ÇçßJ9/()?!&%£♥¡¿")


def rgb(value):
    return tuple(((value >> shift) & 31) * 255 // 31 for shift in (0, 5, 10))


COLORS = [rgb(v) for v in PALETTE_RGB555]


def nearest(color):
    return min(range(16), key=lambda i: sum((a - b) ** 2 for a, b in zip(COLORS[i], color)))


def load(path):
    img = Image.open(path).convert("RGB")
    if img.size != (128, 128):
        sys.exit(f"esperado 128x128, recebido {img.size}")
    px = img.load()
    glyphs = []
    for i in range(128):
        ox, oy = (i % 16) * 8, (i // 16) * 16
        glyphs.append([[nearest(px[ox + c, oy + r]) for c in range(8)] for r in range(16)])
    return glyphs


def save(glyphs, path):
    img = Image.new("RGB", (128, 128))
    px = img.load()
    for i, g in enumerate(glyphs):
        ox, oy = (i % 16) * 8, (i // 16) * 16
        for r in range(16):
            for c in range(8):
                px[ox + c, oy + r] = COLORS[g[r][c]]
    img.save(path)


def clear(g, row):
    g[row] = [BACKGROUND] * 8


def fix(glyphs):
    changed = []
    for i, g in enumerate(glyphs):
        ch = ENCODING[i] if i < len(ENCODING) else ""
        before = [row[:] for row in g]
        if ch == "E" or ch == "D":
            clear(g, 9)
        elif ch in ("L", "l"):
            clear(g, 9)
            g[2] = g[3][:]
        elif ch and ch not in NO_ROW9_RULE and ch.strip() and max(g[9]) < DARK_MIN:
            clear(g, 9)
        if g != before:
            changed.append(ch or f"0x{i:02X}")
    return changed


def main():
    if len(sys.argv) != 3:
        sys.exit(__doc__)
    glyphs = load(sys.argv[1])
    changed = fix(glyphs)
    save(glyphs, sys.argv[2])
    print(f"{len(changed)} glifos ajustados: {''.join(changed)}")


if __name__ == "__main__":
    main()
