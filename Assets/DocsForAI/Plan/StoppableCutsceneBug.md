# イベント停止中にプレイヤーが動けてしまう不具合対応プラン

1. `PartyCongroller.SetCurrentPlayerActive()` が `IStoppable.IsStopped` を強制的に `false` にしている実装を確認し、停止フラグが解除される仕組みを整理する。
2. 停止中 (`PartyCongroller.isStopped == true`) に限り `IStoppable` を解除しないようコードを修正し、停止状態を正しく伝播させる。
3. 修正後の挙動を確認するための確認ポイントと注意事項をドキュメント化する。
