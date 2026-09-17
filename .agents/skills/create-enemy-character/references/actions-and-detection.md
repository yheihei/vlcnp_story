# EnemyAction と検知

既存の挙動は `Assets/Scripts/Combat/EnemyAction/`、制御は `Assets/Scripts/Control/EnemyV2Controller.cs` で確認する。過去の敵の Action 列をそのままコピーせず、要求された検知・待機・攻撃・復帰を既存部品で組む。

## 制御の契約

`EnemyV2Controller` は検知中に `enemyActions` を配列順に実行し、`IsDone` の Action を Reset して次へ進む。末尾の後は先頭へ戻る。disabled MonoBehaviour の public method も直接呼べるため、Inspector の Enabled だけを見て Action が実行されないと判断しない。

新規 Action は `VLCNP.Combat.EnemyAction` namespace で `EnemyAction` を継承する。

- `Execute()` で `IsExecuting || IsDone` を確認して二重起動を防ぎ、開始時に `IsExecuting = true` とする。
- 正常終了・衝突・時間切れなど、その Action の完了経路では `IsDone = true` に到達させる。
- `Stop()` は基底で何もしない。Action が開始した Coroutine や移動を停止し、再開できる状態へ戻す。速度を制御する Action は残留 velocity も解消する。
- 物理移動は Rigidbody2D と物理更新に合わせる。必要な参照をキャッシュし、`SpeedModifier` による速度補正と Player 切り替えに対応する。
- 単なる待ち時間は既存の `Waiting` で分離できる。Ground 接触を終了条件にするなら `CompareTag("Ground")` を使う。

## RangeDetect

現行コードは `Assets/Scripts/Combat/EnemyAction/RangeDetect.cs`。Inspector の設定と Animator の接続を一緒に確認する。

- `enemyDetectionRange`: 初回発見の距離。
- `chaseRange`: 発見後の追跡距離。現行実装は detection range 以上へ補正する。境界で検知が明滅しない余裕を設ける。
- `enableUndetectedAnimation` と `animator`: 未発見演出を有効にする。
- 未発見演出は Animator の bool `isUndetected` を使う。発見演出を使うときは Animation Event から `OnUndetectedAnimationFinished()` を呼び、検知確定待ちを解除する。この接続がないと行動が始まらない。

クラスが変わっている場合は実際のフィールドと遷移を優先する。

行動ループを変えた場合は、要求した順序、2周目、追跡範囲外での停止を重点的に確認する。
