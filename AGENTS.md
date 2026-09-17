# AGENTS.md
# ───────────────────────────────────────────────
# Unity プロジェクト用 Codex ガイドライン
# ───────────────────────────────────────────────

## 受け答えの方法

ユーザーには日本語でレスポンスせよ

## Skills(エージェント共通の作業手順書)

作業別の手順書が `.agents/skills/<name>/SKILL.md` にある。該当する作業を始める前に対応する SKILL.md を読み、その手順に従うこと。
(Claude Code からは `.claude/skills` → `.agents/skills` のシンボリックリンク経由で同じものが見える)

### Unity公式Skills

- Codexに `unity@unity-agent-plugin` を導入済み。作業に合う公式Skillとプロジェクト固有Skillを併用する。共通ルールはこのAGENTS.mdにまとめ、CLAUDE.mdからも参照する。
- Editor操作は公式Unity CLIと `com.unity.pipeline` を使う。UniCLIパッケージは削除済み。グローバルに残る `unicli:unity-development` / `unity-development` はこのプロジェクトでは使用せず、再導入もしない。
- 接続・保存・復旧は [unity-editor-automation](.agents/skills/unity-editor-automation/SKILL.md)、CLI仕様はローカルの [unity-cli](.agents/skills/unity-cli/SKILL.md) と [unity-pipeline](.agents/skills/unity-pipeline/SKILL.md) を読む。公式Skill内の省略例にも、Editor対象の操作では `--project-path` と `--caller plugin --skill <使用Skill名>` を補う。
- 公式Skillは各クライアントで利用可能な一覧から読み込む。見つからない場合は導入元を確認し、読めていないSkillを適用済みとしない。プラグインキャッシュや公式Skillのローカルコピーへプロジェクト独自ルールを書き足さない。
- 依頼と明示済みの方針を優先し、公式Skillの一般的な推奨だけで既存のUI方式・フォント・アセット・パッケージ構成を一括変更しない。必要な入力は既存設定から確認し、結果を左右する不足情報だけ質問する。
- エンジンは `ProjectSettings/ProjectVersion.txt`、パッケージは `Packages/manifest.json` と `Packages/packages-lock.json`、操作の有無と引数は現在の `unity command` で確認する。

| 作業 | 公式Skillとプロジェクト手順 |
| --- | --- |
| 機能追加・不具合修正 | `implementation-workflow` と `unity-project-conventions`。Editorを操作する段階で `unity-editor-automation` |
| UI・HUD | `ui` で方式を確認し、既存Canvasには `ui-ugui`。Editor拡張は対象に応じて `ui-imgui` / `ui-uitk` |
| スプライトのスライス・pivot・outline | `sprite-editor`。新規PNGの生成は `generate-2d-sprite` |
| タイル・建物・ステージ素材 | `building-tilemap-builder` / `stage-map-assets`。Palette作成は `tilemap-palette-create`。RuleTileが必要な場合だけ `tilemap-ruletile-createfromsegment` など |
| ドット絵の表示 | `2d-pixel-perfect`。現在のURPと既存カメラ設定を確認して対象だけ調整 |
| 音声 | Mixerへの振り分けは `audio-setup-mixers`、性能・読み込み設定の診断は `optimize-audio` |
| 性能 | `unity-performance-optimizer`。対象に応じて `manage-sprite-atlas` / `optimize-text-mesh-pro` など |
| 体験版ビルド・配信 | `steam-demo-release`。既存ビルドメニューとWindows / macOS用手順を使う |

上表以外も依頼に合う公式Skillを選ぶ。`new-unity-project` や `migrate-birp-to-urp` は既存プロジェクトの通常作業には使わない。現在はUnity 6・URPへ移行済み。

導入時の参照元は [Unity公式Skills](https://github.com/Unity-Technologies/unity-agent-plugin/tree/c7055702b44e58402ee481ff408aee407bcf9483/skills)。Unity 6移行時の検証記録は [移行記録](docs/unity6-migration-2026-09-17.md)。[2022版の試用記録](docs/unity-official-skills-trial-2026-09-17.md) は旧環境の履歴であり、現在の操作手順には使わない。

## プロジェクト概要
- **エンジン**: Unity 6.3 LTS / 6000.3.24f1
- **ビルド対象**: Windows / macOS Standalone(Steam 配信)
- **主目的**: メトロイドヴァニア型 2D アクション
- **ゴール**: 2026年12月末に Steam でリリース(¥980 / 発売時は日本語のみ)。発売時ウィッシュリスト 2,500 → 発売後1年で累計販売 1,000 本
- **直近マイルストーン**: 2026年10月の Steam Next Fest 出展。体験版の完成度が最重要

## ディレクトリ規約
| フォルダ | 用途 |
| -------- | ---- |
| `Assets/Scripts/` | ゲームプレイロジック |
| `Assets/Game/` | 各種Prefab |
| `Assets/Scenes/` | シーン |

その他は不使用。実装対象外。

## Unityがハングした場合

- Unity Editorがハングした、または操作不能になった場合は、Computer Useで画面と応答状態を確認し、状況に応じた操作で復帰させること。ツールのタイムアウトだけでハングと判断せず、コンパイルやインポートが進行中か確認する。
- ダイアログへの応答、停止可能な処理のキャンセル、Play Modeの終了など、未保存の作業を失わない操作から試す。応答不能なAPI呼び出しを繰り返して放置しない。
- 通常操作で復帰しない場合は、可能な範囲で作業を保存してから、Computer UseでUnity Editorを終了・再起動する。強制終了は通常終了もできない場合の最終手段とし、対象プロジェクトのEditorだけに行う。
- 復帰後は対象プロジェクトとシーン、Consoleの状態を確認して作業を再開する。未保存の変更が失われた可能性があればユーザーに報告する。

## Git作業の決まり

- すべてmainブランチで作業してpushしてよい
- issue がある場合は `#issue番号` のmessageをつけてcommitすること
