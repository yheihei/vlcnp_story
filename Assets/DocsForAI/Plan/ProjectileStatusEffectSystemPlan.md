# プラン - Projectile状態効果システム

## 実装計画概要

既存のProjectileシステムとEnemyActionシステムの調査結果を踏まえ、以下の段階的なアプローチで状態効果システムを実装します。

## フェーズ1: 基盤システムの構築

### 1.1 インターフェース設計
- `IProjectileStatusEffect`インターフェースを作成
- `ISpeedModifiable`インターフェースを作成（敵の速度制御用）

### 1.2 Projectileシステムの拡張
- `IProjectile`インターフェースに`OnTargetHit`イベントを追加
- `Projectile.cs`と`BouncingProjectile.cs`にイベント実装を追加

### 1.3 状態効果適用システム
- `ProjectileStatusEffectApplier`コンポーネントを作成
- UnityEventベースでProjectileとの疎結合を実現

## フェーズ2: 麻痺効果の実装

### 2.1 麻痺状態効果
- `ParalysisStatusEffect`ScriptableObjectを作成
- 速度倍率、継続時間、エフェクトのパラメータ化

### 2.2 敵の速度制御システム
- `ISpeedModifiable`を各EnemyActionクラスに実装
  - `Moving.cs`
  - `RepeatMoving.cs`
  - `Swimming.cs` 
  - `HorizontalRepeatFlyMoving.cs`

### 2.3 ステータス管理
- `EnemyStatusManager`コンポーネントを作成
- 状態効果の適用・解除・重複チェックを管理

## フェーズ3: エフェクトとUI

### 3.1 視覚的エフェクト
- 麻痺状態のエフェクトプレファブ作成
- `StatusEffectVisualController`コンポーネント実装

### 3.2 Unity Editorサポート
- CreateAssetMenuの設定
- カスタムインスペクタ（必要に応じて）

## 実装の詳細

### ファイル構成

```
Assets/Scripts/
├── Core/
│   ├── Status/
│   │   ├── IStatus.cs (既存)
│   │   ├── ISpeedModifiable.cs (新規)
│   │   └── EnemyStatusManager.cs (新規)
├── Combat/
│   ├── IProjectile.cs (拡張)
│   ├── Projectile.cs (拡張)
│   ├── BouncingProjectile.cs (拡張)
│   └── EnemyAction/
│       ├── Moving.cs (拡張)
│       ├── RepeatMoving.cs (拡張)
│       ├── Swimming.cs (拡張)
│       └── HorizontalRepeatFlyMoving.cs (拡張)
├── Projectiles/
│   ├── StatusEffects/
│   │   ├── IProjectileStatusEffect.cs (新規)
│   │   ├── ParalysisStatusEffect.cs (新規)
│   │   └── ProjectileStatusEffectApplier.cs (新規)
└── Effects/
    └── StatusEffectVisualController.cs (新規)
```

### 主要コンポーネントの役割

#### `IProjectileStatusEffect`
```csharp
public interface IProjectileStatusEffect
{
    void ApplyEffect(GameObject target);
    string EffectName { get; }
}
```

#### `ProjectileStatusEffectApplier`
```csharp
public class ProjectileStatusEffectApplier : MonoBehaviour
{
    [SerializeField] private SerializableInterface<IProjectileStatusEffect>[] statusEffects;
    
    void Start()
    {
        // IProjectileのOnTargetHitイベントを購読
    }
    
    private void OnTargetHit(GameObject target)
    {
        foreach(var effect in statusEffects)
        {
            effect.Value.ApplyEffect(target);
        }
    }
}
```

#### `ISpeedModifiable`
```csharp
public interface ISpeedModifiable
{
    void SetSpeedModifier(float modifier);
    float GetCurrentSpeed();
}
```

#### `EnemyStatusManager`
```csharp
public class EnemyStatusManager : MonoBehaviour
{
    private Dictionary<string, Coroutine> activeEffects;
    
    public void ApplyStatusEffect(IProjectileStatusEffect effect)
    {
        // 状態効果の適用・重複チェック・時間管理
    }
}
```

## 実装順序

### ステップ1: 基盤インターフェース（30分）
1. `IProjectileStatusEffect.cs`作成
2. `ISpeedModifiable.cs`作成

### ステップ2: Projectileシステム拡張（45分）
1. `IProjectile`にOnTargetHitイベント追加
2. `Projectile.cs`にイベント実装
3. `BouncingProjectile.cs`にイベント実装

### ステップ3: 状態効果適用システム（30分）
1. `ProjectileStatusEffectApplier.cs`作成

### ステップ4: 敵速度制御システム（60分）
1. `Moving.cs`に`ISpeedModifiable`実装
2. `RepeatMoving.cs`に`ISpeedModifiable`実装
3. 他のEnemyActionクラスにも順次実装

### ステップ5: 麻痺効果実装（45分）
1. `ParalysisStatusEffect.cs`作成
2. `EnemyStatusManager.cs`作成

### ステップ6: 視覚的エフェクト（30分）
1. `StatusEffectVisualController.cs`作成

## テスト計画

### Unity Editor上での確認項目
1. Projectileプレファブに`ProjectileStatusEffectApplier`を追加可能
2. 状態効果アセットをCreateメニューから作成可能
3. 敵に麻痺効果が適用され、移動速度が変化する
4. 状態効果の重複適用が正しく制御される
5. 時間経過で状態効果が解除される

### 既存システムとの互換性確認
1. 既存のProjectileが正常に動作する
2. 状態効果が設定されていないProjectileが正常に動作する
3. 既存の敵の移動が影響を受けない

## 期待される成果

- 柔軟で拡張性の高い状態効果システム
- 既存システムとの完全互換性
- Unity Editorでの直感的な設定機能
- 将来的な状態効果追加の容易性