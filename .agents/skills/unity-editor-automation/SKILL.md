---
name: unity-editor-automation
description: Operate the Unity Editor for vlcnpStory2022 through the UniCli CLI. Use when a task needs asset import, compilation, tests, Play Mode, scene or prefab operations, GameObject or component changes, or Editor menu execution. Covers safe command usage, Eval rules, and the Play Mode Eval deadlock pitfall.
---

# Unity エディタ操作(UniCli)

Unity エディタは `unicli` CLI 経由で操作する。エディタが起動済みでサーバーが動いていることが前提。

## 基本

```bash
unicli status                 # サーバー稼働確認
unicli commands               # 全コマンドと引数の一覧(113個)
unicli exec <Command> '<JSONパラメータ>'   # 実行形式
unicli exec Console.GetLog '{"logType":"Error","maxCount":10}'   # 例
```

- パラメータは第2引数に **JSON 文字列** で渡す。パラメータ無しなら省略可。
- 機械可読な出力が欲しいときは `--json` を付ける。

## 安全ルール

1. `Assets/` 配下のファイルを CLI 外(テキスト編集等)で作成・変更した場合は、コンパイルや Unity 上の操作の前に `AssetDatabase.Import` を実行する。
   ```bash
   unicli exec AssetDatabase.Import '{"path":"Assets/Scripts/Combat/Foo.cs"}'
   ```
2. C# のまとまった変更後と完了報告前に `unicli exec Compile` を実行し、エラー 0 を確認する。編集途中の保存ごとには必要ない。エラー詳細は結果と `Console.GetLog` で見る。
3. シーンを変更したら `Scene.Save`、プレハブは `Prefab.Save` / `Prefab.Apply`。未保存の変更は `Editor.Status` の "Dirty scenes" で分かる。
4. `implementation-workflow` skill に従い、変更種別に応じて検証を選ぶ。プレイモード検証が必要な場合だけ `unity-playmode-verification` skill を使う。

## よく使うコマンド

| 用途 | コマンド |
| ---- | -------- |
| エディタ状態 | `Editor.Status`, `PlayMode.Status`, `Console.GetLog` |
| シーン | `Scene.Open`, `Scene.Save`, `Scene.GetActive`, `Scene.List` |
| GameObject | `GameObject.Find`(tag 検索可), `GameObject.GetHierarchy`, `GameObject.GetComponents`, `GameObject.Create`, `GameObject.AddComponent`, `GameObject.SetTransform`, `GameObject.SetParent`, `GameObject.SetActive` |
| コンポーネント値 | `Component.SetProperty`(SerializedProperty の propertyPath 指定。プレイモード中も使える) |
| プレハブ | `Prefab.Instantiate`, `Prefab.Apply`, `Prefab.Save`, `Prefab.GetStatus`, `Prefab.Unpack` |
| アセット | `AssetDatabase.Find`, `AssetDatabase.GetPath`, `AssetDatabase.Import`, `Search` |
| メニュー実行 | `Menu.List`, `Menu.Execute`(`[MenuItem]` 付きエディタスクリプトの起動に使う) |
| テスト | `TestRunner.RunEditMode`, `TestRunner.RunPlayMode` |
| スクリーンショット | `Screenshot.Capture`(ユーザーが明示的に依頼した場合のみ。**プレイモード必須**) |

## Eval の掟(重要)

- `Eval` は **エディットモード専用**。**プレイモード中に Eval を実行するとコンパイルが保留され、サーバー全体が「Server is busy executing 'Eval'」で固まる(タイムアウトしない)**。復旧にはエディタ側でプレイモードを止めるしかない。
- プレイモード中の確認・操作は非 Eval コマンドのみ使う: `PlayMode.*`, `GameObject.Find`, `GameObject.GetComponents`, `Component.SetProperty`, `Console.GetLog`。`Screenshot.Capture` はユーザーが明示的に依頼した場合だけ使う。
- Eval のコード内では `Object` が曖昧になるため `UnityEngine.Object` のように**完全修飾**する。
- 複雑・反復的な処理は Eval に長いコードを渡すより、`Assets/Scripts/Editor/` に `[MenuItem]` 付きエディタスクリプトを書き、`Compile` → `Menu.Execute` で実行するほうが安全で再利用できる(例: `Assets/Scripts/Editor/Kaze1MapBuilder.cs`)。

## トラブルシューティング

- コマンドが返ってこない → プレイモード中に Eval を投げていないか。エディタ側でプレイモード停止。
- 編集が反映されない → `AssetDatabase.Import` 忘れ、または `Compile` 忘れ。
- 何かがおかしい → `unicli exec Console.GetLog '{"logType":"Error"}'` でエラーを必ず確認。
