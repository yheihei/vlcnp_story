# 目的

ゲームパッドでプレイしている場合に、Flag.IsGamePad のフラグを立てるようにしたい

# 要件

- 新しいスクリプトを作る
- LoadCompleteManager を使い、LoadCompleteManager.Instance.IsLoaded になったタイミングで、ゲームパッドが使用されているかどうかを見に行く
- ゲームパッドが使用されていれば、Flag.IsGamePad のフラグを立てる
- ゲームパッドが使用されていなければ、Flag.IsGamePad のフラグを下げる
- 常時監視しなくてよく、LoadCompleteManager.Instance.IsLoaded になったタイミングで一度だけ見ればよい

# 参考

ゲームパッド周りの処理は
@Assets/Scripts/Control/PlayerInputAdapter.cs
あたりでやっている。調査せよ

# 制約

シーンに追加するのはこちらでやるので、スクリプトだけ作って
