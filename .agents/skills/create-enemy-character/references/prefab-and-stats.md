# Prefab と Health

## 新規敵

`Assets/Game/Characters/Enemy/` の挙動が近い Prefab から Variant を作る。空中敵では `Bee.prefab` が候補。継承した Action・Collider・子オブジェクト・射撃位置を調べ、不要なものを無効化し、継承コンポーネントを重複追加しない。

要求に応じて Sprite、Animator、`EnemyV2Controller`、`RangeDetect`、`enemyActions`、Rigidbody2D、Collider2D、接触ダメージ、ドロップを設定する。既存敵の部分修正で全設定をやり直さない。

## Health の連携

新規敵ごとに専用の `StatClass` を使う。

1. `Assets/Scripts/Stats/StatClass.cs` の enum 末尾へ追加する。途中挿入は保存済みの値を別の敵へずらす。
2. 敵の `BaseStats.statClass` に専用値を設定する。Variant が継承する `BaseStats` がある場合は Override する。
3. `BaseStats.progression` は `Assets/Game/Stats/Progression.asset` を参照する。
4. 同 asset の `objectStatClasses` に専用クラスの `Stat.Health` と level 1 の値を登録する。基準敵が指定されていればその値に合わせる。
5. `BaseStats.GetStat(Stat.Health)` と Health の初期値を確認し、対応項目の欠落による 0 HP や警告がないことを確かめる。

要求されていない Stat は追加しない。

## 配置済み instance の調整

作業前の dirty 状態と関係する override を確認し、指定された instance で調整する。Play Mode で採用した値は終了前に記録し、終了後に Edit Mode の instance へ反映してから、全個体に共通する変更だけ Variant に Apply する。

Prefab asset のルート position は `(0, 0, 0)`、通常 rotation は identity。シーンの配置座標は scene override に残す。対象 Prefab と Scene を保存し、意図しない functional override が残っていないか確認する。他の作業の override や dirty 状態を消すために一括 Apply・保存しない。
