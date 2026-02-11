# 物語進行に応じた敵強化システム設計書

## システム概要
物語の進行状況（フラグ）に基づいて敵の攻撃力を段階的に強化するシステムです。プレイヤーの進行度に応じて敵が強くなることで、適切な難易度調整を実現します。

## アーキテクチャ

### システム構成図
```
FlagManager (フラグ状態管理)
     ↓ OnChangeFlag イベント
EnemyScaler (フラグ監視・経験値調整)
     ↓ SetExperiencePointsIfGreater
Experience (経験値管理)
     ↓ onExperienceGained イベント
BaseStats (レベル計算)
     ↓ GetLevel()
Fighter (攻撃処理)
     ↓ GetDamage(level)
WeaponConfig (レベル別ダメージ設定)
```

### 責任分離
- **ScalableEnemyData**: 設定データの管理（監視フラグ、経験値マッピング）
- **EnemyScaler**: フラグ監視とレベル調整ロジック
- **Experience**: 経験値の管理とレベル更新トリガー
- **BaseStats**: レベル計算とステータス提供
- **WeaponConfig**: レベル別攻撃力定義

## コンポーネント詳細

### ScalableEnemyData (ScriptableObject)

#### 責任
- 敵スケーリング設定データの保持
- フラグと経験値のマッピング定義
- エディター上でのバランス調整

#### 主要プロパティ
```csharp
[SerializeField] private Flag[] watchedFlags;           // 監視対象フラグリスト
[SerializeField] private float[] experiencePerFlagCount; // フラグ数別経験値配列
```

#### 主要メソッド
- `GetWatchedFlags()`: 監視対象フラグの取得
- `GetExperienceForFlagCount(int)`: フラグ数に対応する経験値の取得
- `GetMaxFlagCount()`: 最大フラグ数の取得

#### 設計思想
- Unity Editor上での直感的な設定を重視
- 配列インデックス範囲外エラーの防止
- OnValidate による設定チェック機能

### EnemyScaler (MonoBehaviour)

#### 責任
- フラグ状態の監視
- 経験値の動的調整
- 既存Experience システムとの連携

#### ライフサイクル
1. **Awake**: Experience コンポーネント取得
2. **Start**: FlagManager検索、初期スケーリング適用、イベント登録
3. **OnFlagChanged**: 監視フラグ変更時の再評価
4. **OnDestroy**: イベント登録解除

#### 主要機能
- **フラグ監視**: 指定されたフラグのみに反応する効率的な監視
- **経験値調整**: SetExperiencePointsIfGreater による安全な経験値設定
- **デバッグサポート**: 詳細なログ出力とステータス確認機能

#### エラーハンドリング
- Experience コンポーネント必須チェック
- FlagManager 存在確認
- スケーリングデータnullチェック
- 重複初期化防止

## 統合設計

### 既存システムとの連携

#### Experience システム
- `SetExperiencePointsIfGreater()` を使用して既存経験値より高い場合のみ更新
- `onExperienceGained` イベントでBaseStatsのレベル再計算をトリガー

#### BaseStats システム
- `GetLevel()` で現在レベルを計算
- Progression システムと連携してレベル上限を管理

#### 戦闘システム
- Fighter.cs の直接攻撃 (`DirectAttack`)
- WeaponConfig の間接攻撃 (`LaunchProjectile`)
- どちらも BaseStats.GetLevel() を使用してダメージ計算

### セーブシステム互換性
- Experience コンポーネントが既にセーブシステムに統合済み
- フラグ変更時の自動調整により、ロード後も適切な状態を維持

## 使用方法

### セットアップ手順
1. **ScalableEnemyData アセット作成**
   - Project ウィンドウで右クリック → Create → RPG → Enemy Scaling Data

2. **フラグ設定**
   - Watched Flags に監視したいフラグを配列で設定
   - 例: [VeryEnemyAnimalsBossDefeated, DragDefeated, AkimVeryLong]

3. **経験値マッピング設定**
   - Experience Per Flag Count に経験値を配列で設定
   - インデックス 0: フラグ0個時の経験値
   - インデックス 1: フラグ1個時の経験値
   - インデックス n: フラグn個時の経験値

4. **敵プレハブ設定**
   - 敵GameObject に EnemyScaler コンポーネント追加
   - Scaling Data に作成したアセットを設定
   - Experience コンポーネントが必要（RequireComponent で自動追加）

### 設定例
```
監視フラグ: [VeryEnemyAnimalsBossDefeated, DragDefeated, AkimVeryLong]
経験値設定: [0, 1, 2, 3]
結果:
- フラグ0個: 経験値0 (レベル1)
- フラグ1個: 経験値1 (レベル2) 
- フラグ2個: 経験値2 (レベル3)
- フラグ3個: 経験値3 (レベル4)
```

## パフォーマンス考慮

### 最適化ポイント
- **イベント駆動**: フラグ変更時のみ処理実行
- **選択的監視**: 指定フラグのみに反応
- **遅延初期化**: Start() での初期化によりシーン構築完了後に実行
- **キャッシュ活用**: FlagManager参照をキャッシュ

### リソース使用量
- メモリ: ScriptableObject による設定データ共有
- CPU: フラグ変更時の線形検索（O(n)、nは監視フラグ数）
- ネットワーク: 影響なし（ローカル処理のみ）

## 拡張性

### 将来の拡張可能性
1. **複数スケーリング設定**: 敵種別ごとの異なるスケーリング
2. **非線形スケーリング**: フラグ組み合わせに基づく複雑な計算
3. **プレイヤーレベル考慮**: プレイヤーレベルとの相対的なスケーリング
4. **動的バランス調整**: プレイデータに基づく自動調整

### インターフェース設計
現在の設計はScriptableObjectパターンを使用しており、設定の外部化と変更が容易です。

## テスト戦略

### 単体テスト
- ScalableEnemyData のフラグ数計算
- EnemyScaler のフラグカウント機能
- 境界値テスト（フラグ数0、最大値）

### 統合テスト
- フラグ変更→経験値更新→レベル変更→ダメージ変更のフロー
- 複数敵での同時スケーリング
- セーブ・ロード時の状態復元

### シナリオテスト
- ゲーム進行に応じた段階的な難易度上昇
- エディターでの設定変更の即座反映
- デバッグモードでの状態確認

## 注意事項

### 設定時の注意
- Experience Per Flag Count 配列は (監視フラグ数 + 1) 以上の要素が必要
- フラグの順序変更はセーブデータに影響する可能性
- Progression.cs のレベル設定と整合性を保つ必要

### 運用時の注意
- Debug Mode は開発時のみ有効化
- フラグ変更頻度が高い場合のパフォーマンス影響
- レベル上限値の適切な設定

## 関連ドキュメント
- PRD: Issue #523
- Plan: Assets/DocsForAI/Plan/StoryProgressionEnemyScaling.md
- 既存システム: Experience.cs, BaseStats.cs, Fighter.cs, WeaponConfig.cs