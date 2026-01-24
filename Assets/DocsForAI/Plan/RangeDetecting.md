# Plan: RangeDetecting

## Goals
- enableUndetectedAnimation=true のとき、発見アニメ終了(= OnUndetectedAnimationFinished)まで IsDetect が true を返さない。
- enableUndetectedAnimation=false の挙動は現状維持。
- 発見アニメ中にプレイヤーが範囲外へ出ても「発見した扱い」にする。

## Assumptions / Preconditions
- Animation Event で `RangeDetect.OnUndetectedAnimationFinished()` が呼ばれる。
- enableUndetectedAnimation=false の敵は発見アニメを使わず、検出即確定で問題ない。
- `IsDetect()` は `EnemyV2Controller` から周期的に呼ばれる前提で、状態は `RangeDetect` が保持する。

## Design Outline
- 「発見確定待ち」を単一フラグで表現する。
- enableUndetectedAnimation=false は「最初から発見確定待ちが解除されている」状態として扱う。

### State
- `isDetected`: 発見確定済みかどうか
- `isWaitingDetectConfirm`: 発見アニメ終了待ちかどうか (旧 `isWaitingUndetectedAnimationEnd` の置換候補)

### Flow
1) まだ未発見のときに検出距離内に入る
- enableUndetectedAnimation=true
  - `isWaitingDetectConfirm = true`
  - `isDetected = false` (発見は確定しない)
  - `IsDetect` は false を返し続ける
- enableUndetectedAnimation=false
  - `isWaitingDetectConfirm = false`
  - `isDetected = true`
  - `IsDetect` は true を返す

2) OnUndetectedAnimationFinished
- enableUndetectedAnimation=true のときのみ
  - `isWaitingDetectConfirm = false`
  - `isDetected = true` (発見確定)

3) isDetected=true のとき
- 追跡距離で判定して `IsDetect` を返す
- `isWaitingDetectConfirm` の状態に関係なく、待機中は `IsDetect` を false にする

## Tasks
- RangeDetect の「発見アニメ待機」フラグをリネーム/整理。
- IsDetect のロジックを「待機中は false」「確定後は追跡距離」に整理。
- OnUndetectedAnimationFinished で待機解除と発見確定。
- enableUndetectedAnimation=false のケースは「初期から待機解除済み」として扱う。

## Notes
- Script 変更は `Assets/Scripts/` 以下のみ。
- Animation Event で RangeDetect.OnUndetectedAnimationFinished を必ず呼ぶ。
