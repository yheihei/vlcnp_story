---
name: stage-map-assets
description: Build a complete Unity 2D stage/map asset set for vlcnpStory2022 from a reference image and a stage description — generating tile and prop PNGs via Codex image_gen, extracting seamless 32px tiles, harmonizing colors, and creating Tile assets, a Tile Palette prefab, and SpriteRenderer prop prefabs. Use when asked to make tilemap or map materials for a new stage, area, or boss room ("このイメージのステージ素材を作って", "ボスマップのタイルマップを作って"), to add background/ground tiles or props to an existing map set, or to fix tile seams, tile color mismatch, or prop transparency in Assets/Game/MapObject.
---

# ステージマップ素材作成

参考イメージ画像 + ステージ説明を受け取り、Unity で配置できるタイルセットとプロップ Prefab 一式まで一気通貫で作る。

画像生成は `generate-2d-sprite` skill に従って Codex へ委譲する。Unity 操作は `unity-development` + `unity-editor-automation` skill に従う。配置規約は `unity-project-conventions` skill に従う。本 skill はそれらを重複させず、ステージ素材作成に固有の手順と、実際にハマった落とし穴だけを定める。

実例: `Assets/Game/MapObject/KazeBossTilemap`(風エリア ボス部屋 / 荒廃した武家屋敷)。タイル28枚 + プロップ18個。ビルダーは `Assets/Scripts/Editor/KazeBossTilesetBuilder.cs` と `KazeBossPropsBuilder.cs`。**新規セットはこの2つをコピーして定数を差し替えるのが最短。**

## 全体の流れ

1. 確認(AskUserQuestion)
2. 素材の洗い出し
3. Codex で原画生成
4. 後処理(タイル / プロップ)
5. 目視確認
6. Unity 取り込み
7. ビルダー実行と検証

## 1. 確認

着手前に AskUserQuestion で必ず詰める。ここを飛ばすと作り直しになる。

- **成果物の範囲**: タイル素材一式のみ / + プレビューシーン / + シーンの下地まで
- **タイルのラインナップ**: 背景N種・地面N種(標準は背景6種/地面5種、変種込み25枚前後)
- **素材セット名**: `Assets/Game/MapObject/<Name>Generated` と `<Name>Tilemap/{Tiles,Palette,Prefabs}` に使う
- **Props を作るか**、作るならどのグループか(壁の汚れ / 建具 / 装飾 / 荒廃・自然侵食)
- **Props にコライダーを付けるか**(既定は付けない。足場は ground タイルか InvisibleWall で組む)

## 2. 素材の洗い出し

参考画像を見て「1タイル = 1素材」に分解する。壁は壁だけ、地面は地面だけ。**1タイルに複数要素を混ぜず、特定の隣接タイルを前提にしない**(パゴダ素材のような枠付き壁パネルは作らない)。

- 背景 (`bg_` / コライダー無し): 障子壁、板壁、土壁、天井の梁組、苔むした壁、奥の暗闇 など
- 地面 (`ground_` / Sprite コライダー): 板張りの床、畳、割れた床板、太い梁、苔むした床 など

素材ごとに変種2〜3枚。**「きれいな版」と「傷んだ版」は別素材として必ず両方作る**(ひび割れ版しかないと壁一面が同じ傷だらけになる)。

## 3. Codex で原画生成

`generate-2d-sprite` skill の委譲手順に従う。加えて本作業では次を守る。

### 呼び出し方

`-i/--image` は可変長引数なので `-i ref.png "プロンプト"` と書くとプロンプトを飲み込んで stdin 待ちになる。**必ず stdin で渡す。**

```bash
cat prompt.txt | codex exec --skip-git-repo-check -s workspace-write \
  -c model_reasoning_effort=low -i style_ref.png -
```

Claude Code から呼ぶときは Bash ツールに `dangerouslyDisableSandbox: true`。1バッチ5〜7素材で分割し、`run_in_background: true` で並列に流す。

### プロンプトに必ず入れる

- **「組み込みの image_gen ツールを必ず使う」「Pillow/PIL/numpy で絵柄を描き起こすのは禁止(リサイズ・寸法補正のみ可)」「image_gen の呼び出し回数と `$CODEX_HOME/generated_images` のコピー元パスを報告する」**
- 無加工の原画を `gen/_raw/`(タイル)/ `gen/_props/`(プロップ)にも残させる
- 1024x1024 前後の正方形。「128x128 のドット絵を8倍に拡大したもの」= 1ドット8x8ブロックで描く指示
- **色域を数値で固定する**(例: ベース `#2b2721`〜`#6b6152`、最大でも `#b9ae95`、純白・純黒・鮮やかな原色禁止)。全素材で同じ指定を使うと後処理が楽になる
- 画風参照として、既存タイルを並べた PNG を `-i` で添付する。2セット目以降は1セット目の完成タイルを参照にすると揃う

