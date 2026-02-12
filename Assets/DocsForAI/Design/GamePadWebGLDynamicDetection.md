# GamePadWebGLDynamicDetection Design

## コンポーネントの役割と変更箇所

### `Assets/Scripts/Control/GamePadFlagSetter.cs`
- 役割:
  - `Flag.IsGamePad` を現在の入力デバイス状態に応じて更新する。
- 変更点:
  1. 従来の「ロード完了時1回判定」から、以下の複合監視へ変更。
     - ロード完了直後の初回判定
     - `InputSystem.onDeviceChange` によるデバイス変化イベント監視
     - 0.5秒ごとのポーリング再判定
  2. 状態が変化した時のみ `Flag.IsGamePad` を更新するように変更。
  3. `FlagManager` が未取得のときは即失敗せず、後続の監視タイミングで再試行できる設計に変更。

## Unity Editor上での操作マニュアル

1. 対象シーンに `GamePadFlagSetter` が配置されていることを確認する。
2. `GamePadFlagSetter` の `Enable Debug Log` を必要に応じて ON にする。
3. WebGL ビルドを実行し、ブラウザでゲーム画面を開く。
4. 画面クリックでフォーカス後、ゲームパッドのボタンを操作する。
5. Console で `Flag.IsGamePad` 更新ログ（`[GamePadFlagSetter]`）を確認する。
6. 接続/切断、または入力開始後に UI のゲームパッド向け表示切替が追従することを確認する。

## 想定効果
- WebGL でロード時にゲームパッドが未認識でも、後続タイミングで認識された状態を反映できる。
- ブラウザ起因の認識遅延やイベント取りこぼしに対して復元性が高い。
