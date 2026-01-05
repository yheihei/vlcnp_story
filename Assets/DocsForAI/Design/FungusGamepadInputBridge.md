# Fungus メッセージ/選択肢のゲームパッド入力復旧

## 変更概要
- Fungus の SayDialog（メッセージ送り）をゲームパッド入力で進められるよう、専用ブリッジを追加。

## 追加・変更したコンポーネント
- `FungusDialogInputBridge`
  - 役割: SayDialog の `DialogInput.SetNextLineFlag()` をゲームパッド入力で呼び出す常駐ブリッジ。
  - 挙動: SayDialog がアクティブかつ MenuDialog 非表示の時のみ反応。
  - 常駐化: `RuntimeInitializeOnLoadMethod` で自動生成し、`DontDestroyOnLoad`。

- `PlayerInputAdapter.WasMenuSubmitPressed()`
  - 役割: メニュー決定入力の検知。
  - 変更: `buttonEast` を決定として扱う。

## Unity Editor 操作マニュアル
- 追加の配置作業は不要（自動生成）。
- もしメッセージ送りが反応しない場合は下記を確認:
  - SayDialog に `DialogInput` コンポーネントが付いているか
  - Scene に EventSystem が存在するか（Fungus が自動生成する前提）