タイル素材は「上下左右にシームレス、周期は32ドットまたはその約数、1枚1素材、枠線・文字・余白なし、不透明、局所的な目立つオブジェクトや大きな明暗ムラなし」。

プロップは「**完全にフラットな純マゼンタ `#FF00FF` 背景**、影を描かない、1個だけ中央に配置、輪郭をはっきり」。透過抜きはこちらで行うので Codex 側の背景除去は不要。

### 報告を鵜呑みにしない

Codex は image_gen を呼ばずに Pillow のノイズ関数で描画し、「内蔵画像生成で生成した」と虚偽報告することがある。**次の grep で必ず確認してから先に進む。**

```bash
grep -c "generated_images" codex.log
```

`0` なら手続き描画。生成物を破棄し、上記の禁止事項を明記して作り直す。

## 4. 後処理

`scripts/` の Python を使う。Pillow と numpy が要る(`python3 -m venv` で作業用 venv を切る)。

### タイル: `scripts/tiles_postprocess.py`

```bash
WORK=<作業ディレクトリ> python tiles_postprocess.py spec.json
```

`<WORK>/gen/` の原画を読み、`<WORK>/tiles/` に 32x32 PNG を出す。やっていること:

1. **縮小後の論理解像度を 96〜192 で総当たり**し、「32x32 で切り出したときにラップアラウンドの継ぎ目が最も目立たないスケール」を選ぶ。障子の格子や板幅の周期が自動的に 32 の約数へ寄る
2. そのスケールで継ぎ目コストの低いクロップを、互いに似すぎない条件で N 枚選ぶ
3. 全タイルを1枚のアトラスに連結して**共通パレットへ量子化** + 平均色寄せ。これで隣り合ったタイルの色味が浮かない

spec.json のエントリ:

| キー | 意味 |
| --- | --- |
| `name` / `src` / `variants` | 出力名 / `gen/` からの相対パス / 変種数 |
| `scales` | 探索するスケールを絞る。格子が細かすぎるときに大きい値へ固定する |
| `min_dist` | 変種同士の画素差の下限 |
| `min_offset` | 切り出し位置の距離の下限。藁すさのような高周波テクスチャは 1px ずらすだけで画素差が出るため、`min_dist` だけでは同じ絵柄が並ぶ |
| `bias` | `{"green": 0.6}` で苔が写っているクロップを優先。`{"dark": ...}` で暗さを優先 |

### プロップ: `scripts/props_postprocess.py`

```bash
WORK=<作業ディレクトリ> TILES=<完成済み Tiles ディレクトリ> python props_postprocess.py spec_props.json
```

`<WORK>/gen/_props/` の原画から `<WORK>/props/` に透過 PNG を出す。やっていること:

1. マゼンタからの距離でアルファを作り、半透明部分のマゼンタかぶりを despill で除去
2. 内容のバウンディングボックスで切り詰め
3. **premultiplied alpha で縮小して割り戻す**。被覆率が低い細線でも背景色に引きずられず本来の色が残る
4. アルファは面積平均と **max プーリング**の大きい方。ひび割れや蔦のような細線が平均化で消えるのを防ぐ
5. **タイル群 + プロップ群からまとめて共通パレット**を作って量子化

spec_props.json は `name` / `src` / `w` / `h`。`w`/`h` は 32 の倍数にする。アスペクト比を保って収め、余白は透明で埋まる。

## 5. 目視確認

**必ず自分の目で見てから Unity に入れる。**

```bash
WORK=<作業ディレクトリ> python scripts/preview_tiles.py
```

- `tiles_grid.png`: 各タイルを 3x3 に敷き詰め、単体リピート時の継ぎ目を確認
- `tiles_mix.png`: 全タイルを横一列に並べ、隣接時の色味の浮きを確認
- プロップは市松模様の上に並べて透過とゴミ画素を確認
- 最後にタイルの上にプロップを置いた部屋の合成を作って全体を確認

## 6. Unity 取り込み

配置先(`unity-project-conventions` と `building-tilemap-builder` に準拠):

- `Assets/Game/MapObject/<Name>Generated/` — 再切り出し用の原画。**512x512 程度に縮小して置く**(1024超をそのまま入れるとリポジトリが数十MB膨らむ)
- `Assets/Game/MapObject/<Name>Tilemap/Tiles/` — 32x32 PNG と Tile アセット
- `Assets/Game/MapObject/<Name>Tilemap/Palette/` — パレット prefab
- `Assets/Game/MapObject/<Name>Tilemap/Prefabs/` — プロップ PNG と Prefab

### 落とし穴: フォルダ指定の Import は再帰しない

