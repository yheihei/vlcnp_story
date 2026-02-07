# Plan: RangeDetectPermanentDetection

## Goals
- `RangeDetect` で一度発見した敵は、以後プレイヤーとの距離に関係なく発見状態を維持する。
- `chaseRange` とその判定ロジックを削除し、検出仕様を単純化する。

## Scope
- 対象: `Assets/Scripts/Combat/EnemyAction/RangeDetect.cs`
- 非対象: `EnemyV2Controller` や他検出コンポーネントの仕様変更

## Design Outline
1. `isDetected == true` の場合は `IsDetect()` が常に `true` を返す。
2. 発見後は `SetIsUndetected(false)` を維持し、未発見アニメに戻さない。
3. 未発見時の初回検出ロジック (`enemyDetectionRange`) は現状維持。
4. 使わなくなる `chaseRange` を SerializeField と Gizmo から削除する。

## Tasks
- `RangeDetect` の `IsDetect()` から `inChaseRange` 判定を削除。
- `RangeDetect` から `chaseRange` フィールドを削除。
- `OnDrawGizmosSelected()` の追跡距離表示を削除。
- 参照漏れがないことをテキスト検索で確認。

## Acceptance Criteria
- 一度 `isDetected` が `true` になった敵は、以後 `IsDetect()` が常に `true`。
- `RangeDetect.cs` 内に `chaseRange` / `inChaseRange` が存在しない。
- 既存の未発見→発見遷移（初回検出）は維持される。
