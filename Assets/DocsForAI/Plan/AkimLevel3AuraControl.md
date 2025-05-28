# Akimレベル3オーラ制御機能の実装プラン

## 概要
Akimがレベル3のときに表示されるオーラを、「敵がいない」フラグの状態に応じて制御する機能を実装する。

## 要件
1. 「敵がいない」フラグがONのとき、オーラのparticle systemを停止する
2. 「敵がいない」フラグがOFFのとき、オーラのparticle systemを再開する
3. 単一責任の原則を守った設計にする

## 実装計画

### 1. フラグの追加
- `Assets/Scripts/Core/Flag.cs`に`NoEnemies`フラグを追加
- 既存のセーブデータ互換性のため、末尾に追加

### 2. オーラ制御クラスの作成
- `AuraController`クラスを新規作成
- 責任：オーラのParticle Systemの制御
- FlagManagerからのイベントを監視
- PlayerLevelクラスから独立させて単一責任の原則を守る

### 3. PlayerLevelクラスの修正
- オーラ制御ロジックを`AuraController`に委譲
- オーラインスタンス作成時に`AuraController`をアタッチ

### 4. 実装の流れ
1. Flag.csに新しいフラグを追加
2. AuraControllerクラスを作成
3. PlayerLevel.csを修正してAuraControllerを使用
4. Unity Editorでの設定方法をドキュメント化

## 技術的詳細

### AuraControllerの設計
```csharp
// AuraController
- ParticleSystem参照を保持
- FlagManagerのOnChangeFlagイベントを監視
- NoEnemiesフラグの変更に応じてParticleSystemの再生/停止を制御
```

### PlayerLevelクラスの変更点
- instantiateAura()メソッド内でAuraControllerコンポーネントを追加
- オーラの制御ロジックはAuraControllerに移譲