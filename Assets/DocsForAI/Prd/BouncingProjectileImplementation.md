## PRD: BouncingProjectileの実装

### 背景
Projectileシステムのインターフェース化が完了した後、最初の新しいProjectileタイプとして、マリオのファイアーボールのようなバウンドする弾を実装します。これにより、ゲームプレイに新しい戦略的要素を追加できます。

### 前提条件
- IProjectileインターフェースが実装済みであること
- 既存のProjectileクラスがIProjectileを実装していること

### 目的
- 地面でバウンドする新しいProjectileタイプを追加する
- プレイヤーや敵が使える新しい攻撃パターンを提供する

### 要件

#### 1. BouncingProjectileクラスの作成
以下の仕様でクラスを実装する：

```csharp
namespace VLCNP.Combat
{
    public class BouncingProjectile : MonoBehaviour, IStoppable, IProjectile
    {
        // 基本パラメータ（Projectileと同様）
        [SerializeField] float speed = 20f;
        [SerializeField] GameObject hitEffect = null;
        [SerializeField] float deleteTime = 5f;
        [SerializeField] string targetTagName = "Enemy";
        
        // バウンド専用パラメータ
        [Header("バウンド設定")]
        [SerializeField] float gravityScale = 2f;
        [SerializeField] float bounceForce = 8f;
        [SerializeField] int maxBounceCount = 3;
        [SerializeField] float bounceDamping = 0.8f; // バウンド時の速度減衰
        
        // 物理挙動用
        private Rigidbody2D rb;
        private int currentBounceCount = 0;
        private float horizontalVelocity;
        
        // IProjectile実装
        public bool IsStucking => false; // バウンド弾は地面に刺さらない
    }
}
```

#### 2. 物理挙動の実装

##### 移動処理
- Rigidbody2Dを使用した物理シミュレーション
- 水平方向：一定速度を維持
- 垂直方向：重力の影響を受ける

##### バウンド処理
- 地面（"Ground"タグ）に触れたらバウンド
- バウンド時に`bounceForce`の上向きの力を加える
- バウンド回数が`maxBounceCount`に達したら、次の地面衝突時に破壊
- 各バウンドで水平速度に`bounceDamping`を適用

#### 3. 衝突処理
- 敵への衝突：ダメージを与えて破壊（既存Projectileと同様）
- 壁への衝突：反射して逆方向へ
- 地面への衝突：バウンドまたは破壊

#### 4. エフェクトとビジュアル
- 既存のProjectileと同様のヒットエフェクト
- バウンド時の小さなパーティクルエフェクト（オプション）

### Prefab設定
1. 新規GameObjectを作成
2. 以下のコンポーネントを追加：
   - BouncingProjectile
   - Rigidbody2D (Gravity Scale: 0, 動的に制御)
   - CircleCollider2D または CapsuleCollider2D
   - SpriteRenderer
3. Layer: "Projectile"に設定
4. Tag: "Projectile"に設定

### パラメータ推奨値
- speed: 20f
- gravityScale: 2f
- bounceForce: 8f
- maxBounceCount: 3
- bounceDamping: 0.8f
- deleteTime: 5f

### テスト項目
1. バウンドが正しく動作すること
2. 最大バウンド回数後に破壊されること
3. 壁で反射すること
4. 敵にダメージを与えること
5. エフェクトが正しく表示されること

### 今後の拡張案
- バウンド時にダメージ増加
- バウンド時に分裂
- 地形に応じたバウンド角度の変化