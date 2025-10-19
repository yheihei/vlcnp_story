# コードスタイルと規約

## 名前空間規約
- メイン名前空間: `VLCNP.*`
  - `VLCNP.Core`, `VLCNP.Combat`, `VLCNP.Movement`, `VLCNP.Control`, `VLCNP.Attributes`,
    `VLCNP.Actions`, `VLCNP.Effects`, `VLCNP.UI`, `VLCNP.Movie`, `VLCNP.Stats`,
    `VLCNP.Pickups`, `VLCNP.Projectiles`, `VLCNP.Saving`, `VLCNP.SceneManagement`, `VLCNP.DebugScript`
- サブ名前空間: `Core.Status`, `Projectiles.StatusEffects` など機能別に分割

## C#コーディング規約
- クラス/構造体/enum: PascalCase（例: `FlagManager`, `ProjectileStatus`）
- メソッド: PascalCase（例: `SetFlag`, `CaptureAsJToken`）
- フィールド: camelCase。`[SerializeField]` 付き private フィールドも camelCase（例: `flagDictionary`）。接頭辞 `_` は基本使用しない。
- プロパティ/イベント: PascalCase (例: `OnChangeFlag`, `IsStopped`)
- インターフェース: `I` プレフィックス (例: `IJsonSaveable`, `IStoppable`)
- 初期化: target-typed `new()` を多用。
- コメント: `//` を用いた短い日本語コメントが中心。

## Unityコンポーネントの慣習
- `MonoBehaviour` を継承したコンポーネントが中心。
- ライフサイクルメソッド: `Awake`, `Start`, `Update`, `OnTriggerEnter2D` など Unity 標準を使用。
- イベント駆動: `Action` イベントを使い、`?.Invoke` で通知。
- セーブ処理: `IJsonSaveable` 実装で `CaptureAsJToken`, `RestoreFromJToken` を実装。

## その他
- シリアライズ対象: `[SerializeField]` を private フィールドにつける。
- null チェックやディクショナリアクセスではガード節 (`if (!flagDictionary.ContainsKey(flag)) return`) を使用。
- スペーシング: インデントは4スペース。