# 物語進行に応じた敵強化システム実装計画

## 概要
物語の進行状況（フラグ）に応じて、敵の攻撃力を段階的に向上させるシステムを実装する。

## PRD要件の分析

### 目的
- 特定のフラグリストに基づいて敵の強さを調整
- フラグが立った数に応じて段階的に強化
- 攻撃力で敵の強さを表現

### 技術的制約
- 単一責任の原則を遵守
- Unity Editor上でのコンポーネントアタッチはユーザーが実施
- 既存システムとの適切な統合

### 提案されたアプローチ
- Experienceコンポーネントとフラグ連携
- 初期化時にフラグ数に応じた経験値付与
- Progressionシステムでレベル管理
- BaseStatsとWeaponConfigの連携

## 既存システム分析

### 関連コンポーネント
1. **Flag.cs**: 33種類の物語進行フラグ
2. **FlagManager.cs**: フラグ状態管理、イベント通知
3. **Experience.cs**: 経験値管理、レベル更新トリガー
4. **BaseStats.cs**: レベル計算、ステータス管理
5. **Fighter.cs**: 直接攻撃・間接攻撃処理
6. **WeaponConfig.cs**: レベル別武器性能
7. **Progression.cs**: レベル別ステータス定義

### システム連携フロー
```
Flag状態 → EnemyScaler → Experience値調整 → BaseStats.GetLevel() → WeaponConfig.GetDamage()
```

## 実装計画

### Phase 1: コアシステム設計
#### 1.1 EnemyScaler コンポーネント
- 責任: フラグ監視と経験値調整
- 機能:
  - 指定されたフラグリストの監視
  - フラグ数に応じた経験値計算
  - Experience コンポーネントへの値設定

#### 1.2 ScalableEnemyData ScriptableObject
- 責任: 敵スケーリング設定データ
- 機能:
  - 監視対象フラグリスト定義
  - フラグ数と経験値のマッピング

### Phase 2: 実装詳細

#### 2.1 EnemyScaler.cs
```csharp
public class EnemyScaler : MonoBehaviour
{
    [SerializeField] private ScalableEnemyData scalingData;
    private Experience experience;
    private FlagManager flagManager;
    
    private void Awake()
    {
        experience = GetComponent<Experience>();
        flagManager = FindObjectOfType<FlagManager>();
    }
    
    private void Start()
    {
        ApplyScaling();
        // フラグ変更時の再評価登録
        flagManager.OnChangeFlag += OnFlagChanged;
    }
    
    private void ApplyScaling()
    {
        int flagCount = CountActivatedFlags();
        float experienceToAdd = scalingData.GetExperienceForFlagCount(flagCount);
        experience.SetExperiencePointsIfGreater(experienceToAdd);
    }
}
```

#### 2.2 ScalableEnemyData.cs
```csharp
[CreateAssetMenu(fileName = "ScalableEnemyData", menuName = "RPG/Enemy Scaling Data")]
public class ScalableEnemyData : ScriptableObject
{
    [SerializeField] private Flag[] watchedFlags;
    [SerializeField] private float[] experiencePerFlagCount;
    
    public float GetExperienceForFlagCount(int flagCount)
    {
        // 配列範囲内でのインデックス調整
        int index = Mathf.Clamp(flagCount, 0, experiencePerFlagCount.Length - 1);
        return experiencePerFlagCount[index];
    }
}
```

### Phase 3: 統合とテスト

#### 3.1 既存システムとの統合確認
- Progression.cs の ExperienceToLevelUp 設定
- WeaponConfig.cs のレベル別ダメージ設定
- BaseStats.cs のレベル計算ロジック

#### 3.2 エディター設定フロー
1. ScalableEnemyData アセット作成
2. 監視フラグリスト設定
3. フラグ数別経験値配列設定
4. 敵プレハブに EnemyScaler アタッチ
5. データアセット参照設定

## リスク分析

### 技術的リスク
1. **パフォーマンス**: フラグ変更時の全敵再評価
   - 対策: 効率的なフラグ監視、必要時のみ更新

2. **データ整合性**: フラグと経験値のマッピング不整合
   - 対策: エディター検証、デフォルト値設定

3. **セーブデータ互換性**: 既存セーブデータへの影響
   - 対策: Experience システムの既存実装活用

### 設計リスク
1. **責任分散**: 複数コンポーネント間の責任境界
   - 対策: 明確な責任分離、インターフェース設計

## 成功条件

### 機能要件
- [ ] フラグ数に応じた敵レベル調整
- [ ] リアルタイムでのスケーリング反映
- [ ] エディターでの設定変更対応

### 非機能要件
- [ ] 既存システムへの影響最小化
- [ ] パフォーマンス劣化なし
- [ ] セーブデータ互換性維持

## 次ステップ
1. EnemyScaler コンポーネント実装
2. ScalableEnemyData ScriptableObject 実装
3. 統合テスト
4. エディター用サンプル設定作成
5. 設計ドキュメント作成