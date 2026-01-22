# Plan: RangeDetect undetected animation wait

## Goals
- enableUndetectedAnimation = true のとき、未発見アニメの終了まで検知処理を待機する。
- アニメ側からの通知で待機解除する。

## Tasks
- RangeDetect に待機フラグを追加して IsDetect をガード。
- アニメイベント用の通知メソッドを追加。
- 既存の未発見アニメ設定に合わせて初期待機を開始。

## Notes
- 通知は Animation Event で RangeDetect.OnUndetectedAnimationFinished を呼ぶ。
