# Unity公式Skillsの試用結果

2026-09-17に、現在の作業を保存してから公式プラグインを導入した。

## 採用範囲

分野別の手順書として採用する。Editor操作はUnity 2022.3で動いているUniCliを継続する。
UniCli自体を維持する必要があるという判断ではなく、公式のEditor接続がUnity 6.0以降を対象としているため。

公式版への完全移行は、別コピーでUnity 6への移行とWindows/macOSの動作を検証してから判断する。今回、移行による速度・品質・安定性の改善は測定していない。

## 保存と導入

- 作業前の差分55ファイルを `c4010f64` にcommitし、`origin/main` へのpushを確認。
- Codex CLI: `0.153.3`。
- 追加したプラグイン: `unity@unity-agent-plugin`、`0.1.6-beta`。
- 配布元: `https://github.com/Unity-Technologies/unity-agent-plugin.git`。
- 調査した最新版のcommit: `5ad4bafc1d91acfdeca800bfbae0072fcc7aeb58`。
- `codex plugin list --marketplace unity-agent-plugin --json` で `installed: true`、`enabled: true` を確認。
- インストール先の `skills/*/SKILL.md` は31個。プラグインはユーザー環境にあり、ゲームのGitリポジトリには含まれない。
- 導入したSkillsの自動読み込みは新しいCodexセッションから。今回の試用ではインストール先の手順書を直接読んだ。

## Editor接続

- プロジェクト: Unity `2022.3.62f3`、URP `14.0.12`、TextMeshPro `3.0.7`。
- 既存の公式Unity CLIは `1.0.0-beta.8`。今回インストールしたものではない。
- `unity pipeline list --format json` は起動中の対象Editorを検出したが、`hasPipelinePackage: false`、`isReachable: false`。
- 公式資料でPipelineの前提がUnity 6.0以降であることを確認。2022.3へ強制導入する試験は行っていない。
- UniCliのクライアント・サーバーはともに `1.6.0`。接続成功、115コマンドを検出。
- Unity・プロジェクトのパッケージ・ゲームアセットは更新していない。

## uGUI Skillの実地確認

公式の `ui` と `ui-ugui` を読み、`Assets/Game/Core/Core.prefab` の `Core/CMCamera/HUD/BossStatus` を点検した。
Editorへの読み取りはUniCliのEvalをEdit Modeで実行。公式CLI経由の操作を検証したものではない。

- Canvasは `ScreenSpaceCamera`、CanvasScalerは `ScaleWithScreenSize`、基準解像度は1600×900。
- HP Sliderの範囲は0〜1、`fillRect` は `Fill` を参照し、操作不可に設定済み。
- 装飾・文字のGraphicは6個で、すべて `raycastTarget: false`。
- ボス名と二つ名は既存の `UnityEngine.UI.Text`。どちらにも `DotGothic16-Regular` が割り当てられている。
- HUDのCanvasにGraphicRaycasterはないが、今回の対象は操作しない表示用UIなので追加していない。
- アセットを読み込んだ状態のFill寸法だけでは実行時の表示を判定できない。今回は表示不具合と認定していない。
- シーンは `Kaze3_boss_2`。点検時のPlay Mode・コンパイル中・未保存シーンはいずれもなし。

アンカー、親のLayout制御、Canvas、イベント、Raycastを順に確認する手順は、既存の汎用作業手順に追加できる具体的な知識だった。一方、今回の対象に修正が必要な問題は見つからず、開発時間短縮や品質向上の比較試験は行っていない。

試用後に `Compile` が成功し、エラー・警告はともに0件。ConsoleのWarning/Errorも0件。未保存シーンがないことを確認した。Git差分は運用条件とこの記録の追加だけで、ゲームデータの差分はない。

## 利用時の注意点

31個のうち、少なくとも10個の分野別Skillは本文から `unity-cli` を直接参照する。たとえば `optimize-audio` は公式CLIのEvalを実行前提としているため、その手順を2022.3でそのまま実行できるとは扱わない。

`ui-ugui` のTextMeshPro推奨は、既存のTextを移行する理由にはしない。`optimize-text-mesh-pro` の一部機能にはTMP 3.2以降の条件があり、現在の3.0.7との照合が必要。公式資料でも対象バージョンと既存構成を確認して使う。

## 参照

- [Unity公式プラグインと対応条件](https://docs.unity.com/en-us/ai/unity-plugin/about-unity-plugin)
- [Unity Pipelineと対応条件](https://docs.unity.com/en-us/unity-production-pipeline/local-tools-cli/unity-pipeline-package)
- [公式uGUI Skill](https://github.com/Unity-Technologies/unity-agent-plugin/blob/5ad4bafc1d91acfdeca800bfbae0072fcc7aeb58/skills/ui-ugui/SKILL.md)
- [Codexプラグインの読み込み](https://learn.chatgpt.com/docs/plugins)
