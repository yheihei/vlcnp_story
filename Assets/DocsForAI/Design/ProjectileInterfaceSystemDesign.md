# Projectileシステムのインターフェース化とBouncingProjectile実装設計書

## システム概要
Projectileシステムにおいて、既存のインターフェース設計を活用し、新しいBouncingProjectileクラスを実装しました。マリオのファイアーボールのようなバウンド動作を持つ弾丸を実現しています。

## コンポーネント構成

### 1. IProjectile インターフェース
**ファイル**: `Assets/Scripts/Combat/IProjectile.cs`

**役割**: 
- 発射物の共通インターフェースを定義
- 異なる種類の発射物クラス間の統一されたアクセス方法を提供

**メンバー**:
- `bool IsStucking { get; }`: 地面に刺さっているかの状態
- `void SetDirection(bool isLeft)`: 発射方向の設定
- `void SetDamage(float damage)`: ダメージ値の設定
- `void ImpactAndDestroy()`: 衝突時の破壊処理

### 2. Projectile クラス（修正）
**ファイル**: `Assets/Scripts/Combat/Projectile.cs`

**変更内容**:
- `IProjectile`インターフェースを実装
- 既存の全機能を保持（後方互換性維持）

**実装インターフェース**:
- `IStoppable`: 停止機能
- `IProjectile`: 発射物インターフェース

### 3. Shield クラス（修正）
**ファイル**: `Assets/Scripts/Combat/Shield.cs`

**変更内容**:
- `Projectile`型参照を`IProjectile`型に変更
- インターフェース経由での衝突処理に変更

**主な機能**:
- IProjectile実装オブジェクトとの衝突検知
- 地面に刺さったProjectileの無視
- ブロック効果音の再生

### 4. BouncingProjectile クラス（新規追加）
**ファイル**: `Assets/Scripts/Combat/BouncingProjectile.cs`

**実装インターフェース**:
- `IStoppable`: 停止機能
- `IProjectile`: 発射物インターフェース

**主な機能**:
- **物理ベースの移動**: Rigidbody2Dを使用した重力とバウンド
- **バウンド動作**: 地面との衝突時に反発係数を適用してバウンド
- **設定可能なパラメータ**: 速度、重力、反発係数、最大バウンド回数
- **エフェクト対応**: 衝突エフェクトと消滅エフェクトの生成
- **ダメージ処理**: 既存のProjectileと同様のダメージシステム

**設定可能パラメータ**:
- `speed`: 初期速度（デフォルト30）
- `gravityScale`: 重力倍率（デフォルト2.0）
- `bounceCoefficient`: バウンド反発係数（デフォルト0.85）
- `maxBounceCount`: 最大バウンド回数（デフォルト18）
- `hitEffect`: 衝突時のエフェクトPrefab
- `destroyEffect`: 消滅時のエフェクトPrefab
- `targetTagName`: ダメージ対象のタグ（デフォルト"Enemy"）
- `groundTagName`: 地面のタグ（デフォルト"Ground"）
- `deleteTime`: 自動削除時間（デフォルト10秒）

## Unity Editor操作マニュアル

### BouncingProjectile Prefabの作成

#### 1. 基本Prefabの作成
1. **空のGameObjectを作成**
   - Hierarchy で右クリック → Create Empty
   - 名前を「BouncingProjectile」に変更

2. **必要なコンポーネントを追加**
   - `BouncingProjectile` スクリプトをアタッチ
   - `Rigidbody2D` コンポーネントを追加（スクリプトが自動追加します）
   - `SpriteRenderer` コンポーネントを追加
   - `Collider2D` コンポーネントを追加（CircleCollider2D推奨）

3. **Sprite設定**
   - SpriteRenderer の Sprite フィールドに弾丸用のスプライトを設定
   - 適切なサイズに調整

#### 2. BouncingProjectileコンポーネントの設定

**基本パラメータ**:
- `Speed`: 初期速度（30が推奨）
- `Gravity Scale`: 重力倍率（2.0が推奨）
- `Bounce Coefficient`: バウンド反発係数（0.85が推奨）
- `Max Bounce Count`: 最大バウンド回数（18回が推奨）

