# 目的

Ghostの EnemyV2Controllerに付与する新しいenemyActionを作りたい。
ふわふわしながらプレイヤーのほうに近づくというIEnemyAction

# 詳細

- Actionの参考は @Assets/Scripts/Combat/EnemyAction/RandomFlyToPlayer.cs 。Executeで EnemyV2Controller から呼ばれる想定
- 上下に常にふわふわしている。 @Assets/Scripts/Movement/Floating.cs のようなイメージ
- デフォルトのスピードはゆっくり目。スピードはSerializeFieldで指定できる
- 何秒間近づくかはSerializeFieldで指定できる
- Assets/Scripts/Combat/EnemyAction 配下に実装せよ

