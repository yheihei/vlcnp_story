# Unity 6への移行と公式CLIへの切り替え

2026-09-17、移行用コピー `/Users/yhei/unity/vlcnpStory6` をUnity 6.3 LTSへ更新した。元の `/Users/yhei/unity/vlcnpStory2022` は2022.3.62f3のまま残している。移行前のcommitは `44903530f36d2253cf4b1dce8de78a364121cc26`。

## 構成

| 項目 | 移行後 |
| --- | --- |
| Unity Editor | 6000.3.24f1 / Apple Silicon |
| 追加ビルドモジュール | Windows Build Support / Mono |
| Unity CLI | 1.0.0-beta.10 |
| Unity Pipeline | com.unity.pipeline 0.7.0-exp.1 |
| URP | 17.3.0 |
| Cinemachine | 2.10.7 |
| uGUI | 2.0.0 |
| Unity公式Skills | unity-agent-plugin 0.1.6-beta / 31個 |

Pipelineは実験版。Unity公式Skillsの対応条件を満たし、Editorへの操作も確認できたが、31個すべての個別機能を検証したわけではない。

## 変更

- UnityのAPI Updaterで52個のC#ファイルを変換。主な変更は `velocity` → `linearVelocity`、`drag` → `linearDamping`、`angularDrag` → `angularDamping`。
- Unityの標準アップグレードでパッケージ、URPマテリアル、レンダリング設定、ProjectSettingsを更新。Cinemachine 2と既存uGUIを継続。
- 初回インポート中に空になったFungusエディタ用アイコン参照25件を、公式CLIの `run_script` とSerializedObjectで復元。元のGUID参照を維持。
- `Kaze3_boss_2` で見つかった `DebugExperience.Start` のNullReferenceExceptionに対し、未設定の経験値項目を飛ばすガードを追加。設定済みキャラクターへの処理は維持。
- 公式CLIの `package_remove` で `com.yucchiy.unicli-server` を削除。
- AGENTS.md、CLAUDE.md、プロジェクトのEditor操作・Play Mode・ビルド手順を公式CLIへ切り替えた。
- `unity skill install codex --local` でCLIとPipelineの公式Skillを `.agents/skills/` へ配置。Unity 6が生成する `.slnx` をGit除外対象へ追加。

## 検証

- Unity 6のGUI起動と通常終了・再起動。Safe Modeには入っていない。
- `unity status` の `ready`、`pipeline list` の `isReachable: true` を確認。操作コマンド151個を取得。
- `run_script` でアセットの参照修復とUI検査。`open_scene` でシーン切り替え。`editor_play` / `editor_stop` でPlay Mode操作。
- タイトルのロゴ、日本語メニュー、キャラクター表示を目視確認。
- 公式CLIから既存の `OpeningCursorAlignmentTests.Cursor_AlignsWithSelectedLabel` を実行し、1件成功、失敗0件。選択位置を切り替えた場合のアイコン位置もテスト対象。
- 公式 `ui` / `ui-ugui` SkillでCore Prefabを検査。CanvasはScreenSpaceCamera、ScaleWithScreenSize、1600×900。ボスHP Sliderは0〜1、Fill参照あり、操作不可。ボス名のフォントはDotGothic16-Regular、装飾GraphicのRaycast対象は0件。Core PrefabのMissing Scriptは0件。
- `Kaze3_boss_2` のキャラクター、背景、ボス、プレイヤーHP、ボス名・HPバーの描画を確認。DebugExperience修正後の再コンパイルは失敗なし、同シーンのPlay ModeでError・Warningともに0件。
- Windows / macOSの `BuildPipeline.IsBuildTargetSupported` は両方true。今回、実行ファイルのビルド、Steamアップロード、Windows実機での動作確認は行っていない。

## 初回移行時の注意

初回インポート後はコンパイル警告293件を確認した。253件は非推奨APIのCS0618で、残りは未使用フィールドなど。全警告の解消は今回の対象に含めていない。

初回のタイトルシーン読み込みは30秒を超え、CLIがタイムアウトした後に完了した。同じ操作を再送せず、ログと画面を確認して待った。強制終了はしていない。

