# イベント停止機能 修正設計メモ

## 変更概要
- `Assets/Scripts/Control/PartyCongroller.cs`
  - `SetCurrentPlayerActive()` 内で `IStoppable.IsStopped` を一律 `false` に戻していた処理を廃止し、`PartyCongroller` 自身の `isStopped` 状態を各コンポーネントに伝播するよう修正しました。
  - これにより `StoppableController.StopAll()` 実行後にキャラクター切り替えや再アクティブ化が行われても、停止状態が維持されます。

## Unity Editor 操作メモ
1. 該当シーン（例: `Assets/Scenes/VeryLongFarm_1_boss.unity`）を開く。
2. `StoppableController.StopAll()` を呼び出すフローチャートやイベントを再生し、プレイヤーが停止することを確認。
3. 停止中にキャラクター切り替えイベントや `SetCurrentPlayerByName` を呼ぶ処理を実行し、引き続き移動ができないことを確認。
4. 停止解除イベント（`StartAll()` 等）後に通常どおり操作可能になることを確認。
