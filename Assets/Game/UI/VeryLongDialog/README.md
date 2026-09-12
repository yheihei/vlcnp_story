# 会話ウィンドウの共通素材

`dialog_frame_256x96.png` は、濃い青灰色の本文背景と、薄い生成り色の輪郭、小さな動物の顔を持つ会話枠です。
[VeryLongAnimals公式のガイネンアニマル](https://verylonganimals.com/download/)と、このプロジェクトの `VeryLongHUD` の枠を参考にしています。

## 反映先

- `Assets/Game/Chats/CustomDialog.prefab`
- `Assets/Scenes/Ohirunebeya_tuti_1/CustomDialog.prefab`

2026-09-12時点の `Assets/Game` と `Assets/Scenes` を調査し、入れ子のプレハブを通じて45シーンがこの2つへつながることを確認しました。独立した非プレハブのSayDialogはなく、会話グラフィックへの色・画像の個別上書きもありませんでした。新しく独立したDialogを作る場合は、上記の共通プレハブを使ってください。

枠の変更はこのPNGを差し替えると両方へ反映されます。名前や送り矢印の設定は2つのプレハブ側にあります。本文のフォント、文字サイズ、領域、会話の参照やコールバックは既存のものを使っています。名前は少し下げて上余白を確保し、立ち絵より前に描画して暗色の縁取りを付けています。

## インポート設定

- RGBA、256×96、外側のみ透過、本文背景は不透明
- Point、Mip Mapなし、無圧縮、Clamp、Sprite Full Rect
- Pixels Per Unit: 16
- Border: left 9 / bottom 8 / right 29 / top 31
- UI ImageはSliced、色は白、Preserve Aspectはオフ
- 本文背景 `#191E25` と白文字のコントラスト比は約16.75:1
- 生成り `#E6E3D6` と本文背景は約13.02:1

## 素材の作成

組み込みimage_genで生成しました。最終画像の外側に描かれた市松模様を除去し、最も大きい連結成分を残して外側を透明化しました。256×96への最近傍縮小と4色への整理を行い、中央の伸縮範囲が単色であることを確認しました。元画像の顔と輪郭を使い、絵柄は手で描き直していません。

最後の生成指示は次のとおりです。九分割の境界は、完成画像を確認した後、上の値に調整しています。

> Edit this dialogue panel sprite only. Change the long pale header strip to the SAME flat dark blue charcoal as the body, so WHITE character-name text can be placed on it and remain readable. Keep a single thin continuous warm ivory outside border around the whole box. Retain the little pale animal head with its black eyes, small mouth, two ear nubs at the far right upper corner. Pale fill should now exist ONLY in the thin outside border and the small face at the right end. Inside the panel use ONE SOLID flat #191E25 color, no gradients/no texture. Keep the animal face and stepped pixel corners and composition. Transparent background outside, alpha zero, NOT a painted checkerboard. Crop the canvas snugly to the sprite, 2-pixel native padding only. Produce crisp pixel art intended to be downsampled to exactly 256x96 pixels. No text/numbers/arrows/extra icons. This is a Unity 9-sliced dialog panel image, native border 6 left/6 bottom/24 right/22 top pixels; all center areas flat so stretching doesn't distort the face. Output path eventually /Users/yhei/unity/vlcnpStory2022/Assets/Game/UI/VeryLongDialog/dialog_frame_256x96.png.

## 確認結果

2つのプレハブを分離して読み込み、720pと1080p相当のサイズでUIメッシュを描画しました。日本語のサンプル文、名前の縁取りと描画順、送り矢印の文字が表示されることを確認しています。このプレビューはゲームのスクリーンショットではありません。

本文領域、文字サイズ、キャラクター画像、Fungusの会話処理、送りボタンのコールバックが変更前と同じであることも確認しました。全45シーンでの実際の会話進行は未検証です。
