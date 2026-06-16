# Steam Demo Release Plan

## 前提
- 対象 issue: #603
- 目的: Steam では本編発売前にデモ版だけを先行公開する。
- ストアページは既に作成済み。
- ストアページの App ID は Base App ID として扱う。
- Demo App ID は Base App に紐づく別 App ID として作成する。
- Demo App ID: `1223071`
- Steam publisher: `388474`
- Demo packages:
  - Developer Comp: `1688384`
  - Beta Testing: `1688385`
  - Public Demo package: `1688386`

## Steamworks で確認する ID
- Base App ID: 既存ストアページの App ID。
- Demo App ID: `1223071`
- Demo Windows Depot ID: Demo App の Windows 用 depot。
- Demo Mac Depot ID: Demo App の macOS 用 depot。

## Steamworks 側の設定
- Demo App の Supported OS は Windows / macOS を有効にする。
- Demo App の Launch Options:
  - Windows: `VlcnpStory.exe`
  - macOS: `VlcnpStory.app`
- Demo App の Depots:
  - Windows depot: OS を Windows にする。
  - macOS depot: OS を macOS にする。
- Demo App の packages:
  - Developer Comp package に Windows / macOS depot が含まれていることを確認する。
- Steam Cloud を今回入れる場合:
  - Demo App 側で Steam Cloud を有効化する。
  - Windows は `%USERPROFILE%\AppData\LocalLow\YheiWebDesign\VlcnpStory` 相当を対象にする。
  - macOS は `~/Library/Application Support/YheiWebDesign/VlcnpStory` 相当を対象にする。
  - 実際のパスは Player.log に `Application.persistentDataPath` を出して確認する。

## Unity ビルド
- Windows:
  - Editor menu: `Tools/VLCNP/Build/Steam Demo Windows Release Build`
  - batchmode:
    - `Unity.exe -quit -batchmode -projectPath <repo> -executeMethod VLCNP.Editor.DesktopBuildUtility.BuildSteamDemoWindowsReleaseFromCommandLine -vlcnpBuildPath <output>/VlcnpStory.exe -vlcnpSteamAppId <Demo App ID>`
- macOS:
  - Editor menu: `Tools/VLCNP/Build/Steam Demo macOS Release Build`
  - batchmode:
    - `/Applications/Unity/Hub/Editor/2022.3.62f3/Unity.app/Contents/MacOS/Unity -quit -batchmode -projectPath <repo> -executeMethod VLCNP.Editor.DesktopBuildUtility.BuildSteamDemoMacReleaseFromCommandLine -vlcnpBuildPath <output>/VlcnpStory.app -vlcnpSteamAppId <Demo App ID>`
- `-vlcnpSteamAppId` を渡すとローカル Steam テスト用に `steam_appid.txt` を出力する。
- SteamPipe へアップロードする depot には `steam_appid.txt` を含めない。

## ローカル検証結果
- 2026-06-16: macOS Steam Demo Release Build 成功。
  - 出力: `/tmp/vlcnpStory_SteamDemoMac/VlcnpStory.app`
  - サイズ: `321M`
  - Bundle ID: `com.yheiwebdesign.vlcnpstory`
  - Steamworks native plugin: `VlcnpStory.app/Contents/PlugIns/steam_api.bundle/Contents/MacOS/libsteam_api.dylib`
  - `steam_appid.txt`: `/tmp/vlcnpStory_SteamDemoMac/steam_appid.txt` と `.app/Contents/MacOS/steam_appid.txt` に `1223071`
  - 短時間起動確認: Steam クライアント未起動時は `SteamAPI.Init` が失敗するが、ゲームは継続する。
- 2026-06-16: Windows Steam Demo Release Build 成功。
  - 出力: `/tmp/vlcnpStory_SteamDemoWindowsBatch/VlcnpStory.exe`
  - サイズ: `81M`
  - 必須ファイル: `VlcnpStory.exe`, `UnityPlayer.dll`, `VlcnpStory_Data/`
  - Steamworks native plugin: `VlcnpStory_Data/Plugins/x86_64/steam_api64.dll`
  - `steam_appid.txt`: `/tmp/vlcnpStory_SteamDemoWindowsBatch/steam_appid.txt` に `1223071`
  - Windows 実機起動確認は未実施。今回の範囲では Windows はビルド成果物の作成確認まで。

## SteamPipe アップロード準備
- Steamworks SDK の `tools/ContentBuilder/scripts` に以下のテンプレートをコピーする。
  - `Assets/DocsForAI/Plan/SteamPipe/app_build_demo_template.vdf`
  - `Assets/DocsForAI/Plan/SteamPipe/depot_build_demo_windows_template.vdf`
  - `Assets/DocsForAI/Plan/SteamPipe/depot_build_demo_macos_template.vdf`
- `<DEMO_WINDOWS_DEPOT_ID>` / `<DEMO_MAC_DEPOT_ID>` を Steamworks の値へ置換する。
- `ContentRoot` は Steamworks SDK の `tools/ContentBuilder/content` を指す。
- ビルド成果物を以下へ配置する。
  - Windows: `content/windows/VlcnpStory.exe` と関連ファイル一式
  - macOS: `content/macos/VlcnpStory.app`
- steamcmd 実行例:
  - `steamcmd +login <builder_account> +run_app_build ../scripts/app_build_demo_template.vdf +quit`

## 完了判定
- Mac release build が作成できている。
- Windows release build が作成できている。
- Demo App ID はテンプレートへ反映済み。
- depot ID を差し込めば SteamPipe upload script が成立する。
- Demo App の Steam Cloud 方針が決まっている。
