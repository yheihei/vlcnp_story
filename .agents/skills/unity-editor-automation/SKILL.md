---
name: unity-editor-automation
description: vlcnpStory6 を公式Unity CLIとPipelineで操作するときの接続、保存、一時処理、復旧手順。
---

# Unity Editor 操作の補足

公式Unity CLIと `com.unity.pipeline` を使う。CLIの構文・導入は [unity-cli](../unity-cli/SKILL.md)、実行中Editorへの接続や実行方法は [unity-pipeline](../unity-pipeline/SKILL.md) の必要な箇所を参照する。

## 接続と実行

- `unity status --format json` で対象と接続を確認する。コマンドと引数は現在の `unity command` を正とする。
- 操作には `--project-path <このプロジェクトの絶対パス>` と `--caller plugin --skill <使用Skill名>` を付ける。CLIは1.0.0-beta.10以降が必要。
- 自動操作前に `set_autotick --enable true`。編集前は `editor_status` とシーンのdirty状態を確認する。dirtyは `UnityEngine.SceneManagement.SceneManager` から取得できる。
- C#変更後は `recompile` を起動し、`recompile_status` の完了とエラーを確認する。ドメイン再読み込み中の一時的な切断を失敗と決めつけない。
- パッケージ操作では、公式 `unity-package-management` のCLI非対応という記述と、接続中Pipelineの機能を区別する。追加・削除は現在のコマンド一覧に存在する場合、 `package_add` / `package_remove` を使い、`package_status` と再コンパイル完了を確認する。

公式Skillの `run_script` にあるビルダー保存例やRuntime code reloadは、毎回必要な工程ではない。既存のシーン・Prefabの差分更新を基本とし、検査のためだけに `[CodeReload]` や永続ビルダーを追加しない。

## 保存と一時処理

- シーンは対象を指定した `save_scene`、Prefabは変更方法に応じた保存APIで保存する。他の作業のdirty状態を一括保存・破棄して解消しない。
- シーン・Prefabの実データを正とする。自動処理は対象への差分更新にし、全削除・再配置でユーザーの手調整を上書きしない。
- 複数行の処理はファイルに書いて `run_script` で実行する。一時検査はAssets外に置ける。残すEditor拡張は `Assets/Scripts/Editor/` に置く。
- 一時C#をAssets内に置いた場合は作業後に `.meta` とともに除き、Refreshと再コンパイルを確認する。古い生成処理を将来の編集方法として残さない。
- `eval` は短い単発処理に限る。型名が曖昧なら `UnityEngine.Object` のように完全修飾する。

## Play Modeでの観察

実行中の挙動を直接見る必要がある場合に行う。有効な自動テストで確認できる変更に追加しない。

- Play Modeは `editor_play` / `editor_stop`、観察は `find_gameobjects`、`get_component_properties`、`console`、`capture_game_view` を使う。時間・フレームの観察には `editor_pause` やEditorのStepボタンを使える。
- Play Mode中に再コンパイルを伴う検証コードを追加しない。
- 検証のために開始したPlay Modeは観察後に終了する。採用する調整値は終了前に記録し、終了後にEdit Modeへ反映して対象を保存する。Play Mode中の変更は自動保存されない。
- Consoleの新規Error / Exceptionを確認する。共有コンポーネントなど影響が広い変更でなければ、代表シーンを巡回する回帰確認は追加しない。

## 復旧

タイムアウトは処理の取消しではない。初回のシーン読み込みがタイムアウトしても後から完了する場合がある。同じ操作を再送せず、画面とEditorログを確認する。操作不能や残留する進捗表示には [Editorの復旧](references/editor-recovery.md) を使う。
