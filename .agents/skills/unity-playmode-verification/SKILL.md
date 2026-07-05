---
name: unity-playmode-verification
description: Verification loop and definition of done for vlcnpStory2022. Use before reporting any code, scene, or prefab change as complete — compile check, Play Mode behavioral observation via UniCli, console error check, and honest reporting of what was and was not verified.
---

# 動作検証と「完了」の定義

## 完了の定義(Definition of Done)

変更を「完了」「動きます」と報告してよいのは、以下を**すべて**満たしたときだけ。

1. `unicli exec Compile` がエラー 0。
2. 変更した動作を**プレイモードまたはテストで実際に観測した**。
3. `Console.GetLog '{"logType":"Error"}'` に今回の変更起因の新規エラー・例外がない。
4. 変更したシーン・プレハブが保存済み(`Editor.Status` の "Dirty scenes" を確認)。

満たせなかった項目がある場合、報告に「**未検証: ○○**」と明記する。推測で「動くはず」と書かない。

## プレイモード検証レシピ

このプロジェクトには独自の自動テストがほぼ無いため、挙動の検証はプレイモードでの実観測が基本。

```bash
unicli exec Scene.Open '{"path":"Assets/Scenes/対象シーン.unity"}'
unicli exec PlayMode.Enter
# 1) 状態を作る: Component.SetProperty でプレイヤーをテレポートさせる等
unicli exec GameObject.Find '{"name":"Player"}'          # tag 検索も可
unicli exec GameObject.GetComponents '{"path":"..."}'    # SerializedProperty 値も見える
unicli exec Component.SetProperty '{...}'                # 位置・フラグ等の状態変更
# 2) 観測する: ログ・値・見た目
unicli exec Console.GetLog '{"maxCount":30}'
unicli exec Screenshot.Capture '{"path":"/tmp/verify.png"}'   # PNG を目視確認
unicli exec PlayMode.Exit
```

- **プレイモード中に Eval を使わない**(サーバーごと固まる。詳細は `unity-editor-automation` skill)。
- 時間経過やフレーム単位の確認には `PlayMode.Pause` / `PlayMode.Step`。
- 実績のある例: Kaze1 の落下ミス検証は Enter → Find/SetProperty で穴の上へテレポート → GetLog で Kill 発火確認 → Exit で行った。

## 見た目(タイル・配置)の検証

- プレイモード中: `Screenshot.Capture` で PNG 保存 → 画像を開いて目視確認。
- エディットモード(タイル配置など): Eval で一時カメラ(`HideFlags.HideAndDontSave`)+ RenderTexture を作り PNG 書き出し → 目視。Eval 内の型は完全修飾すること。

## 回帰確認

変更したコンポーネント・プレハブを使っている代表シーンを 1 つ開き、`PlayMode.Enter` → `Console.GetLog` でエラーが出ていないことを確認してから完了報告する。
