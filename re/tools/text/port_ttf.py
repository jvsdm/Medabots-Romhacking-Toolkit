"""port_ttf.py ROM_IN ROM_OUT TTF SIZE [--weight W] [--italic TTF_ITALIC] [--gamma G]
Rasteriza uma fonte TrueType nas celulas 8x16 4bpp da fonte do dialogo (Metabee E, endereco do jogo) com a
linha de base na linha 10, converte a cobertura do antialias nos tons 1 (fundo) a F e grava as larguras.
Avanco maximo 8: com 9 o jogo apaga a letra seguinte em algumas posicoes (testado: "Tem um" saia "Tem  m").
Avancos = largura da propria fonte (+0,3 px antes de arredondar, espaco >= 3), como o navegador; '·' (reticencias
"···" no texto) e desenhado como '.' na linha de base. Teste: Roboto 10 px, peso 400 ou 500 (normal e italica)."""
import argparse, sys
from PIL import Image, ImageDraw, ImageFont
sys.path.insert(0, r"D:\claude-brain\Medabots\git\Medabots-Romhacking-Toolkit\re\tools\text")
import medtext as m

FONTS = {False: (0x4BFC64, 0x3B4E08), True: (0x4C59A4, 0x3B5008)}
BASELINE = 10
# acrescimo antes de arredondar: em 10 px as larguras sao fracionadas e o arredondamento aperta o texto
TRACKING = 0.3
# glifos 0x50/0x51/0x64/0x65 mostram Ã ã Õ õ na fonte PT-BR
CHARS = list(m.CHARS)
for g, c in ((0x50, 'Ã'), (0x51, 'ã'), (0x64, 'Õ'), (0x65, 'õ')):
    CHARS[g] = c


def load(path, size, weight):
    f = ImageFont.truetype(path, size)
    if weight:
        try:
            f.set_variation_by_axes([weight] + [100] * (len(f.get_variation_axes()) - 1))
        except Exception:
            pass
    return f


def raster(font, ch, gamma):
    # caneta na coluna 0 da celula e linha de base na linha 10, como o navegador desenha; avanco = largura da
    # propria fonte (arredondada). '·' e reticencias no jogo ("···"): desenhado como '.' na linha de base.
    if ch == '·':
        ch = '.'
    im = Image.new("L", (32, 32), 0)
    d = ImageDraw.Draw(im)
    asc = font.getmetrics()[0]
    d.text((8, BASELINE + 8 - asc), ch, font=font, fill=255)
    cell = [[round(15 * (im.getpixel((8 + x, 8 + y)) / 255) ** gamma) for x in range(8)] for y in range(16)]
    return cell, round(font.getlength(ch) + TRACKING)


def encode(cell):
    out = bytearray(64)
    for y in range(16):
        for x in range(8):
            v = max(1, cell[y][x])  # 0/1 = fundo; tinta 2-F
            i = (y // 8) * 32 + (y % 8) * 4 + x // 2
            out[i] |= v << 4 if x & 1 else v
    return bytes(out)


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("rom_in"); ap.add_argument("rom_out"); ap.add_argument("ttf"); ap.add_argument("size", type=int)
    ap.add_argument("--weight", type=float); ap.add_argument("--italic"); ap.add_argument("--gamma", type=float, default=0.8)
    a = ap.parse_args()
    rom = bytearray(open(a.rom_in, "rb").read())
    for italic, path in ((False, a.ttf), (True, a.italic)):
        if not path:
            continue
        font = load(path, a.size, a.weight)
        base, widths = FONTS[italic]
        for g in range(1, 128):
            cell, advance = raster(font, CHARS[g], a.gamma)
            rom[base + g * 64:base + g * 64 + 64] = encode(cell)
            rom[widths + g * 2] = max(1, min(8, advance))
        rom[widths] = max(3, min(8, round(font.getlength(' ') + TRACKING)))  # espaco
    open(a.rom_out, "wb").write(rom)


if __name__ == "__main__":
    main()
