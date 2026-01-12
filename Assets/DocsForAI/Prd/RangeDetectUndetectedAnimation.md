# 目的

未発見状態のアニメーションを固定で再生できるようにしたい。
RangeDetectで未発見状態アニメのON/OFFを制御できるようにする。

## 実装箇所

@Assets/Scripts/Combat/EnemyAction/RangeDetect.cs

## 実装詳細

- RangeDetectに「未発見状態アニメを有効にする」ためのフラグを追加（デフォルトfalse）
- 上記フラグがONのとき、Start時にAnimatorのboolパラメータ `isUndetected` をtrueにする
- 発見状態になったら `isUndetected` をfalseにする
- AnimatorControllerは以下を前提とする
  - State: Undetected
  - Parameter: isUndetected
  - Transition:
    - Any State → Undetected (isUndetected = true)
    - Undetected → Wait (isUndetected = false)
