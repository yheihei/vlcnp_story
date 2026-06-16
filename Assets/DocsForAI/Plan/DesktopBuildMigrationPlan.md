# Desktop Build Migration Plan

## 背景と目的
- 対象 issue: #603
- 子 issue: #636 / #637
- 更新日: 2026-06-16
- WebGL から Windows / macOS Standalone と Steam 配信へ移行するため、親チケットとしての現状、判断、残作業を整理する。
- クラウドセーブは #627、実績は #605、Next Fest 提出管理は #635 で扱う。

## 現時点の到達点
- Unity バージョンは `2022.3.62f3`。
- #636 で Mac Standalone の事前検証は完了済み。
  - 通常 C# コンパイル: 成功。
  - `StandaloneOSX` 向けスクリプトコンパイル: 成功。
  - `StandaloneWindows64` 向けスクリプトコンパイル: 成功。
  - Mac Standalone Development Build: 成功。
  - Mac Player 起動、タイトルからゲーム画面への遷移、基本キー入力後の継続稼働を確認済み。
  - 詳細: `Assets/DocsForAI/Plan/MacStandaloneMigrationProbe.md`
- 2026-06-16 に `main` 上でも `StandaloneWindows64` 向けスクリプトコンパイルを再確認し、50 assemblies / error 0 / warning 0。
- 2026-06-16 に Unity 2022.3.62f3 へ Windows Build Support (Mono) を追加し、Mac から `StandaloneWindows64` Steam Demo Release Build の作成を確認済み。
  - 出力先: `/tmp/vlcnpStory_SteamDemoWindowsBatch/VlcnpStory.exe`
  - 結果: 成功。`VlcnpStory.exe`, `UnityPlayer.dll`, `VlcnpStory_Data/` を確認済み。
  - Steamworks native plugin: `VlcnpStory_Data/Plugins/x86_64/steam_api64.dll`
  - 既存 Editor プロセスでは module 追加前の状態が残り `Build target 'StandaloneWindows64' not supported` になったため、module 追加後は Unity 再起動または fresh batchmode で実行する。

## プロジェクト設定メモ
- `companyName`: `YheiWebDesign`
- `productName`: `VlcnpStory`
- `bundleVersion`: `0.2.1`
- `applicationIdentifier`: `com.yheiwebdesign.vlcnpstory`
  - Steam / Standalone 配布に向けて固定済み。
- Standalone 画面設定:
  - `defaultScreenWidth`: 1920
  - `defaultScreenHeight`: 1080
  - `fullScreenMode`: `FullScreenWindow`
  - `resizableWindow`: false
  - `allowFullscreenSwitch`: true
  - `usePlayerLog`: true
- Windows 実機検証では、フルスクリーン切り替え、Alt+Enter、Alt+F4、ウィンドウクローズ、複数解像度の破綻を確認する。

## 保存と入力の現状
- 保存処理は `Assets/Scripts/Saving/JsonSavingSystem.cs` で `Application.persistentDataPath` 配下に JSON を保存している。
- WebGL 用の `Application.ExternalEval("FS.syncfs...")` は `UNITY_WEBGL && !UNITY_EDITOR` でガード済みのため、Standalone コンパイルには影響しない。
- Windows 実機では以下を Player.log と実ファイルで確認する。
  - 新規保存ファイルが `Application.persistentDataPath` 配下に作成される。
  - ゲーム再起動後にロードできる。
  - セーブ削除後に例外が出ない。
- 入力は `Assets/Scripts/Control/PlayerInputAdapter.cs` に集約され、キーボードと Unity Input System の `Gamepad.current` を併用している。
- Windows 実機では Xbox 系コントローラーを優先して、移動、ジャンプ、攻撃、メニュー決定、キャラ切り替えを確認する。

## Steamworks ラッパー方針
- 初回 Steam 対応は Steamworks.NET を採用する。
- 理由:
  - Unity Package Manager から導入でき、リポジトリ管理しやすい。
  - Valve の Steamworks C++ API に近い wrapper で、後続の実績 #605 / クラウドセーブ #627 に繋げやすい。
  - Windows / macOS / Linux Standalone 対応が明記されている。
  - Steamworks.NET 公式ドキュメントに SteamManager の開始点と `steam_appid.txt` のローカル確認手順がある。
- 2026-06-16 時点の GitHub latest release は `2025.163.0`。
- Package Manager の git URL を release tag で固定済み。
  - `https://github.com/rlabrecque/Steamworks.NET.git?path=/com.rlabrecque.steamworks.net#2025.163.0`
- 参考:
  - https://steamworks.github.io/
  - https://steamworks.github.io/installation/
  - https://github.com/rlabrecque/Steamworks.NET
