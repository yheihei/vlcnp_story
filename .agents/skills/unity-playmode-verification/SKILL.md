---
name: unity-playmode-verification
description: Targeted Play Mode verification guidance for vlcnpStory2022. Use when a runtime behavior change cannot be covered by a useful test, or when a scene or prefab change needs runtime observation. Covers focused observation, console checks, and safe UniCli usage without requiring Play Mode for every change.
---

# プレイモード検証

## 使う場合

- 変更した挙動を直接検証できる自動テストがあれば、それを優先する。
- 有効なテストがなく、実行時の挙動を観測する必要がある場合にプレイモードを使う。
- シーン・プレハブ変更は、実行時の結果が関係する場合だけプレイモードで確認する。
- ドキュメント、Skill、その他 Unity に影響しない変更では使わない。

## 対象を絞った検証

1. 変更した挙動の再現に必要なシーンだけを開く。
2. プレイモードに入り、`GameObject.Find`, `GameObject.GetComponents`, `Component.SetProperty` などの非 Eval コマンドで必要な状態を作る。
3. 対象の状態、値、またはログを観測し、プレイモードを終了する。
4. `Console.GetLog '{"logType":"Error"}'` で、今回の変更起因の新規エラーや例外がないことを確認する。
5. シーンまたはプレハブを変更した場合は、`Editor.Status` または対応する保存・状態確認コマンドで保存済みを確認する。

- **プレイモード中に Eval を使わない**(サーバーが固まることがある。詳細は `unity-editor-automation` skill)。
- 時間経過やフレーム単位の観測が必要な場合だけ `PlayMode.Pause` / `PlayMode.Step` を使う。
- スクリーンショットは標準検証に含めない。`Screenshot.Capture` はユーザーが明示的に依頼した場合だけ使う。

## 報告

実際に観測した結果と、重要な未検証事項を報告する。観測していない挙動を「動きます」と報告しない。代表シーンでの広範な回帰確認は、共有コンポーネントや影響範囲の広い変更に限って行う。
