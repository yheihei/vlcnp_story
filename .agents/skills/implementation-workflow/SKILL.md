---
name: implementation-workflow
description: High-quality task execution process for vlcnpStory2022. Use at the start of every non-trivial development task in this repository — investigate existing code first, plan minimal changes, implement following project conventions, verify behavior in the Unity Editor, and report honestly in Japanese.
---

# 実装ワークフロー

この skill は、このリポジトリでタスクを高品質に完遂するための進め方を手順化したもの。手順を省略しない。

## 1. 着手前(調査)

- 依頼を自分の言葉で言い換え、完了条件を 1〜2 行で明確にする。issue 番号があれば控える。
- **類似の既存実装を必ず先に検索する**(grep / `unicli exec Search` / `unicli exec AssetDatabase.Find`)。このプロジェクトには Health, Flag, FallMissZone, CameraConfineArea など再利用できる部品が多い。ゼロから発明しない。
- 触るシーン・プレハブの現状を `Editor.Status` / `GameObject.GetHierarchy` で確認してから手を入れる。

## 2. 実装

- 最小差分で書く。依頼されていないリファクタや「ついで修正」をしない。
- 規約は `unity-project-conventions` skill に従う。
- C# を編集するたびに `AssetDatabase.Import` → `Compile`(手順は `unity-editor-automation` skill)。エラーを残したまま次の編集に進まない。
- 同じアプローチで 3 回失敗したら、繰り返さずにアプローチを変えるか、状況と選択肢をユーザーに報告する。

## 3. 検証

- `unity-playmode-verification` skill の「完了の定義」を満たすまで完了と言わない。

## 4. 報告・コミット

- 日本語で報告する: 変更内容 / 検証した事実(実行したコマンドと結果)/ 未検証事項。
- コミットは `git-workflow` skill に従う(main 直コミット可、日本語メッセージ、`#issue番号`)。

## 禁止事項

- 検証せずに「動作します」と報告する(推測は推測と明記する)。
- コンパイル結果を確認しないままコミットする。
- Console のエラーや例外を握りつぶして進める(`Console.GetLog` で確認し、原因を潰す)。
