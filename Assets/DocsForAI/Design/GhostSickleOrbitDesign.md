# GhostSickle 渦巻き軌道設計

## 変更概要
- `GhostSickle` を「発射した敵の位置」を中心とした反時計回りの渦巻き軌道へ変更。
- 角速度 (`speed` deg/s) と半径成長速度 (`radiusGrowthPerSecond`) をフレーム毎に積分し、`transform.position` を直接更新。
- 発射位置と中心が同一の場合でも、左右向きに応じてわずかに離した半径からスタートし、時間経過で外側へ発散。
- 本体スプライトも `selfRotationSpeed` で自転させ、回転演出を付与。
- 既存のフェードアウト・自動削除・ヒット判定ロジックは保持。

## コンポーネントの役割と変更箇所
- `Assets/Scripts/Projectiles/GhostSickle.cs`
  - フィールド追加: `radiusGrowthPerSecond`, `initialRadius`, `centerOffsetX`, `selfRotationSpeed`, `currentAngleRad`, `currentRadius`, `currentCenter`, `spawnPosition`。
  - `Start` で生成位置を `spawnPosition` として保持し、`centerOffsetX` を x 方向に加えた座標を中心に設定。
  - `InitializeOrbit` で中心・初期半径・角度を設定 (中心と同位置なら左右向きで初期角度を決定)。
  - `FixedUpdate` で角度を増分 (反時計回り)、半径を拡大し、中心+オフセットで位置を更新。さらに `selfRotationSpeed` で自身を自転させる。中心は生成時点から固定される。

## Unity Editor 操作マニュアル
1) ゴーストシックルの Prefab を選択し、`GhostSickle` コンポーネントを確認。
2) 調整する主要パラメータ:
   - `Speed` : 角速度 [deg/s]。大きいほど回転が速い (デフォルト 180)。
   - `Radius Growth Per Second` : 半径の広がり速度。大きいほど渦巻きの広がりが早い。
   - `Initial Radius` : 発射直後の半径。中心と同位置で生成された場合の初期オフセットにも利用。
   - `Center Offset X` : 生成位置から x 方向にずらしたい量。例: 5 を指定すると生成位置の x+5 を中心に回転。
   - `Self Rotation Speed` : 本体の自転角速度 [deg/s]。正で反時計回り、負で時計回り。
   - `Delete Time` / `Is Fade Out` : 既存どおり寿命とフェードアウト挙動を制御。
3) 敵の発射処理で `Center Offset X` を設定しておけば、生成位置からオフセットした固定点を中心に渦巻きする。寿命経過で自動削除される。
