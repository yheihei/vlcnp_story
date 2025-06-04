# オロチ毒玉実装計画

## 概要
特定の武器のProjectileで敵にダメージを与えた時に毒を付与する機能を実装する。

## 実装方針

### 1. 敵用毒ステータスクラスの作成
- **クラス名**: `EnemyPoisonStatus.cs`
- **継承元**: MonoBehaviour
- **主要機能**:
  - 毒の継続時間管理
  - 一定時間ごとのダメージ処理（HPは1まで）
  - 毒開始・終了イベントの発行
  - PoisonEffectControllerとの連携

### 2. 毒付与可能なProjectileインターフェースの作成
- **インターフェース名**: `IPoisonous.cs`
- **メソッド**:
  - `bool IsPoisonous { get; }`
  - `float GetPoisonDamage()`
  - `float GetPoisonDuration()`
  - `float GetPoisonInterval()`

### 3. 毒付与Projectileクラスの作成
- **クラス名**: `PoisonProjectile.cs`
- **継承**: Projectile, IPoisonous
- **機能**: 
  - 通常のProjectile機能
  - 毒付与パラメータの設定
  - OnTriggerEnter2Dで毒付与処理

### 4. 既存システムとの統合
- Projectile.csのOnTriggerEnter2Dメソッドを仮想化
- PoisonProjectileでオーバーライドして毒付与処理を追加
- Health.csと連携して毒ダメージを与える

## 実装手順

### Step 1: 基本クラスの作成
1. `EnemyPoisonStatus.cs`を作成
   - 毒の状態管理
   - 定期ダメージ処理（コルーチン使用）
   - イベント発行

2. `IPoisonous.cs`インターフェースを作成
   - 毒関連のプロパティ定義

### Step 2: Projectileの拡張
1. `Projectile.cs`のOnTriggerEnter2Dをprotected virtualに変更
2. `PoisonProjectile.cs`を作成
   - Projectileを継承
   - IPoisonousを実装
   - 敵ヒット時に毒付与

### Step 3: エフェクトとの連携
- 既存のPoisonEffectControllerを活用
- EnemyPoisonStatusのイベントと連携

### Step 4: Unity Editor設定用の準備
- SerializeFieldでパラメータを公開
- インスペクターから調整可能に

## 考慮事項

### パフォーマンス
- 毒の定期ダメージはコルーチンで実装
- 同時に多数の敵が毒状態になることを想定

### 拡張性
- 将来的に他の状態異常（麻痺、凍結など）を追加しやすい設計
- インターフェースベースで実装

### エラーハンドリング
- 必要なコンポーネントが無い場合の処理
- 毒の重複付与の制御

## テスト項目
1. 毒付与Projectileが敵にヒットした時、毒が付与されるか
2. 毒ダメージが一定間隔で与えられるか
3. HPが1より下がらないか
4. 毒が一定時間で治るか
5. 毒エフェクトが正しく表示されるか
6. 通常のProjectileには影響がないか

## Unity Editor上での設定手順（ユーザー向け）
1. PoisonProjectileプレファブを作成
2. EnemyPoisonStatusを敵キャラクターにアタッチ
3. PoisonEffectControllerを敵キャラクターにアタッチ
4. WeaponConfigでPoisonProjectileを武器に設定