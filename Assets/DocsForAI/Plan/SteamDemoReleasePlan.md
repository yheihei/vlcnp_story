# Steam Demo Release Plan

## 前提
- 対象 issue: #603
- 目的: Steam では本編発売前にデモ版だけを先行公開する。
- ストアページは既に作成済み。
- ストアページの App ID は Base App ID として扱う。
- Demo App ID は Base App に紐づく別 App ID として作成する。
- Base App ID: `4829520`
- Demo App ID: `4861250`
- Store item ID: `1223071`
- Steam publisher: `388474`
- Demo packages:
  - Developer Comp: `1688384`
  - Beta Testing: `1688385`
  - Public Demo package: `1688386`

## Steamworks で確認する ID
- Base App ID: `4829520`
- Demo App ID: `4861250`
- Demo Content Depot ID: `4861251`
  - 現在は Windows / macOS 個別 depot ではなく、オペレーティングシステム `すべてのOS` の単一 depot。
  - depot 名: `Very Long CNP物語 Demo Content`
  - 3 個の package から参照されている。

## Steamworks 側の設定
- Demo App の Supported OS は Windows / macOS を有効にする。
- Demo App の Launch Options:
  - Windows: `VlcnpStory.exe`
  - macOS: `VlcnpStory.app`
- Demo App の Depots:
  - 現在の depot: `4861251` / `すべてのOS`。
  - このまま進める場合は Windows build と macOS app bundle の両方を同じ depot root に置く。
  - OS 別 download にしたい場合は Steamworks 上で Windows 用 / macOS 用 depot を追加し、SteamPipe template を 2 depot 構成へ戻す。
- Demo App の packages:
  - Developer Comp / Beta Testing / Public Demo package に depot `4861251` が含まれていることを確認する。
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
  - 今後のローカル Steam テストでは `-vlcnpSteamAppId 4861250` を使う。

## ローカル検証結果
- 2026-06-16: macOS Steam Demo Release Build 成功。
  - 出力: `/tmp/vlcnpStory_SteamDemoMac/VlcnpStory.app`
  - サイズ: `321M`
  - Bundle ID: `com.yheiwebdesign.vlcnpstory`
  - Steamworks native plugin: `VlcnpStory.app/Contents/PlugIns/steam_api.bundle/Contents/MacOS/libsteam_api.dylib`
  - `steam_appid.txt`: `/tmp/vlcnpStory_SteamDemoMac/steam_appid.txt` と `.app/Contents/MacOS/steam_appid.txt` に `1223071`
  - 2026-06-18 の Chrome 確認で正しい Demo App ID は `4861250` と判明したため、ローカル Steam テスト時は `steam_appid.txt` を `4861250` に差し替える。
  - 短時間起動確認: Steam クライアント未起動時は `SteamAPI.Init` が失敗するが、ゲームは継続する。
- 2026-06-16: Windows Steam Demo Release Build 成功。
  - 出力: `/tmp/vlcnpStory_SteamDemoWindowsBatch/VlcnpStory.exe`
  - サイズ: `81M`
  - 必須ファイル: `VlcnpStory.exe`, `UnityPlayer.dll`, `VlcnpStory_Data/`
  - Steamworks native plugin: `VlcnpStory_Data/Plugins/x86_64/steam_api64.dll`
  - `steam_appid.txt`: `/tmp/vlcnpStory_SteamDemoWindowsBatch/steam_appid.txt` に `1223071`
  - 2026-06-18 の Chrome 確認で正しい Demo App ID は `4861250` と判明したため、ローカル Steam テスト時は `steam_appid.txt` を `4861250` に差し替える。
  - Windows 実機起動確認は未実施。今回の範囲では Windows はビルド成果物の作成確認まで。

## SteamPipe アップロード準備
- Steamworks SDK の `tools/ContentBuilder/scripts` に以下のテンプレートをコピーする。
  - `Assets/DocsForAI/Plan/SteamPipe/app_build_demo_template.vdf`
  - `Assets/DocsForAI/Plan/SteamPipe/depot_build_demo_content_template.vdf`
- `ContentRoot` は Steamworks SDK の `tools/ContentBuilder/content` を指す。
- ビルド成果物を以下へ配置する。
  - Windows: `content/all/VlcnpStory.exe` と関連ファイル一式
  - macOS: `content/all/VlcnpStory.app`
  - 単一 depot のため、現設定では Windows / macOS 両方の成果物を同じ depot に含める。
- steamcmd 実行例:
  - `steamcmd +login <builder_account> +run_app_build ../scripts/app_build_demo_template.vdf +quit`

## 完了判定
- Mac release build が作成できている。
- Windows release build が作成できている。
- Demo App ID `4861250` はテンプレートへ反映済み。
- Demo Content Depot ID `4861251` はテンプレートへ反映済み。
- Demo App の Steam Cloud 方針が決まっている。
