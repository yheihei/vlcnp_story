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
↓   銃を下に向ける
スペース	ジャンプ
X	こうげき
Z	キャラ切り替え
```

### GamePadでの操作方法

https://docs.unity3d.com/ja/Packages/com.unity.inputsystem@1.4/manual/Gamepad.html

を参考に、Xbox, PlayStationコントローラーで動くようにしたい。

PlayStation:
https://docs.unity3d.com/ja/Packages/com.unity.inputsystem@1.4/manual/Gamepad.html#playstation-%E3%82%B3%E3%83%B3%E3%83%88%E3%83%AD%E3%83%BC%E3%83%A9%E3%83%BC

Xbox:
https://docs.unity3d.com/ja/Packages/com.unity.inputsystem@1.4/manual/Gamepad.html#xbox-%E3%82%B3%E3%83%B3%E3%83%88%E3%83%AD%E3%83%BC%E3%83%A9%E3%83%BC

GamePadのキー配置:
```
キー	説明
考えてほしい    移動
考えてほしい    しらべる
考えてほしい   銃を下に向ける
B	ジャンプ
Y	こうげき
X	キャラ切り替え
```

## 実装箇所

- @Assets/Scripts/Control/PlayerController.cs
- @Assets/Scripts/Control/PartyCongroller.cs
- 他 Input.GetKey を使っているものを洗い出し、実装部分を洗い出せ