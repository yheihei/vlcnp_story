# Steam Demo Release Handoff

## 目的
- 更新日: 2026-06-19
- 対象: #603 / #627 / #637
- 他の AI セッションが、Steam 体験版を先に出すための現状と次アクションを把握できるようにする。
- 方針: Windows 実機確認は後回しにする。#603 はまだ閉じないが、体験版先行公開の次ゲートは #627 の Steam クラウドセーブ対応と見る。

## 現状判断
- #603 `WebGL -> デスクトップビルド移行` は、親 issue としては未完了。
  - #637 の Windows 実機ビルド、起動、基本操作、セーブ/ロード、Steam Overlay 確認が未完了のため。
  - Windows 実機確認を後回しにするなら、#603 / #637 は open のまま残す。
- 体験版をとりあえず出す目的では、#637 はリリース前のリスクとして受け入れて後回しにできる。
- その場合、次に潰すべき実装ゲートは #627 `Steamクラウドセーブ対応`。
- #605 `Steam実績` は初回の体験版公開では必須にしない認識。実績は別 issue として後続で扱う。

## Steamworks の状態
- Base App ID: `4829520`
- Demo App ID: `4861250`
- Store item ID: `1223071`
- Steam publisher: `388474`
- Demo packages:
  - Developer Comp: `1688384`
  - Beta Testing: `1688385`
  - Public Demo package: `1688386`
- Demo depots:
  - Windows Depot ID: `4861251`
  - macOS Depot ID: `4861252`
- Demo App の depot / package / OS 別 download 設定は Steamworks 上で公開済み。
- Demo App の Steam Cloud / Auto-Cloud 設定は Steamworks 上で保存・公開済み。
- 注意: `1223071` は store item ID。Unity の `steam_appid.txt` や SteamPipe の `AppID` に使うのは Demo App ID `4861250`。
- まだ未完了または未確認:
  - SteamPipe で実際の Windows / macOS build を upload したかは未完了扱い。この端末では `steamcmd` / Steamworks SDK `ContentBuilder` は未検出。
  - upload 後の build live 化、Steam クライアントからの install / 起動確認は未完了扱い。
  - Steam クライアント経由の Cloud upload は macOS で確認済み。Cloud download は SteamPipe upload 後に Steam クライアント起動で確認する。

## リポジトリの状態
- 作業ブランチ: `main`
- 直近の反映済み commit:
  - `8eb26663` `SteamデモをOS別Depot構成にする #603`
  - `7dc5c241` `SteamデモのDepot IDを反映 #603`
  - `584b9103` `Steamデモ向けビルド準備を追加 #603`
- 重要ファイル:
  - `Assets/DocsForAI/Plan/DesktopBuildMigrationPlan.md`
  - `Assets/DocsForAI/Plan/SteamDemoReleasePlan.md`
  - `Assets/DocsForAI/Plan/SteamPipe/app_build_demo_template.vdf`
  - `Assets/DocsForAI/Plan/SteamPipe/depot_build_demo_windows_template.vdf`
  - `Assets/DocsForAI/Plan/SteamPipe/depot_build_demo_macos_template.vdf`
  - `Assets/Scripts/Editor/DesktopBuildUtility.cs`
  - `Assets/Scripts/Steam/SteamBootstrap.cs`
  - `Assets/Scripts/Saving/JsonSavingSystem.cs`

## Build / SteamPipe の状態
- 2026-06-19: 現在の Steam Cloud 実装入りで Steam Demo Release Build を再作成済み。
  - macOS: `/tmp/vlcnpStory_SteamDemoMacSteamPipe/VlcnpStory.app`
    - サイズ: `321M`
    - Bundle ID: `com.yheiwebdesign.vlcnpstory`
    - Version: `0.2.1`
    - `steam_appid.txt`: root と `.app/Contents/MacOS/` に `4861250`
    - Steamworks native plugin: `.app/Contents/PlugIns/steam_api.bundle/Contents/MacOS/libsteam_api.dylib`
  - Windows: `/tmp/vlcnpStory_SteamDemoWindowsSteamPipe/VlcnpStory.exe`
    - サイズ: `297M`
    - `steam_appid.txt`: `4861250`
    - 必須ファイル: `VlcnpStory.exe`, `UnityPlayer.dll`, `VlcnpStory_Data/`
    - Steamworks native plugin: `VlcnpStory_Data/Plugins/x86_64/steam_api64.dll`
  - `unicli exec Compile`、`BuildPlayer.Compile --target StandaloneOSX`、`BuildPlayer.Compile --target StandaloneWindows64` は error 0 / warning 0。
- 2026-06-19: SteamPipe upload 用 staging を作成済み。
  - staging: `/tmp/vlcnpStory_SteamPipeDemo_20260619`
  - `scripts/`: SteamPipe VDF 3 ファイル
  - `content/windows`: Windows build 一式
  - `content/macos`: `VlcnpStory.app`
  - `builder/` / `output/`: ContentBuilder 互換の作業ディレクトリ
  - upload 対象内に `steam_appid.txt` が存在しないことを確認済み。
