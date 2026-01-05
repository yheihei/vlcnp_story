# 目的

ゲームパッドの接続状態により、アタッチしたゲームオブジェクトをactive/非activeするようにしたい

# 要件

- FlagManagerを使用して、Flag.IsGamePadの値を監視する
- Flag.IsGamePadがtrueの場合、アタッチしたゲームオブジェクトをactiveにする
- Flag.IsGamePadがfalseの場合、アタッチしたゲームオブジェクトを非activeにする
- チェックタイミング
  - Start後、LoadCompleteManager.Instance.IsLoaded になったタイミングでチェック
  - flagManager.OnChangeFlag を使って、何かのフラグが更新されたときにも再チェックする

# その他の要望

スクリプトのクラス名、GamePadActivater にしているが、より良い名前を考えてほしい