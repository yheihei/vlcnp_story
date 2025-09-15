# GamePad 対応実装設計書

## 概要
Unity Input Systemを使用して、PCキーボードとGamePadの両方に対応した入力システムを実装しました。

## コンポーネント構成

### 1. Input Actions アセット
- **ファイル**: `Assets/Scripts/Input/PlayerInputActions.inputactions`
- **アクションマップ**:
  - **Player**: ゲームプレイ中の操作
  - **UI**: メニュー操作

### 2. InputManager クラス
- **ファイル**: `Assets/Scripts/Input/InputManager.cs`
- **役割**: 入力の統合管理を行うシングルトンクラス
- **主要メソッド**:
  - `IsMovingRight()` / `IsMovingLeft()`: 横移動
  - `IsMovingUp()` / `IsMovingDown()`: 縦移動
  - `IsJumpPressed()` / `IsJumpHeld()` / `IsJumpReleased()`: ジャンプ
  - `IsAttackPressed()`: 攻撃
  - `IsInteractPressed()`: 調べる
  - `IsSwitchCharacterPressed()`: キャラ切り替え
  - `IsNavigateUpPressed()` / `IsNavigateDownPressed()`: UI操作
  - `IsSubmitPressed()`: 決定

## 操作マッピング

| アクション | キーボード | GamePad |
|-----------|-----------|---------|
| 移動（左右） | 矢印キー左右 | 左スティック / 十字キー |
| 移動（上下） | 矢印キー上下 | 左スティック / 十字キー |
| ジャンプ | スペース | Bボタン (South) |
| 攻撃 | X | Yボタン (North) |
| 調べる | 上矢印 | 左スティック上 / 十字キー上 |
| キャラ切り替え | Z | Xボタン (West) |
| UI決定 | Return / スペース | Aボタン (East) |

## Unity Editor 上での設定手順

### 1. Input System Package の確認
`Packages/manifest.json` に以下が含まれていることを確認:
```json
"com.unity.inputsystem": "1.4.4"
```

### 2. InputManager の初期化
ゲーム開始時に自動的にInputManagerのシングルトンインスタンスが生成されます。
手動での設定は不要です。

### 3. Input Actions アセットの生成
`Assets/Scripts/Input/PlayerInputActions.inputactions` ファイルを選択し、
インスペクターで「Generate C# Class」にチェックが入っていることを確認してください。

### 4. プレイヤー設定
各プレイヤーキャラクターのGameObjectには以下のコンポーネントがアタッチされている必要があります：
- PlayerController
- Mover
- Jump
- Fighter
- Leg

### 5. UI設定
GameSelectなどのUIコンポーネントは、`EnableSelect()`メソッド呼び出し時に自動的にUIアクションマップに切り替わります。

## 変更されたスクリプト

1. **PlayerController.cs**: Input.GetKey → InputManager経由
2. **PartyCongroller.cs**: KeyCode.Z → InputManager経由
3. **Mover.cs**: 移動入力をInputManager経由
4. **Jump.cs**: ジャンプ入力をInputManager経由
5. **KabeKickEffectController.cs**: 壁キックジャンプをInputManager経由
6. **GameSelect.cs**: UI操作をInputManager経由

## テスト手順

### キーボード操作テスト
1. キーボードの矢印キーで移動できることを確認
2. スペースキーでジャンプできることを確認
3. Xキーで攻撃できることを確認
4. Zキーでキャラ切り替えができることを確認
5. 上矢印キーで調べるアクションができることを確認

### GamePad操作テスト
1. 左スティック/十字キーで移動できることを確認
2. Bボタンでジャンプできることを確認
3. Yボタンで攻撃できることを確認
4. Xボタンでキャラ切り替えができることを確認
5. 左スティック上/十字キー上で調べるアクションができることを確認

### 同時操作テスト
キーボードとGamePadを同時に接続し、両方の入力が正しく動作することを確認

## トラブルシューティング

### InputManagerが見つからない場合
コンパイルエラーが出る場合は、Unity EditorでInput Actions アセットを選択し、
「Generate C# Class」を再実行してください。

### GamePadが認識されない場合
1. Unity Editor → Project Settings → Input System Package
2. 「Supported Devices」にGamepadが含まれていることを確認
3. PCにGamePadが正しく接続されていることを確認

## 今後の拡張
- 複数プレイヤー対応
- カスタムキーバインド設定
- 振動フィードバック対応