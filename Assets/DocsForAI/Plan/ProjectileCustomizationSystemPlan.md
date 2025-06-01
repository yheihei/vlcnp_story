# 射出体カスタマイズシステム実装計画書

## 概要
既存の`Projectile.cs`を継承ベースの設計に変更し、様々な移動パターンと衝突挙動をカスタマイズ可能にする。

## 現在の問題点
- すべての射出体が同じ動作（直線移動）をする
- バウンドする弾などの特殊な挙動が実装できない
- 新しい射出体タイプの追加が困難

## 設計方針

### 1. 抽象クラス `ProjectileBase` の作成
既存の`Projectile.cs`の共通機能を抽象クラスとして分離：

#### 共通フィールド（現在のProjectile.csから移行）
- `speed`: 移動速度
- `hitEffect`: ヒットエフェクト
- `deleteTime`: 削除時間
- `targetTagName`: ターゲットタグ
- `groundTagName`: 地面タグ
- `IsPenetration`: 貫通機能
- `isStuckInGround`: 地面刺さり機能
- `isBreakOnGround`: 地面衝突破壊機能
- `isFadeOut`: フェードアウト機能
- `damage`: ダメージ値
- `isLeft`: 左向きフラグ
- `isStopped`: 停止フラグ

#### 抽象メソッド
- `HandleMovement()`: 移動処理（各実装クラスで定義）
- `HandleCollision(Collider2D other)`: 衝突処理（各実装クラスで定義）

#### 共通メソッド
- `SetDirection(bool isLeft)`: 方向設定
- `SetDamage(float damage)`: ダメージ設定
- `ImpactAndDestroy()`: 衝突時破壊処理
- `FadeOut()`: フェードアウト処理
- `StuckInGround()`: 地面刺さり処理

### 2. 具体実装クラス

#### `LinearProjectile`
- 現在の`Projectile.cs`の挙動を継承
- 直線移動のみ
- 既存のPrefabとの互換性を保つ

#### `BounceProjectile`
- 地面でバウンドする弾丸
- X軸方向は等速移動
- Y軸方向は重力と反発を模擬
- 壁にあたったら反対方向に移動

### 3. 実装順序
1. `ProjectileBase.cs`の作成
2. `LinearProjectile.cs`の作成（既存機能移行）
3. `BounceProjectile.cs`の実装
4. 既存Prefabの更新検討（必要に応じて）

## ファイル構成
```
Assets/Scripts/Combat/
├── ProjectileBase.cs          # 新規作成
├── LinearProjectile.cs        # 新規作成
├── BounceProjectile.cs        # 新規作成
└── Projectile.cs              # 既存（将来的に削除候補）
```

## 既存コードとの互換性
- `LinearProjectile`は既存の`Projectile`と同じインターフェースを提供
- 段階的な移行により既存システムへの影響を最小化
- `IStoppable`インターフェースは継承

## 今後の拡張性
- 新しい射出体タイプの追加が容易
- 各実装クラスで独自のパラメータ追加可能
- Unity Editorでの設定も個別に調整可能