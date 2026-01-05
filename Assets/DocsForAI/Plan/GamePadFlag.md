# GamePadFlag Plan

## 目的
- ゲームパッドでプレイしている場合に `Flag.IsGamePad` を ON にする
- ロード完了（`LoadCompleteManager.Instance.IsLoaded == true`）のタイミングで 1 回だけ判定する（常時監視しない）

## 実装方針
- `Assets/Scripts/Control/` に新規 MonoBehaviour を追加する
- `Start()` でコルーチンを開始し、`LoadCompleteManager.Instance` が存在し `IsLoaded` になるまで待つ
- `Start()` から 20 秒経過してもロード完了まで到達できない場合は、警告ログを出して処理を終了する（フラグ更新は行わない）
- ロード完了後に Input System の `Gamepad` を確認して `isGamePad` を決定する
  - 判定基準：`Gamepad.current != null` または `Gamepad.all.Count > 0` を「ゲームパッド使用（=接続されている想定）」とみなす
- `FlagManager`（Tag: `FlagManager`）を取得し、`flagManager.SetFlag(Flag.IsGamePad, isGamePad)` を実行する

## 作業項目
1. 既存の入力実装（`PlayerInputAdapter`）とロード完了タイミングを調査
2. GamePad 判定＋フラグ更新用スクリプトを追加
3. 例外/欠損（`LoadCompleteManager`/`FlagManager` 不在）時のログを最小限追加
4. `Assets/DocsForAI/Design/` に設計書とエディタ操作手順を記載
