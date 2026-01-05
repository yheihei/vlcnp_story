# GamePadActiveController Plan

## 目的
- `Flag.IsGamePad` の値に応じて、アタッチした GameObject を active/非active に切り替える
- `LoadCompleteManager.Instance.IsLoaded == true` になったタイミングで初回チェックを行う
- `FlagManager.OnChangeFlag` 経由でフラグ更新があった場合にも再チェックする

## 実装方針
- `Assets/Scripts/Control/` に `GamePadActiveController` を追加する
- `Start()` で `FlagManager` を取得し、`OnChangeFlag` に登録する
- コルーチンで `LoadCompleteManager.Instance` の存在と `IsLoaded` を待ち、完了後に初回チェックを行う
- `OnChangeFlag` では、ロード完了後のみ再チェックする（フラグ種別は問わない）
- 破棄時に `OnChangeFlag` を解除する

## 作業項目
1. 既存の `FlagManager` / `LoadCompleteManager` 実装とパターン確認
2. `GamePadActiveController` の追加とフラグ反映処理の実装
3. `Assets/DocsForAI/Design/` に設計書と Unity Editor 操作メモを作成
