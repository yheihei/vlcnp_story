# Kaze1 OuterWallSky 引き継ぎメモ

## 現状

- 対象シーン: `Assets/Scenes/Kaze1.unity`
- 現在の最新コミット: `55df5ff3 月と雲の背景表示を調整 #583`
- 背景ルート: `BackGround/OuterWallSky`
- 目的: 洞窟物語「外壁」風の夜空、満月、4層雲パララックス背景
- 制約: `BackGroundLoop` は使わない

## 実装概要

- `Assets/Scripts/Core/OuterWallSkyParallax.cs`
  - `Camera.main` に追従して背景Spriteを画面サイズへフィットさせる。
  - 月は `moonScreenHeightRatio` とSprite実寸からスケールを計算する。
  - 雲4層はTransform移動ではなくMaterialのUV offsetで流す。
- `Assets/Game/MapObject/OuterWallSky/OuterWallCloudScroll.shader`
  - 雲専用のUnlit透明シェーダー。
  - `_Tiling` で雲の細かさ、`_HorizontalOffset` で横スクロールを制御する。
- `Assets/Game/MapObject/OuterWallSky/outer_wall_cloud_layer_*.png`
  - 雲4層のPNG。
  - 透明キャンバス上に雲帯を描いた横方向シームレス素材。

## 現在の設定値

`Kaze1.unity` の `OuterWallSkyParallax`:

- `screenPadding`: `(4, 3)`
- `moonViewportPosition`: `(0.76, 0.68)`
- `moonScreenHeightRatio`: `0.24`
- 雲速度:
  - Layer 1: `0.015`
  - Layer 2: `0.03`
  - Layer 3: `0.06`
  - Layer 4: `0.12`
- 雲tiling:
  - Layer 1: `1.4`
  - Layer 2: `1.7`
  - Layer 3: `2.0`
  - Layer 4: `2.3`

## 直近の問題意識

ユーザーは「基本の挙動は良いが、まだ雲を直したい」と言っている。

前回の修正で対応済み:

- Play時に月が巨大化する問題は修正済み。
- Play中の月scaleは検証時点で `(1.04, 1.04, 1.00)`。
- 雲は前より小さいもこもこになったが、まだ見た目を詰めたい状態。

次セッションでまず確認すべき観点:

- 雲がまだ画面を覆いすぎていないか。
- 奥/中/手前の4層が似すぎていないか。
- 低解像度ドットとして、もこもこの粒が自然か。
- 塔の視認性を邪魔していないか。
- 月や星が雲で隠れすぎていないか。

## 雲調整の触り方

主に触る候補は3つ。

1. 雲PNGを再生成する
   - 対象: `outer_wall_cloud_layer_1.png` から `outer_wall_cloud_layer_4.png`
   - 現状はPython/Pillowで生成したオリジナルPNG。
   - 透明キャンバスを維持し、横方向はシームレスにする。
   - Blobを小さくする場合は、円/楕円の半径を下げるだけでなく、層ごとのY位置と透明余白も見直す。

2. `_Tiling` を変える
   - 対象: `M_OuterWallSky_CloudLayer*.mat` と `Kaze1.unity` の `cloudLayers.tilingX`
   - 数値を上げるほど雲パターンが横に細かく繰り返される。
   - 目安:
     - 控えめ: `1.2 / 1.4 / 1.6 / 1.8`
     - 現状: `1.4 / 1.7 / 2.0 / 2.3`
     - かなり細かい: `1.8 / 2.2 / 2.6 / 3.0`

3. 雲の描画順や濃さを変える
   - SortingLayerは `BackGround`。
   - 雲orderは現在、月より後ろ側に入っている。
   - PNG側のalphaを下げると塔の視認性は上がる。

## 注意点

- `BackGroundLoop` は使わない。
- `OuterWallSkyParallax` は1コンポーネントで全レイヤーを管理する方針を維持する。
- SpriteRenderer標準シェーダーではMaterial texture offsetが効かなかったため、雲は `VLCNP/OuterWallCloudScroll` を使う。
- `_MainTex` はSpriteRendererの `[PerRendererData]` なので、雲の細かさは `_Tiling` で制御する。
- `Packages/manifest.json` と `Packages/packages-lock.json` はこの作業では触らない。

## 検証手順

Unity Editorが起動している前提。

```bash
unicli exec AssetDatabase.Import --path "Assets/Game/MapObject/OuterWallSky" --json
unicli exec AssetDatabase.Import --path "Assets/Scripts/Core/OuterWallSkyParallax.cs" --json
unicli exec Compile --timeout 60000 --json
unicli exec Console.GetLog --logType "Warning,Error" --maxCount 80 --stackTraceLines 0 --json
```

Play中確認:

- 月がEditor表示と同じ大きさか。
- 雲4層が左から右へ流れるか。
- 雲が塔やキャラを邪魔しすぎないか。

前回の検証結果:

- Compile: error 0 / warning 0
- Console Warning/Error: 表示対象 0件
- UV差分: `cloudOffsetDiff=70281`

## コミット方針

- mainで作業してよい。
- issue番号は `#583`。
- 次に雲を調整した場合のコミット例:

```text
雲背景の見た目を調整 #583
```
