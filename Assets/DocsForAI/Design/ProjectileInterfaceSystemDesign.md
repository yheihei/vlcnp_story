# Projectileシステムのインターフェース化設計書

## システム概要
Projectileシステムをインターフェースベースの設計に変更し、将来的な拡張性を向上させました。

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

### 4. WeaponConfig クラス（修正）
**ファイル**: `Assets/Scripts/Combat/WeaponConfig.cs`

**変更内容**:
- `WeaponLevel.projectile`を`WeaponLevel.projectilePrefab`（GameObject型）に変更
- `LaunchProjectile`メソッドでIProjectileインターフェース経由でアクセス

## Unity Editor操作マニュアル

### 必要な再設定作業

#### 1. WeaponConfig の再設定
既存のWeaponConfig ScriptableObjectで以下の作業が必要です：

1. **WeaponConfigアセットを開く**
   - Project ウィンドウで既存のWeaponConfigアセットを選択
   - Inspector で内容を確認

2. **Projectile Prefab の再アタッチ**
   - 各 Weapon Level の「Projectile Prefab」フィールドに既存のProjectile Prefabを再度ドラッグ＆ドロップ
   - 元々「Projectile」フィールドに設定されていたPrefabと同じものを設定

#### 2. 確認すべきPrefab
以下のPrefabでProjectileコンポーネントが正常に動作することを確認：
- 既存の全Projectile Prefab
- WeaponConfig で参照されているProjectile Prefab

#### 3. テスト手順
1. **基本動作テスト**
   - プレイモードでProjectileが正常に発射されること
   - ダメージが正常に与えられること
   - 方向設定が正常に機能すること

2. **Shield機能テスト**
   - ShieldでProjectileが正常にブロックされること
   - ブロック時の効果音が再生されること
   - 地面に刺さったProjectileが無視されること

3. **WeaponConfig機能テスト**
   - 武器からProjectileが正常に発射されること
   - レベル別の設定が正常に機能すること

## 拡張性の向上

### 新しいProjectileタイプの追加方法
1. `IProjectile`インターフェースを実装した新しいクラスを作成
2. `MonoBehaviour`を継承し、必要な物理挙動を実装
3. インターフェースで定義されたメソッドを実装
4. WeaponConfigのProjectile Prefabとして設定

### 例: カスタムProjectileの実装
```csharp
public class CustomProjectile : MonoBehaviour, IProjectile
{
    public bool IsStucking { get; private set; }
    
    public void SetDirection(bool isLeft) 
    {
        // カスタム方向設定ロジック
    }
    
    public void SetDamage(float damage) 
    {
        // カスタムダメージ設定ロジック
    }
    
    public void ImpactAndDestroy() 
    {
        // カスタム衝突処理ロジック
    }
}
```

## 後方互換性
- 既存のProjectile機能は完全に保持
- 既存のゲームプレイに影響なし
- Prefabの再設定以外の追加作業は不要

## 注意事項
- WeaponConfigでのProjectile参照は手動での再設定が必要
- 新しいProjectileタイプを作成する際は、IProjectileインターフェースの完全な実装が必要
- インターフェース経由でのアクセスのため、Projectile固有の機能にアクセスする場合は型キャストが必要