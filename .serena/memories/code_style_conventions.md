# コードスタイルと規約

## 名前空間規約
- メインの名前空間: `VLCNP.*`
  - UI: `VLCNP.UI`
  - 戦闘: `VLCNP.Combat`
  - 移動: `VLCNP.Movement`
  - コントロール: `VLCNP.Control`
  - 属性: `VLCNP.Attributes`
  - アクション: `VLCNP.Actions`
  - エフェクト: `VLCNP.Effects`
  - ムービー: `VLCNP.Movie`
  - コア: `VLCNP.Core`
  - デバッグ: `VLCNP.DebugSctipt`
  - その他: `VLCNP.Stats`, `VLCNP.Pickups`, `VLCNP.Projectiles`, `VLCNP.Saving`, `VLCNP.SceneManagement`
- その他の名前空間:
  - `Core.Status` - ステータス管理
  - `Projectiles.StatusEffects` - 投射物の状態効果
  - `CLCNP.Core` - 一部のコアクラス

## コーディング規約
- **クラス名**: PascalCase (例: `PlayerController`, `EnemyController`)
- **メソッド名**: PascalCase (例: `AttackBehaviour`, `Update`)
- **フィールド名**: camelCase (例: `attackButton`, `isStopped`)
- **プロパティ名**: PascalCase (例: `IsStopped`)
- **インターフェース**: `I`プレフィックス (例: `IStoppable`, `ICollisionAction`)

## Unityコンポーネントパターン
- MonoBehaviourを継承
- Unity標準メソッド使用: `Awake()`, `Update()`, `OnTriggerEnter2D()`, `OnTriggerExit2D()`
- Singletonパターン: `Instance`プロパティ使用

## セーブデータ
- JSON形式で保存
- `IJsonSaveable`インターフェース実装
- `CaptureAsJToken()`と`RestoreFromJToken()`メソッド使用

## 型ヒント
C#の標準的な型システムを使用、明示的な型宣言を基本とする