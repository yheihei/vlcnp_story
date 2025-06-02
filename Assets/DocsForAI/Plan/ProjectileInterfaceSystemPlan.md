# Projectileシステムのインターフェース化とBouncingProjectile実装計画書

## 概要
現在のProjectileクラスは等速直線運動のみに対応しており、異なる物理的な動きを実装することができません。この問題を解決するため、Projectileシステムを拡張可能な設計にし、BouncingProjectileクラスを実装します。

## 背景
マリオのファイアーボールのようなバウンドする弾や、その他の特殊な動きをする弾を実装するためには、Projectileシステムを拡張可能な設計にする必要があります。

## 現状分析
- 既存の`Assets/Scripts/Combat/IProjectile.cs`インターフェースが存在
- `Assets/Scripts/Combat/Projectile.cs`クラスが等速直線運動を実装
- インターフェースには以下のメソッド・プロパティが定義済み：
  - `bool IsStucking { get; }`
  - `void SetDirection(bool isLeft)`
  - `void SetDamage(float damage)`
  - `void ImpactAndDestroy()`

## 実装手順

### 1. BouncingProjectileクラスの作成
**ファイル**: `Assets/Scripts/Combat/BouncingProjectile.cs`

**基本仕様**:
- `MonoBehaviour`、`IStoppable`、`IProjectile`を実装
- マリオのファイアーボールのような動き：
  - 地面に触れたらバウンドする
  - 重力の影響を受ける
  - バウンド時の反発係数は設定可能
  - 最大バウンド回数の設定

**SerializeFieldパラメータ**:
- `float speed` - 初期速度（既存Projectileと同様）
- `float gravityScale` - 重力倍率（デフォルト2.0）
- `float bounceCoefficient` - バウンド反発係数（デフォルト0.85）
- `int maxBounceCount` - 最大バウンド回数（デフォルト18）
- `GameObject hitEffect` - 衝突エフェクト
- `GameObject destroyEffect` - 消滅エフェクト
- `string targetTagName` - ターゲットタグ（デフォルト"Enemy"）
- `string groundTagName` - 地面タグ（デフォルト"Ground"）
- `float deleteTime` - 自動削除時間

**物理実装**:
- `Rigidbody2D`を使用した重力とバウンドの実装
- 重力値：`-9.8 * gravityScale`
- バウンド処理：`OnCollisionEnter2D`で地面との衝突を検出
- 水平速度は維持、垂直速度のみ反発係数を適用

**既存機能の継承**:
- ダメージ処理（Health.TakeDamage）
- 方向設定（SetDirection）
- エフェクト生成（hitEffect）
- 消滅処理（ImpactAndDestroy）

### 2. 実装の詳細手順
1. BouncingProjectileクラスのスケルトン作成
2. 基本的なIProjectileインターフェース実装
3. 物理計算とバウンド処理の実装
4. ダメージとエフェクト処理の実装
5. テストとデバッグ

## 影響範囲
- 既存のProjectileクラスには影響を与えない
- 新しいBouncingProjectileが追加されるのみ
- ゲームプレイに影響なし

## 制約事項
- Unity 2021.3の物理エンジンを活用
- WebGLビルドでの動作を考慮した軽量実装
- 地面に刺さる機能は不要（PRD要件）

## テスト方針
- BouncingProjectileのバウンド動作が正常に動作することを確認
- ダメージ処理が正常に動作することを確認
- エフェクト生成が正常に動作することを確認
- 最大バウンド回数での消滅が正常に動作することを確認