---
name: unity-editor-automation
description: >-
  Project-specific supplements for Unity Editor automation in vlcnpStory2022.
  Apply together with the `unity-development` skill when a task uses UniCli;
  this skill adds the project workflow, save requirements, and the Play Mode
  Eval deadlock safeguard.
---

# Unity エディタ操作のプロジェクト補足

UniCli による標準的な操作、コマンド探索、`AssetDatabase.Import`、コンパイル、テストの手順は、必ず **`unity-development` skill** に従う。本 skill はその内容を重複させず、vlcnpStory2022 固有の追加ルールだけを定める。

## 作業フロー

1. Unity エディタを操作するタスクでは、まず `unity-development` skill を適用する。
2. 変更種別に応じた実装・検証は `implementation-workflow` skill に従う。ランタイム観察が必要なときだけ `unity-playmode-verification` skill を追加で使う。
3. シーンを変更したら `Scene.Save` を実行する。プレハブを変更したら `Prefab.Save` または `Prefab.Apply` を実行し、未保存の変更が残っていないことを `Editor.Status` で確認する。
4. UniCli のコマンドや引数は記憶に頼らず、`unity-development` で定めるコマンド探索手順で確認する。

## Eval の重要な制約

- `Eval` は **エディットモード専用**。プレイモード中に実行するとコンパイルが保留され、サーバー全体が `Server is busy executing 'Eval'` のまま固まる。タイムアウトでは復旧せず、エディタ側でプレイモードを停止する必要がある。
- プレイモード中の確認・操作には `Eval` ではなく、対応する非 Eval コマンドを使う。スクリーンショットはユーザーが明示的に依頼した場合だけ取得する。
- Eval 内では `Object` の曖昧さを避けるため、`UnityEngine.Object` のように完全修飾する。
- 複雑または反復的な処理は長い Eval ではなく、`Assets/Scripts/Editor/` に `[MenuItem]` 付きエディタスクリプトを置き、コンパイル後にメニューから実行する。例: `Assets/Scripts/Editor/Kaze1MapBuilder.cs`。

## 問題が起きた場合

- コマンドが返らない場合は、プレイモード中に Eval を実行していないか確認し、該当すればエディタ側でプレイモードを停止する。
- エラーや警告の確認、アセットの再インポート、コンパイル確認は `unity-development` skill の手順に従う。
