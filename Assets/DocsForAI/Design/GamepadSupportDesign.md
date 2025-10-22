# Gamepad 対応 設計メモ

## 変更概要
- `Assets/Scripts/Control/PlayerInputAdapter.cs` を新設し、キーボードとゲームパッドの両入力を抽象化。Unity Input System を前提として `Gamepad` API を直接利用。
- 既存の入力判定ロジック（`PlayerController`, `Mover`, `Jump`, `KabeKickEffectController`, `Dash`, `PartyCongroller`, `GameSelect`）をすべて `PlayerInputAdapter` 経由に置き換え、操作割り当てを一元管理。
- ハイレベルでは旧来の `Input.GetKey` 依存を維持しつつ、Gamepad の Stick/ボタン/D-Pad を追加対応することで PC キーボードと Gamepad を並行サポート。

## コンポーネントごとの役割と変更点
- `Control/PlayerInputAdapter`：操作別のラッパーメソッドを提供。移動・武器上下・攻撃・ジャンプ・キャラ切替・ダッシュ・UI メニュー操作をカバー。左スティックのデッドゾーンを 0.3 に設定。
- `Control/FungusMenuInputBridge`：Fungus の `MenuDialog` に対してゲームパッドの上下移動・決定入力（B/○）をブリッジ。
- `Movement/Mover`：横移動量を `GetMoveHorizontal()` の戻り値で計算し、アナログ値に応じた速度へ拡張。
- `Control/PlayerController`：武器方向と攻撃入力をラッパーに委譲し、ゲームパッドのボタン（Y）・スティック上下をサポート。インタラクトはボタン South（A/×）。
- `Movement/Jump` と `Movement/KabeKickEffectController`：ジャンプの押下／ホールド／リリースを共通化し、B/○ または Space で挙動統一。
- `Movement/Dash`：RB/R1 を新たなダッシュ入力として追加（既存のキーボード X は維持）。
- `Control/PartyCongroller`：キャラ切替にゲームパッドの West ボタン（X/□）を追加。
- `UI/GameSelect`：ゲームパッドの D-Pad/左スティックで上下選択、決定は South ボタンで実行可能に。

## 操作割り当て
- 移動：キーボード左右矢印／Gamepad 左スティック X 軸／D-Pad 左右
- 武器上下：キーボード上下矢印／Gamepad 左スティック Y 軸／D-Pad 上下
- インタラクト：キーボード上矢印／Gamepad 左スティック Y 軸／D-Pad 上（押下）
- ジャンプ：キーボード Space／Gamepad South（Xbox: A, PlayStation: ×）
- こうげき：キーボード X／Gamepad West（Xbox: X, PlayStation: □）
- キャラ切替：キーボード Z／Gamepad North（Xbox: Y, PlayStation: △）
- ダッシュ：キーボード X のみ（ゲームパッドは割り当てなし）
- UI 決定：キーボード Enter or Space／Gamepad East（Xbox: B, PlayStation: ○）
- UI 上下：キーボード上下矢印／Gamepad D-Pad または左スティック上下

## Unity Editor 操作マニュアル
1. Input System パッケージ（`com.unity.inputsystem`）が導入済みであること、`Edit > Project Settings > Player > Active Input Handling` が `Both` もしくは `Input System Package (New)` になっていることを確認。
2. 対応ゲームパッド（Xbox/PlayStation）を PC に接続。
3. 対象シーンを開き Play モードに入る。
4. 左スティックで移動、A/× でジャンプ、X/□ で攻撃、Y/△ でキャラ切替できるか確認。
5. NPC 付近で左スティック上または D-Pad 上を弾いてインタラクトが反応するか確認。
6. タイトル（`GameSelect`）シーンで D-Pad/左スティック上下と B/○ 決定が機能するか確認。

## テスト観点
- キーボード操作が従来どおり動作し、ゲームパッド入力追加による回 regressions がないか。
- ゲームパッド接続時、入力の押しっぱなし・連打が期待通り（ジャンプ長押しなど）に動作するか。
- アナログスティックの軽い揺れで誤反応しない（デッドゾーン設定）こと。
- ゲームパッド未接続の状態で例外が発生しないこと（`Gamepad.current` null への防御が機能しているか）。 