`unicli exec AssetDatabase.Import --path "<フォルダ>"` は `refreshed: true` を返すが**中のファイルの .meta を作らない**。`TextureImporter not found` で失敗する。再帰インポートを使う。

```bash
unicli eval 'UnityEditor.AssetDatabase.ImportAsset("Assets/Game/MapObject/<Name>Tilemap", UnityEditor.ImportAssetOptions.ImportRecursive); UnityEditor.AssetDatabase.Refresh(); return true;' --json
```

`.meta` の数がファイル数と一致することを確認してから次へ進む。

## 7. ビルダー実行と検証

`Assets/Scripts/Editor/` に `[MenuItem]` 付きビルダーを置き、`Menu.Execute` から実行する(長い Eval にしない)。`KazeBossTilesetBuilder.cs` / `KazeBossPropsBuilder.cs` をコピーして定数を差し替える。ビルダーがやること:

- テクスチャ設定の統一: Sprite / Single / **PPU 32** / **Point** / mipmap off / 非圧縮 / alphaIsTransparency / pivot 中央
- Tile アセット生成: `bg_*` は `ColliderType.None`、`ground_*` は `ColliderType.Sprite`
- パレット prefab 生成: `Grid` + 子 `Layer1`(`Tilemap` + `TilemapRenderer`)、8列。**レイヤーは 0**(既存の PagodaPalette 等に合わせる。31 ではない)
- プロップ Prefab 生成: `SpriteRenderer` のみ。コライダーは付けない

PNG を差し替えて再実行しても同じ結果になるよう冪等にする。

```bash
unicli exec AssetDatabase.Import --path "Assets/Scripts/Editor/<Name>TilesetBuilder.cs" --json
unicli exec Compile --json
unicli exec Editor.Status
unicli exec Menu.Execute --menuItemPath "Tools/VLCNP/MapObject/Build <Name> Tileset" --json
unicli exec Console.GetLog '{"logType":"All","maxCount":3}' --json
```

`Menu.Execute` の引数名は `menuItemPath`(`path` ではない)。**成功を返しても中で例外が出ていることがあるので `Console.GetLog` を必ず見る。**

### 完了前に Eval で検証する

タイル側: `tiles` 数、`nullSprite`、`badPPU`、`badFilter`、`badSize`、`bg`/`ground` のコライダー種別、`paletteTiles`、`GridPalette` サブアセットの有無。

プロップ側: `prefabs` 数、`noRenderer`、`nullSprite`、`withCollider`、`extraChildren`、`badPPU`、`badFilter`、各スプライトの実寸。

### パレットが Tile Palette に出ないとき

ドロップダウンに出る条件は prefab に `GridPalette`(名前は `Palette Settings`)サブアセットがあること。`PrefabUtility.SaveAsPrefabAsset` 後に Unity 側が自動で付ける。認識されているかは次で確認でき、キャッシュが古いだけなら `CleanCache()` で直る。

```csharp
var t = /* UnityEditor.Tilemaps.GridPalettes を全アセンブリから探す */;
const System.Reflection.BindingFlags F = System.Reflection.BindingFlags.Public
    | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static;
t.GetMethod("CleanCache", F)?.Invoke(null, null);
var palettes = t.GetProperty("palettes", F).GetValue(null) as System.Collections.Generic.List<GameObject>;
```

`GridPalettes` は `Unity.2D.Tilemap.Editor` アセンブリにあり `typeof(UnityEditor.Editor).Assembly` からは引けない。全アセンブリ走査で探すこと。

## ハマりどころまとめ

| 症状 | 原因と対処 |
| --- | --- |
| 生成物がノイズ模様で既存の絵柄と全く合わない | Codex が image_gen を呼ばず Pillow で描画。ログを `generated_images` で grep して確認し、禁止事項を明記して再生成 |
| codex が stdin 待ちで止まる | `-i` が可変長でプロンプトを飲んだ。`cat prompt.txt \| codex exec ... -i ref.png -` |
| 障子の格子などが細かすぎる / 粗すぎる | spec の `scales` でスケールを固定する |
| 変種が見た目そっくり | `min_dist` だけでなく `min_offset` を指定する |
| 苔や草の緑が茶色く飛ぶ | 共通パレットが茶系に偏っている。母集団にプロップも入れ、色数を増やす(96 程度) |
| 細いひび割れや蔦が消える | 面積平均で縮小している。max プーリングを併用する |
| プロップの縁に黒いゴミ画素 | premultiplied を uint8 に丸めている。float で縮小し、被覆率下限(0.05)で孤立画素を落とす |
| `TextureImporter not found` | フォルダ指定 Import が再帰していない。`ImportAssetOptions.ImportRecursive` |
| メニュー実行が成功を返すのに何も起きない | 中で例外。`Console.GetLog` を見る |
