---
name: implementation-workflow
description: Proportionate implementation workflow for vlcnpStory2022. Use at the start of non-trivial development tasks in this repository to investigate existing code, make minimal changes, choose verification based on the change type, and report honestly in Japanese.
---

# 実装ワークフロー

この skill は、このリポジトリでタスクを完遂するための標準フローを定める。変更内容に応じた調査と検証だけを適用する。

## 1. 着手前(調査)

- 依頼を自分の言葉で言い換え、完了条件を 1〜2 行で明確にする。issue 番号があれば控える。
- 機能追加や設計変更では、類似の既存実装を先に検索する(grep / `unicli exec Search` / `unicli exec AssetDatabase.Find`)。Health, Flag, FallMissZone, CameraConfineArea など、適用できる既存部品を再利用する。
- シーンまたはプレハブを変更する場合だけ、必要に応じて `Editor.Status` / `GameObject.GetHierarchy` で現状を確認する。

## 2. 実装

- 最小差分で書く。依頼されていないリファクタや「ついで修正」をしない。
- 規約は `unity-project-conventions` skill に従う。
- C# のまとまった変更後と完了報告前に、対象アセットを `AssetDatabase.Import` して `Compile` する(手順は `unity-editor-automation` skill)。編集途中の保存ごとにコンパイルする必要はない。
- 同じアプローチで 3 回失敗したら、繰り返さずにアプローチを変えるか、状況と選択肢をユーザーに報告する。

## 3. 検証

- 変更種別に応じて検証を選ぶ。
  - C# 変更: 最終的に `Compile` する。
  - 実行時の挙動変更: 関連テストを実行し、有効なテストがなければ対象の挙動をプレイモードで確認する。
  - シーン・プレハブ変更: 保存済みを確認し、実行時挙動に影響する場合だけプレイモードを使う。
  - ドキュメント・Skill のみの変更: Unity での検証は不要。
- プレイモードを使う場合は `unity-playmode-verification` skill の安全手順に従う。

## 4. 報告・コミット

- 日本語で報告する: 変更内容 / 検証結果 / 重要な未検証事項。結果の説明に必要な場合を除き、全コマンドを列挙しない。
- コミットは `git-workflow` skill に従う(main 直コミット可、日本語メッセージ、`#issue番号`)。

## 禁止事項

- 検証せずに「動作します」と報告する(推測は推測と明記する)。
- コンパイル結果を確認しないままコミットする。
- Console のエラーや例外を握りつぶして進める(`Console.GetLog` で確認し、原因を潰す)。
