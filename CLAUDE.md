# AGENTS.md
# ───────────────────────────────────────────────
# Unity プロジェクト用 ガイドライン
# ───────────────────────────────────────────────

## 受け答えの方法

ユーザーには日本語でレスポンスせよ

## Skills(エージェント共通の作業手順書)

作業別の手順書が `.agents/skills/<name>/SKILL.md` にある。**該当する作業を始める前に必ず対応する SKILL.md を読み、その手順に従うこと。**
(Claude Code からは `.claude/skills` → `.agents/skills` のシンボリックリンク経由で同じものが見える)

| skill | 読むタイミング |
| ----- | -------------- |
| `implementation-workflow` | すべてのタスクの開始時 |
| `unity-editor-automation` | UniCli で Unity エディタを操作するとき(C# 編集後のコンパイル、シーン/プレハブ/アセット操作、プレイモード) |
| `unity-playmode-verification` | 変更を「完了」と報告する前 |
| `unity-project-conventions` | C# スクリプト・プレハブ・シーンを新規作成/編集するとき |
| `git-workflow` | commit / push するとき |

## プロジェクト概要
- **エンジン**: Unity 2022.3
- **ビルド対象**: WebGL
- **主目的**: メトロイドヴァニア型 2D アクション
- **ゴール**: 年末リリース、月間アクティブ 1,000 人

## ディレクトリ規約
| フォルダ | 用途 |
| -------- | ---- |
| `Assets/Scripts/` | ゲームプレイロジック |
| `Assets/Game/` | 各種Prefab |
| `Assets/Scenes/` | シーン |

その他は不使用。実装対象外。

## Git作業の決まり

- すべてmainブランチで作業してpushしてよい
- issue がある場合は `#issue番号` のmessageをつけてcommitすること
