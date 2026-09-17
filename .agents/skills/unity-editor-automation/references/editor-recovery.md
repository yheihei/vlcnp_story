# Editor の復旧

タイムアウトだけでハングと断定しない。利用可能な Computer Use で画面と応答を確認し、Editor ログや API が応答する場合の `Editor.Status` と照合する。応答しない API を連打しない。

## Play Mode 中の Eval 待ち

Play Mode 中の Eval はコンパイル待ちでサーバーを塞ぐ。実行中の Eval を再送せず、Editor の UI で Play Mode を終了する。作業中の状態を保護し、復帰後に対象シーンと Console を確認する。

## 残留するコンパイル表示

`Compiling Scripts` / `ScriptCompilation: Running Backend` が残るときだけ、次を確認する。

1. `Editor.Status` または UI・ログでコンパイルとインポートが実行中でないか調べる。進行中なら表示を消さない。
2. `Compiling: False`、`Updating: False`、`Playing: False` を確認でき、UI に表示だけ残っている場合に限り一度実行する。

```bash
unicli eval 'UnityEditor.EditorUtility.ClearProgressBar(); return true;' --json
```

3. UI と Editor の応答を確認する。消えなければ Eval や Compile を繰り返さず、Editor ログと `bee_backend` の状態を調べる。

自分が開始した Play Mode は終了してよい。表示を消す目的だけで、ユーザーが開始した可能性のある Play Mode を停止しない。所有者不明の dirty シーンを保存・破棄しない。

## 通常操作でも復帰しないとき

AGENTS.md の復旧方針に従い、ダイアログへの応答、停止可能な処理のキャンセル、Play Mode 終了から試す。可能な範囲で作業を保存したうえで対象プロジェクトの Editor を通常終了・再起動する。通常終了もできない場合だけ強制終了を検討する。未保存の変更が失われた可能性は報告し、復帰後にプロジェクト・シーン・Console を確認する。
