# Story-based Enemy Strengthening System - Implementation Plan

## 概要
物語の進行に応じて敵を強くするシステムの実装計画書。
特定のフラグが立った数に応じて、敵の武器ダメージを段階的に上げる。

## 技術的アプローチ

### 1. 新しいコンポーネントの作成
`FlagBasedEnemyStrengthening.cs` を作成し、以下の機能を実装：

- **フラグ条件の管理**: 強化条件となるフラグのリストを保持
- **自動レベルアップ**: フラグ数に応じて経験値を自動付与
- **既存システムとの連携**: Experience/BaseStats/Progressionと統合

### 2. 実装詳細

#### コンポーネント設計
```csharp
public class FlagBasedEnemyStrengthening : MonoBehaviour
{
    [SerializeField] Flag[] strengtheningFlags;  // 強化条件フラグリスト
    [SerializeField] float experiencePerFlag = 1f;  // フラグ1つあたりの経験値
    
    private FlagManager flagManager;
    private Experience experience;
    private int lastFlagCount = 0;
}
```

#### 動作フロー
1. **初期化時**: 現在のフラグ状態をチェックし、必要な経験値を設定
2. **フラグ変更時**: FlagManagerのイベントを監視し、新しいフラグが立ったら経験値を追加
3. **経験値管理**: `Experience.SetExperiencePointsIfGreater()` を使用して安全に経験値設定

### 3. 既存システムとの統合

#### 利用する既存コンポーネント
- `Experience`: 経験値管理（既存のSetExperiencePointsIfGreaterメソッド活用）
- `BaseStats`: レベル計算とWeaponDamage取得
- `Progression`: 各レベルでの武器ダメージ値を定義
- `FlagManager`: フラグ状態の監視

#### Unity Editor での設定
- 敵Prefabに `FlagBasedEnemyStrengthening` をアタッチ
- 強化対象フラグを Inspector で設定
- フラグ1つあたりの経験値量を調整可能

### 4. 実装手順
1. `FlagBasedEnemyStrengthening.cs` の作成
2. 必要なテストケースの確認
3. Unity Editor での動作確認

### 5. 設計原則の遵守
- **単一責任の原則**: フラグベースの強化のみを担当
- **既存システム活用**: Experience/BaseStats を再利用
- **設定の柔軟性**: Inspector でフラグリストを調整可能
- **パフォーマンス**: フラグ変更時のみ処理実行

## 期待される動作
1. ボス撃破などでフラグが立つ
2. 該当する敵の経験値が自動的に増加
3. BaseStatsによりレベルが再計算される
4. Progressionから新しいWeaponDamageが取得される
5. 敵の攻撃力が段階的に向上

## テストケース
- フラグが1つ立った時の経験値増加
- 複数フラグが同時に立った時の処理
- セーブ/ロード時の状態復元
- Experience コンポーネントがない敵での動作