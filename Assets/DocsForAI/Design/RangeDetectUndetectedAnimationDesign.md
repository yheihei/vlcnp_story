# RangeDetect 未発見アニメ制御 設計書

## 変更コンポーネントと役割

- `RangeDetect`
  - 未発見状態アニメのON/OFFフラグを持ち、Animatorの `isUndetected` を切り替える
  - Start時に未発見アニメを初期状態として設定する（フラグON時）
  - 検知/見失いに応じて `isUndetected` を切り替える

## 変更箇所

- `Assets/Scripts/Combat/EnemyAction/RangeDetect.cs`
  - `enableUndetectedAnimation`（未発見アニメ有効化フラグ）を追加
  - `animator` 参照を追加（未設定時は同一GameObjectから取得）
  - Start時・検知判定時に `isUndetected` を更新

## Unity Editor 操作マニュアル

1. 対象の敵GameObjectを選択し、`RangeDetect` を持つことを確認する
2. `RangeDetect` の `Enable Undetected Animation` をONにする
3. `Animator` フィールドが空の場合は、同一GameObjectに `Animator` を追加する（既にある場合は空でもOK）
4. AnimatorControllerに `isUndetected`（bool）パラメータがあることを確認する
5. 以下の遷移が設定されていることを確認する
   - Any State → Undetected（`isUndetected = true`）
   - Undetected → Wait（`isUndetected = false`）
