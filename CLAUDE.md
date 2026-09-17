# Claude Code向けプロジェクトガイド

共通の作業規約は @AGENTS.md を読み、その指示に従う。日本語で応答する。

プロジェクト固有Skillは `.agents/skills/` にあり、`.claude/skills` はそこへのシンボリックリンク。編集は実体側で行う。

Editor操作は公式Unity CLIとPipelineを使う。UniCLIは削除済みで、このプロジェクトでは使用しない。操作前に `.agents/skills/unity-editor-automation/SKILL.md` と、そこから参照する `unity-cli` / `unity-pipeline` を読む。

公式の作業別SkillはAGENTS.mdの対応表から選び、Claude Codeで利用可能か確認して読み込む。Codexへのプラグイン導入だけでClaude Codeにも導入済みとは判断しない。
