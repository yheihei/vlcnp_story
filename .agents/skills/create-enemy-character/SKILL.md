---
name: create-enemy-character
description: Create or revise Unity 2D enemy characters in vlcnpStory2022 using existing enemy prefab variants, EnemyV2Controller, RangeDetect, reusable EnemyAction components, dedicated StatClass and Progression health values, Animator and sprite assets, colliders, and focused Play Mode verification. Use when adding an enemy under Assets/Game/Characters/Enemy, converting a bespoke enemy controller to EnemyV2 actions, configuring enemy stats or action cycles, fixing enemy direction or animation behavior, or applying tested scene-instance changes back to an enemy prefab.
---

# 敵キャラ作成

## 基本方針

敵の挙動を、小さな `EnemyAction` の順序付きリストとして組み立てる。最初から敵固有の Controller を作らず、`EnemyV2Controller`、`RangeDetect`、既存 Action、既存敵の Prefab Variant を優先して再利用する。

この skill と一緒に、作業内容に応じて以下を適用する。

- Unity のファイル・Editor 操作: `unity-development`、`unity-editor-automation`
- 実装と変更範囲の判断: `implementation-workflow`
- コード・アセット・シーン規約: `unity-project-conventions`
- ランタイム挙動の確認: `unity-playmode-verification`
- commit / push: `git-workflow`

## 構成を決める

1. 要求を「検知条件」「待機」「向き変更」「移動・攻撃」「復帰」の単位に分解する。
2. `Assets/Scripts/Combat/EnemyAction/` と `Assets/Game/Characters/Enemy/` を検索し、流用できる Action と近い敵 Prefab を探す。
3. Action の並びだけで表現できる場合は `EnemyV2Controller` を使う。専用の `<EnemyName>Controller` は作らない。
4. 新しい Action が必要な場合も、一つの責務だけを持たせ、別の敵でも使える名前と設計にする。

既存 Action の有無は名前の記憶に頼らず、少なくとも次を実行して確認する。

```bash
rg --files Assets/Scripts/Combat/EnemyAction
rg -n "class .*: EnemyAction|class .*:.*IDetect" Assets/Scripts/Combat/EnemyAction
rg --files Assets/Game/Characters/Enemy | rg '\.prefab$'
```

空中敵なら `Bee.prefab`、地上敵なら移動・攻撃方法が近い既存敵を出発点の候補にする。ただし、継承元の不要な Action、Collider、子オブジェクト、射撃位置などを必ず確認する。

## Prefab Variantを作る

新規敵は原則として `Assets/Game/Characters/Enemy/` に既存敵の Prefab Variant として作る。既存 Prefab を丸ごと複製して共通設定を分岐させない。

次を設定する。

- Sprite と Animator Controller(新規スプライト画像が必要な場合は `generate-2d-sprite` スキルに従い Codex CLI で生成する)
- `EnemyV2Controller` と `RangeDetect`
- 順序付きの `enemyActions`
- 専用の `StatClass`、`BaseStats`、`Progression` の Health
- Rigidbody2D と実際の絵に合う Collider2D
- 敵のステータス、接触ダメージ、ドロップなど継承元から変える値
- 不要な継承コンポーネントと子オブジェクトの無効化

Prefab Asset のルート座標は `(0, 0, 0)`、通常回転は identity にする。シーン上の配置座標を Prefab に Apply しない。大きさや挙動パラメータなど、全個体に共通する値だけを Prefab に反映する。

## BaseStatsとHealthを設定する

新しい敵ごとに専用の `StatClass` を作り、共有クラスの値を流用したままにしない。

1. `Assets/Scripts/Stats/StatClass.cs` の enum **末尾**へ敵名の値を追加する。途中へ挿入して、Prefab や Asset に保存済みの enum 数値をずらさない。
2. 敵ルートの `BaseStats` を確認する。既存敵の Prefab Variant が継承している場合は重複追加せず、継承した `BaseStats.statClass` を専用値で Override する。
3. `BaseStats.progression` が `Assets/Game/Stats/Progression.asset` を参照していることを確認する。
4. `Progression.asset` の `objectStatClasses` に専用 `StatClass` の項目を追加する。
5. その項目に `Stat.Health` と level 1 の Health 値を登録する。要求に基準敵がある場合は、その敵の level 1 Health と同じ値にする。
6. `BaseStats.GetStat(Stat.Health)` が設定値を返し、Health 初期化時に警告や 0 HP が発生しないことを確認する。

