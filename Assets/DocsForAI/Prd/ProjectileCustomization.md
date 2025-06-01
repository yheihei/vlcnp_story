## PRD: Projectileシステムのインターフェース化

### 背景
現在のProjectileクラスは等速直線運動のみに対応しており、異なる物理的な動きを実装することができません。マリオのファイアーボールのようなバウンドする弾や、その他の特殊な動きをする弾を実装するためには、Projectileシステムを拡張可能な設計にする必要があります。

### 目的
- Projectileの物理的な動きを抽象化し、異なる動きのパターンを実装可能にする
- 既存のコードへの影響を最小限に抑えながら、新しいProjectileタイプを追加できるようにする

### 要件

#### 1. IProjectileインターフェースの作成
以下のメンバーを持つインターフェースを作成する：
```csharp
public interface IProjectile
{
    // プロパティ
    bool IsStucking { get; }
    
    // メソッド
    void SetDirection(bool isLeft);
    void SetDamage(float damage);
    void ImpactAndDestroy();
}
```

#### 2. 既存Projectileクラスの修正
- IProjectileインターフェースを実装する
- 既存の動作は変更しない（後方互換性を維持）

#### 3. BouncingProjectileクラスの作成
- IProjectileインターフェースを実装する
- マリオのファイアーボールのような動き：
  - 地面に触れたらバウンドする
  - 重力の影響を受ける
  - バウンド時の反発係数は設定可能（SerializeField）
  - 最大バウンド回数の設定（SerializeField）
- その他の機能（ダメージ処理、エフェクト等）は既存のProjectileクラスと同様

#### 4. 使用側の修正

##### WeaponConfig.cs
- WeaponLevelクラスのprojectileフィールドの型は変更しない（Projectile型のまま）
  - 理由：SerializeFieldの互換性を保つため
- 将来的にBouncingProjectileを使う場合は、新しいWeaponConfigを作成する

##### Shield.cs
- GetComponent<Projectile>()の処理は維持
- 追加でGetComponent<IProjectile>()でも取得を試みる（BouncingProjectile対応）

### 実装の詳細

#### BouncingProjectileの物理仕様
- 初期速度：既存のProjectileと同じspeedパラメータを使用
- 重力：-9.8 * gravityScale（gravityScaleはSerializeField、デフォルト2.0）
- バウンド反発係数：0.7（SerializeField）
- 最大バウンド回数：3回（SerializeField）
- バウンド後も水平速度は維持

### テスト項目
1. 既存のProjectileが従来通り動作すること
2. BouncingProjectileが正しくバウンドすること
3. BouncingProjectileがバウンド上限に達したら地面を貫通すること
4. Shieldが両方のProjectileタイプに対応すること
5. WeaponConfigから既存のProjectileが正しく発射されること

### 今後の拡張性
このインターフェース化により、以下のようなProjectileも追加可能になる：
- HomingProjectile（ホーミング弾）
- SineWaveProjectile（波状に動く弾）
- SplitProjectile（分裂する弾）