# おひるねべや（闇）→おひるねべや（土）遷移時フリーズ調査メモ

## 概要

- 調査日: 2026-07-08
- 確認環境: Steam macOS版体験版
- Steam BuildID: `24106900`
- 症状: おひるねべや（闇）クリア後、おひるねべや（土）へ移動すると画面がフリーズしたようになる。
- 結論: クラッシュではなく、`PartyCongroller`で`NullReferenceException`が毎フレーム発生する例外ストーム。

## 再現時の状態

遷移直前のオートセーブには次の状態が記録されていた。

- `currentPlayerName`: `Mitama`
- `VLMitamaJoined`: `true`
- 闇エリアの主要進行フラグは完了済み
- 遷移先のBuild Settings scene index `20`は`Assets/Scenes/Ohirunebeya_tuti_1.unity`

## ログ

再現セッションのログ:

`/Users/yhei/Library/Logs/YheiWebDesign/VlcnpStory/Player-prev.log`

遷移先シーンのロード自体は完了している。

```text
scene load end: 20
Loading: autoSave
Party Restore ... "currentPlayerName": "Mitama"
```

その直後から次の例外が発生する。

```text
NullReferenceException: Object reference not set to an instance of an object
  at VLCNP.Control.PartyCongroller.RestoreFromJToken (...)
  at VLCNP.Saving.JsonSaveableEntity.RestoreFromJToken (...)
  at VLCNP.Saving.JsonSavingSystem.LoadOnlyState (...)
  at VLCNP.SceneManagement.TransitionEvent+<Transition>d__22.MoveNext (...)
```

続いて`PartyCongroller.Start()`、`ChangeHud()`、`Update()`でも例外が発生し、特に`Update()`の例外が毎フレーム出続ける。

macOSのクラッシュレポートは生成されていない。ユーザー終了後は通常のUnity shutdownログが記録されている。

## 根本原因

`Assets/Scripts/Control/PartyCongroller.cs`の`RestoreFromJToken()`は、保存されたキャラクター名を`members`から検索する。

```csharp
currentPlayer = Array.Find(members, member => member.name == currentPlayerName);
RefreshCurrentMemberCache();
```

おひるねべや（土）のローカルParty Prefab:

`Assets/Scenes/Ohirunebeya_tuti_1/Party.prefab`

このPrefabの`members`は3体のみ。

1. Player / Akim
2. Leelee
3. Orochi

`Mitama`が含まれていないため、闇エリアから`currentPlayerName=Mitama`の状態で遷移すると`Array.Find()`が`null`を返す。その後`currentMemberCache`を参照して例外になる。

比較対象の標準Party Prefab:

`Assets/Game/Characters/Party.prefab`

こちらの`members`は4体で、`VLMitamaPlayerVariant`も含まれている。

## 推奨修正

両方を実施するのが安全。

1. `Assets/Scenes/Ohirunebeya_tuti_1/Party.prefab`へMitamaを追加する。
   - 土エリア固有のPlayer / Leelee Prefab差分があるため、標準Party Prefabへの単純置換は避け、既存差分を確認してから追加する。
   - `members`配列だけでなく、Mitama GameObject本体と必要コンポーネントを正しく追加する。
2. `PartyCongroller.RestoreFromJToken()`へ防御処理を追加する。
   - 保存キャラクターが`members`に存在しない場合はエラー内容を一度だけログ出力する。
   - Akimなど確実に存在するメンバーへフォールバックしてから`RefreshCurrentMemberCache()`を呼ぶ。
   - `Update()`で毎フレーム例外を出さないよう、`currentMemberCache`未設定時のガードも検討する。

## 検証項目

1. 闇エリアでMitamaを操作キャラにする。
2. 闇エリアをクリアし、土エリアへ遷移する。
3. `Ohirunebeya_tuti_1`がロードされ、Mitama・カメラ・HUDが正常に復元されることを確認する。
4. `Player.log`とUnity Consoleに`PartyCongroller`由来の例外がないことを確認する。
5. Akim、Leelee、Orochiを操作キャラにした状態でも同じ遷移を確認する。
6. 存在しない`currentPlayerName`を含む旧／異常セーブを用意し、フォールバックが機能することを確認する。
7. macOS Steam配信版でも同じセーブデータから再確認する。

## 関連ファイル

- `Assets/Scripts/Control/PartyCongroller.cs`
- `Assets/Scenes/Ohirunebeya_tuti_1.unity`
- `Assets/Scenes/Ohirunebeya_tuti_1/Party.prefab`
- `Assets/Game/Characters/Party.prefab`
- `ProjectSettings/EditorBuildSettings.asset`