Health 以外の Stat が要求されていなければ追加しない。`StatClass`、Prefab の `BaseStats`、`Progression` の3点を一組として変更し、どれか一つだけの変更を残さない。

## RangeDetectとEnemyV2Controllerを設定する

`EnemyV2Controller` は `detect.IsDetect()` が真の間、`enemyActions` を配列順に一つずつ `Execute()` し、完了したら次へ進み、末尾の後は先頭へ戻る。この順序を敵の状態遷移として扱う。

`RangeDetect` には次の意味で値を設定する。

- `enemyDetectionRange`: 初めて Player を発見する距離
- `chaseRange`: 発見後に追跡を継続する距離
- `undetectedAnimationName`: 未検知中に再生する待機 Animation State。不要なら空にする

`chaseRange` を `enemyDetectionRange` 以上にしてヒステリシスを持たせ、境界付近で検知が明滅しないようにする。実際のステージ縮尺と Player の侵入方向で調整する。

例として VeryShortSwallow は次の合成で実現している。

```text
RangeDetect
  └─ EnemyV2Controller
       LookAtPlayer → Waiting → WeakTrackingCharge → Waiting → FuwaFuwaRise → 先頭へ
```

これは構成例であり、すべての敵にこの並びをコピーしない。要求に必要な最小の Action だけを登録する。

## EnemyActionを実装する

新規 Action は `Assets/Scripts/Combat/EnemyAction/` に置き、`VLCNP.Combat.EnemyAction` namespace で `EnemyAction` を継承する。

次の契約を守る。

- `Execute()` の先頭で `IsExecuting || IsDone` を確認し、二重起動を防ぐ。
- 開始時に `IsExecuting = true` にする。
- 正常終了、衝突終了、時間切れのすべてで最終的に `IsDone = true` にする。
- `Stop()` で Coroutine を止め、Rigidbody2D の速度をゼロにし、実行中状態を解消する。
- `Awake()` で Rigidbody2D、Animator、`SpeedModifier` などをキャッシュする。
- 物理移動は Rigidbody2D と `WaitForFixedUpdate` を使う。
- 速度補正が必要なら `SpeedModifier.CalculateModifiedSpeed()` または基底クラスの仕組みを使う。
- Player が消える・切り替わる可能性を考慮し、キャッシュした Transform の有効性を確認する。
- 待ち時間だけが必要なら Action 内に埋め込まず、既存の `Waiting` をリストへ挟む。
- Ground 衝突を終了条件にするときは `CompareTag("Ground")` を使う。

Action コンポーネントの Inspector 上の Enabled は、`EnemyV2Controller` から直接 `Execute()` されるかどうかの条件ではない。参照された disabled MonoBehaviour の public method も直接呼び出せるため、実行順と有効性は `enemyActions` の参照内容を基準に確認する。

## 向きと回転を扱う

### 左向き素材を正とする

このプロジェクトの敵は、正の X scale のとき元絵が左を向く規約にそろえる。`VLCNP.Combat.EnemyAction.LookAtPlayer` と Bee もこの前提で動く。

- Player が左: `localScale.x = +Abs(localScale.x)`
- Player が右: `localScale.x = -Abs(localScale.x)`
- `SpriteRenderer.flipX` は使わない

追加する素材が右向きなら、共有コードや敵ごとの例外で補正せず、全 Animation Frame の元絵を水平反転して左向きに直す。待機と攻撃で基準方向を混在させない。

Action リストで向きを変える場合は `VLCNP.Combat.EnemyAction.LookAtPlayer` を使う。常時追従用など別 namespace の同名クラスと取り違えない。

### 進行方向へ傾ける

左向き素材を X scale で反転しつつ進行方向へ回転するときは、左移動だけ上下反転して見えないよう、回転計算の基準ベクトルも反転する。

```csharp
bool isFacingLeft = direction.x < 0f;
Vector3 localScale = transform.localScale;
localScale.x = isFacingLeft
    ? Mathf.Abs(localScale.x)
    : -Mathf.Abs(localScale.x);
transform.localScale = localScale;

Vector2 orientationDirection = isFacingLeft ? -direction : direction;
float angle = Mathf.Atan2(orientationDirection.y, orientationDirection.x)
    * Mathf.Rad2Deg;
transform.rotation = Quaternion.Euler(0f, 0f, angle);
```

突き刺さりなど衝突後の姿勢が演出に必要なら、衝突時には回転を変更せず、その角度のまま停止する。浮上・復帰 Action の中で開始角度から `Quaternion.identity` へ補間し、完了時に identity を明示する。

## Animationを設定する

