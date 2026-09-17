---
name: create-enemy-character
description: vlcnpStory2022 の敵を追加する、または EnemyV2 の行動・検知・向き・敵Prefabを修正するときに使う。
---

# 敵キャラ作成

敵の構成は `Assets/Scripts/Combat/EnemyAction/` と `Assets/Game/Characters/Enemy/` の近い実装から選ぶ。`EnemyV2Controller`、`RangeDetect`、既存 Action の順序で表現できる挙動はそれらを使い、共通設定は Prefab Variant で継承する。

## 変更に応じて読む

- 新規敵の作成、ステータス、シーン調整の反映: [Prefab と Health](references/prefab-and-stats.md)
- 行動・検知・停止条件の実装: [EnemyAction と検知](references/actions-and-detection.md)
- 向き・回転・アニメーション・当たり判定: [描画と物理](references/visuals-and-physics.md)
- 新しい PNG が必要な場合だけ [スプライト生成](../generate-2d-sprite/SKILL.md)

## 維持する条件

- `StatClass` の追加は enum 末尾。既存の保存済み数値をずらさない。新規敵では専用値、`BaseStats`、`Progression` の Health を一組で設定する。
- 元絵は左向き。正の X scale で左、負で右。`SpriteRenderer.flipX` と混用しない。
- シーンの配置座標を Prefab へ Apply しない。Play Mode の調整は終了後に Edit Mode へ反映する。
- `enemyActions` は配列サイズだけでなく、各 component 参照と実行順を確認する。

C#・アセットを変更したら [Editor 操作](../unity-editor-automation/SKILL.md) に従って Import・必要な Compile・保存を行う。実行時の確認は [Play Mode 検証](../unity-playmode-verification/SKILL.md) を使い、変更に関係する観点だけ選ぶ。行動ループなら2周目と中断、向きなら左右、Health なら実際の初期値を確認する。
