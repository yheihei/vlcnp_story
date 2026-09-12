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


## 会話中の選択肢

選択肢は次の2つの共通プレハブに設定しています。会話枠のPNGとは別に、Unity標準のImageとTextで描画します。

- `Assets/Game/UI/CustomMenuDialog.prefab`
- `Assets/Scenes/Ohirunebeya_tuti_1/CustomMenuDialog.prefab`

通常は本文と同じ `#191E25` の背景と `#E6E3D6` の文字です。選択中は背景と文字色を反転し、左端に「▶」を表示します。マウスを重ねた項目は背景を少し明るくし、選択中の項目と区別します。項目間の余白は12、文字サイズは従来の最大40・最小30の自動調整です。

各ボタンの `Surface` が背景、`Text` が選択肢の文、`SelectionMarker` が矢印です。背景色はButtonのColor Tint、文字色と矢印の切り替えは `Assets/Scripts/UI/DialogueChoiceButton.cs` で管理します。Fungusが取得する最初のTextは選択肢の文にしてください。矢印を先に置くと、会話処理が矢印へ文を設定してしまいます。

2026-09-12時点で、Core・HealPointの入れ子を含む45シーンが上記の共通プレハブを参照しています。色・画像・選択状態への個別上書きはありません。既存の配置の上書きは保持しています。

720p・1080pの分離プレビューで3項目・6項目と長めの日本語を確認しました。両プレハブのPlay Mode確認では、Fungusによる文の設定、最初の選択、上下移動、ホバー、押下、無効項目、決定、クリック、Clear後の再表示を検証しています。実機ゲームパッドと全シーンでの会話進行は未検証です。


## チュートリアルスライド

`Assets/Game/UI/TutorialSlideShow.prefab` も同じ会話枠PNGを使用します。2026-09-12時点の配置先はSampleSceneとVeryLongFarm_1の2シーンで、外観への個別上書きはありません。

ウィンドウは1240×900、動画・静止画の領域は従来の960×540です。見出し60・説明文38の文字サイズを維持し、文字領域と下部の余白を調整しています。下部には左右のページ切替案内、ページ数、アイボリーの決定案内を配置しました。動画再生、スライドデータ、入力処理、閉じた後のコールバックは既存のものを使います。

共通スプライトの大きさを会話枠に合わせるため、WindowのImageはSliced、Pixels Per Unit Multiplierは3.125です。CanvasのReference Pixels Per Unitは100のままです。枠や小さな顔の変更は共通PNG、配置や配色の変更はTutorialSlideShowプレハブに反映してください。

実際の9枚分の説明文を720p・1080p相当の分離プレビューで確認しました。Play Modeでは静止画、動画の再生とフレーム進行、ページ番号、先頭・最終ページの矢印、動画から静止画への切替、閉じるコールバックとゲーム入力の復帰を確認しています。実機ゲームパッドによる操作は未検証です。
