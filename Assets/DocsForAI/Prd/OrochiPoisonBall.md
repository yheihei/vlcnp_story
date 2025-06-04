# PRD

## 目的

- Playerが特定の武器のProjectileで敵にダメージを与えたときに、敵に毒を付与したい
- プレイヤー用の毒のステータス`Assets/Scripts/Stats/PoisonStatus.cs`とは別に、敵キャラ用の毒のステータスを作りそれがアタッチされている敵のみ、毒を受ける
- 毒を受けている敵は、一定時間ごとにダメージを受ける。ただしHPは1までしか下がらない
- 毒は一定時間で治る
- プレイヤー用の毒のステータスと同じく、毒を受けている間キャラクターにエフェクトが表示される

## 技術的制約
- 単一責任の原則を守りつつより良いクラス設計にすること
- Unity Editor上での各種コンポーネントのアタッチ作業はユーザーが行うので、仕組みだけ用意して欲しい
- 敵キャラに切り替え機能はないため、切り替え時の挙動については考えない

## 開発ガイドライン(参考)

- Projectileは`IProjectile`クラスを継承しているもの
- 武器の定義は`Assets/Scripts/Combat/WeaponConfig.cs`。`LaunchProjectile`メソッドでProjectileが発射される
- 敵キャラのHPはプレイヤーと同じく`Assets/Scripts/Attributes/Health.cs`で管理されている
