# Story-based Enemy Strengthening System - Design Document

## 概要
物語の進行に応じて敵を自動的に強化するシステムの設計書。
特定のフラグが立った数に応じて、敵の武器ダメージを段階的に向上させる。

## システム設計

### 1. アーキテクチャ
```
FlagManager (既存)
    ↓ フラグ変更イベント
FlagBasedEnemyStrengthening (新規)
    ↓ 経験値付与
Experience (既存)
    ↓ レベル計算
BaseStats (既存)
    ↓ WeaponDamage取得
Progression (既存)
```

### 2. 新規コンポーネント: FlagBasedEnemyStrengthening

#### クラス設計
```csharp
public class FlagBasedEnemyStrengthening : MonoBehaviour
{
    [SerializeField] Flag[] strengtheningFlags;     // 強化条件フラグ
    [SerializeField] float experiencePerFlag = 1f;  // フラグあたりXP量
    
    private FlagManager flagManager;
    private Experience experience;
    private int lastFlagCount = 0;
}
```

#### 主要メソッド
- `InitializeEnemyStrength()`: 初期化時にフラグ状態をチェック
- `OnFlagChanged()`: フラグ変更時の処理
- `GetActiveFlagCount()`: アクティブなフラグ数を計算

### 3. 動作フロー

#### 初期化フェーズ
1. Awake: Experience コンポーネントの取得
2. Start: FlagManager の取得とイベント登録
3. InitializeEnemyStrength: 現在のフラグ状態に基づく経験値設定

#### ランタイムフェーズ
1. フラグ変更検知 (FlagManager.OnChangeFlag)
2. 対象フラグかチェック
3. フラグ数増加時のみ経験値付与
4. Experience.SetExperiencePointsIfGreater() で安全な経験値設定

### 4. データフロー

#### 強化プロセス
```
ボス撃破 → Flag設定 → FlagManager.OnChangeFlag発火 
→ FlagBasedEnemyStrengthening.OnFlagChanged 
→ Experience.SetExperiencePointsIfGreater 
→ BaseStats.UpdateLevel → WeaponDamage更新
```

#### 経験値計算
```
必要経験値 = アクティブフラグ数 × experiencePerFlag
```

### 5. 設定とカスタマイズ

#### Unity Inspector設定
- **Strengthening Flags**: 強化条件となるフラグリスト
- **Experience Per Flag**: フラグ1つあたりの経験値量

#### 推奨設定例
```
強化対象フラグ:
- VeryEnemyAnimalsBossDefeated (ベリーエネミーアニマルズボス撃破)
- DragDefeated (ドラァグクイーン撃破)
- OhirunebeyaBlockBroken (お昼寝部屋落盤破壊)

経験値設定:
- 1フラグ = 1XP (レベル2に上がる)
- 2フラグ = 2XP (レベル3に上がる)
```

### 6. パフォーマンス考慮

#### 最適化ポイント
- フラグ変更時のみ処理実行（常時監視なし）
- SetExperiencePointsIfGreater で無駄な経験値設定を回避
- Array.IndexOf でフラグ一致チェック（O(n)だが設定フラグ数は少数）

#### メモリ使用量
- フラグ配列: 小さな参照配列
- イベントリスナー: FlagManager への1つのサブスクライブ
- 状態変数: int型のlastFlagCountのみ

### 7. エラーハンドリング

#### 必須コンポーネントチェック
- Experience コンポーネント不在時: Warning ログ出力
- FlagManager 不在時: Error ログ出力、機能停止

#### Null安全性
- flagManager null チェック
- experience null チェック
- strengtheningFlags null チェック

### 8. 拡張性

#### 将来の拡張ポイント
1. **複雑な条件**: フラグの組み合わせ条件
2. **異なる強化方式**: Health や他のStatの強化
3. **段階的強化**: フラグごとに異なる経験値量
4. **地域別強化**: シーンやエリア別の強化設定

#### アーキテクチャの柔軟性
- 既存システム(Experience/BaseStats)への依存のみ
- 新しい強化方式への拡張が容易
- Inspector での設定変更が簡単

### 9. 使用方法

#### 開発者向け手順
1. 敵Prefabに `FlagBasedEnemyStrengthening` をアタッチ
2. Inspector で強化対象フラグを選択
3. フラグあたりの経験値量を設定
4. Progression ScriptableObject でレベル別WeaponDamageを調整

#### 動作確認方法
1. Debug.Log でフラグ変更とXP付与を確認
2. Inspector でExperienceコンポーネントの値変化を監視
3. BaseStats.GetStat(Stat.WeaponDamage) でダメージ値確認

## 技術的利点

### 既存システムとの親和性
- Experience/BaseStats の仕組みを完全活用
- フラグシステムとの自然な統合
- ScriptableObject(Progression) による調整の柔軟性

### 保守性
- 単一責任の原則を遵守
- 既存コードの変更なし
- 明確なイベント駆動型設計

### テスタビリティ
- 公開メソッドによる状態確認
- イベントベースの分離されたロジック
- Inspector での設定確認が容易