- 2026-06-18: macOS Steam Demo Release Build を Demo App ID `4861250` で再作成済み。
  - 出力: `/tmp/vlcnpStory_SteamDemoMacCloud/VlcnpStory.app`
  - `steam_appid.txt`: `/tmp/vlcnpStory_SteamDemoMacCloud/steam_appid.txt` と `.app/Contents/MacOS/steam_appid.txt` に `4861250`
  - Steamworks Cloud 設定は保存・公開済み。
  - 当初は Steam クライアント未ログイン/更新前で `SteamAPI.Init` が失敗したが、ログイン後に `SteamAPI.Init` / Cloud status / Auto-Cloud upload は確認済み。
- macOS Steam Demo Release Build は 2026-06-16 に成功済み。
  - 出力: `/tmp/vlcnpStory_SteamDemoMac/VlcnpStory.app`
  - ただし当時の `steam_appid.txt` は `1223071` だったため、ローカル Steam テストでは `4861250` で再ビルドするか差し替える。
- Windows Steam Demo Release Build は 2026-06-16 に Mac から作成成功済み。
  - 出力: `/tmp/vlcnpStory_SteamDemoWindowsBatch/VlcnpStory.exe`
  - 必須ファイル: `VlcnpStory.exe`, `UnityPlayer.dll`, `VlcnpStory_Data/`
  - ただし Windows 実機起動確認は未実施。
  - 当時の `steam_appid.txt` は `1223071` だったため、ローカル Steam テストでは `4861250` で再ビルドするか差し替える。
- SteamPipe テンプレートは Demo App ID / OS 別 Depot ID 反映済み。
  - `app_build_demo_template.vdf` の `AppID`: `4861250`
  - Windows depot: `4861251`
  - macOS depot: `4861252`
  - depot upload 対象から `steam_appid.txt` は除外済み。

## #627 で次にやること
- #627 の現在の checklist:
  - Steamworks Partner 設定で Cloud 有効化・Quota 設定
  - セーブファイルの配置パスを Steam Cloud 対象に揃える
  - Steam Cloud への読み書き処理を実装
  - 競合時の挙動を整理
  - 複数端末で同期できることを確認
- 現在の保存処理:
  - `Assets/Scripts/Saving/JsonSavingSystem.cs`
  - `Application.persistentDataPath` 配下に `<saveFile>.json` を保存する。
  - auto save 名は `autoSave` なので、実ファイルは `autoSave.json` の想定。
- 現在の Steam 起動処理:
  - `Assets/Scripts/Steam/SteamBootstrap.cs`
  - 起動時に `Application.persistentDataPath` を Player.log へ出す。
  - Steam 初期化に失敗してもゲームは継続する。
- 最短方針:
  - まず Steam Auto-Cloud で既存の `Application.persistentDataPath` 配下の JSON を同期対象にする。
  - ゲーム側に Steam RemoteStorage API を直接追加するのは、Auto-Cloud で不足が出た場合の後続対応にする。
  - #627 の「読み書き処理」は、既存の `JsonSavingSystem` が Cloud 対象パスに JSON を書くことで満たす方針にできるか確認する。
- 2026-06-18 追記:
  - `Assets/Scripts/Steam/SteamCloudSaveSync.cs` を追加し、Steam 初期化済みの Standalone 起動時だけ Cloud 状態 / quota を Player.log に出す。
  - `JsonSavingSystem` の保存・削除を `SteamRemoteStorage.BeginFileWriteBatch()` / `EndFileWriteBatch()` で囲み、Auto-Cloud にローカルディスク書き込みのまとまりを通知する。
  - Steam 未起動、Steam 初期化失敗、Editor、WebGL では no-op。
