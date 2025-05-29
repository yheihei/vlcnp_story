# Akimオーラフラグ制御機能 設計書

## 概要
Akimのレベル3オーラを「敵がいない」フラグの状態に応じて制御する機能の設計書。
単一責任の原則に従い、既存コードとの結合度を最小限に抑えた設計を採用。

## アーキテクチャ設計

### システム構成
```
┌─────────────────┐    ┌─────────────────┐
│   FlagManager   │    │   BaseStats     │
│                 │    │                 │
│ - OnChangeFlag  │    │ - OnChangeLevel │
└─────────┬───────┘    └─────────┬───────┘
          │                      │
          └───────┬──────────────┘
                  │
         ┌────────▼────────┐
         │ AuraController  │
         │                 │
         │ - フラグ監視    │
         │ - レベル監視    │
         │ - PS制御       │
         └─────────────────┘
```

### クラス設計

#### AuraController
**責任**: オーラエフェクトのフラグベース制御
**協調**: 既存の`ChangeSpriteAnimationOnLevelUp`と独立して動作

**主要メソッド**:
- `OnFlagChanged(Flag flag)`: フラグ変更の監視
- `OnLevelChanged(int newLevel)`: レベル変更の監視
- `UpdateAuraState()`: オーラ状態の更新
- `UpdateParticleSystemReference()`: パーティクルシステム参照の更新
- `ForceUpdateAuraState()`: 外部からの強制更新

**設計原則**:
- 単一責任: オーラのフラグ制御のみ
- 低結合: 既存クラスへの変更を最小限に
- 高凝集: 関連機能を一つのクラスに集約

## 実装詳細

### フラグ管理
```csharp
public enum Flag
{
    // ... 既存のフラグ (35個)
    NoEnemies, // 新規追加: 敵がいない
}
```

### オーラ制御ロジック
1. **レベル判定**: レベル3以上でオーラ制御が有効
2. **フラグ判定**: `Flag.NoEnemies`の状態を監視
3. **パーティクル制御**:
   - フラグON時: `ParticleSystem.Stop()`
   - フラグOFF時: `ParticleSystem.Play()`

### イベント駆動設計
- `FlagManager.OnChangeFlag`イベントを監視
- `BaseStats.OnChangeLevel`イベントを監視
- リアルタイムでオーラ状態を更新

## 技術仕様

### 対象Unity版
- Unity 2021.3

### 依存関係
- `UnityEngine`
- `VLCNP.Core` (Flag, FlagManager)
- `VLCNP.Stats` (BaseStats)

### パフォーマンス考慮
- イベント駆動によりフレーム毎の処理を削減
- パーティクルシステムの参照をキャッシュ
- 不要な更新処理を最小化
- タグベース検索によるFlagManager取得の最適化

### 依存性解決
**FlagManager取得**:
```csharp
GameObject flagManagerObject = GameObject.FindWithTag("FlagManager");
if (flagManagerObject != null)
{
    flagManager = flagManagerObject.GetComponent<FlagManager>();
}
```
- `FindObjectOfType`から`FindWithTag`に変更してパフォーマンス向上
- 複数インスタンス存在時の動作安定化
- 適切なnull安全性の実装

## セーブデータ互換性

### フラグ追加による影響
- `Flag.NoEnemies`を末尾に追加
- 既存のセーブデータとの互換性を維持
- 新規フラグのデフォルト値は`false`

### マイグレーション不要
- 新規フラグは任意機能のため
- 既存プレイヤーへの影響なし

## 使用方法

### Unity Editorでの設定
1. **オーラprefab自体**に`AuraController`コンポーネントを追加
2. FlagManagerオブジェクトに"FlagManager"タグを設定
3. 追加の手動設定は不要

### 配置方針の変更
- **旧設計**: プレイヤーオブジェクトに配置
- **新実装**: オーラprefab自体に配置
- **理由**: より直感的で保守しやすい構造

### ParticleSystem取得方法
オーラprefab自体にスクリプトを配置する想定に合わせて簡素化：
```csharp
private void UpdateParticleSystemReference()
{
    // まず自分自身でParticleSystemを検索
    if (auraParticleSystem == null)
    {
        auraParticleSystem = GetComponent<ParticleSystem>();
    }
    
    // 見つからない場合は子オブジェクトから検索
    if (auraParticleSystem == null)
    {
        auraParticleSystem = GetComponentInChildren<ParticleSystem>();
    }
}
```

### コードでの制御
```csharp
// 敵がいない状態にする（オーラ非表示）
flagManager.SetFlag(Flag.NoEnemies, true);

// 敵がいる状態にする（オーラ表示）
flagManager.SetFlag(Flag.NoEnemies, false);
```

## テスト観点

### 機能テスト
- [ ] レベル3未満でのオーラ非表示
- [ ] レベル3以上でのフラグ連動
- [ ] フラグON時のパーティクル停止
- [ ] フラグOFF時のパーティクル再生
- [ ] レベル変更時の正常動作

### 統合テスト
- [ ] 既存のオーラシステムとの協調
- [ ] セーブ/ロード時の状態保持
- [ ] フラグマネージャーとの連携

### パフォーマンステスト
- [ ] イベント処理のオーバーヘッド確認
- [ ] メモリリークの確認

## 今後の拡張性

### 他キャラクターへの適用
- `AuraController`を汎用化
- キャラクター固有の設定を外部化

### 追加フラグ対応
- 他のゲーム状態フラグとの連携
- 複数条件による制御

### エフェクト拡張
- オーラ以外のエフェクトへの適用
- 段階的な制御（フェードイン/アウト等）

## 実装履歴と改善点

### レビューフィードバック対応
1. **FlagManager取得の最適化** (実装済み)
   - `FindObjectOfType`から`FindWithTag`に変更
   - パフォーマンス向上と動作安定化を実現

2. **配置方式の見直し** (実装済み)
   - プレイヤーオブジェクトからオーラprefab自体への配置変更
   - より直感的な構造に改善

3. **ParticleSystem取得の簡素化** (実装予定)
   - 複雑な階層検索から自分自身・子要素検索に変更
   - より堅牢で保守しやすいコードに改善

### 技術的改善点
- null安全性の強化
- タグベースの依存性解決
- より予測可能なコンポーネント検索

## まとめ

本設計により以下を実現:
- ✅ 単一責任の原則に従った設計
- ✅ 既存コードへの影響最小化
- ✅ セーブデータ互換性の維持
- ✅ イベント駆動による効率的な制御
- ✅ 拡張性を考慮した実装
- ✅ パフォーマンス最適化
- ✅ 保守性の向上

レビュープロセスを通じて更に堅牢で効率的な実装に改善され、Akimのレベル3オーラが「敵がいない」フラグに応じて適切に制御される。