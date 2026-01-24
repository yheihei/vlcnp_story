# RangeDetecting Design

## 変更内容
- `RangeDetect` に「発見アニメ待機中は IsDetect を false で固定」する制御を追加。
- Animation Event で `OnUndetectedAnimationFinished` が呼ばれた後にのみ発見確定するよう調整。
- 発見アニメ中にプレイヤーが範囲外へ出ても、発見確定は解除されない。

## 対象コンポーネント
- `RangeDetect` (`Assets/Scripts/Combat/EnemyAction/RangeDetect.cs`)
  - 役割: プレイヤーとの距離に応じて敵の検出状態を返す。
  - 変更点: 発見アニメ待機フラグを追加し、待機中は `IsDetect` を必ず false にする。

## Unity Editor 操作手順
- `RangeDetect` を持つ敵オブジェクトで `enableUndetectedAnimation` を true にする。
- 発見アニメの末尾に Animation Event を追加し、`RangeDetect.OnUndetectedAnimationFinished()` を呼ぶ。
- `enemyDetectionRange` と `chaseRange` を必要に応じて調整する。
