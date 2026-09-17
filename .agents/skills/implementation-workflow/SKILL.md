---
name: implementation-workflow
description: vlcnpStory2022 の機能追加・不具合修正で、既存実装の再利用と変更に応じた検証範囲を判断する。
---

# 実装ワークフロー

依頼された結果が実装され、必要な検証と保存が済むまで進める。完了条件は依頼と既存の文脈から判断し、結果を左右する不足情報だけ確認する。

- 機能追加や設計変更では、関連する既存実装を検索する。再利用候補は `Health`、`Flag`、`FallMissZone`、`CameraConfineArea` など。小さな修正に全体調査を追加しない。
- C#・アセットの配置やシリアライズを変えるときは [プロジェクト規約](../unity-project-conventions/SKILL.md) を使う。
- Unity を操作するときは [Editor 操作](../unity-editor-automation/SKILL.md) を使う。まとまった C# 変更を Import して Compile し、その後に編集がなければ同じ検証を繰り返さない。
- 実行時の挙動は関連テストで確認する。直接観察が必要なら [Play Mode 検証](../unity-playmode-verification/SKILL.md) を使う。シーン・Prefab は対象の保存も確認する。
- 文書・Skill だけの変更では、参照や形式を検証する。Unity の起動・コンパイル・Play Mode は不要。
- 失敗が続く場合はログや状態を調べ、原因に応じて手段を変える。回数だけを理由に中断しない。
- commit / push を行うときは [Git 規約](../git-workflow/SKILL.md) を使う。

日本語で、変更結果・実際の検証結果・重要な未検証事項を伝える。既存のエラーと変更で生じたエラーを区別し、未検証の動作を確認済みとしない。
