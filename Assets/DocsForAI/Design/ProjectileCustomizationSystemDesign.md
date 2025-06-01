# 射出体カスタマイズシステム設計書

## 概要
既存の`Projectile.cs`を継承ベースの設計に変更し、様々な移動パターンと衝突挙動をカスタマイズ可能にしました。

## 実装されたコンポーネント

### 1. ProjectileBase.cs（抽象基底クラス）
**場所**: `Assets/Scripts/Combat/ProjectileBase.cs`

#### 役割
- 全射出体の共通機能を提供する抽象基底クラス
- 既存のProjectile.csの機能を抽象化し、継承可能な形に整理

#### 主要なフィールド
```csharp
protected float speed = 30f;              // 移動速度
protected GameObject hitEffect = null;     // ヒットエフェクト
protected float deleteTime = 0.18f;        // 削除時間
protected string targetTagName = "Enemy";  // ターゲットタグ
protected bool IsPenetration = false;      // 貫通機能
protected bool isStuckInGround = false;    // 地面刺さり機能
protected bool isBreakOnGround = false;    // 地面衝突破壊
protected bool isFadeOut = false;          // フェードアウト機能
```

#### 抽象メソッド
- `HandleMovement()`: 移動処理（各継承クラスで実装）
- `HandleCollision(Collider2D other)`: 衝突処理（各継承クラスで実装）

#### 共通メソッド
- `SetDirection(bool isLeft)`: 射出方向の設定
- `SetDamage(float damage)`: ダメージ値の設定
- `ImpactAndDestroy()`: 衝突時の破壊処理
- `HandleTargetCollision()`: 敵との衝突処理
- `HandleGroundCollision()`: 地面との衝突処理

### 2. LinearProjectile.cs（直線移動射出体）
**場所**: `Assets/Scripts/Combat/LinearProjectile.cs`

#### 役割
- 既存のProjectile.csと同じ直線移動の挙動を実現
- 既存のPrefabとの互換性を保持

#### 実装詳細
```csharp
protected override void HandleMovement()
{
    int directionX = isLeft ? -1 : 1;
    transform.Translate(directionX * speed / 50, 0, 0);
}
```

### 3. BounceProjectile.cs（バウンド射出体）
**場所**: `Assets/Scripts/Combat/BounceProjectile.cs`

#### 役割
- 地面でバウンドしながら移動する射出体
- マリオのファイアボールのような物理挙動を実現

#### 追加パラメータ
```csharp
[SerializeField] private float bounceHeight = 3f;      // バウンド高さ
[SerializeField] private float gravity = 9.8f;         // 重力
[SerializeField] private float bounceReduction = 0.8f; // バウンド減衰率
```

#### 特殊機能
- **重力システム**: Y軸方向に重力を適用
- **バウンド処理**: 地面との衝突で上向きの速度を生成
- **壁反射**: 壁との衝突でX軸方向の速度を反転
- **減衰システム**: バウンドごとに高さが減少

## Unity Editor上での操作マニュアル

### 既存Prefabの更新方法
1. **LinearProjectileへの移行**
   - 既存のProjectileコンポーネントを持つPrefabを開く
   - Projectileコンポーネントを削除
   - LinearProjectileコンポーネントを追加
   - 既存の設定値を新しいコンポーネントにコピー

### 新しいBounceProjectile Prefabの作成
1. **基本設定**
   - 新しいGameObjectを作成
   - SpriteRendererとCircleCollider2D（IsTrigger=true）を追加
   - BounceProjectileコンポーネントを追加

2. **BounceProjectileの推奨設定**
   ```
   Speed: 15-25 (LinearProjectileより少し遅め)
   Bounce Height: 2.0-4.0
   Gravity: 8.0-12.0
   Bounce Reduction: 0.7-0.9
   Delete Time: 3.0-5.0 (バウンドを考慮して長め)
   ```

### パラメータ調整ガイド

#### LinearProjectile
- **Speed**: 射出速度（推奨: 20-40）
- **Delete Time**: 削除時間（推奨: 0.15-0.3）
- **Target Tag Name**: "Enemy"（デフォルト）

#### BounceProjectile
- **Bounce Height**: 初期バウンド高さ（値が大きいほど高くバウンド）
- **Gravity**: 重力の強さ（値が大きいほど早く落下）
- **Bounce Reduction**: バウンド減衰率（0.5-1.0、1.0に近いほど減衰しない）

### デバッグ用の確認項目
1. **衝突判定**: Collider2DのIsTriggerが有効になっているか
2. **レイヤー設定**: 適切なPhysics2D設定がされているか
3. **タグ設定**: Ground、Enemy、Wallタグが正しく設定されているか
4. **エフェクト**: HitEffectプレハブが適切に設定されているか

## コードの変更箇所

### 新規作成ファイル
- `Assets/Scripts/Combat/ProjectileBase.cs`
- `Assets/Scripts/Combat/LinearProjectile.cs`
- `Assets/Scripts/Combat/BounceProjectile.cs`

### 既存ファイルへの影響
- `Assets/Scripts/Combat/Projectile.cs`は残存（互換性のため）
- 今後はLinearProjectileの使用を推奨

## 今後の拡張性
このシステムにより以下の新しい射出体タイプを容易に追加できます：
- **WaveProjectile**: 波形軌道の射出体
- **HomingProjectile**: ターゲット追尾型の射出体
- **ExplodingProjectile**: 時限爆発型の射出体

各新規クラスは`ProjectileBase`を継承し、`HandleMovement()`と`HandleCollision()`をオーバーライドするだけで実装可能です。