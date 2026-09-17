---
name: unity-editor-automation
description: vlcnpStory2022 を UniCli で操作するときの保存、一時スクリプト、Eval 停止対策。
---

# Unity Editor 操作の補足

UniCli のコマンド探索・Import・Compile は、利用可能な `unity-development` skill に従う。ここではこのプロジェクト固有の制約を扱う。

## 保存と一時処理

- シーンは `Scene.Save`、Prefab は変更方法に応じて `Prefab.Save` / `Prefab.Apply` で保存する。対象の保存を確認し、他の作業の dirty 状態を一括保存・破棄して解消しない。
- シーン・Prefab の実データを正とする。自動処理は対象への差分更新にし、全削除・再配置でユーザーの手調整を上書きしない。
- 反復処理に一時的な `[MenuItem]` スクリプトを使った場合、作業後にそのスクリプトと `.meta` を除く。追跡済みなら `git rm`、未追跡なら通常の削除を使う。繰り返し使う計測・検証ツールは残してよい。
- 一時 C# の削除後も Refresh / Import と Compile を確認する。古い生成テーブルの再実行を将来の編集方法として残さない。

## Eval の制約

- `Eval` は Edit Mode 専用。Play Mode 中はコンパイル待ちに陥り、サーバーが `Server is busy executing 'Eval'` のまま停止する。タイムアウトで解消しない。
- Play Mode 中は対応する非 Eval コマンドを使う。挙動の観察手順は必要なときだけ [Play Mode 検証](../unity-playmode-verification/SKILL.md) を読む。
- Eval の型名が曖昧なら `UnityEngine.Object` のように完全修飾する。長い処理や再利用する処理にはエディタスクリプトを検討する。

## 応答やダイアログに問題があるとき

タイムアウト、残留した `Compiling Scripts`、操作不能の報告がある場合に [Editor の復旧](references/editor-recovery.md) を読む。正常に完了したすべての操作へ UI 点検や復旧処理を追加しない。