初回Play ModeでFMOD音声出力の初期化エラーが1件出た。通常再起動後のタイトル実行では再発せず、Error 0件を確認した。音声を聴いて比較する検証は行っていない。

検証ログと画像はGit対象外の `Logs/unity6-migration/` に保存した。移行差分と操作手順はmainブランチで管理する。

## 今後の操作

このプロジェクトではUniCLIを使わない。以下はプロジェクトルートで実行する例。

```sh
unity status --format json
unity command --project-path "$PWD" --caller plugin --skill unity-cli --detail compact
unity command editor_status --project-path "$PWD" --caller plugin --skill unity-cli --format json
```

公式Skillを使っても、必要パッケージや対象Unityバージョンの条件は作業ごとに確認する。既存UIやアセット構成をSkillの一般論だけで置き換えない。

## 参照

- [Unity公式プラグインの対応条件](https://docs.unity.com/en-us/ai/unity-plugin/about-unity-plugin)
- [Unity Pipelineの対応条件と導入](https://docs.unity.com/en-us/unity-production-pipeline/local-tools-cli/unity-pipeline-package)
- [Unity 6へのアップグレード](https://docs.unity3d.com/6000.3/Documentation/Manual/UpgradeGuideUnity6.html)
- [URP 17へのアップグレード](https://docs.unity3d.com/6000.3/Documentation/Manual/urp/upgrade-guide-unity-6.html)

## 実行ファイルのビルド・起動確認

2026-09-17、既存のSteam Demo Release BuildメニューでWindows / macOSをビルドした。作業開始時から保存済みの `BlockChainRoom.unity` 差分を含む。

- 初回Windowsビルドは、URP Global Settingsに残った `m_EnableRenderCompatibilityMode: 1` が原因で失敗。Unity 6.3では `URP_COMPATIBILITY_MODE` 未定義時の実際の描画はすでにRender Graphであり、保存値を0へ合わせて解消した。
- Windowsの再ビルドは成功。約249.5秒、356,011,941 bytes。`Builds/SteamDemo/Windows/VlcnpStory.exe`、`UnityPlayer.dll`、`steam_api64.dll` を確認。Windows実機での起動は未確認。
- macOSビルドは成功。約288.2秒、374,411,020 bytes。BuildReportはError 0、Warning 178、BuildOptions.None。警告には非推奨API、未使用フィールド、一部マテリアルの2D SRP Batcher非対応、Unity Servicesのプロジェクト連携、Pipeline Runtime設定未作成が含まれる。
- `Builds/SteamDemo/Mac/VlcnpStory.app` の実行ファイルは `x86_64 arm64`。`steam_api.bundle` を確認。
- macOSでは既存のFontLicenseBuildProcessorが署名後にOFL.txtを追加するため、生成直後の署名検証は不整合になった。今回の成果物にはローカル検証用のアドホック再署名を行い、`codesign --verify --deep --strict` が成功。配布用のDeveloper ID署名・公証は未実施。
- このMacでアプリを起動し、CryptoNinja Gamesロゴ、タイトル、日本語メニュー、下キーによる「つづきから」への選択移動を目視確認。実行ログにExceptionはなし。Steam未起動のためSteamAPI初期化は失敗し、既存のフォールバックで起動継続。Steam連携、ゲーム本編の通しプレイ、音声の聴取は今回の確認範囲外。
- 初回ビルドがURPのprefilter/runtime settings、DefaultVolumeProfile、Standalone batching、UnityConnectSettingsのシリアライズを更新した。これらの更新と互換モード修正を作業ツリーに保持。ユーザーのBlockChainRoom差分は変更していない。

検証ログは `Logs/unity6-build-check/`。Steamへのアップロード・公開は実施していない。

起動確認後はアプリのQuitメニューで終了し、プロセス消滅を確認した。終了時ログにはスレッド終了とComputeBuffer解放に関する警告が残った。EditorはBlockChainRoom、dirty=false、Play Mode停止、readyの状態。
