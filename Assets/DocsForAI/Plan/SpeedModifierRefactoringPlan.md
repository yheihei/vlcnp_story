# SpeedModifierリファクタリング計画

## 背景と目的

現在の実装では、各EnemyActionクラスがISpeedModifiableインターフェースを実装する必要があり、保守性と拡張性に課題があります。SpeedModifierコンポーネントを導入することで、より疎結合で柔軟なシステムに改善します。

## 現在の設計の課題

1. **コード重複**: 各EnemyActionクラスに同じISpeedModifiable実装が必要
2. **保守性**: 新しいEnemyActionクラス追加時に毎回実装が必要
3. **密結合**: EnemyActionクラスが状態効果システムを直接意識する設計

## 新しい設計の利点

1. **単一責任の原則**: SpeedModifierが速度倍率管理に特化
2. **疎結合**: EnemyActionクラスとStatusEffectシステムが独立
3. **拡張性**: 新しいEnemyActionクラスが自動的に対応
4. **Unity Editor統合**: コンポーネントベースで視覚的に設定可能

## 実装計画

### フェーズ1: SpeedModifierコンポーネント

#### 1.1 SpeedModifier.cs（完了済み）
- 速度倍率を保持・管理
- 基準速度から実効速度を計算するメソッド提供
- デバッグ機能とUnity Editor統合

### フェーズ2: EnemyActionクラスの改修

#### 2.1 基底クラスの拡張検討
```csharp
// EnemyAction.csに以下のメソッドを追加
protected float GetModifiedSpeed(float baseSpeed)
{
    SpeedModifier modifier = GetComponent<SpeedModifier>();
    if (modifier != null)
    {
        return modifier.CalculateModifiedSpeed(baseSpeed);
    }
    return baseSpeed;
}
```

#### 2.2 各EnemyActionクラスの修正方針
- ISpeedModifiable実装を削除
- speedModifierフィールドを削除
- GetCurrentSpeed()の代わりにGetModifiedSpeed()を使用

### フェーズ3: 状態効果システムの改修

#### 3.1 ParalysisStatusEffect
- ISpeedModifiable[]の代わりにSpeedModifierを使用
- ApplyParalysisメソッドのシグネチャ変更

#### 3.2 ParalysisStatusController
- SpeedModifierの参照を保持
- 効果適用・解除時にSetModifier/ResetModifierを呼び出し

### フェーズ4: 移行戦略

#### 4.1 段階的移行
1. SpeedModifierコンポーネント作成（完了）
2. EnemyAction基底クラスにヘルパーメソッド追加
3. 各EnemyActionクラスを順次修正
   - Moving.cs
   - RepeatMoving.cs
   - RandomFlyToPlayer.cs
   - その他のクラス
4. ParalysisStatusEffectをSpeedModifier対応に変更
5. ISpeedModifiableインターフェースの削除

#### 4.2 互換性維持
- 移行期間中は両方のアプローチが共存可能
- SpeedModifierがない場合は通常速度で動作

## 実装詳細

### EnemyActionクラスの修正例

#### 修正前（Moving.cs）
```csharp
float currentSpeed = GetCurrentSpeed();
UpdateMoveSpeed(position.x < transform.position.x ? -currentSpeed : currentSpeed);
```

#### 修正後（Moving.cs）
```csharp
float modifiedSpeed = GetModifiedSpeed(speed);
UpdateMoveSpeed(position.x < transform.position.x ? -modifiedSpeed : modifiedSpeed);
```

### Unity Editor上での使用方法

1. **敵プレファブ設定**
   - SpeedModifierコンポーネントを追加
   - EnemyStatusManager（オプション）を追加
   - StatusEffectVisualController（オプション）を追加

2. **動作確認**
   - SpeedModifierが存在する場合：速度倍率が適用される
   - SpeedModifierが存在しない場合：通常速度で動作

## テスト計画

1. **単体テスト**
   - SpeedModifier単体での倍率計算
   - 各EnemyActionクラスのSpeedModifier参照

2. **統合テスト**
   - ParalysisStatusEffectによる速度変更
   - 複数の敵タイプでの動作確認

3. **後方互換性テスト**
   - SpeedModifierなしでの通常動作
   - 既存シーンでの動作確認

## リスクと対策

1. **リスク**: 既存のプレファブへの影響
   - **対策**: SpeedModifierがオプショナルな設計

2. **リスク**: パフォーマンスへの影響
   - **対策**: GetComponent呼び出しの最適化（キャッシュ）

3. **リスク**: 移行期間中の混乱
   - **対策**: 明確なドキュメントと段階的移行

## 期待される成果

- より保守性の高いコード
- 新しいEnemyActionクラスへの自動対応
- Unity Editorでの直感的な設定
- 将来の拡張（加速効果など）への対応が容易