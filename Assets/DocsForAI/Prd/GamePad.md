# PRD

## 目的

- GamePadで動かせるようにしたい
- 現行のPCでのキーでも動かせるようにしたい

## 詳細

### 現行の操作方法

```
キー	説明
← →	移動
↑	しらべる
スペース	ジャンプ
X	こうげき
Z	キャラ切り替え
```

### GamePadでの操作方法

https://docs.unity3d.com/ja/Packages/com.unity.inputsystem@1.4/manual/Gamepad.html

を参考に、下記のXbox コントローラーのキー配置で動くようにしたい

```
キー	説明
考えてほしい    移動
考えてほしい    しらべる
B	ジャンプ
Y	こうげき
X	キャラ切り替え
```

## 実装箇所

- @Assets/Scripts/Control/PlayerController.cs
- @Assets/Scripts/Control/PartyCongroller.cs
- 他 Input.GetKey 某を使っているものを洗い出し、実装部分を洗い出せ