---
name: unity-editor-automation
description: >-
  Project-specific supplements for Unity Editor automation in vlcnpStory2022.
  Apply together with the `unity-development` skill when a task uses UniCli;
  this skill adds the project workflow, save requirements, and the Play Mode
  Eval deadlock safeguard, plus safe cleanup of stale Compiling Scripts
  progress dialogs after automation.
---

# Unity エディタ操作のプロジェクト補足

UniCli による標準的な操作、コマンド探索、`AssetDatabase.Import`、コンパイル、テストの手順は、必ず **`unity-development` skill** に従う。本 skill はその内容を重複させず、vlcnpStory2022 固有の追加ルールだけを定める。

## 作業フロー

1. Unity エディタを操作するタスクでは、まず `unity-development` skill を適用する。
2. 変更種別に応じた実装・検証は `implementation-workflow` skill に従う。ランタイム観察が必要なときだけ `unity-playmode-verification` skill を追加で使う。
3. シーンを変更したら `Scene.Save` を実行する。プレハブを変更したら `Prefab.Save` または `Prefab.Apply` を実行し、未保存の変更が残っていないことを `Editor.Status` で確認する。
4. UniCli のコマンドや引数は記憶に頼らず、`unity-development` で定めるコマンド探索手順で確認する。

## 作業完了時の残留コンパイルダイアログ

C# の変更、`Compile`、または `Eval` を伴う作業では、最終報告の前に次を実行する。

1. `unicli exec Editor.Status` で `Playing`、`Compiling`、`Updating`、`Dirty scenes` を確認する。
2. `Compiling: True` または `Updating: True` なら実処理中なので進捗表示を消さず、完了を待ってから再確認する。
3. `Compiling: False` かつ `Updating: False` になったら、UI を確認できる環境では `computer-use` skill を適用し、Unity の最前面に `Compiling Scripts` / `ScriptCompilation: Running Backend` が残っていないかアクセシビリティ情報で確認する。ユーザーから残留の報告があった場合も同じ扱いにする。
4. ダイアログが残り、かつ `Playing: False` の場合だけ、次を一度実行する。

```bash
unicli eval 'UnityEditor.EditorUtility.ClearProgressBar(); return true;' --json
```

5. UI と `Editor.Status` を再確認し、ダイアログが消え、`Compiling: False` / `Updating: False` であることを確認してから完了を報告する。

- `Playing: True` の間は上記 Eval を実行しない。エージェント自身が開始した Play Mode なら停止してから再確認し、ユーザーが開始した可能性がある場合は勝手に停止しない。
- 解除に失敗しても Eval やコンパイルを繰り返さない。Editor ログと `bee_backend` の有無を調査して報告する。
- `Dirty scenes: Yes` の場合、残留ダイアログを理由に Unity を終了・再起動したり、所有者不明の変更を保存・破棄したりしない。

## Eval の重要な制約

- `Eval` は **エディットモード専用**。プレイモード中に実行するとコンパイルが保留され、サーバー全体が `Server is busy executing 'Eval'` のまま固まる。タイムアウトでは復旧せず、エディタ側でプレイモードを停止する必要がある。
- プレイモード中の確認・操作には `Eval` ではなく、対応する非 Eval コマンドを使う。スクリーンショットはユーザーが明示的に依頼した場合だけ取得する。
- Eval 内では `Object` の曖昧さを避けるため、`UnityEngine.Object` のように完全修飾する。
- 複雑または反復的な処理は長い Eval ではなく、`Assets/Scripts/Editor/` に `[MenuItem]` 付きエディタスクリプトを置き、コンパイル後にメニューから実行する。例: `Assets/Scripts/Editor/Kaze1MapBuilder.cs`。

## 問題が起きた場合

- コマンドが返らない場合は、プレイモード中に Eval を実行していないか確認し、該当すればエディタ側でプレイモードを停止する。
- エラーや警告の確認、アセットの再インポート、コンパイル確認は `unity-development` skill の手順に従う。
