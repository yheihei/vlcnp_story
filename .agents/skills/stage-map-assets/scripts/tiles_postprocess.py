#!/usr/bin/env python3
"""KazeBoss タイル後処理。

生成された大きいテクスチャから、単体でリピートしても継ぎ目が出にくい 32x32 タイルを切り出す。

1. 論理解像度(縮小後サイズ)を複数試し、「32x32 で切ったときにラップアラウンドの継ぎ目が
   最も目立たないスケール」を探す。障子の格子・板の幅などの模様周期を 32 の約数へ自動で寄せる効果がある。
2. そのスケールで継ぎ目コストの低いクロップを、互いに似すぎないように N 枚選ぶ。
3. 全タイル共通パレットへ量子化し、平均色を全体へ寄せて色味が浮かないようにする。
"""
import os
import sys
import json
from PIL import Image
import numpy as np

WORK = os.environ.get("WORK", os.path.dirname(os.path.abspath(__file__)))
GEN = os.path.join(WORK, "gen")
OUT = os.path.join(WORK, "tiles")
TILE = 32
SCALES = [96, 104, 112, 120, 128, 136, 144, 152, 160, 176, 192]


def load_logical(path, size):
    im = Image.open(path).convert("RGB")
    return np.asarray(im.resize((size, size), Image.BOX)).astype(np.float32)


def all_crops(arr):
    """トーラス状の全 32x32 クロップを (N, 32, 32, 3) で返す。"""
    n = arr.shape[0]
    tiled = np.concatenate([np.concatenate([arr, arr], axis=1)] * 2, axis=0)
    idx = np.arange(TILE)
    crops = np.empty((n, n, TILE, TILE, 3), dtype=np.float32)
    for y in range(n):
        for x in range(n):
            crops[y, x] = tiled[y + idx][:, x + idx]
    return crops


def seam_costs(crops):
    """タイル単体を上下左右にリピートしたときの継ぎ目の目立ちやすさ(小さいほど良い)。"""
    h_seam = np.abs(crops[..., :, -1, :] - crops[..., :, 0, :]).mean(axis=(-2, -1))
    v_seam = np.abs(crops[..., -1, :, :] - crops[..., 0, :, :]).mean(axis=(-2, -1))
    h_ref = np.abs(np.diff(crops, axis=-2)).mean(axis=(-3, -2, -1)) + 1e-6
    v_ref = np.abs(np.diff(crops, axis=-3)).mean(axis=(-3, -2, -1)) + 1e-6
    return h_seam / h_ref + v_seam / v_ref


def greenness(crops):
    """苔・草の量。緑成分が赤青平均をどれだけ上回るか。"""
    m = crops.mean(axis=(-3, -2))
    return m[..., 1] - (m[..., 0] + m[..., 2]) / 2.0


def apply_bias(costs, crops, bias):
    """素材ごとの好み(苔が写っている / 暗い など)をコストに反映する。"""
    if not bias:
        return costs
    out = costs.copy()
    if "green" in bias:
        out -= greenness(crops) * float(bias["green"])
    if "dark" in bias:
        out += crops.mean(axis=(-3, -2, -1)) * float(bias["dark"])
    return out


def best_scale(path, scales=SCALES, bias=None):
    """継ぎ目コストの下位が最も小さくなる論理解像度を選ぶ。"""
    best = None
    for s in scales:
        arr = load_logical(path, s)
        crops = all_crops(arr)
        costs = apply_bias(seam_costs(crops), crops, bias)
        score = np.sort(costs.ravel())[: max(4, costs.size // 200)].mean()
        if best is None or score < best[0]:
            best = (score, s, arr, crops, costs)
    return best


def pick_variants(crops, costs, n_variants, min_dist=11.0, min_offset=0):
    """min_offset: 切り出し位置がこれ未満しか離れていない候補は同じ絵柄とみなして弾く。

    藁すさのような高周波テクスチャでは 1px ずらすだけで画素差が大きく出るため、
    画素差だけでは「見た目が同じ変種」を除けない。位置の距離も併用する。
    """
    n = costs.shape[0]

    def far_enough(y, x, py, px):
        dy = min(abs(y - py), n - abs(y - py))
        dx = min(abs(x - px), n - abs(x - px))
        return max(dy, dx) >= min_offset

    order = np.dstack(np.unravel_index(np.argsort(costs.ravel()), costs.shape))[0]
    picked = []
    for y, x in order:
        if len(picked) >= n_variants:
            break
        t = crops[y, x]
        if all(np.abs(t - p[3]).mean() >= min_dist for p in picked) and \
           all(far_enough(y, x, p[1], p[2]) for p in picked):
            picked.append((float(costs[y, x]), int(y), int(x), t))
    i = 0
    while len(picked) < n_variants and i < len(order):
        y, x = order[i]
        t = crops[y, x]
        if not any(np.array_equal(t, p[3]) for p in picked):
            picked.append((float(costs[y, x]), int(y), int(x), t))
        i += 1
    return picked[:n_variants]


def harmonize(tiles, target_mean, strength):
    out = []
    for t in tiles:
        shift = (target_mean - t.reshape(-1, 3).mean(axis=0)) * strength
        out.append(np.clip(t + shift, 0, 255))
    return out


def shared_palette_quantize(tiles, colors):
    n = len(tiles)
    atlas = Image.new("RGB", (TILE * n, TILE))
    for i, t in enumerate(tiles):
        atlas.paste(Image.fromarray(t.astype(np.uint8)), (i * TILE, 0))
    q = atlas.quantize(colors=colors, method=Image.MEDIANCUT, dither=Image.NONE).convert("RGB")
    return [np.asarray(q.crop((i * TILE, 0, (i + 1) * TILE, TILE))).astype(np.float32) for i in range(n)]


def main(spec_path):
    with open(spec_path) as f:
        spec = json.load(f)
    os.makedirs(OUT, exist_ok=True)

    names, tiles, meta = [], [], []
    for entry in spec:
        src = os.path.join(GEN, entry["src"])
        if not os.path.exists(src):
            print("MISSING:", src)
            continue
        scales = entry.get("scales", SCALES)
        score, s, arr, crops, costs = best_scale(src, scales, entry.get("bias"))
        picked = pick_variants(crops, costs, entry["variants"],
                               entry.get("min_dist", 11.0), entry.get("min_offset", 0))
        print("%-22s scale=%-4d score=%.3f" % (entry["name"], s, score))
        for i, (cost, y, x, t) in enumerate(picked, start=1):
            names.append("%s_%02d_32x32" % (entry["name"], i))
            tiles.append(t)
            meta.append((entry["src"], s, round(cost, 3), y, x))

    all_px = np.concatenate([t.reshape(-1, 3) for t in tiles], axis=0)
    target = all_px.mean(axis=0)
    print("global mean:", target.round(1))

    tiles = harmonize(tiles, target, float(os.environ.get("HARMONIZE", "0.5")))
    tiles = shared_palette_quantize(tiles, int(os.environ.get("PALETTE", "64")))

    for name, t, m in zip(names, tiles, meta):
        Image.fromarray(t.astype(np.uint8)).save(os.path.join(OUT, name + ".png"))
        print("  %-34s scale=%-4d cost=%-7s @(%d,%d)" % (name + ".png", m[1], m[2], m[3], m[4]))
    print("total tiles:", len(names))


if __name__ == "__main__":
    main(sys.argv[1])
