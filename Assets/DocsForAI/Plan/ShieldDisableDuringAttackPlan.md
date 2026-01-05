# Shield 攻撃中無効化プラン

## 目的
攻撃アニメーション中のみ特定の Shield オブジェクトを無効化し、攻撃が通るようにする。複数の攻撃アクションから共通で使える設計にする。

## 前提
- 攻撃中 = アニメーションの攻撃ステート再生中
- Shield は複数の独立した GameObject
- 攻撃アクションは複数存在

## 方針
- Shield の ON/OFF を集約する `ShieldController` を用意し、複数要因の重なりに耐えられるように参照カウント方式で管理する。
- 攻撃中かどうかの判定は Animator のステートに紐づけ、アニメーション開始/終了で Shield を切替する。
- アクション本体は Shield 制御の詳細を持たず、Animator 側の仕組みに寄せて再利用性を上げる。

## タスク
1. 既存の Shield 構成と該当アクション群を調査し、どの敵/プレハブで Shield を制御する必要があるか整理する。
2. `ShieldController` の責務と API を決定する（例：`Disable(reason)` / `Enable(reason)`、内部は disableCount か HashSet）。
3. Animator 連動の仕組みを決定する。
   - 候補 A: `StateMachineBehaviour` で `OnStateEnter/Exit` による切替
   - 候補 B: Animation Event で切替
   - 攻撃アニメの管理負荷と再利用性の観点で A/B を選定
4. Unity Editor 手順を整理（どのステートに何を付与するか、Shield 対象の割当方法）。
5. 競合ケースの扱いを明文化（同時に別攻撃が走る、キャンセルされるなど）。

## 期待する動作
- 攻撃ステートに入った瞬間に Shield が無効化され、退出時に有効化される。
- 複数の攻撃が重なっても、最後の攻撃が終了するまで Shield が有効化されない。
- プレイヤー不在/攻撃不成立時は Shield の状態が変わらない。
