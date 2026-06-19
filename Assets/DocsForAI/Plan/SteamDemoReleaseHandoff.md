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
- SteamPipe upload と build live 化は完了済み。
  - BuildID: `23819735`
  - default branch: `23819735` を live 設定済み
  - Windows manifest: `5795665032736831212`
  - macOS manifest: `1137948321395811809`
- Demo App の Installation > General Installation launch options は Steamworks 上で保存・公開済み。
  - Launch option 0: `VlcnpStory.exe` / Windows
  - Launch option 1: `VlcnpStory.app` / macOS
- まだ未完了または未確認:
  - Steam クライアント経由の install / 起動 / Cloud upload / Cloud download / 再起動ロードは macOS で確認済み。
  - 複数端末同期は未確認。

## リポジトリの状態
- 作業ブランチ: `main`
- 直近の反映済み commit:
  - `260f64da` `SteamPipeアップロード手順を補助する #627`
  - `800528fb` `SteamPipeステージング作成を再現可能にする #627`
  - `b20ea2e3` `SteamPipe向けデモビルド準備を追記 #627`
  - `167b1420` `Steam Cloudダウンロード検証メモを追記 #627`
  - `9159431a` `Steam Cloudアップロード検証を追記 #627`
- 重要ファイル:
  - `Assets/DocsForAI/Plan/DesktopBuildMigrationPlan.md`
  - `Assets/DocsForAI/Plan/SteamDemoReleasePlan.md`
  - `Assets/DocsForAI/Plan/SteamPipe/app_build_demo_template.vdf`
  - `Assets/DocsForAI/Plan/SteamPipe/depot_build_demo_windows_template.vdf`
  - `Assets/DocsForAI/Plan/SteamPipe/depot_build_demo_macos_template.vdf`
  - `Assets/DocsForAI/Plan/SteamPipe/prepare_steam_demo_staging.sh`
  - `Assets/DocsForAI/Plan/SteamPipe/upload_steam_demo_build.sh`
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
  - repo 内の `Assets/DocsForAI/Plan/SteamPipe/prepare_steam_demo_staging.sh` で同じ staging を再作成できる。
    - 検証済みコマンド: `Assets/DocsForAI/Plan/SteamPipe/prepare_steam_demo_staging.sh /tmp/vlcnpStory_SteamDemoMacSteamPipe /tmp/vlcnpStory_SteamDemoWindowsSteamPipe /tmp/vlcnpStory_SteamPipeDemo_script_test_20260619`
  - SteamCMD は公式配布 archive から `/tmp/steamcmd_osx_20260619` に展開し、`+quit` で自己更新・起動確認済み。
  - repo 内の `Assets/DocsForAI/Plan/SteamPipe/upload_steam_demo_build.sh` で upload command を実行できる。パスワードは受け取らず保存しない。
    - dry-run 検証済み: `Assets/DocsForAI/Plan/SteamPipe/upload_steam_demo_build.sh /tmp/vlcnpStory_SteamPipeDemo_20260619 yhei_hei --dry-run`
- 2026-06-19: SteamPipe upload 完了。
  - 初回 upload は SteamCMD の作業ディレクトリ差分により `../scripts/app_build_demo_template.vdf` が見つからず失敗した。
  - `Assets/DocsForAI/Plan/SteamPipe/upload_steam_demo_build.sh` を、`builder/generated` に絶対パス入り runtime VDF を生成して `+run_app_build` に渡す方式へ修正済み。
  - upload command は `/tmp/vlcnpStory_SteamPipeDemo_20260619/output/app_build_4861250.log` で `Successfully finished AppID 4861250 build (BuildID 23819735)` を確認済み。
  - Windows depot `4861251`: manifest `5795665032736831212`
  - macOS depot `4861252`: manifest `1137948321395811809`
- 2026-06-19: Steamworks で Demo App `4861250` の default branch を BuildID `23819735` に live 設定済み。
  - Steamworks の build ページで recent build の current column に `default` が付いていることを確認済み。
  - Steamworks history に `ライブに設定 BuildID 23819735 for branch "default"` が残っている。
