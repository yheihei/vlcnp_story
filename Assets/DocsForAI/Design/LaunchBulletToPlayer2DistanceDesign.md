# LaunchBulletToPlayer2 距離判定設計

## 変更概要
- `fireDistanceThreshold`（デフォルト 8、0 で無効）を追加し、プレイヤーが閾値より遠い場合はアニメーショントリガーを送らず `IsDone` を立てて即終了するようにした。
- プレイヤー取得共通化のため `TryGetPlayer` を導入し、取得失敗時や発射 Transform 未設定時に安全に早期リターンするようにした。

## コンポーネントの役割と変更箇所
- `Assets/Scripts/Combat/EnemyAction/LaunchBulletToPlayer2.cs`
  - フィールド追加: `fireDistanceThreshold`（距離しきい値、Tooltip 付きで 0 の場合は常時発射）。
  - コルーチン `Launch` に距離チェックを追加し、超過時はアニメーションを再生せず完了扱いにする。プレイヤー取得失敗時も即終了。
  - `OnLaunchAnimationEvent` で共通のプレイヤー取得メソッドを使用し、`launchTransform` 未設定時にも安全に抜けるようにした。

## Unity Editor 操作マニュアル
1. 対象敵オブジェクトの `LaunchBulletToPlayer2` コンポーネントを選択する。
2. `Fire Distance Threshold` を調整する。
   - 8（デフォルト）: 約 8m 以上離れていれば弾を発射せず行動完了。
   - 0: 距離判定を無効化し、常に発射する。
3. `Launch Transform` が未設定の場合は発射しないため、手元など適切な Transform を指定しておく。
