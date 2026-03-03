# ArrowAlignToVelocity2D 左向き上下反転修正設計

## 変更概要
- `ArrowAlignToVelocity2D` の角度計算後に、`transform.lossyScale.x < 0f` の場合は `angle += 180f` を加える補正を追加した。
- これにより、`Projectile.SetDirection` による左右反転（負スケール）が入っている弾でも、左向き射出時の見た目上下反転を防ぐ。

## コンポーネントの役割と変更箇所
- `Assets/Scripts/Combat/ArrowAlignToVelocity2D.cs`
  - 役割: `Rigidbody2D.velocity` に基づいて弾の回転を更新する。
  - 変更点: 左右反転状態（`lossyScale.x < 0`）を検出し、速度ベクトル由来の角度に 180 度補正を加える処理を追加。
  - 非変更点: `minSpeed` しきい値判定と `Atan2` の計算式は維持。

## Unity Editor 操作マニュアル
1. `SampleScene` を開き、`SkeletonArcher` が Arrow を発射する状況を再生する。
2. 敵が右向き・左向きの両方で発射し、鏃が常に進行方向を向くことを確認する。
3. 斜め上/斜め下への射出でも、左右で鏃向きが反転しないことを確認する。
4. `Arrow` プレハブの `ArrowAlignToVelocity2D` は有効のままにし、追加設定変更なしで確認する。
