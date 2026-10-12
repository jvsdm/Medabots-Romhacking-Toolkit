"""port_ttf.py ROM_IN ROM_OUT TTF SIZE [--weight W] [--italic TTF_ITALIC] [--gamma G]
Rasteriza uma fonte TrueType nas celulas 8x16 4bpp da fonte do dialogo (Metabee E, endereco do jogo) com a
linha de base na linha 10, converte a cobertura do antialias nos tons 1 (fundo) a F e grava as larguras.
Avanco maximo 8: com 9 o jogo apaga a letra seguinte em algumas posicoes (testado: "Tem um" saia "Tem  m").
Teste: Roboto 10 px peso 500 (normal e italica) e a mais proxima da fonte do jogo."""
import argparse, sys
from PIL import Image, ImageDraw, ImageFont
sys.path.insert(0, r"D:\claude-brain\Medabots\git\Medabots-Romhacking-Toolkit\re\tools\text")
import medtext as m

FONTS = {False: (0x4BFC64, 0x3B4E08), True: (0x4C59A4, 0x3B5008)}
BASELINE = 10
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
    # desenha grande o bastante e recorta a celula: x a partir da tinta, linha de base na linha 10
    im = Image.new("L", (32, 32), 0)
    d = ImageDraw.Draw(im)
    asc = font.getmetrics()[0]
    d.text((8, BASELINE + 8 - asc), ch, font=font, fill=255)
    box = im.getbbox()
    cell = [[0] * 8 for _ in range(16)]
    if not box:
        return cell, 0
    left = box[0]
    right = -1
    for y in range(16):
        for x in range(8):
            v = im.getpixel((left + x, y + 8)) if left + x < 32 else 0
            cell[y][x] = round(15 * (v / 255) ** gamma)
    for x in range(8):
        if any(cell[y][x] >= 4 for y in range(16)):
            right = x
    return cell, right


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
            cell, right = raster(font, CHARS[g], a.gamma)
            rom[base + g * 64:base + g * 64 + 64] = encode(cell)
            rom[widths + g * 2] = min(8, right + 2) if right >= 0 else 3
        rom[widths] = 3  # espaco
    open(a.rom_out, "wb").write(rom)


if __name__ == "__main__":
    main()
