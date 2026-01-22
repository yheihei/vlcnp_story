# RangeDetect 未発見アニメ待機

## 変更概要
- RangeDetect に「未発見アニメ終了まで検知を待機する」フラグを追加。
- アニメイベントで待機解除できるように通知メソッドを追加。

## コンポーネントの役割
- RangeDetect: プレイヤーとの距離で検知状態を判定し、未発見/発見のアニメ状態を制御する。

## 変更箇所
- `Assets/Scripts/Combat/EnemyAction/RangeDetect.cs`
  - `isWaitingUndetectedAnimationEnd` を追加し、待機中は `IsDetect()` を常に `false` にする。
  - `OnUndetectedAnimationFinished()` を追加し、アニメ終了通知で待機解除。

## Unity Editor 操作手順
1. 対象 Enemy の Animator に未発見アニメ（未発見開始〜終了）を設定。
2. 未発見アニメの最後のフレームに Animation Event を追加。
3. Animation Event の関数名に `OnUndetectedAnimationFinished` を指定（引数なし）。
4. `RangeDetect` の `enableUndetectedAnimation` を `true` に設定。

## 補足
- Animation Event が発火するまで検知は開始されません。
