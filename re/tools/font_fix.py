"""Corrige o alinhamento da fonte do dialogo (Metabee E) sem mudar o desenho dos glifos.

    python font_fix.py font.png font_corrigida.png [--sombra N]

Entrada e saida no formato do insert_font.py: 128x128, 16 glifos 8x16 por linha, cores da paleta da fonte
(0x084E43D8). Glifos novos (0x4F, 0x7D-0x7F etc.) passam intactos, exceto pela regra geral da linha 9.

Correcoes (linhas 0-15 de cada glifo; maiusculas ocupam 2-8):
- linha 9 so com tons claros (indice < 9) em letras e algarismos sem descendente: era um borrao abaixo
  da base que faz B, F, K, M, S, Y, k, m, v, 0-8 parecerem mais baixos que A, C, G, O...
- E: barra de baixo dupla (linhas 8 e 9) -> fica so a 8.
- D: borrao escuro na linha 9 -> removido.
- L: pe duplo (8 e 9) e topo apagado (linha 2) -> linha 9 removida, linha 2 = linha 3.
- l: topo apagado (linha 2) e borrao na 9 -> linha 2 = linha 3, linha 9 removida.
- pontuacao na base (linha 8): '.' e o ponto de '!' e '?' sobem 1 linha; ',' sobe 1 (cauda na 9); o fim da
  haste do '?' (linha 7) sai para nao encostar no ponto.
- sombra: tons claros (indices 2..N, padrao 5) viram fundo; 0 desliga. Indices maiores = cores mais escuras.
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


def move_up(g, rows):
    for r in rows:
        g[r - 1] = g[r][:]
        clear(g, r)


PUNCTUATION_UP = {".": [9], ",": [8, 9, 10], "!": [9]}


def fix(glyphs, shadow=5):
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
        if ch in PUNCTUATION_UP:
            move_up(g, PUNCTUATION_UP[ch])
        elif ch == "?":
            clear(g, 7)
            move_up(g, [9])
        if shadow:
            for r in range(16):
                g[r] = [BACKGROUND if 1 < v <= shadow else v for v in g[r]]
        if g != before:
            changed.append(ch or f"0x{i:02X}")
    return changed


def main():
    args = sys.argv[1:]
    shadow = 5
    if "--sombra" in args:
        k = args.index("--sombra")
        shadow = int(args[k + 1])
        del args[k:k + 2]
    if len(args) != 2:
        sys.exit(__doc__)
    glyphs = load(args[0])
    changed = fix(glyphs, shadow)
    save(glyphs, args[1])
    print(f"{len(changed)} glifos ajustados: {''.join(changed)}")


if __name__ == "__main__":
    main()
