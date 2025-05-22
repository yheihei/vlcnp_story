# 目的

敵キャラがプレイヤーと当たった時に、プレイヤーにダメージを与えるクラス`DirectAttack`を作りたい

- FighterクラスをUnity Editor上から指定できるようにする
- 接触範囲をCollider2DでUnity Editor上から指定できるようにする
- 接触対象のタグ名を複数Unity Editor上から指定できるようにする
- OnTriggerStay2D契機でプレイヤーにダメージを与える

# 制約

ダメージを与える仕組みは @EnemyV2Controller.cs を真似すること