- 2026-06-19: Steamworks で Demo App `4861250` の launch options を保存・公開済み。
  - `VlcnpStory.exe` / Windows
  - `VlcnpStory.app` / macOS
  - Steamworks publishing で `Publish to steam OK` / `Publishing successful!` を確認済み。
  - SteamCMD `app_info_print 4861250` で server appinfo の change number `36699390` に `launch` / `executable` / `oslist` が反映されていることを確認済み。
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
  - 2026-06-19 に default branch live 後、`steam://rungameid/4861250` は Steam client の `LaunchApp -> SynchronizingCloud` まで進んだが、`cloud_log.txt` は `[login=false][offlineMode=false]` で sync failed。Auto-Cloud は `autoSave.json` / `save.json` を watch している。
  - 同日、通常 Steam クライアント再起動後も `connection_log.txt` は `Access Denied`、`loginusers.vdf` は `RememberPassword=0` / `AllowAutoLogin=0`、Steam process は `steamid=0`。再実行した `steam://rungameid/4861250` は `not allowed yet` で Cloud ログ更新なし。
  - 2026-06-19 20:33 JST に通常 Steam クライアントへ再ログイン後、Steamworks launch options を保存・公開し、Steam クライアントを再起動。`steam://rungameid/4861250` で `LaunchApp -> CreatingProcess -> Completed` まで進み、`VlcnpStory.app` が tracked process として起動した。
  - Player.log に `Steam initialized. AppID=4861250` と `[SteamCloudSaveSync] Cloud status accountEnabled=True appEnabled=True quotaTotalBytes=10485760 quotaAvailableBytes=10481615 ... pattern=*.json` を確認。
  - Cloud download 確認として、ローカルの `autoSave.json` / `save.json` を `/tmp/vlcnp_cloud_download_backup_20260619_203826` へ退避して削除し、Steam クライアントから再起動した。`cloud_log.txt` に missing 検知、`Need to download file ...`, `HTTP download ... - Success`, `Download complete, result OK`, `Successfully synced to ChangeNumber 1` を確認。復元後の SHA-256 はバックアップと一致。
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
   - SteamPipe upload と default branch live は完了済み。
   - Steamworks launch options は公開済み。Steam クライアントからの install / 起動 / 再起動 load / Cloud download は macOS で確認済み。
   - 残りは別端末または別 OS での同期確認。
2. Demo App ID `4861250` で Windows / macOS Steam Demo Release Build を作り直す。
   - 2026-06-19 に作成済み。`steam_appid.txt` はローカル確認用だけに使い、SteamPipe upload には含めない。
3. SteamPipe で upload する。
   - 2026-06-19 に BuildID `23819735` として upload 済み。
   - SteamCMD は `/tmp/steamcmd_osx_20260619/steamcmd.sh` で起動確認済み。
   - `/tmp/vlcnpStory_SteamPipeDemo_20260619` は `tools/ContentBuilder` 互換の staging 済み。
   - staging を作り直す場合: `Assets/DocsForAI/Plan/SteamPipe/prepare_steam_demo_staging.sh [mac_build_dir] [windows_build_dir] [stage_dir]`
   - upload helper: `Assets/DocsForAI/Plan/SteamPipe/upload_steam_demo_build.sh /tmp/vlcnpStory_SteamPipeDemo_20260619 <builder_account>`
   - helper は `builder/generated` に絶対パス入り runtime VDF を生成する。SteamCMD を手動で呼ぶ場合も相対 `../scripts/...` には依存しない。
   - builder account のパスワード / Steam Guard は SteamCMD の対話プロンプトで入力する。
4. Steamworks で build を live にする。
   - default branch は BuildID `23819735` で live 済み。
   - 次は通常 Steam クライアントへ再ログインし、少なくとも Beta Testing package で install / launch / Cloud download を確認する。
   - Public Demo 公開前に、macOS 側だけでも Steam クライアント経由の起動確認を完了する。
5. 公開後も #603 / #637 は open のままにする。
   - Windows 実機確認は後追いで完了させる。

## 次セッションへの依頼文
次の AI セッションには、以下のように依頼するとよい。

```text
Assets/DocsForAI/Plan/SteamDemoReleaseHandoff.md と #627 を読んで、体験版先行公開に必要な Steam Cloud 対応を進めてください。
Windows 実機確認は後回しでよいです。#603 / #637 はまだ閉じないでください。
SteamPipe upload と default branch live は完了済みです。通常 Steam クライアントへ再ログインしたうえで Demo App 4861250 を Steam クライアントから起動し、Cloud download / 再起動ロード / 複数端末同期の確認を進めてください。
Steam クライアントからの macOS install / 起動 / Cloud download / 再起動ロードは確認済みです。次は複数端末同期と Windows 実機確認を進めてください。
```
