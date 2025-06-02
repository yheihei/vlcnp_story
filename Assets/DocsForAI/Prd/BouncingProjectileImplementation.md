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
        // 基本パラメータ
        [SerializeField] float speed = 30f;
        [SerializeField] float gravityScale = 2.0f;
        [SerializeField] float maxBounceHeight = 0.8f;
        [SerializeField] int maxBounceCount = 18;
        [SerializeField] GameObject hitEffect = null;
        [SerializeField] GameObject destroyEffect = null;
        [SerializeField] string targetTagName = "Enemy";
        [SerializeField] string groundTagName = "Ground";
        [SerializeField] float deleteTime = 10f;
        
        // 壁反射用
        [SerializeField] private FrontCollisionDetector frontCollisionDetector = null;
        
        // IProjectile実装
        public bool IsStucking => false; // バウンド弾は地面に刺さらない
    }
}
```

#### 2. 物理挙動の実装

##### 移動処理
- Rigidbody2Dを使用した物理シミュレーション
- 水平方向：一定速度を維持（FixedUpdateで速度を常に設定）
- 垂直方向：重力の影響を受ける（gravityScale）

##### バウンド処理
- 地面（"Ground"タグ）に触れたらバウンド
- Physics2Dマテリアルによる物理ベースバウンド（反発係数は別途マテリアルで設定）
- バウンド後の最大高さを物理公式で制限：
  - v² = u² + 2as を使用して最大到達高さから必要な初速度を逆算
  - バウンド直後にy方向速度を制限
- バウンド回数が`maxBounceCount`に達したら破壊

#### 3. 衝突処理
- 敵への衝突：ダメージを与えて破壊（既存Projectileと同様）
- 壁への衝突：FrontCollisionDetectorで検出して反射
  - 連続反転を防ぐためのフラグ管理
  - SetDirection()で方向を切り替え
- 地面への衝突：バウンドまたは破壊

#### 4. エフェクトとビジュアル
- 既存のProjectileと同様のヒットエフェクト
- 消滅時のdestroyEffect（PRD要件）
- 方向転換時のスケール反転処理

### Prefab設定
1. 新規GameObjectを作成
2. 以下のコンポーネントを追加：
   - BouncingProjectile
   - Rigidbody2D (Gravity Scaleはスクリプトで動的設定)
   - Collider2D × 2:
     - ダメージ判定用（Is Trigger: true）
     - バウンド判定用（Is Trigger: false、PhysicsMaterial2D付き）
   - SpriteRenderer
3. 子オブジェクトとして：
   - FrontCollisionDetector（壁検出用）
4. Layer: "Projectile"に設定
5. Tag: 適切に設定

### PhysicsMaterial2D設定
- Bounciness: 0.8〜1.0（反発係数）
- Friction: 0（摩擦なし）

### パラメータ推奨値
- speed: 30f
- gravityScale: 2.0f
- maxBounceHeight: 0.8f
- maxBounceCount: 18
- deleteTime: 10f

### テスト項目
1. バウンドが正しく動作すること
2. バウンド高さがmaxBounceHeightを超えないこと
3. 最大バウンド回数後に破壊されること
4. 壁で反射すること（FrontCollisionDetectorが正しく設定されている場合）
5. 敵にダメージを与えること
6. エフェクトが正しく表示されること

### 実装上の注意点
- FrontCollisionDetectorは「Ground」「Item」「Enemy」タグのみ検出
- 壁反射にはFrontCollisionDetectorのtargetTagsに壁のタグを追加する必要がある
- バウンド高さ制限はコルーチンで1フレーム待機後に適用

### 今後の拡張案
- バウンド時にダメージ増加
- バウンド時に分裂
- 地形に応じたバウンド角度の変化