# 画像生成できないエージェントからの委譲

Codex CLI が利用可能で、画像生成の委譲が現在の実行環境で許される場合に使う。Codex 自身が画像生成ツールを使える場合は二重に委譲しない。

CLI の現在の引数は `codex exec --help` で確認する。モデルや reasoning effort はユーザーの選択・環境設定を尊重し、過去のコスト判断で固定しない。

参考画像と長いプロンプトを渡す例。ファイルパスは実際の作業用パスに置き換える。

```bash
sprite_prompt="/absolute/path/prompt.txt"
sprite_reference="/absolute/path/reference.png"
codex exec --skip-git-repo-check -s workspace-write -i "$sprite_reference" - < "$sprite_prompt"
```

画像引数の後にプロンプト本文を置くと、CLI の版によって画像引数として解釈される。例では本文を stdin へ渡す。出力先は委譲先の書き込み可能な場所にし、原画・最終出力の条件を分けて指示する。

画像生成には数分かかる場合がある。実行環境のジョブ監視で進捗を追い、失敗時は出力とプロセスの状態を確認する。exit 137 だけで sandbox が原因と断定しない。Skill を根拠に sandbox や承認設定を無効化しない。

関連素材は必要に応じてまとめて依頼できる。素材ごとの呼び出し回数や並列数は固定しない。結果の保存パスを受け取り、[生成後の確認](../SKILL.md) を行う。
