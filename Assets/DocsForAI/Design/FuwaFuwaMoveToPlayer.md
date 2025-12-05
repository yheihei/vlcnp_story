## 役割と変更箇所
- `Assets/Scripts/Combat/EnemyAction/FuwaFuwaMoveToPlayer.cs`
  - Ghost向けの新しい `EnemyAction`。`Execute()` でプレイヤー方向へ一定時間接近しながら、サイン波で上下に揺れる移動を行う。
  - `speed` を `SpeedModifier` の倍率込みで適用、`approachDuration` 経過で動作完了し `IsDone` を立てる。
  - `floatAmplitude` と `floatCycleDuration` でふわふわの揺れ幅・周期を調整。`Stop()` でコルーチンを停止し速度をクリア。

## Unity Editor 操作マニュアル
1. Ghost の `EnemyV2Controller` を持つPrefab/シーンオブジェクトを選択。
2. `FuwaFuwaMoveToPlayer` コンポーネントを追加し、`EnemyV2Controller.enemyActions` リストにドラッグ＆ドロップで登録。
3. パラメータ設定例:
   - `Speed`: 2 (ゆっくり接近させるデフォルト)
   - `Approach Duration`: 2.5 (この秒数だけプレイヤーへ接近し続ける)
   - `Float Amplitude`: 0.25 (上下揺れの幅)
   - `Float Cycle Duration`: 2 (揺れの一周期秒数)
4. 必要に応じて Animator の `vx` パラメータを利用するアニメーションブレンドがある場合は、`Animator` が同じオブジェクトに付いていることを確認。
