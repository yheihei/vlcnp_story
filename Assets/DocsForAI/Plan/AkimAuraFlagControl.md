# Akimオーラフラグ制御機能 実装計画

## 概要
Akimがレベル3のときに表示されるオーラを、「敵がいない」フラグの状態に応じて制御する機能を実装する。

## 要件分析

### 機能要件
- 「敵がいない」フラグがONの時：オーラのParticle Systemを停止
- 「敵がいない」フラグがOFFの時：オーラのParticle Systemを再開
- レベル3の場合のみ制御対象とする
- 単一責任の原則を守った設計

### 技術要件
- `Assets/Scripts`配下にコードを配置
- 適切なnamespaceを使用
- Unity Editor上での手動設定は最小限に

## 現在の実装構造

### Flag.cs
- 35個のフラグが定義済み
- セーブデータ互換性のため順番変更不可
- 末尾に新しいフラグを追加する必要がある

### FlagManager.cs
- フラグ設定・取得機能
- `OnChangeFlag`イベントでフラグ変更通知
- セーブ/ロード対応

### ChangeSpriteAnimationOnLevelUp.cs
- レベルアップに応じたスプライト変更
- `auraEffect`フィールドでオーラエフェクト管理
- `UpdateAuraEffect`メソッドでオーラ制御

## 実装方針

### 1. フラグ追加
- `Flag.cs`の末尾に`NoEnemies`フラグを追加
- セーブデータ互換性を維持

### 2. オーラ制御クラス設計
新しいクラス`AuraController`を作成：
- 単一責任：オーラの表示/非表示制御のみ
- フラグ変更イベントを監視
- Particle Systemの制御を担当

### 3. 統合方法
`ChangeSpriteAnimationOnLevelUp`との協調：
- レベル3判定は既存クラスで実施
- フラグベースの制御は新しいクラスで実施
- 両方の条件を満たした場合のみオーラを表示

## 実装ステップ

1. **Flag.cs修正**
   - `NoEnemies`フラグを末尾に追加

2. **AuraController.cs作成**
   - フラグ監視機能
   - Particle System制御機能
   - レベル3判定との連携

3. **ChangeSpriteAnimationOnLevelUp.cs修正**
   - AuraControllerとの連携
   - オーラ制御ロジックの分離

4. **テスト考慮事項**
   - フラグON/OFF時の動作確認
   - レベル変更時の動作確認
   - セーブ/ロード時の状態保持確認

## 期待される成果物

- Flag.cs（NoEnemiesフラグ追加）
- AuraController.cs（新規作成）
- ChangeSpriteAnimationOnLevelUp.cs（修正）
- 設計文書（Assets/DocsForAI/Design/配下）

## リスク・制約

- セーブデータ互換性の維持
- 既存のオーラ制御との競合回避
- Unity Editor上での設定の最小化