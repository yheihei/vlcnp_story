# Projectileシステムのインターフェース化実装計画書

## 概要
既存のProjectileクラスを具象クラスからインターフェースベースの設計に変更し、将来的な拡張性を向上させる。

## 背景
現在のProjectileクラスは具象クラスとして実装されており、異なる物理的動きを持つProjectileを追加する際の拡張性に欠けている。インターフェース化により、様々な動きのProjectileを容易に実装できるようにする必要がある。

## 実装手順

### 1. IProjectileインターフェースの作成
**ファイル**: `Assets/Scripts/Combat/IProjectile.cs`

**内容**:
- `bool IsStucking { get; }` プロパティ
- `void SetDirection(bool isLeft)` メソッド
- `void SetDamage(float damage)` メソッド
- `void ImpactAndDestroy()` メソッド

### 2. 既存Projectile.csの修正
**ファイル**: `Assets/Scripts/Combat/Projectile.cs`

**変更内容**:
- クラス宣言に`IProjectile`インターフェースを追加
- 既存の動作は完全に保持（後方互換性維持）
- 既存メソッドがインターフェースの要件を満たすことを確認

### 3. Shield.csの修正
**ファイル**: `Assets/Scripts/Combat/Shield.cs`

**変更内容**:
- `Projectile`型の参照を`IProjectile`型に変更
- 具象クラスへの依存を除去
- 地面に刺さったProjectileの処理ロジックを維持

### 4. WeaponConfig.csの修正
**ファイル**: `Assets/Scripts/Combat/WeaponConfig.cs`

**変更内容**:
- `WeaponLevel`クラスの`projectile`フィールドを`projectilePrefab`（GameObject型）に変更
- `LaunchProjectile`メソッドでIProjectileインターフェース経由でアクセス
- Projectile生成時の音声処理を維持

## 影響範囲
- 既存のPrefabは手動で再設定が必要
- ゲームプレイに影響なし
- 将来の新しいProjectileタイプの追加が容易になる

## 制約事項
- 既存のProjectile PrefabのWeaponConfigでの参照は切れるため、ユーザーが手動で再設定する必要がある
- Unity Editorでの再アタッチ作業が発生する

## テスト方針
- 既存のProjectile機能が正常に動作することを確認
- Shield機能が正常に動作することを確認
- WeaponConfigからのProjectile生成が正常に動作することを確認