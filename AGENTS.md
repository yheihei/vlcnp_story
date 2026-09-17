# AGENTS.md

日本語で応答する。

## プロジェクト

Unity 6.3 LTS・URPのメトロイドヴァニア型2Dアクション。Windows / macOS StandaloneをSteamで配信する。正確なエンジン・パッケージ版は `ProjectSettings/ProjectVersion.txt` と `Packages/manifest.json`・`packages-lock.json` を参照する。

直近は2026年10月のSteam Next Festに向け、体験版の完成度を優先する。2026年12月末発売、¥980、日本語のみ。目標は発売時ウィッシュリスト2,500、発売後1年の累計販売1,000本。

## 実装範囲と参照

- ゲームロジックは `Assets/Scripts/`、Prefab・素材は `Assets/Game/`、シーンは `Assets/Scenes/`。その他のアセットフォルダは実装対象外。
- C#・Prefab・シーンの変更時は [プロジェクト規約](.agents/skills/unity-project-conventions/SKILL.md) を参照する。
- Editor操作時は [unity-editor-automation](.agents/skills/unity-editor-automation/SKILL.md) を使う。公式Unity CLIと `com.unity.pipeline` が対象。旧UniCLI用Skillは使わず、UniCLIも再導入しない。
- Editorが応答しない場合はComputer Useで状態を確認し、[復旧手順](.agents/skills/unity-editor-automation/references/editor-recovery.md) に従って復帰させる。
- 検証は変更内容に応じて行う。文書・Skillだけの変更ではUnityの起動・コンパイル・Play Modeは不要。

## Skillの扱い

プロジェクト固有Skillは `.agents/skills/`。`.claude/skills` はそこへのシンボリックリンクなので、編集は実体側で行う。作業に必要なSkillと、その作業に関係する参照だけを読む。

Codexには `unity@unity-agent-plugin` を導入済み。公式Skillは各クライアントで利用可能なものから選ぶ。公式の一般的な推奨だけで既存のUI方式・フォント・アセット・パッケージ構成を一括変更しない。プラグインキャッシュや公式Skillのローカルコピーに独自ルールを書き足さず、このファイルかプロジェクト固有Skillへ記載する。

Unity 6・公式CLIへの移行経緯が必要な場合は [移行記録](docs/unity6-migration-2026-09-17.md) を参照する。2022版の記録は旧環境の履歴。

## Git

- `main` に直接commit・pushしてよい。依頼がなければ作業用ブランチを新設せず、既存の作業ブランチも勝手に切り替えない。
- commit messageは日本語。関連issueがある場合は `#issue番号` を含める。
