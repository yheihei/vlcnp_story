# 左上HUDの刷新

状態: 反映・検証完了。

## 目的

左上のHP・経験値・LV表示に、ベリロンの少しマヌケな世界観を表す。

参考: [VeryLongAnimals公式素材](https://verylonganimals.com/download/)

## 合意した方針

- 方向は「静かな変さ」。妙に長い形と、とぼけた顔にある間の抜け方を手掛かりにする。
- 文字やゲージを含めてドット絵で揃える。
- 刷新対象はHP・経験値・LV表示。下に並ぶ既存の顔アイコンは変更対象に含めない。
- HPゲージ自体を長い動物に見立て、端に小さなとぼけた顔を付ける。
- HPゲージの外枠は固定長とし、中の色で残HPを示す。残HPによって顔や外枠を縮めない。
- 上段を主役のHP表示とし、数値は現在値／最大値を併記する。
- 下段には小さなLV表示と細い経験値ゲージを置く。
- 通常時は静止し、被弾・回復・LV上昇の瞬間だけ短い反応を付ける。
- 画像案3を採用。HP数値はゲージの内側に置く。

## 現行仕様

- HP・経験値・LVは操作中キャラの値を表示し、キャラ切替時に参照先が変わる。
- HP数値は現在値／最大値、HPゲージは現在値を最大値で割った割合。
- 経験値ゲージは次のLVまでの進捗。最大LVでは満タンとなり、LV表示は「MAX」になる。
- 最大HPの成長は経験値によるLVとは別。パーティ共通のHP成長段階によって増える。
- アキムの最大HPの設定値は5から41まである。HP1につき一つの図形を並べる表示では、成長後の幅を考慮する必要がある。

根拠:

- `Assets/Scripts/Control/PartyCongroller.cs`
- `Assets/Scripts/UI/HPBar.cs`
- `Assets/Scripts/UI/ExperienceBar.cs`
- `Assets/Scripts/UI/LevelDisplay.cs`
- `Assets/Scripts/Stats/BaseStats.cs`
- `Assets/Game/Stats/Progression.asset`

## HUDの実装

- `Assets/Game/Core/Core.prefab` のHP・LV・経験値表示を更新した。
- 共通プレハブとは別のHUDも更新した。Yami5Fは1個、VeryLongFarm_1_bossは2個、VeryLongFarm_Yama_RevengeとBlockChainRoom2は各3個、計9個。
- 動物の枠は白系、HPは赤系、経験値は黄系。HP数値に黒い縁を付け、空のゲージでも読めるようにした。
- 枠素材は256×40ピクセルの透過PNG。外側とゲージの穴は透過し、枠と顔は残HPに依存しない。
- フォントはPress Start 2P。配布元のOFLをフォントと一緒に配置した。
- 反応は0.18秒。HP変化時は枠の色と位置、LV上昇時は経験値の色とLV数値の位置を少し変える。
- キャラ切替時は反応を止め、比較用の直前値をリセットする。
- 待機中に経験値を設定・復元されたキャラは、有効化時にレベルを再計算する。レベル別の姿と当たり判定も同期する。
- 既存の仲間アイコンと選択枠は維持する。

## 確認結果

共通プレハブを分離して読み込み、編集モードで次を確認した。

- HPの現在値／最大値とゲージ割合。
- 現在HPが変わらず、最大HPだけが増えた場合の数値更新。
- HPゼロでも顔と固定長の枠が残ること。
- キャラ切替時のHP参照と、被弾・回復の比較値のリセット。
- 最大LV時の「MAX」と経験値満タン、および別キャラへ切り替えた後の解除。
- `HP 41/41` と `MAX` が文字領域に収まること。
- 仲間アイコンと選択表示の28個のシリアライズ対象が変更前と同一であること。

SampleSceneのPlay Modeでは、実際のパーティとHUDを使って16項目を確認した。HP減少・回復・ゼロ、最大HP、最大LVの表示が更新され、HPとLVの反応が終わると元の色と位置へ戻る。HPの反応中に実際のパーティ切替を行った場合も、反応が停止し、新しいキャラのHPを表示する。切替による余分な被弾・回復・LV上昇の反応は発生しない。

コンパイルは成功し、Play Mode中のConsoleエラーはなかった。検証専用コードは削除した。

Ohirunebeya_tuti_2の追加検証では、非アクティブ中のミタマに経験値35を設定してから実際にパーティを切り替え、LV 3・経験値ゲージ約33%となることを確認した。初回有効化前の経験値設定、待機中のセーブ復元、最大LVからの戻り、経験値の増減も確認した。アキムでは待機中の経験値変更後に、レベル別の姿と当たり判定が同期することを確認した。15項目を通過し、Consoleエラーはなかった。

次の4シーンは編集モードで反映・保存した。HUD反映時のPlay Modeでの確認対象はSampleScene。

- `Assets/Scenes/Yami5F.unity`
- `Assets/Scenes/VeryLongFarm_1_boss.unity`
- `Assets/Scenes/VeryLongFarm_Yama_Revenge.unity`
- `Assets/Scenes/BlockChainRoom2.unity`

## 比較用の画像案

次の番号は会話内での画像の表示順に対応する。3を採用した。

1. [HP数値をゲージの上に置く](hud-concepts/01-hp-above.png)
2. [HP数値をゲージの左に置く](hud-concepts/02-hp-left.png)
3. [HP数値をゲージの内側に置く](hud-concepts/03-hp-inside.png)

共通の図案は、白いガイネンアニマルを参考にした長い輪郭、右端の小さな顔、赤系のHP、黄系の経験値。
サンプルの状態はHP 5/8・LV 2・経験値35%。実データを撮影した画面ではない。
画像生成による拡大モックのため、下の仲間アイコンを含む細部のピクセルは元画像と同一ではない。
実装時は既存の仲間アイコンを使用し、モックから切り出して置き換えない。

生成方法: 組み込みimagegen。元のHUD画像を編集対象とし、公式のガイネンアニマル画像を絵柄の参考として添付した。

## 素材

- 枠: `Assets/Game/UI/VeryLongHUD/very_long_health_frame_256x40.png`
- フォント: `Assets/Game/Fonts/PressStart2P/PressStart2P-Regular.ttf`
- フォントの配布元: [Google Fonts](https://github.com/google/fonts/tree/main/ofl/pressstart2p)
- 画像生成の指示と処理: [HUD素材の作成記録](hud-asset-generation.md)