- 待機・Hover は Loop Time を有効にする。
- 突撃・攻撃など一回だけの Animation は Loop Time を無効にする。
- Action 開始時は、serialized field にした State 名を `animator.Play(stateName, 0, 0f)` で先頭から再生する。
- Animation Clip の Sprite key は Sprite sub-asset の参照と順番を確認する。
- 再生時間を伸ばす場合は key 時刻または sample rate を調整し、最後の Frame まで到達して保持されることを確認する。
- 待機・攻撃・復帰のすべての Sprite Frame で元絵の左右方向を統一する。

Animation の見た目だけで終了時刻を推測しない。Action の完了条件と Clip 長を意図的に組み合わせる。

## Rigidbody2DとColliderを設定する

- 空中敵は Gravity Scale を 0 にする。
- 高速突撃は Collision Detection を Continuous にする。
- 補間が見た目に必要なら Interpolate を有効にする。
- コードで姿勢を制御する敵は物理トルクに任せず、意図しない回転を防ぐ設定にする。
- 継承元の不要な BoxCollider2D などを無効化し、見た目と攻撃判定に合う Collider2D を一つずつ確認する。
- Collider は通常姿勢だけでなく、左右の突撃姿勢と Ground 衝突時でも確認する。

Collider のサイズを Sprite の寸法だけで決めず、実際のシーンで Player、Ground、狭い通路との接触を試す。

## シーン上で調整してPrefabへ反映する

ユーザーが配置済みの敵を指定した場合は、そのシーン instance をテスト対象に使う。作業前にシーンの dirty 状態と既存 override を記録し、ユーザーの未保存変更を消さない。

1. Play Mode で挙動を調整する。
2. Play Mode を終了する。
3. 残すべき functional override を確認する。
4. 対象 instance から Prefab Variant へ Apply する。
5. Prefab Asset のルート position を `(0, 0, 0)`、通常 rotation を identity に戻す。
6. シーン instance の配置座標は scene override として保持する。
7. Prefab と Scene を明示的に保存する。
8. 対象 instance に意図しない functional override が残っていないことと、`Editor.Status` に未保存変更がないことを確認する。

## 検証する

C# を編集したら対象ファイルを Import して Compile を確認する。Prefab、Animator、Animation、Sprite を編集したら関連 Asset を Import してから Play Mode に入る。

Play Mode では要求に関係する一体だけを重点的に観察する。

- 検知前は指定の待機 Animation と位置を維持するか
- Detection Range へ入ると、Action が登録順に一度ずつ進むか
- 左右どちら側の Player にも正しく顔・武器・くちばしなどが向くか
- `SpriteRenderer.flipX` が使われず、X scale の符号だけで反転するか
- 斜め移動時に背中や腹が意図しない方向を向かないか
- Ground 衝突、攻撃終了、時間切れで速度が止まるか
- 衝突後に必要な角度を維持するか
- 復帰中に回転が identity へ滑らかに戻るか
- 一回再生の Clip がループせず、最終 Frame まで表示されるか
- 末尾 Action の後に先頭へ戻り、2周目も同じ挙動になるか
- Player が Chase Range 外へ出たとき停止できるか
- `BaseStats` が専用 `StatClass` と共通 `Progression.asset` を参照しているか
- `Progression` の専用項目から level 1 Health を取得できるか
- Console に新しい Error / Exception がないか

Play Mode 中に Eval を実行しない。観察に必要な情報を取得したら Play Mode を終了し、保存状態を再確認する。

## 失敗しやすい点

- 素材の基準方向が右なのに、`LookAtPlayer` や共有 Action を変更して帳尻を合わせる。
- `SpriteRenderer.flipX` と負の X scale を混用する。
- 左右反転後も右向き用の角度をそのまま使い、左突撃だけ上下が逆になる。
- `enemyActions` の size だけ変更し、要素の component 参照や順序が欠ける。
- Action が `IsDone` に到達せず、次の Action へ進まない。
- Coroutine 停止後も Rigidbody2D の velocity が残る。
- 継承元の Collider や攻撃用子オブジェクトが残り、判定が二重になる。
- 継承元と同じ `StatClass` のままにして、後から個別のHealth調整ができなくなる。
- `StatClass` を enum の途中へ挿入し、既存Prefabの保存済み数値を別クラスへずらす。
- `BaseStats.statClass` だけ変更し、`Progression` に対応項目がなくHealthが0になる。
- Scene instance の配置座標まで Prefab に Apply する。
- Play Mode の変更を保存できたと思い込み、終了後の Prefab / Scene を確認しない。
