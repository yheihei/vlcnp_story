#!/usr/bin/env python3
"""タイルの確認用プレビュー生成。

tiles_grid.png  : 各タイルを 3x3 に敷き詰めて、単体リピート時の継ぎ目を確認する
tiles_mix.png   : 全タイルを横一列に並べて、隣り合ったときの色味の浮きを確認する
tiles_room.png  : 背景タイルの上に地面タイルを置いた、実際の使われ方に近い合成
"""
import os
import sys
import glob
from PIL import Image, ImageDraw

WORK = os.environ.get("WORK", os.path.dirname(os.path.abspath(__file__)))
OUT = os.path.join(WORK, "tiles")
Z = 4  # 表示倍率


def load_all():
    paths = sorted(glob.glob(os.path.join(OUT, "*.png")))
    bg = [p for p in paths if os.path.basename(p).startswith("bg_")]
    gr = [p for p in paths if os.path.basename(p).startswith("ground_")]
    return bg, gr


def grid_sheet(paths, cols=7):
    cell = 32 * 3 * Z
    pad, label = 10, 16
    rows = (len(paths) + cols - 1) // cols
    sheet = Image.new("RGB", (cols * (cell + pad) + pad, rows * (cell + pad + label) + pad), (30, 30, 32))
    d = ImageDraw.Draw(sheet)
    for i, p in enumerate(paths):
        t = Image.open(p).convert("RGB")
        block = Image.new("RGB", (96, 96))
        for y in range(3):
            for x in range(3):
                block.paste(t, (x * 32, y * 32))
        block = block.resize((cell, cell), Image.NEAREST)
        cx = (i % cols) * (cell + pad) + pad
        cy = (i // cols) * (cell + pad + label) + pad
        sheet.paste(block, (cx, cy))
        d.text((cx, cy + cell + 2), os.path.basename(p)[:-10], fill=(200, 200, 200))
    return sheet


def mix_row(paths):
    """タイルを1枚ずつ横に隣接させ、色味の段差を見る。"""
    w = len(paths) * 32
    strip = Image.new("RGB", (w, 64))
    for i, p in enumerate(paths):
        t = Image.open(p).convert("RGB")
        strip.paste(t, (i * 32, 0))
        strip.paste(t, (i * 32, 32))
    return strip.resize((w * Z, 64 * Z), Image.NEAREST)


def room(bg, gr):
    """背景を敷いた上に地面を置いた、部屋らしい構図の合成。"""
    W, H = 24, 14
    canvas = Image.new("RGB", (W * 32, H * 32))
    bgs = [Image.open(p).convert("RGB") for p in bg]
    grs = [Image.open(p).convert("RGB") for p in gr]
    for y in range(H):
        for x in range(W):
            canvas.paste(bgs[(x * 3 + y * 5) % len(bgs)], (x * 32, y * 32))
    for x in range(W):
        for y in range(H - 3, H):
            canvas.paste(grs[(x * 2 + y) % len(grs)], (x * 32, y * 32))
    for x in range(6, 12):
        canvas.paste(grs[x % len(grs)], (x * 32, (H - 7) * 32))
    return canvas.resize((W * 32 * 3, H * 32 * 3), Image.NEAREST)


def main():
    bg, gr = load_all()
    allp = bg + gr
    grid_sheet(allp).save(os.path.join(WORK, "tiles_grid.png"))
    mix_row(allp).save(os.path.join(WORK, "tiles_mix.png"))
    if bg and gr:
        room(bg, gr).save(os.path.join(WORK, "tiles_room.png"))
    print("bg=%d ground=%d" % (len(bg), len(gr)))


if __name__ == "__main__":
    main()
