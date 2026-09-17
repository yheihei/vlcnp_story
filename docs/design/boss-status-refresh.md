# BossStatusの刷新

採用案は、右端のとぼけた顔に小さな王冠を載せた長い動物のゲージ。
2026-09-12に反映。

## 見た目と配置

- BossStatus全体の位置・アンカー・スケールを維持し、従来の画面上部中央に配置する。
- 二つ名、ボス名、HPゲージの順に並べる。名前と二つ名の文言は既存のまま。
- プレイヤーHUDと共通の白い動物の輪郭、黒い縁、赤いHPを使う。右端に小さな金色の王冠を付ける。
- 枠と顔は固定長。HPに応じて変わるのは赤い領域だけで、数値は表示しない。
- ボス名と二つ名は日本語対応のDotGothic16を使用する。長い文字列には既存のTextの自動サイズ調整を使う。
- ボスHPの計算処理、参照キャラクター、表示・非表示の制御は変更しない。
- 空中のボスを見やすくするため、HPバーを名前側へ引き上げて間隔を詰めた。HPSliderのローカルYを0から32へ変更し、フルHD換算で約17ピクセル、HUD下端を上げている。名前・二つ名・王冠の大きさは維持し、長いボス名でも王冠との余白を確保した。共通プレハブと独立した9個に反映。

## 反映先

共通の `Assets/Game/Core/Core.prefab` と、以下の独立したBossStatus計9個を更新。

- `Assets/Scenes/Yami5F.unity`: 1個
- `Assets/Scenes/VeryLongFarm_1_boss.unity`: 2個
- `Assets/Scenes/VeryLongFarm_Yama_Revenge.unity`: 3個
- `Assets/Scenes/BlockChainRoom2.unity`: 3個

`Ohirunebeya_5_2` と `Ohirunebeya_5_boss` は、旧レイアウト用のボス名の位置オーバーライドを解除した。BossStatus全体の位置調整は維持する。

## 素材

- 枠: `Assets/Game/UI/VeryLongHUD/very_long_boss_frame_512x48.png`
- 書体: `Assets/Game/Fonts/DotGothic16/DotGothic16-Regular.ttf`
- 書体の配布元: [Google Fonts](https://github.com/google/fonts/tree/main/ofl/dotgothic16)。OFLを同じフォルダに同梱。
- フォント本体を含める設定 `includeFontData: 1` を使用。利用者のPCへのフォントのインストールは不要。
- `FontLicenseBuildProcessor` がビルド後に `Assets/Game/Fonts` 内のOFL本文をコピーする。Windowsは実行ファイルと同じ階層の `Licenses/<書体名>/OFL.txt`、macOSはアプリ内の `Contents/Resources/Licenses/<書体名>/OFL.txt` に配置する。DotGothic16とプレイヤーHUDのPressStart2Pが対象。
- 絵柄の参考: [VeryLongAnimals公式素材](https://verylonganimals.com/download/)

枠は組み込みimagegenで採用モックとプレイヤー枠を参考に生成した。
最初の出力に市松模様が描かれたため、次の指示で単色背景に修正した。

```text
Exact sprite cleanup, preserve the long white boss frame, its black keyline, right deadpan face, and tiny gold crown precisely. REPLACE EVERY checkerboard pixel both OUTSIDE the frame AND INSIDE the hollow HP channel with perfectly flat solid pure MAGENTA #FF00FF, no transparency checkerboard anywhere. Magenta is a temporary chroma key, not artwork. Remove all checkerboard, texture, gray remnants, shadows and gradients from empty areas; the only non-magenta pixels should be ivory frame and face, pure dark outline/facial pixels, gold crown. Keep whole border sharp with uniform 2 logical pixels white plus 1 logical black keyline. No text, no red fill. Same thin long straight proportion, whole sprite visible and tightly centered. Intended final exact crop and nearest-neighbor processed target 512x48 RGBA sprite with all magenta alpha0 at /Users/yhei/unity/vlcnpStory2022/Assets/Game/UI/VeryLongHUD/very_long_boss_frame_512x48.png.
```

ImageMagickで背景色を透過にし、余白を除去した後、最近傍補間で512×44に縮小。上下2ピクセルの余白を加えて512×48にした。
白・黒・金・透過の4色に整理。外側とゲージの穴はalpha 0、残す部分はalpha 255。
UnityではPointフィルタ、圧縮なし、Mip Mapなし。枠は1344×126のRectTransformに配置する。

## 確認

- 共通プレハブと独立した9個について、HPゲージの0・50・100%と固定枠を編集モードで確認。
- ボス名・二つ名の18パターンについて、文字欠けがなく1行に収まることをTextGeneratorで確認。
- 保存データを変更前と比較し、10個のBossStatusのルート位置・表示状態、BossHPBarの参照、既存の文言が同一であることを確認。
- Kaze3_boss_2のGameビューで、一時的にHPを68%にした表示を確認。プレイヤーHUDとの重なりなし。確認後は一時変更を戻した。
- 実行時コードは変更しておらず、Play Modeのボス戦は未実施。
- 一時エディタスクリプトを削除して再インポート後、コンパイルエラー0を確認。既存のIssue649ThrusterSetupの非推奨API警告が1件。
- フォント同梱処理の追加後もコンパイルエラー0。ビルド後の処理がUnityから検出されること、ビルド対象シーンの依存アセットにDotGothic16が含まれることを確認。
- 一時出力先でWindows・macOS用のライセンスコピーを実行し、両書体の原文とのバイト一致、再実行による更新、他のライセンスファイルの保持を確認。
- Windows・macOSのリリースビルドを作成し、両版に元のTTF本体とライセンスが含まれることを確認。macOS版を実行してナルカミ戦とヤマタノオロチ戦の文字・枠・余白を目視確認した。Windows版の実機表示は未確認。詳細は[リリース記録](../releases/2026-09-12-boss-hud.md)を参照。

反映・検証用の一時エディタスクリプトは削除済み。正はシーンとプレハブの保存データ。

## 土エリアの追加確認

`Ohirunebeya_tuti_1`〜`5` と `Ohirunebeya_tuti_6_boss_1`〜`3` の計8シーンをUnityで個別に読み込み、BossStatusの参照と継承結果を確認した。

- 8シーンともBossStatusの参照元は `Assets/Game/Core/Core.prefab`。
- 王冠付きの枠、DotGothic16、1344×126のゲージ設定を全シーンで継承している。
- 全シーンのBossStatusの位置は既存の `(8, 294)`。
- `tuti_6_boss_2` は「Very Long ヤマタノオロチ」、HP参照先は `VLOrochiVariant`。表示状態も維持している。
- 土エリア専用のGameEvent、GameEventTransition、DangeonParticleなどは別プレハブだが、BossStatusの参照元には影響しない。

追加のアセット修正は不要。確認用に読み込んだシーンは保存せず閉じた。
