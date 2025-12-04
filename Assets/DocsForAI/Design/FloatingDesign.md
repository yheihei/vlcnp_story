# Floating 浮遊挙動設計

## 変更概要
- `Floating` コンポーネントで Y 軸方向にのみサイン波オフセットを付与し、他の移動処理を維持したままフワフワ上下動させる。
- 前フレームに適用したオフセットを一度打ち消してから新しいオフセットを足すことで、他スクリプトの座標更新との干渉を回避。
- `IStoppable` を実装し、停止時にオフセットをリセットして位置を元に戻す。`useUnscaledTime` で Time.timeScale の影響を受けない挙動にも切り替え可能。

## コンポーネントの役割と変更箇所
- `Assets/Scripts/Movement/Floating.cs`
  - フィールド追加: `amplitude`(振幅), `cycleDuration`(1周期秒), `phaseOffset`(位相ラジアン), `useUnscaledTime`(非スケール時間使用), 内部状態 `elapsedTime`, `lastOffset`, `isStopped`。
  - `LateUpdate` で `sin` によるオフセットを計算し、前フレームの `lastOffset` を引いた上で新オフセットを加算。X/Z 成分は変更しない。
  - `IsStopped` 設定時/OnDisable/OnEnable で `ResetOffset` を呼び、停止時に元の位置へ戻して上下動を止める。

## Unity Editor 操作マニュアル
1. 浮遊させたい敵オブジェクトに `Floating` コンポーネントを追加する。
2. 調整パラメータ:
   - `Amplitude`: 上下振幅。例) 0.25 で ±0.25m 揺れる。
   - `Cycle Duration`: 1 往復にかかる秒数。小さいほど速く揺れる。
   - `Phase Offset`: ラジアン指定の位相ずれ。複数体の揺れをずらしたいときに使用。
   - `Use Unscaled Time`: チェックで `Time.timeScale` の影響を受けずに揺れ続ける。
3. `StoppableController` などから `IsStopped` を true にするとオフセットが即時解除され、再開時は元位置から再び揺れ始める。