**エフェクト設定**:
- `Hit Effect`: 衝突時のエフェクトPrefab（既存のものを流用可能）
- `Destroy Effect`: 消滅時のエフェクトPrefab（PRD要件）

**ターゲット設定**:
- `Target Tag Name`: ダメージを与える対象のタグ（通常は"Enemy"）
- `Ground Tag Name`: バウンドする地面のタグ（通常は"Ground"）
- `Delete Time`: 自動削除時間（10秒推奨）

#### 3. 物理設定
**Rigidbody2D設定**:
- Gravity Scale: BouncingProjectileスクリプトが自動設定
- Freeze Rotation Z: チェック推奨（回転を防ぐ）

**Collider2D設定**:
- Is Trigger: ダメージ判定用（敵との衝突）
- 物理衝突用に別途Collider2Dを追加する場合は、Is Triggerを無効にしてバウンド判定に使用

#### 4. WeaponConfigでの使用
1. **WeaponConfig ScriptableObjectを開く**
2. **Projectile Prefab フィールドに作成したBouncingProjectile Prefabを設定**
3. **武器の設定に応じてダメージなどのパラメータを調整**

### テスト手順
1. **バウンド動作テスト**
   - プレイモードで弾丸を発射
   - 地面に触れた際にバウンドすることを確認
   - 設定したバウンド回数に達したら消滅することを確認

2. **ダメージテスト**
   - 敵に当たった際にダメージが与えられることを確認
   - 敵に当たったら即座に消滅することを確認

3. **エフェクトテスト**
   - 衝突時にhitEffectが生成されることを確認
   - 消滅時にdestroyEffectが生成されることを確認

## 拡張性の向上

### 新しいProjectileタイプの追加方法
1. `IProjectile`インターフェースを実装した新しいクラスを作成
2. `MonoBehaviour`を継承し、必要な物理挙動を実装
3. インターフェースで定義されたメソッドを実装
4. WeaponConfigのProjectile Prefabとして設定

### 例: BouncingProjectileの実装例
BouncingProjectileクラスは、以下のような実装になっています：

```csharp
public class BouncingProjectile : MonoBehaviour, IStoppable, IProjectile
{
    // 物理パラメータ
    [SerializeField] float speed = 30;
    [SerializeField] float gravityScale = 2.0f;
    [SerializeField] float bounceCoefficient = 0.85f;
    [SerializeField] int maxBounceCount = 18;
    
    // バウンド処理
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(groundTagName))
        {
            if (bounceCount >= maxBounceCount)
                ImpactAndDestroy();
            else
                // バウンド処理実行
        }
    }
}
```

## 実装の特徴

### 物理計算について
- **重力**: Rigidbody2D.gravityScaleによる自然な重力効果
- **バウンド**: 垂直速度に反発係数を適用、水平速度は維持
- **方向制御**: SetDirectionメソッドでの水平方向の反転対応

### エフェクトシステム
- **hitEffect**: 敵との衝突時に生成される衝突エフェクト
- **destroyEffect**: 弾丸消滅時に生成される消滅エフェクト（PRD要件）
- エフェクトは自動的に2秒後（destroyEffect）または1秒後（hitEffect）に削除

### ダメージシステム
- 既存のProjectileクラスと同じHealth.TakeDamageを使用
- 重複ダメージ防止機能（penetratedObjectsリスト）
- 敵に当たると即座に消滅（貫通なし）

## 後方互換性
- 既存のProjectileクラスには一切影響なし
- 既存のWeaponConfigシステムとの互換性を維持
- 新しいBouncingProjectileの追加のみ

## トラブルシューティング

### よくある問題と解決方法

1. **バウンドしない場合**
   - Collider2DのIs Triggerが有効になっていないか確認
   - Ground タグが正しく設定されているか確認
   - OnCollisionEnter2D用のCollider2Dが追加されているか確認

2. **エフェクトが表示されない場合**
   - hitEffect または destroyEffect Prefabが設定されているか確認
   - エフェクトPrefabにParticleSystemまたはアニメーションが設定されているか確認

3. **ダメージが入らない場合**
   - Target Tag Nameが正しく設定されているか確認
   - 敵にHealthコンポーネントがアタッチされているか確認
   - ダメージ用のCollider2DのIs Triggerが有効になっているか確認