# 風エリアの夜空(OuterWallSky)と環境演出の引き継ぎメモ

最終更新: 2026-09-07(#658)。それ以前の記述(`OuterWallSkyParallax.cs`、速度 0.015〜0.12、tiling 1.4〜2.3)は古く、現在の実装と一致しない。

## 対象

- `Assets/Scenes/Kaze1.unity`、`Assets/Scenes/Kaze2.unity`(夜空の構成と値は両シーンで同じ)
- 目的: 洞窟物語「外壁」風の夜空。満月、星、4 層の雲が右から左へ流れる
- 制約: `BackGroundLoop` は使わない。カメラ ortho は 6(#653)。`CameraConfineArea` と `FallMissZone` は触らない

## 構成

夜空は Core プレハブのメインカメラの子として置いてあるので、カメラに追従する。

```
Core/Main Camera/
  OuterWallSky            OuterWallSkyCloudScroll。position (0, 0, 10.75)、scale 1.2
    SkyBase               SpriteRenderer、BackGround 層 order -100、scale (2.72, 2.89)。M_OuterWallSky_Base
    Stars                 SpriteRenderer、order -99、静止。OuterWallSkyStarTwinkle(#658)で明滅。M_OuterWallSky_Stars
    FullMoon              SpriteRenderer、order -98、position (5.66, 3.64)、scale 1.04。M_OuterWallSky_Moon
    CloudLayer_1_Farthest SpriteRenderer、order -97。M_OuterWallSky_CloudLayer1
    CloudLayer_2_Far      order -96。M_OuterWallSky_CloudLayer2
    CloudLayer_3_Mid      order -95。M_OuterWallSky_CloudLayer3
    CloudLayer_4_Near     order -94、position (0, -2.82)、scale (1.78, 1.36)。M_OuterWallSky_CloudLayer4
```

素材とマテリアルは `Assets/Game/MapObject/OuterWallSky/` にある。

- `outer_wall_sky_base.png` / `outer_wall_stars.png` / `outer_wall_full_moon.png`: Sprites/Default のマテリアル
- `outer_wall_cloud_layer_1〜4.png`: 透明キャンバスに雲帯を描いた横方向シームレス素材。`OuterWallCloudScroll.shader` を使う
- Core 配下なので、環境演出のマテリアル差し替え(`AmbientMaterialReplacer`)の対象外。空はライトの影響を受けない

## 雲のスクロール

`Assets/Scripts/Core/OuterWallSkyCloudScroll.cs`

- `cloudLayers[]` に 4 層の Renderer・`speed`・`tilingX`・`tilingY` を持つ
- `Awake` で各層のマテリアルを複製し、`LateUpdate` で `offsetX += speed * deltaTime`(2 で折り返し)を
  シェーダの `_HorizontalOffset` と `_Tiling` へ渡す。Transform は動かさない
- シェーダ `VLCNP/OuterWallCloudScroll` は `texcoord * _Tiling + (_HorizontalOffset, 0)` でサンプリングする。
  オフセットが増えると絵は左へ動く。つまり雲は右から左へ流れる。風の粒子もこの向きに合わせてある(後述)
- `_MainTex` は SpriteRenderer の `[PerRendererData]` なので、雲の細かさは `_Tiling` で制御する

現在の値(Kaze1・Kaze2 とも同じ):

| 層 | speed | tilingX | tilingY |
| --- | --- | --- | --- |
| CloudLayer_1_Farthest | 0.05 | 2.7778 | 1 |
| CloudLayer_2_Far | 0.10 | 2.7778 | 1 |
| CloudLayer_3_Mid | 0.15 | 2.7778 | 1 |
| CloudLayer_4_Near | 0.50 | 2.7778 | 1 |

マテリアル側の `_Tiling` も 2.7778 で、シーン側の値が実行時に上書きする。

## 星の明滅(#658)

`Assets/Scripts/Core/OuterWallSkyStarTwinkle.cs` を `Stars` に付けてある。

- `Awake` で基準色を控え、`Update` で `unscaledTime` の正弦波 2 つ(主周期 6 秒・短周期 2.3 秒を 25% 混ぜる)で
  明るさを `minBrightness`(0.7)〜1.0 の間で変える。アルファは変えない
- 会話中(timeScale 0)も動く。`OnDisable` で基準色に戻す
- 位相は `Random` なので、同じシーンでも起動ごとにずれる

## 環境演出(#658)

`Assets/Scripts/Editor/Issue658KazeAtmosphereSetup.cs` の `Tools/Issue658/Apply Kaze Atmosphere To All Scenes` で
2 シーンに適用して保存する。再実行すると前回の生成物を消してから置き直す。`Remove ... From Open Scene` で外せる。
窓の光・前景・確認用カメラ位置はこのスクリプト内の表が正。手直しは表へ戻す。

置くもの:

1. `AreaAtmosphere_Kaze`(`Assets/Game/Effect/Ambient/AreaAtmosphere_Kaze.prefab`、#656)。
   月夜の青紫のグローバルライト(0.72, 0.78, 1.0)と `Kaze_Post Processing Profile`。
   漂う埃(Dust)は風の場面に合わないので無効にしてある
   - 風筋と葉(WindStreaks): velocity.x −7〜−4(右から左)、FollowMainCamera の offset (+11, 0) で画面右端の外から湧く。
     発生率 3.5/秒、最大 36 粒
   - 霧(Fog): velocity.x −0.45〜−0.15、色 (0.8, 0.86, 1.0)。Effect 層 order −5 にして塔(タイルマップと小物)の手前、
     プレイヤーの奥を横切らせる
2. 窓の点光源。`AmbientPointLight` を表にある窓(round_window_lattice_02 / round_window_cross_01 のインスタンス)の子に置く。
   色 (0.125, 0.22, 0.227)、強さ 2.5、半径 0.4〜2.8、減衰 0.5、ゆらぎ(LightFlicker2D)は無効。
   Kaze1 6 か所、Kaze2 6 か所。WindowGun(敵)が重なる窓は避けている
3. 前景。`ivy_overlay_01_32x96` と `spider_web_01_32x32` を PlayerUpperObject 層に、青みの暗い色 (0.42, 0.46, 0.62) で
   1 シーン 3 個。壁面(BGTilemap)の前に置き、足場やジャンプ先の上には置かない
4. マテリアル差し替え。`AmbientMaterialReplacer.Replace(scene, requireGlobalLight, extraSources, replaceMissing)` で
   `Tilemap`(builtin Sprites-Default)と `BGTilemap`(独自マテリアル `torn_ofuda_01_32x32.mat`。中身は Sprites/Default)を
   `AmbientSpriteLit` にする。壁の小物(窓・蔦・扉など)は元から URP の `Sprite-Lit-Default` を参照しているので差し替え不要

生存粒子は風筋 36 + 霧 12 で 1 シーン 200 以内。

## 調整の触り方

1. 雲の速さ: シーンの `OuterWallSky` にある `cloudLayers[].speed`。奥ほど遅く、手前ほど速くする
2. 雲の細かさ: `cloudLayers[].tilingX`(マテリアルの `_Tiling` は実行時に上書きされる)。
   数値を上げるほど雲パターンが横に細かく繰り返される
3. 雲の絵: `outer_wall_cloud_layer_1〜4.png` を作り直す。透明キャンバスを維持し、横方向はシームレスにする
4. 星の明滅: `Stars` の `OuterWallSkyStarTwinkle` の `minBrightness` / `period` / `secondaryPeriod` / `secondaryWeight`
5. 風筋・霧・窓の光・前景: `Issue658KazeAtmosphereSetup` の定数と表を変えて Apply を再実行する

## 検証手順

Unity Editor が起動している前提。

```bash
unicli exec AssetDatabase.Import --path "Assets/Scripts/Core/OuterWallSkyCloudScroll.cs" --json
unicli exec AssetDatabase.Import --path "Assets/Scripts/Core/OuterWallSkyStarTwinkle.cs" --json
unicli exec Compile --timeout 60000 --json
unicli exec Scene.Open '{"path":"Assets/Scenes/Kaze1.unity","dirtyAction":"discard"}' --json
unicli exec PlayMode.Enter --json
unicli exec Screenshot.Capture '{"path":"docs/issues/658/check.png"}' --json
unicli exec PlayMode.Exit --json
```

プレイ中の確認:

- `Tools/Issue658/Debug/Camera Spot 1〜4` で Cinemachine を切って確認位置へカメラを置ける(`Restore Camera Follow` で戻す)
- 雲は右から左へ流れる。スクリーンショットを 2 秒空けて 2 枚撮り、横方向のずれで向きを確かめられる
  (2026-09-07 の確認では中景の層が 2 秒で約 450px 左へ動いた)
- 星は `Stars` の SpriteRenderer の色が数秒周期で 0.7〜1.0 の間を往復する(`GameObject.GetComponents` で読める)
- 風筋・葉・霧が右から左へ流れ、窓の周りが冷たく光り、蔦と蜘蛛の巣がプレイヤーの手前に描かれる
- Console に Warning/Error が無い。エディットモードのメニュー実行のログは `Console.GetLog` に出ないことがあるので、
  必要なら `~/Library/Logs/Unity/Editor.log` を `[Issue658]` で grep する

性能(2026-09-07、Kaze2 スポーン地点、ScenePerfProbe Pass A): Frame Time avg 1.70ms / p95 2.22ms、Draw Calls 60、SetPass 54。
#656 の基準(1.60ms / 1.94ms / 58 / 48)との差は avg +0.10ms。

## 注意点

- `BackGroundLoop` は使わない
- 空は Core プレハブのカメラの子で、`Stars` などはプレハブに属さないシーン上のオブジェクト。Core を再保存しても値は変わらない
- Play Mode の Enter/Exit や Cinemachine のエディタ処理でシーンが dirty になる。ディスクの状態が正なら
  `Scene.Open` の `dirtyAction: discard` で揃える
- `Packages/manifest.json` と `Packages/packages-lock.json` はこの作業では触らない

## コミット方針

- main で作業してよい。issue 番号は `#658`
- コミット例: `風エリアの雲の速さを調整 #658`
