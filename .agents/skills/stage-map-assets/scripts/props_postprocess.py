#!/usr/bin/env python3
"""KazeBoss プロップ後処理。

image_gen が出したマゼンタ背景の原画から、透過 PNG のプロップスプライトを作る。

1. 純マゼンタ背景をアルファへ抜く(にじみの中間色も距離ベースで判定し、色かぶりを除去)
2. 中身のバウンディングボックスで切り詰める
3. アスペクト比を保ったまま指定サイズに収め、透明で余白を埋めて寸法を確定させる
4. タイルセットと同じ共通パレットへ寄せて色味を揃える
"""
import os
import sys
import json
import glob
from PIL import Image
import numpy as np

WORK = os.environ.get("WORK", os.path.dirname(os.path.abspath(__file__)))
RAW = os.path.join(WORK, "gen", "_props")
OUT = os.path.join(WORK, "props")
# 完成済みタイル群のディレクトリ。共通パレットの母集団に使う。
TILES = os.environ["TILES"]
KEY = np.array([255.0, 0.0, 255.0])


def unkey(im, hard=90.0, soft=170.0):
    """マゼンタ背景を抜く。hard 未満は完全透明、soft 以上は完全不透明、間は線形。"""
    a = np.asarray(im.convert("RGB")).astype(np.float32)
    dist = np.sqrt(((a - KEY) ** 2).sum(axis=2))
    alpha = np.clip((dist - hard) / (soft - hard), 0.0, 1.0)

    # 半透明部分に残るマゼンタかぶりを除去する(despill): 緑を超える赤青成分を抑える
    r, g, b = a[..., 0], a[..., 1], a[..., 2]
    spill = np.minimum(r, b) - g
    m = spill > 0
    r = np.where(m, r - spill * 0.5, r)
    b = np.where(m, b - spill * 0.5, b)
    rgb = np.clip(np.dstack([r, g, b]), 0, 255)

    out = np.dstack([rgb, alpha * 255.0]).astype(np.uint8)
    return Image.fromarray(out, "RGBA")


def trim(im):
    bbox = im.split()[-1].point(lambda v: 255 if v > 24 else 0).getbbox()
    return im.crop(bbox) if bbox else im


def maxpool_to(mask, nw, nh):
    """max プーリングで縮小する。面積平均と違い、ひび割れや蔦のような細い線が消えない。"""
    a = mask
    while a.shape[1] > nw * 2 and a.shape[0] > nh * 2:
        h, w = a.shape
        a = np.pad(a, ((0, h % 2), (0, w % 2)), mode="constant")
        a = np.maximum.reduce([a[0::2, 0::2], a[0::2, 1::2], a[1::2, 0::2], a[1::2, 1::2]])
    return np.asarray(Image.fromarray(a).resize((nw, nh), Image.BOX))


def fit(im, w, h, thin_boost=0.75, thresh=128):
    """アスペクト比を保って (w, h) に収め、透明余白で中央寄せする。

    色は premultiplied alpha で縮小してから割り戻す。こうすると被覆率が低い細い線でも
    背景色に引きずられず本来の色が残る。アルファは面積平均と max プーリングの大きい方を
    採用し、細部を残したまま二値化する。
    """
    sw, sh = im.size
    scale = min(w / sw, h / sh)
    nw, nh = max(1, round(sw * scale)), max(1, round(sh * scale))

    a = np.asarray(im).astype(np.float32)
    alpha = a[..., 3] / 255.0

    # uint8 に丸めると被覆率の低い画素の色が 0 に潰れて黒いゴミになるため float のまま縮小する
    def shrink(plane):
        return np.asarray(Image.fromarray(plane.astype(np.float32), "F").resize((nw, nh), Image.BOX))

    ad = shrink(alpha)
    color = np.clip(np.dstack([shrink(a[..., c] * alpha) for c in range(3)]) / np.maximum(ad, 1e-3)[..., None], 0, 255)

    # 細部は max プーリングで救うが、被覆率が極端に低い孤立画素(輪郭のにじみ)は拾わない
    hard = ((a[..., 3] >= thresh) * 255).astype(np.uint8)
    boost = maxpool_to(hard, nw, nh).astype(np.float32) * thin_boost * (ad >= 0.05)
    out_a = np.where(np.maximum(ad * 255.0, boost) >= thresh, 255, 0).astype(np.uint8)

    small = Image.fromarray(np.dstack([color.astype(np.uint8), out_a]), "RGBA")
    canvas = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    canvas.paste(small, ((w - nw) // 2, (h - nh) // 2))
    return canvas


def joint_palette(props, colors=96):
    """タイル群 + プロップ群からまとめて共通パレットを作る。

    タイルだけから作るとパレットが茶系に偏り、草や蔦の緑が飛ぶ。
    プロップ側の色も母集団に入れつつ、タイルと同じ色空間に乗せる。
    """
    samples = []
    for p in sorted(glob.glob(os.path.join(TILES, "*.png"))):
        samples.append(np.asarray(Image.open(p).convert("RGB")).reshape(-1, 3))
    for im in props:
        a = np.asarray(im)
        visible = a[a[..., 3] > 0][:, :3]
        if len(visible):
            # プロップ1個あたりの寄与をタイル1枚分(1024px)程度に揃える
            idx = np.random.default_rng(0).choice(len(visible), size=min(len(visible), 1024), replace=False)
            samples.append(visible[idx])
    px = np.concatenate(samples, axis=0)
    side = int(np.ceil(np.sqrt(len(px))))
    canvas = np.zeros((side * side, 3), dtype=np.uint8)
    canvas[: len(px)] = px
    atlas = Image.fromarray(canvas.reshape(side, side, 3), "RGB")
    return atlas.quantize(colors=colors, method=Image.MEDIANCUT, dither=Image.NONE)


def to_palette(im, pal):
    """タイルと同じパレットへ寄せる。透明部分はそのまま残す。"""
    rgb = im.convert("RGB").quantize(palette=pal, dither=Image.NONE).convert("RGB")
    out = np.dstack([np.asarray(rgb), np.asarray(im)[..., 3]])
    return Image.fromarray(out.astype(np.uint8), "RGBA")


def main(spec_path):
    with open(spec_path) as f:
        spec = json.load(f)
    os.makedirs(OUT, exist_ok=True)

    # 1パス目: マゼンタ抜き + 寸法確定
    cut, names, missing = [], [], []
    for e in spec:
        src = os.path.join(RAW, e["src"])
        if not os.path.exists(src):
            missing.append(e["src"])
            continue
        w, h = e["w"], e["h"]
        cut.append(fit(trim(unkey(Image.open(src))), w, h))
        names.append("%s_%dx%d" % (e["name"], w, h))

    # 2パス目: タイルと共通のパレットへ量子化
    pal = joint_palette(cut)
    made = []
    for name, im in zip(names, cut):
        out = to_palette(im, pal)
        out.save(os.path.join(OUT, name + ".png"))
        cover = (np.asarray(out)[..., 3] > 0).mean()
        made.append((name, out.size, round(float(cover), 2)))

    for n, size, cover in made:
        print("  %-28s %s alpha_cover=%.2f" % (n + ".png", size, cover))
    if missing:
        print("MISSING:", ", ".join(missing))
    print("props:", len(made))


if __name__ == "__main__":
    main(sys.argv[1])
