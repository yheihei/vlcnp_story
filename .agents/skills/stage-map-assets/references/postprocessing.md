# 後処理スクリプト

利用中の画像ツールがこの後処理を許す場合に使う。必要なライブラリは Pillow と numpy。利用できる Python 環境を使い、未導入なら作業用の環境を用意する。既存環境を変更する必要はない。

以下はリポジトリのルートで実行する例。`WORK` はスクリプトが受け取る作業ディレクトリ。`gen/` に原画を置く。スクリプトは既存の出力名を上書きし、欠落した入力をスキップするため、初回や再評価では専用の作業ディレクトリを使い、終了コードだけでなく出力の対応を確認する。

## タイル

```bash
WORK="/absolute/path/work" python3 .agents/skills/stage-map-assets/scripts/tiles_postprocess.py "/absolute/path/spec.json"
```

spec は次のオブジェクトの配列。

| キー | 意味 |
| --- | --- |
| `name` | 出力名の接頭辞。例: `bg_plaster` |
| `src` | `WORK/gen/` からの原画の相対パス |
| `variants` | 変種数 |
| `scales` | 任意。探索する論理解像度。既定は96〜192の候補 |
| `min_dist` | 任意。変種間の画素差 |
| `min_offset` | 任意。切り出し位置の距離 |
| `bias` | 任意。`{"green": 0.6}` など、候補の色への重み |

`WORK/tiles/` に `<name>_01_32x32.png` などを出力する。継ぎ目コストによる切り出し、平均色寄せ、共通パレットへの量子化を行う。`HARMONIZE` と `PALETTE` 環境変数で色寄せの強さと色数を調整できる。

画素差だけで似た変種を拾う場合は `min_offset`、格子の細かさは `scales` を調整する。評価値が低くても継ぎ目の消失は保証されない。

## Props

```bash
WORK="/absolute/path/work" TILES="/absolute/path/tiles" python3 .agents/skills/stage-map-assets/scripts/props_postprocess.py "/absolute/path/spec_props.json"
```

spec は `name`、`src`、`w`、`h` を持つオブジェクトの配列。`src` は `WORK/gen/_props/` からの相対パス。`WORK/props/<name>_<w>x<h>.png` に縦横比を保って収め、透明余白を付ける。サイズは配置単位に合う32の倍数を目安に、指定寸法を優先する。

入力はマゼンタ背景を前提とし、既存の alpha は使わない。すでに透過した画像や本体にマゼンタがある画像には適用しない。`TILES` は完成タイルのディレクトリで、Props と共通パレットを作る母集団になる。

細線は premultiplied alpha の縮小と max pooling で残す。細線が消える場合や縁が汚れる場合は原画と出力を比較する。苔などの色が失われる場合は共通パレットの母集団を見直す。

## 確認用プレビュー

```bash
WORK="/absolute/path/work" python3 .agents/skills/stage-map-assets/scripts/preview_tiles.py
```

- `tiles_grid.png`: 各タイルの3×3反復。
- `tiles_mix.png`: タイル同士の隣接色。
- `tiles_room.png`: `bg_` と `ground_` の両方がある場合の部屋合成。

Props の市松背景プレビューやタイルへの重ね合わせは、このスクリプトの出力には含まれない。必要な組み合わせを別途確認する。