- Facepunch.Steamworks は C# らしい API が利点だが、初回の起動確認、Overlay、実績、クラウド保存に寄せる段階では Steamworks.NET の方が既存の導入資料と Valve API 対応を追いやすい。

## 最小 Steam 起動実装の範囲
- ストアページは作成済みのため、その App ID を Base App ID として扱う。
- デモ版だけを先行公開する場合も、Steamworks では Base App に紐づく Demo App ID を別途作成する。
- Demo App ID は `1223071`。
- Windows Depot ID / Mac Depot ID が確定したら、`Assets/DocsForAI/Plan/SteamDemoReleasePlan.md` と SteamPipe template に反映する。
- Steamworks.NET 導入後、最初に実装する範囲は起動と終了だけに絞る。
- 起動時:
  - `RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)` で Steam 初期化用 GameObject を作る。
  - `DontDestroyOnLoad` でシーン遷移後も維持する。
  - `SteamAPI.Init()` に失敗しても、Steam 経由ではないローカル起動を即終了させず警告ログに留める。
  - Player.log に `Application.persistentDataPath` を出す。
- 終了時:
  - `OnApplicationQuit` で `SteamAPI.Shutdown()` を呼ぶ。
- ローカル確認:
  - Windows 実機で Steam クライアントを起動する。
  - `steam_appid.txt` を使ってローカル起動する。
  - Shift+Tab で Steam Overlay が表示されることを確認する。
  - Player.log に Steam 初期化成功/失敗と終了処理ログが残ることを確認する。
- この段階では実績 API、クラウドセーブ API、ストア連携、DLC、リッチプレゼンスは実装しない。

## Thirdweb / WebGL 整理方針
- Standalone 移行で優先して削除候補にする対象:
  - `Assets/Core/Wallet/WalletConnectSample.cs`
  - `Assets/ExtraPackage/Thirdweb`
  - `Assets/Plugin/thirdweb.jslib`
  - `Assets/WebGLTemplates/Thirdweb`
- `Assets/WebGLTemplates/StaticAspect` は WebGL 配信を完全終了するまでは維持する。
- Thirdweb / WalletConnect 一式の削除は影響範囲が大きいため、Steam 初回起動確認より前に混ぜない。

## #637 で完了させる Windows 実機ゲート
- Unity 2022.3.62f3 で Windows 環境からプロジェクトを開ける。
- Windows Build Support / IL2CPP モジュールの有無を確認する。
- Windows x86_64 / Standalone へ切り替える。
- Mac 側でも Windows Build Support 追加後に `StandaloneWindows64` release build は作成できている。
- ただし Windows 実機での起動、基本操作、セーブ/ロード、Steam Overlay は未確認。
- Development Build の `.exe` を作成する。
  - Editor メニュー: `Tools/VLCNP/Build/Windows Development Build`
  - batchmode:
    - `Unity.exe -quit -batchmode -projectPath <repo> -executeMethod VLCNP.Editor.DesktopBuildUtility.BuildWindowsDevelopmentFromCommandLine -vlcnpBuildPath <output>/VlcnpStory.exe`
  - `-vlcnpBuildPath` を省略した場合は `Builds/Windows/VlcnpStory.exe` に出力する。
- Steam デモ用 Release Build を作成する。
  - Editor メニュー: `Tools/VLCNP/Build/Steam Demo Windows Release Build`
  - batchmode:
    - `Unity.exe -quit -batchmode -projectPath <repo> -executeMethod VLCNP.Editor.DesktopBuildUtility.BuildSteamDemoWindowsReleaseFromCommandLine -vlcnpBuildPath <output>/VlcnpStory.exe -vlcnpSteamAppId <Demo App ID>`
- `.exe` を単体起動し、Player.log に致命的な例外がない。
- キーボードで開始、移動、ジャンプ、攻撃、メニュー決定ができる。
- ゲームパッドが認識され、基本操作ができる。
- フルスクリーン、解像度、ウィンドウ終了操作で破綻しない。
- セーブ/ロードが `Application.persistentDataPath` 前提で動作する。
- Steamworks.NET 導入後、`steam_appid.txt` で起動し、Steam Overlay 表示を確認する。

## #603 を閉じる条件
- #636 の Mac 事前検証成果が `main` に入っている。
- #637 の Windows 実機ビルド、起動、基本操作、セーブ/ロード確認が完了している。
- Steamworks.NET を採用する方針が確定している。
- Steam 初期化/終了の最小実装方針が決まっている。
- Steam Overlay の確認方法と担当環境が決まっている。
- クラウドセーブ #627 と実績 #605 に進むための前提が揃っている。
