# Akimレベル3オーラ制御機能設計書

## 概要
Akimがレベル3のときに表示されるオーラを、「敵がいない」フラグの状態に応じて動的に制御する機能の設計書。

## 実装されたコンポーネント

### 1. Flag.cs の拡張
- 新しいフラグ `NoEnemies` を追加
- 既存のセーブデータとの互換性を保つため、enum の末尾に追加

### 2. AuraController クラス
**ファイル**: `Assets/Characters/Player/Script/AuraController.cs`

**責任**:
- オーラのParticle Systemの表示/非表示を制御
- FlagManagerのフラグ変更イベントを監視
- NoEnemiesフラグの状態に応じてパーティクルシステムを制御

**主要メソッド**:
- `Initialize()`: 初期化処理（ParticleSystem取得、FlagManager取得、イベント監視開始）
- `OnFlagChanged()`: フラグ変更イベントハンドラ
- `UpdateAuraVisibility()`: オーラの表示状態を更新
- `ForceUpdateVisibility()`: 外部からの強制更新用メソッド

**設計の特徴**:
- 単一責任の原則に従い、オーラ制御のみに特化
- エラーハンドリングを含む堅牢な実装
- メモリリークを防ぐためのイベント購読解除

### 3. PlayerLevel.cs の修正
**変更点**:
- `instantiateAura()` メソッドでAuraControllerコンポーネントを自動追加
- オーラ制御ロジックをAuraControllerに委譲

## 動作フロー

1. **オーラ生成時**:
   - PlayerLevelがレベル3になる
   - `instantiateAura()` でオーラオブジェクトが生成される
   - 自動的にAuraControllerコンポーネントが追加される

2. **フラグ監視**:
   - AuraControllerがFlagManagerのOnChangeFlagイベントを監視
   - NoEnemiesフラグの変更を検知

3. **オーラ制御**:
   - NoEnemies = true → パーティクルシステム停止
   - NoEnemies = false → パーティクルシステム再生

## Unity Editor での設定

### 前提条件
- オーラPrefabにParticleSystemコンポーネントが含まれていること
- FlagManagerがシーンに存在すること

### 自動設定される項目
- AuraControllerコンポーネントの追加（コードで自動実行）
- ParticleSystemコンポーネントの参照取得（コードで自動実行）
- FlagManagerの参照取得（コードで自動実行）

### 手動設定が必要な項目
なし（全て自動で設定される）

## 使用方法

### フラグの設定
```csharp
// 敵がいない状態にする
flagManager.SetFlag(Flag.NoEnemies, true);

// 敵がいる状態にする
flagManager.SetFlag(Flag.NoEnemies, false);
```

### 強制更新（必要に応じて）
```csharp
// オーラオブジェクトのAuraControllerから強制更新
auraController.ForceUpdateVisibility();
```

## エラーハンドリング

- ParticleSystemが見つからない場合のエラーログ出力
- FlagManagerが見つからない場合のエラーログ出力
- コンポーネント破棄時のイベント購読解除
- 初期化前の処理呼び出しに対する安全な処理

## 拡張性

### 将来的な拡張ポイント
1. 他のフラグによるオーラ制御の追加
2. オーラのエフェクトレベル調整
3. フェードイン・フェードアウトアニメーション
4. 音響エフェクトの同期制御

### 設計の利点
- 単一責任の原則により、他のオーラ関連機能の追加が容易
- イベント駆動設計により、フラグシステムとの疎結合を実現
- エラーハンドリングにより運用時の安定性を確保