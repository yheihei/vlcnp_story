# Gamepad 対応 Plan

## 背景と目的
- PC キーボード操作を維持したまま、Xbox/PlayStation コントローラーでプレイヤー操作・UI 操作ができるようにする。
- 既存コードの `Input.GetKey` 依存を洗い出し、ゲームパッド入力を追加する。
- 入力周りの実装は `Assets/Scripts/` 配下のみ修正可能という制約を順守する。

## 操作割り当て方針
- 移動：キーボード左右矢印 / Gamepad 左スティック (X 軸) / D-Pad 左右。
- 武器の上下：キーボード上下矢印 / Gamepad 左スティック (Y 軸) / D-Pad 上下。
- インタラクト：キーボード上矢印 / Gamepad 左スティック (Y 軸) / D-Pad 上。
- ジャンプ：キーボード Space / Gamepad South ボタン (Xbox: A, PS: ×)。
- こうげき：キーボード X / Gamepad West ボタン (Xbox: X, PS: □)。
- キャラ切り替え：キーボード Z / Gamepad North ボタン (Xbox: Y, PS: △)。
- (不要) ダッシュ：キーボード X のみ（ゲームパッドは割り当てなし）。
- UI 選択 (GameSelect)：D-Pad または左スティック上下で移動、East ボタンで決定。

## 作業項目
1. `Input.GetKey*` の利用箇所確認（済）
2. キーボード + Gamepad 入力を両対応する共通ユーティリティクラス実装
3. 以下の各コンポーネントでユーティリティを使用するよう改修  
   - `Movement/Mover`
   - `Control/PlayerController`
   - `Movement/Jump`, `Movement/KabeKickEffectController`, `Movement/Dash`
   - `Control/PartyCongroller`
   - `UI/GameSelect`
4. 影響範囲テスト：  
   - エディタ再生でゲームパッド/キーボード双方の主要操作確認  
   - UI GameSelect シーンでメニュー移動と決定を確認
5. `Assets/DocsForAI/Design/` に変更内容とエディタ操作手順を記載

## 留意事項
- `UnityEngine.InputSystem`（新 Input System）がプロジェクトで有効であることを前提にする。
- 既存のインスペクター設定（例：`PlayerController.attackButton`, `Jump.jumpButton`）は維持する。
- 入力の Dead Zone（0.3）を設定し、微小なスティック揺れで誤作動しないようにする。
- タップ判定（`wasPressedThisFrame` 相当）と押し続け判定を使い分けてジャンプの長押し制御を維持する。

## 懸念・フォローアップ
- プロジェクト設定で新 Input System が無効の場合、ゲームパッドは利用不可のまま。必要なら別途設定変更を依頼。
- `Dash` のゲームパッド割り当ては現状未対応。必要に応じて別入力の検討を予定。
