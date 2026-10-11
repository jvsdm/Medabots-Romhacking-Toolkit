"""
custom_lab.py - Bot de teste a partir de uma folha de sprites customizada (vista montada + vista explodida).

Le data/custom/<bot>.json: recorte do bloco na folha, caixa da vista montada e da explodida, e as pecas (parte,
centro aproximado na vista explodida, molde do Core equivalente para o movimento, pai na arvore). Para cada peca:
recorta da vista explodida, acha a posicao na vista montada (casamento de pixels), escolhe o quadro do sprite
(8..64), o pivo (centro na raiz, ponto mais perto do pai nos filhos) e o offset ate o pai, de modo que a pose
parada reproduz a vista montada. Pes na linha do chao do Core (36 px abaixo da origem, mediana dos bots do Core).
Grava moldes novos 50+ (tabela 64, espelho, movimento com 63 linhas, como mold_lab) e as linhas do bot core_bot.

Uso: python tools/custom_lab.py IN.gba OUT.gba data/custom/kwg_03bh.json
"""
import json
import struct
import sys
from pathlib import Path

import numpy as np
from PIL import Image

import mold_lab as ml
from codecs_gba import lz77_store
from medarom import ROOT

GBA = 0x08000000
CORE_ROWS = {"head": (0x7EE70, 16), "right_arm": (0x7F610, 16), "left_arm": (0x7FDB0, 16), "legs": (0x80550, 32)}
SIZES = [(8, 8), (16, 16), (32, 32), (64, 64), (16, 8), (32, 8), (32, 16), (64, 32), (8, 16), (8, 32), (16, 32), (32, 64)]
GROUND = 36          # pe mais baixo, px abaixo da origem das juntas (mediana dos bots do Core na pose parada)
ROTATING = 0x4000    # bit 14 da entrada: sprite afim (o lado espelhado gira pela matriz, sem hflip)


def components(alpha):
    h, w = alpha.shape
    lab = np.zeros((h, w), int)
    n = 0
    for y in range(h):
        for x in range(w):
            if alpha[y, x] and not lab[y, x]:
                n += 1
                stack = [(y, x)]
                lab[y, x] = n
                while stack:
                    cy, cx = stack.pop()
                    for ny in (cy - 1, cy, cy + 1):
                        for nx in (cx - 1, cx, cx + 1):
                            if 0 <= ny < h and 0 <= nx < w and alpha[ny, nx] and not lab[ny, nx]:
                                lab[ny, nx] = n
                                stack.append((ny, nx))
    return lab, n