- Steam Cloud の想定 path:
  - Windows: `%USERPROFILE%\AppData\LocalLow\YheiWebDesign\VlcnpStory`
  - macOS: `~/Library/Application Support/YheiWebDesign/VlcnpStory`
  - 正確な値は Player.log の `[SteamBootstrap] persistentDataPath=...` で確認する。
  - 2026-06-18 の macOS demo build 起動ログでは `/Users/yhei/Library/Application Support/YheiWebDesign/VlcnpStory`。既存の `autoSave.json` / `save.json` は JSON object として読み込み可能。
  - Steam クライアント未起動時は `SteamAPI_IsSteamRunning() did not locate a running instance of Steam.` で `SteamAPI.Init` が失敗。
  - Steam クライアント起動後の再確認では、クライアント更新前は `No SteamClient023`、更新後は `ConnectToGlobalUser: Steam denied appID 4861250` で失敗。Steam アカウントのログイン状態、Developer Comp / release-state override package、Steam からの起動経路を確認してから再テストする。
  - 追加確認では Steam UI はプレイヤー選択画面まで表示されたが、`connection_log.txt` は `Logged Off`、`loginusers.vdf` は `AllowAutoLogin=0`。この端末ではまず Steam へ手動ログインし、Developer Comp `1688384` または release-state override package で Demo App `4861250` の起動権限がある状態にする必要がある。
  - Codex から Steam UI を操作するには macOS のアクセシビリティ許可ダイアログが出る。システム設定変更はユーザー操作で行う。
  - 2026-06-18 15:40 JST に Steam へログイン後、同じ macOS demo build を起動して `SteamAPI.Init` 成功を確認。Player.log に `Steam initialized. AppID=4861250` と `[SteamCloudSaveSync] Cloud status accountEnabled=True appEnabled=True quotaTotalBytes=10485760 quotaAvailableBytes=10485760 ... pattern=*.json` が出た。
  - 同起動の終了後、`cloud_log.txt` に AppID `4861250` の Auto-Cloud upload 成功を確認。`YheiWebDesign/VlcnpStory/save.json` と `YheiWebDesign/VlcnpStory/autoSave.json` が `Upload OK`、`Upload complete, result OK`。
  - 2026-06-19 に同一 Mac で local JSON を退避して Cloud download を試したが、直接バイナリ起動では download は発火しなかった。Player.log は `SteamAPI.Init` / Cloud status 成功、終了時の `cloud_log.txt` は `No launch record found` のまま Auto-Cloud upload 側を評価し、既存ファイルは `Skipping un-modified file` だった。退避した `autoSave.json` / `save.json` はバックアップから復元済みで SHA-1 は remote cache と一致。
  - Cloud download / 再起動ロード確認は、SteamPipe upload 後に Steam クライアントの package / build から Demo App `4861250` を起動して行う。
- Steamworks Auto-Cloud の公開済み設定:
  - Demo App ID: `4861250`
  - 2026-06-18 に Steam Cloud ページで保存し、Steamworks publishing で公開済み。
  - `Byte quota per user`: `10485760`
  - `Number of files allowed per user`: `32`
  - Root Path: `WinAppDataLocalLow` / `YheiWebDesign/VlcnpStory` / `*.json` / `All OSes` / recursive off
  - Root Override: `WinAppDataLocalLow` -> macOS `MacAppSupport`, Add/Replace Path `YheiWebDesign/VlcnpStory`, Replace Path on
  - Developer-only Cloud support: off
  - Dynamic Cloud Sync: off
  - `Shared cloud APP ID`: 初回体験版では `0`
- 競合ルールの最小案:
  - 初回は Steam Auto-Cloud の既定挙動に寄せる。
  - 仕様としては「最後に更新された `autoSave.json` を採用。競合 UI は初回体験版では持たない」と明記する。
  - 将来、複数スロットや明示的な競合 UI が必要なら別 issue に分ける。

## 体験版先行公開までの推奨順
1. #627 を実装・確認する。
   - Steamworks Demo App `4861250` で Steam Cloud を有効化済み。
   - Auto-Cloud の root / path / pattern は公開済み。
   - Steam クライアントへログインした状態で、macOS build の `SteamAPI.Init` / Cloud status / Auto-Cloud upload は確認済み。
   - SteamPipe upload 後に Steam クライアントから起動し、再起動 load と Cloud download を確認する。
   - 可能なら別端末または別 OS で同期確認する。
2. Demo App ID `4861250` で Windows / macOS Steam Demo Release Build を作り直す。
   - 2026-06-19 に作成済み。`steam_appid.txt` はローカル確認用だけに使い、SteamPipe upload には含めない。
3. SteamPipe で upload する。
   - この端末では `steamcmd` / Steamworks SDK `ContentBuilder` は未検出。
   - `/tmp/vlcnpStory_SteamPipeDemo_20260619` は `tools/ContentBuilder` 互換の staging 済み。Steamworks SDK がある環境ではこの内容を `tools/ContentBuilder` に配置する。
   - 実行例: `cd <Steamworks SDK>/tools/ContentBuilder/builder && steamcmd +login <builder_account> +run_app_build ../scripts/app_build_demo_template.vdf +quit`
4. Steamworks で build を live にする。
   - 少なくとも Beta Testing package で install / launch を確認する。
   - Public Demo package を live にする前に、macOS 側だけでも Steam クライアント経由の起動確認をする。
5. 公開後も #603 / #637 は open のままにする。
   - Windows 実機確認は後追いで完了させる。

## 次セッションへの依頼文
次の AI セッションには、以下のように依頼するとよい。

```text
Assets/DocsForAI/Plan/SteamDemoReleaseHandoff.md と #627 を読んで、体験版先行公開に必要な Steam Cloud 対応を進めてください。
Windows 実機確認は後回しでよいです。#603 / #637 はまだ閉じないでください。
まずは Steam Auto-Cloud で Application.persistentDataPath 配下の autoSave.json を同期する方針で、Steamworks 設定と必要なコード・ドキュメント更新を整理してください。
```
