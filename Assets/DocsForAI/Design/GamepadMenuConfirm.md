# Gamepadメニュー決定ボタン設計メモ

## 役割と変更箇所
- `Assets/Scripts/Control/PlayerInputAdapter.cs`
  - メニュー決定入力判定で使用している `WasMenuSubmitPressed()` を修正。
  - キーボード決定を `return` のみに限定し、ゲームパッドでは Xbox B ボタンに対応する `buttonEast` を判定するよう変更。

## Unity Editor 操作メモ
1. オープニングシーンを開く。
2. シーン内の `GameSelect` オブジェクト（または該当 UI ハンドラー）を確認し、`PlayerInputAdapter` を経由して決定入力を受け取る設定が保たれていることを確認。
3. 実機または入力シミュレータで Xbox コントローラーを接続し、B ボタンでメニュー決定が機能することを確認する。
4. キーボード入力では Enter キーのみが決定となり、Space キーでは決定できないことを確認する。