def best_match(piece, mask, scene, scene_alpha, window=None):
    """Posicao (y, x) da peca na cena com mais pixels de cor igual (dentro da janela (y0, y1, x0, x1), se dada)."""
    ph, pw = mask.shape
    sh, sw = scene_alpha.shape
    best = (-1, 0, 0)
    wy0, wy1, wx0, wx1 = window or (-ph // 2, sh - ph // 2, -pw // 2, sw - pw // 2)
    for y in range(wy0, wy1):
        for x in range(wx0, wx1):
            y0, x0, y1, x1 = max(y, 0), max(x, 0), min(y + ph, sh), min(x + pw, sw)
            if y1 <= y0 or x1 <= x0:
                continue
            m = mask[y0 - y:y1 - y, x0 - x:x1 - x]
            eq = np.all(piece[y0 - y:y1 - y, x0 - x:x1 - x] == scene[y0:y1, x0:x1], axis=2) & scene_alpha[y0:y1, x0:x1]
            score = int((m & eq).sum())
            if score > best[0]:
                best = (score, y, x)
    return best


def frame_size(h, w):
    fits = [(sw, sh) for sw, sh in SIZES if sw >= w and sh >= h]
    return min(fits, key=lambda s: (s[0] * s[1], s))


def tiles(indices):
    h, w = indices.shape
    out = bytearray()
    for ty in range(h // 8):
        for tx in range(w // 8):
            for y in range(8):
                for x in range(0, 8, 2):
                    out.append(indices[ty * 8 + y, tx * 8 + x] | indices[ty * 8 + y, tx * 8 + x + 1] << 4)
    return bytes(out)


def main():
    src, dst, spec_path = sys.argv[1], sys.argv[2], Path(sys.argv[3])
    spec = json.loads(spec_path.read_text(encoding="utf-8"))
    sheet = np.array(Image.open(ROOT / spec["sheet"]).convert("RGBA"))
    bx0, by0, bx1, by1 = spec["block"]
    block = sheet[by0:by1, bx0:bx1]
    rgb, alpha = block[..., :3].astype(int), block[..., 3] > 0

    ax0, ay0, ax1, ay1 = spec["assembled"]
    scene, scene_alpha = rgb[ay0:ay1, ax0:ax1], alpha[ay0:ay1, ax0:ax1]
    ex0, ey0, ex1, ey1 = spec["exploded"]
    lab, n = components(alpha[ey0:ey1, ex0:ex1])

    # Pecas: componente da vista explodida mais perto do centro dado; posicao na vista montada.
    pieces = {}
    for p in spec["pieces"]:
        cx, cy = p["at"][0] - ex0, p["at"][1] - ey0
        best = min(range(1, n + 1), key=lambda i: min(
            (yy - cy) ** 2 + (xx - cx) ** 2 for yy, xx in zip(*np.where(lab == i))))
        ys, xs = np.where(lab == best)
        y0, x0 = ys.min(), xs.min()
        mask = np.zeros((ys.max() - y0 + 1, xs.max() - x0 + 1), bool)
        mask[ys - y0, xs - x0] = True
        img = rgb[ey0 + y0:ey0 + ys.max() + 1, ex0 + x0:ex0 + xs.max() + 1]
        window = None
        if p["parent"]:
            # Filho: perto do pai, deslocado metade do que esta na vista explodida (que afasta as pecas).
            parent = pieces[p["parent"]]
            cy = parent["y"] + (y0 - parent["ey"]) // 2
            cx = parent["x"] + (x0 - parent["ex"]) // 2
            window = (cy - 12, cy + 13, cx - 12, cx + 13)
        score, my, mx = best_match(img, mask, scene, scene_alpha, window)
        pieces[p["name"]] = dict(p, mask=mask, img=img, y=my, x=mx, ey=y0, ex=x0, score=score)
        print(f"{p['name']:11} {mask.shape[1]:2}x{mask.shape[0]:<2} montada em ({mx},{my})  {score}/{mask.sum()} px")

    # Profundidade: na sobreposicao, quem aparece na vista montada fica na frente.
    names = list(pieces)
    wins = {k: 0 for k in names}
    for i, a in enumerate(names):
        for b in names[i + 1:]:
            pa, pb = pieces[a], pieces[b]
            va = vb = 0
            for yy, xx in zip(*np.where(pa["mask"])):
                sy, sx = pa["y"] + yy, pa["x"] + xx
                by_, bx_ = sy - pb["y"], sx - pb["x"]
                if 0 <= by_ < pb["mask"].shape[0] and 0 <= bx_ < pb["mask"].shape[1] and pb["mask"][by_, bx_] \
                        and 0 <= sy < scene.shape[0] and 0 <= sx < scene.shape[1] and scene_alpha[sy, sx]:
                    va += np.array_equal(scene[sy, sx], pa["img"][yy, xx])
                    vb += np.array_equal(scene[sy, sx], pb["img"][by_, bx_])
            if va > vb:
                wins[a] += 1
            elif vb > va:
                wins[b] += 1
    order = sorted(names, key=lambda k: -wins[k])          # frente -> fundo
    zabs = {k: 2 * i - len(order) for i, k in enumerate(order)}   # menor = frente
    print("frente -> fundo:", order)

    # Uma paleta por parte (cada parte tem o seu banco), <= 15 cores + transparente.
    palettes = {}
    for part in CORE_ROWS:
        colors = sorted({tuple(c) for p in pieces.values() if p["part"] == part for c in p["img"][p["mask"]]})
        merged = {c: c for c in colors}
        while len(colors) > 15:          # junta as duas cores mais proximas
            a_, b_ = min(((x, y) for i, x in enumerate(colors) for y in colors[i + 1:]),
                         key=lambda t: sum((u - v) ** 2 for u, v in zip(*t)))
            colors.remove(b_)
            merged = {k: (a_ if v == b_ else v) for k, v in merged.items()}
        palette = [(0, 0, 0)] + colors
        data = b"".join(struct.pack("<H", (r >> 3) | (g >> 3) << 5 | (b >> 3) << 10) for r, g, b in palette)
        palettes[part] = ({k: palette.index(v) for k, v in merged.items()}, data + bytes(32 - len(data)))
        print(f"{part}: {len(merged)} cores -> {len(colors)}")

    # Previa: pecas remontadas nas posicoes achadas (fundo -> frente) ao lado da vista montada.
    preview = np.zeros(scene.shape[:2] + (3,), np.uint8) + 40
    for name in reversed(order):
        q = pieces[name]
        for yy, xx in zip(*np.where(q["mask"])):
            sy, sx = q["y"] + yy, q["x"] + xx
            if 0 <= sy < preview.shape[0] and 0 <= sx < preview.shape[1]:
                preview[sy, sx] = q["img"][yy, xx]
    original = np.where(scene_alpha[..., None], scene, 40).astype(np.uint8)
    both = np.concatenate([original, np.zeros((scene.shape[0], 4, 3), np.uint8), preview], axis=1)
    Image.fromarray(both).resize((both.shape[1] * 4, both.shape[0] * 4), Image.NEAREST).save(Path(dst).with_suffix(".preview.png"))

    # Quadros: pares espelhados com o mesmo tamanho (o lado direito troca as artes dos bracos peca a peca).
    for p in pieces.values():
        p["frame"] = frame_size(*p["mask"].shape)
    for a, b in spec.get("mirror_pairs", []):
        w = max(pieces[a]["frame"][0], pieces[b]["frame"][0])
        h = max(pieces[a]["frame"][1], pieces[b]["frame"][1])
        pieces[a]["frame"] = pieces[b]["frame"] = frame_size(h, w)

    # Pivos: centro na raiz; nos filhos, pixel mais perto do pivo do pai. Coordenadas absolutas da vista montada.
    def place(name):
        p = pieces[name]
        ys, xs = np.where(p["mask"])
        if p["parent"] is None:
            py, px = int(round(ys.mean())), int(round(xs.mean()))
        else:
            parent = pieces[p["parent"]]
            if "pivot_abs" not in parent:
                place(p["parent"])
            # Ponto de contato: pixel do filho mais perto de algum pixel do pai (a articulacao).
            pys, pxs = np.where(parent["mask"])
            pys, pxs = pys + parent["y"], pxs + parent["x"]
            d = (ys[:, None] + p["y"] - pys[None, :]) ** 2 + (xs[:, None] + p["x"] - pxs[None, :]) ** 2
            k = int(np.argmin(d.min(axis=1)))
            py, px = int(ys[k]), int(xs[k])
        p["pivot"] = (py, px)
        p["pivot_abs"] = (p["y"] + py, p["x"] + px)
    for name in pieces:
        place(name)

    # Origem das juntas: pivo do quadril na horizontal; na vertical, pes GROUND px abaixo.
    feet = max(p["y"] + int(np.where(p["mask"])[0].max()) for p in pieces.values())
    hip = pieces[next(k for k, p in pieces.items() if p["part"] == "legs" and p["parent"] is None)]
    origin = (feet - GROUND, hip["pivot_abs"][1])

    rom = bytearray(open(src, "rb").read())
    space = ml.Space(rom)
    table = bytearray(rom[ml.MOLDS:ml.MOLDS + 4 * ml.MOLD_COUNT]) + bytearray(4 * (ml.NEW_MOLDS - ml.MOLD_COUNT))
    mold = {name: 50 + i for i, name in enumerate(pieces)}
    children = {}
    for name, p in pieces.items():
        if p["parent"]:
            children.setdefault(p["parent"], []).append(name)

    for name, p in pieces.items():
        base = rom[ml.u32(rom, ml.MOLDS + 4 * p["analog"]) - GBA:][:0x18]
        rec = bytearray(base)
        w, h = p["frame"]
        index, pal_bytes = palettes[p["part"]]
        canvas = np.zeros((h, w), int)
        for yy, xx in zip(*np.where(p["mask"])):
            canvas[yy, xx] = index[tuple(p["img"][yy, xx])]
        picture = GBA + space.store(lz77_store(tiles(canvas)))
        pal = GBA + space.store(pal_bytes)
        art = GBA + space.store(struct.pack("<II", picture, pal) * 256)      # qualquer seletor -> mesma arte
        struct.pack_into("<I", rec, 0, art)
        struct.pack_into("<ii", rec, 4, p["pivot"][0], p["pivot"][1])
        if p["parent"]:
            parent = pieces[p["parent"]]
            dy, dx = p["pivot_abs"][0] - parent["pivot_abs"][0], p["pivot_abs"][1] - parent["pivot_abs"][1]
            dz = zabs[name] - zabs[p["parent"]]
        else:
            dy, dx = p["pivot_abs"][0] - origin[0], p["pivot_abs"][1] - origin[1]
            dz = zabs[name]
        struct.pack_into("<bbb", rec, 0x0C, dy, dx, dz)
        kids = children.get(name, [])
        rec[0x10] = mold[kids[0]] if kids else 0
        siblings = children.get(p["parent"], []) if p["parent"] else []
        nxt = siblings.index(name) + 1 if name in siblings else 0
        rec[0x0F] = mold[siblings[nxt]] if siblings and nxt < len(siblings) else 0
        rec[0x12], rec[0x13] = h, w
        rec[0x15] = 0
        put = GBA + space.store(bytes(rec))
        ml.put32(table, 4 * mold[name], put)
        print(f"molde {mold[name]} {name:11} {w}x{h} pivo {p['pivot']} offset ({dy},{dx},{dz}) filho {rec[0x10]} irmao {rec[0x0F]}")

    new_table = GBA + space.store(bytes(table))
    for at in ml.loaded_literals(rom, GBA + ml.MOLDS):
        ml.put32(rom, at, new_table)
    mirror = bytearray(rom[ml.MIRROR_MAP:ml.MIRROR_MAP + ml.MOLD_COUNT]) + bytearray(range(ml.MOLD_COUNT, ml.NEW_MOLDS))
    for a, b in spec.get("mirror_pairs", []):
        mirror[mold[a]], mirror[mold[b]] = mold[b], mold[a]
    new_mirror = GBA + space.store(bytes(mirror))
    for at in ml.loaded_literals(rom, GBA + ml.MIRROR_MAP):
        ml.put32(rom, at, new_mirror)
    print("movimento: blocos", ml.expand_motion(rom, space, {mold[k]: p["analog"] for k, p in pieces.items()}))
    # Os deslocamentos (tabela B) do Metabee valem para a geometria dele: as pecas novas ficam so com os angulos.
    zeroed = set()
    for script in range(ml.SCRIPTS):
        c = ml.u32(rom, ml.MOTION_C + 4 * script) - GBA
        keys = 0
        while rom[c + keys]:
            keys += 1
        b_rows = ml.u32(rom, ml.MOTION_B + 4 * script) - GBA
        if keys == 0 or b_rows in zeroed:
            continue
        zeroed.add(b_rows)
        for k in pieces:
            at = b_rows + (mold[k] - 1) * keys * 2
            rom[at:at + keys * 2] = bytes(keys * 2)

    bot = spec["core_bot"]
    for part, (rows, size) in CORE_ROWS.items():
        at = rows + bot * size
        rom[at:at + size] = bytes(size)
        names_in = [k for k, p in pieces.items() if p["part"] == part]
        for k, name in enumerate(names_in):
            ml.put32(rom, at + 4 * k, mold[name] | 1 << 6 | ROTATING)
    open(dst, "wb").write(rom)
    print("->", dst)


if __name__ == "__main__":
    main()
