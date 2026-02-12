# GamePadWebGLDynamicDetection Plan

## 背景
- Unity Editor ではゲームパッド入力が動作するが、WebGL ビルドで初期判定時に認識されないケースがある。
- 現行の `GamePadFlagSetter` はロード完了時の1回判定のみで、後から認識された接続状態を反映できない。

## 実装方針
1. `GamePadFlagSetter` でロード完了後の初回判定を維持する。
2. `InputSystem.onDeviceChange` を購読し、ゲームパッド接続状態変化時に再判定する。
3. WebGL の取りこぼし対策として、短周期ポーリングで再判定する。
4. 状態が変化したときだけ `Flag.IsGamePad` を更新する。
5. 既存の `FlagManager` 未生成時のガードを維持しつつ、後続フレームで再試行可能にする。

## 完了条件
- WebGL 実行中にゲームパッドを後から接続/操作しても `Flag.IsGamePad` が正しく反映される。
- 既存シーンロード待機 (`LoadCompleteManager`) の仕様を壊さない。
