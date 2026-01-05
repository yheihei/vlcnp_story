# GamePadActiveController 設計メモ

## 役割と変更箇所
- `Assets/Scripts/Control/GamePadActiveController.cs`
  - `Flag.IsGamePad` に応じてアタッチした GameObject を active/非active に切り替える
  - `ActiveCondition` で `WhenGamepad` / `WhenNotGamepad` を選択できる
  - `Start()` で `LoadCompleteManager.Instance.IsLoaded` を待機し、ロード完了時に初回反映
  - `FlagManager.OnChangeFlag` でフラグ更新時に再評価する

## Unity Editor 操作メモ
1. 対象の GameObject に `GamePadActiveController` をアタッチする。
2. `ActiveCondition` を用途に合わせて選択する（WhenGamepad / WhenNotGamepad）。
3. シーン内に Tag が `FlagManager` の GameObject があり、`FlagManager` コンポーネントが付いていることを確認する。
4. シーン内に `LoadCompleteManager` が存在することを確認する。
5. 再生して `Flag.IsGamePad` の値に応じて対象 GameObject が active/非active に切り替わることを確認する。
