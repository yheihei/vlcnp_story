# ArrowAlignToVelocity2D 左向き上下反転修正プラン

## 目的
`ArrowAlignToVelocity2D` で速度ベクトルに回転追従させた際、左向き射出（`lossyScale.x < 0`）だけ鏃が上下逆に見える不具合を解消する。

## タスク
1. `Assets/Scripts/Combat/ArrowAlignToVelocity2D.cs` の `FixedUpdate()` で算出している角度に対し、`transform.lossyScale.x < 0f` のとき `+180f` を加える補正を追加する。
2. `minSpeed` 判定、`Atan2` による角度算出、`transform.rotation` への反映は既存仕様を維持する。
3. 変更対象を `ArrowAlignToVelocity2D` のみに限定し、`Projectile` / `WeaponConfig` / Prefab 設定は変更しない。
4. 確認観点として、左右・斜め射出時の鏃向きが進行方向と一致することを確認する。
