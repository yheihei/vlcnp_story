# RangeDetect 未発見アニメ制御 実装プラン

## 目的
未発見状態のアニメーションを固定で再生できるようにし、発見/見失いに合わせて `isUndetected` を切り替える。

## タスク
1. 現状把握: `Assets/Scripts/Combat/EnemyAction/RangeDetect.cs` の検知フローと `isDetected` の使われ方を確認する。
2. 設計: 未発見アニメON/OFFのフラグ、Animator参照の持ち方、`isUndetected` の切替条件を決める。
3. 実装: `RangeDetect` にフラグ/Animator連携を追加し、Start時と検知判定時に `isUndetected` を更新する。
4. 動作確認: `isUndetected` が Start時・発見時・見失い時に想定どおり切り替わることをコード上で確認する。
