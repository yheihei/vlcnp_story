# 設計書 - Projectile状態効果システム

## システム概要

Projectileが敵にヒットした際に様々な状態効果（麻痺、毒など）を適用できる柔軟で拡張性の高いシステムを実装しました。UnityEventとSerializableInterfaceを活用した疎結合設計により、既存システムへの影響を最小限に抑えつつ、新しい状態効果の追加が容易に行えます。

## 主要コンポーネントの役割

### 1. インターフェース層

#### `IProjectileStatusEffect`
- **場所**: `Assets/Scripts/Projectiles/StatusEffects/IProjectileStatusEffect.cs`
- **役割**: 状態効果の基本インターフェース
- **主要メソッド**:
  - `string EffectName { get; }`: 効果名を取得
  - `void ApplyEffect(GameObject target)`: ターゲットに効果を適用

### 2. 弾丸システム拡張

#### `IProjectile`の拡張
- **場所**: `Assets/Scripts/Combat/IProjectile.cs`
- **変更点**: `UnityEvent<GameObject> OnTargetHit` イベントを追加
- **目的**: ターゲットヒット時のイベント通知

#### `Projectile` / `BouncingProjectile`の拡張
- **場所**: `Assets/Scripts/Combat/Projectile.cs`, `Assets/Scripts/Combat/BouncingProjectile.cs`
- **変更点**: OnTargetHitイベントの実装を追加
- **動作**: ダメージ適用後にイベントを発火

### 3. 状態効果適用システム

#### `ProjectileStatusEffectApplier`
- **場所**: `Assets/Scripts/Projectiles/StatusEffects/ProjectileStatusEffectApplier.cs`
- **役割**: Projectileと状態効果システムを橋渡し
- **動作**: 
  - IProjectileのOnTargetHitイベントを購読
  - 設定された状態効果を順次適用
- **設定項目**: SerializableInterface配列で複数の状態効果を設定可能

### 4. 敵速度制御システム

#### `SpeedModifier`
- **場所**: `Assets/Scripts/Core/Status/SpeedModifier.cs`
- **役割**: 速度倍率を管理する独立したコンポーネント
- **主要メソッド**:
  - `SetModifier(float modifier)`: 速度倍率を設定
  - `ResetModifier()`: 速度倍率をリセット（1.0に戻す）
  - `CalculateModifiedSpeed(float baseSpeed)`: 修正後の速度を計算
- **特徴**: Unity Editorで直接設定可能

#### `EnemyAction`基底クラスの拡張
- **場所**: `Assets/Scripts/Combat/EnemyAction/EnemyAction.cs`
- **追加メソッド**: `GetModifiedSpeed(float baseSpeed)`
- **動作**: SpeedModifierコンポーネントを参照し、倍率適用後の速度を返す
- **互換性**: SpeedModifierがない場合は基準速度をそのまま返す

### 5. 麻痺効果実装

#### `ParalysisStatusEffect`
- **場所**: `Assets/Scripts/Projectiles/StatusEffects/ParalysisStatusEffect.cs`
- **役割**: 麻痺効果のScriptableObject
- **設定項目**:
  - `speedMultiplier`: 速度倍率（0.1-1.0）
  - `duration`: 継続時間（秒）
  - `effectPrefab`: 視覚エフェクト（オプション）
- **CreateAssetMenu**: "Projectile Status Effects/Paralysis"

#### `ParalysisStatusController`
- **場所**: 同上ファイル内
- **役割**: 麻痺状態の実際の制御
- **機能**:
  - Coroutineによる時間管理
  - 速度倍率の適用・解除
  - エフェクト表示・削除
  - 重複適用の防止

### 6. 状態管理システム

#### `EnemyStatusManager`
- **場所**: `Assets/Scripts/Core/Status/EnemyStatusManager.cs`
- **役割**: 敵の状態効果を統合管理
- **機能**:
  - 重複チェック
  - アクティブな効果の追跡
  - デバッグ情報表示
  - エディタ用テスト機能

### 7. 視覚効果システム

#### `StatusEffectVisualController`
- **場所**: `Assets/Scripts/Effects/StatusEffectVisualController.cs`
- **役割**: 状態効果の視覚的表現を管理
- **機能**:
  - 効果別エフェクトプレファブ管理
  - エフェクト表示・非表示制御
  - 位置オフセット設定
  - 親オブジェクトへのアタッチ設定

## Unity Editor上での操作マニュアル

### 1. 状態効果アセットの作成

1. Projectウィンドウで右クリック
2. "Create > Projectile Status Effects > Paralysis" を選択
3. 作成されたアセットを選択
4. Inspectorで以下を設定:
   - **Speed Multiplier**: 速度倍率（0.5 = 半分の速度）
   - **Duration**: 継続時間（秒）
   - **Effect Prefab**: 視覚エフェクト（オプション）

### 2. Projectileへの状態効果設定

1. Projectileプレファブを選択
2. "Add Component" → "Projectile Status Effect Applier" を追加
3. "Status Effects" リストのサイズを設定
4. 各要素に作成した状態効果アセットを割り当て

### 3. 敵キャラクターへの設定

#### 速度制御対応
1. 敵プレファブに "Speed Modifier" コンポーネントを追加
2. 通常時はCurrent Modifierが1.0のまま
3. 状態効果適用時に自動的に倍率が変更される

#### 状態管理（オプション）
1. 敵プレファブに "Enemy Status Manager" コンポーネントを追加
2. 状態効果の重複チェックや統合管理が可能

#### 視覚エフェクト（オプション）
1. 敵プレファブに "Status Effect Visual Controller" コンポーネントを追加
2. 以下を設定:
   - **Paralysis Effect Prefab**: 麻痺エフェクトプレファブ
   - **Effect Offset**: エフェクト位置のオフセット
   - **Attach To Parent**: 親オブジェクトにアタッチするか

### 4. テスト方法

1. ゲームを実行
2. 状態効果付きProjectileで敵を攻撃
3. Console窓で状態効果の適用・解除ログを確認
4. 敵の移動速度変化を目視確認
5. エフェクトが設定されている場合は視覚的確認

## 拡張方法

### 新しい状態効果の追加

1. `IProjectileStatusEffect`を実装したScriptableObjectクラスを作成
2. `CreateAssetMenu`属性を追加
3. `ApplyEffect`メソッドで効果の実装
4. 必要に応じて専用コントローラーコンポーネントを作成

### 新しい移動タイプへの対応

1. 既存のEnemyActionクラスを継承して作成
2. 移動処理で`GetModifiedSpeed(基準速度)`を使用
3. SpeedModifierコンポーネントの有無に関わらず正常動作

## システムの利点

1. **疎結合設計**: ProjectileとStatusEffectが独立
2. **高い拡張性**: 新効果の追加が容易
3. **既存互換性**: 既存システムへの影響なし
4. **Unity Editor統合**: 直感的な設定・管理機能
5. **重複防止**: 同じ効果の重複適用を自動防止

## 注意事項

1. SerializableInterfaceパッケージが必要（TNRDネームスペース）
2. 状態効果アセットは事前作成が必要
3. **SpeedModifierコンポーネントが必須**（速度効果を使用する敵の場合）
4. エフェクトプレファブの設定は任意（なくても動作）
5. EnemyStatusManagerは任意コンポーネント（統合管理時のみ必要）

## 移行ガイド（既存プロジェクト向け）

1. 敵プレファブにSpeedModifierコンポーネントを追加
2. 各EnemyActionクラスはすでに対応済み（GetModifiedSpeed使用）
3. 新規作成するEnemyActionクラスも自動的に対応