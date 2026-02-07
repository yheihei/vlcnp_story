# RangeDetect Permanent Detection Design

## コンポーネントの役割と変更箇所
- `RangeDetect` (`Assets/Scripts/Combat/EnemyAction/RangeDetect.cs`)
  - 役割: プレイヤー検出状態を返し、敵AI開始条件を提供する。
  - 変更:
    - 発見済み (`isDetected == true`) のとき、距離判定を行わず `IsDetect()` を常に `true` で返す。
    - 発見後のアニメ状態は `SetIsUndetected(false)` を維持する。
    - 不要になった `chaseRange` フィールドと Gizmo 赤円描画を削除。

## Unity Editor上での操作マニュアル
1. 対象敵プレハブ/シーンオブジェクトを選択し、`RangeDetect` を確認する。
2. `enemyDetectionRange` を初回発見距離として設定する。
3. `enableUndetectedAnimation` が有効な場合、従来どおり発見アニメ終了時に `OnUndetectedAnimationFinished()` を呼ぶ設定を維持する。
4. `chaseRange` 項目は削除されるため、Inspectorで追跡距離の調整は不要。

## 想定挙動
- 初回発見前: `enemyDetectionRange` 内で発見処理が走る。
- 初回発見後: プレイヤーが離脱しても敵は発見状態を維持し、AIは停止しない。
