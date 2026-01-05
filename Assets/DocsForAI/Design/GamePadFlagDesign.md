# GamePadFlag 機能設計

## 目的
ロード完了時点でゲームパッドが利用可能な場合に、`Flag.IsGamePad` を ON にする。

## 追加コンポーネント
- `Assets/Scripts/Control/GamePadFlagSetter.cs`
  - `LoadCompleteManager.Instance.IsLoaded` が `true` になるまで待機する
  - `Start()` から 20 秒以内にロード完了まで到達できない場合は、警告ログを出して終了する（フラグ更新は行わない）
  - ロード完了後にゲームパッドの有無を 1 回だけ判定し、`Flag.IsGamePad` を更新する
  - `enableDebugLog` を `true` にすると、フラグ更新ログを出力する

## 判定ロジック
- 「ゲームパッド使用」は、以下のいずれかを満たす場合に `true` とみなす
  - `UnityEngine.InputSystem.Gamepad.current != null`
  - `UnityEngine.InputSystem.Gamepad.all.Count > 0`（= 接続されている Gamepad が存在）
- 常時監視は行わないため、起動後に抜き差ししても `Flag.IsGamePad` は自動更新されない

## 変更点
- 新規追加: `Assets/Scripts/Control/GamePadFlagSetter.cs`

## Unity Editor 操作手順
1. シーン内の任意の GameObject（例: `Managers`）に `GamePadFlagSetter` を Add Component する
2. シーン内に `LoadCompleteManager` が存在することを確認する
3. `FlagManager` を持つ GameObject の Tag が `FlagManager` になっていることを確認する
4. （任意）`Enable Debug Log` を ON にする
5. Play 後、`FlagManager` の管理している `Flag.IsGamePad` が期待通り更新されることを確認する
