# Steam Demo Release Handoff

## 目的
- 更新日: 2026-06-18
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
- 注意: `1223071` は store item ID。Unity の `steam_appid.txt` や SteamPipe の `AppID` に使うのは Demo App ID `4861250`。
- まだ未完了または未確認:
  - SteamPipe で実際の Windows / macOS build を upload したかは未完了扱い。
  - upload 後の build live 化、Steam クライアントからの install / 起動確認は未完了扱い。

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
- Steam Cloud の想定 path:
  - Windows: `%USERPROFILE%\AppData\LocalLow\YheiWebDesign\VlcnpStory`
  - macOS: `~/Library/Application Support/YheiWebDesign/VlcnpStory`
  - 正確な値は Player.log の `[SteamBootstrap] persistentDataPath=...` で確認する。
- 競合ルールの最小案:
  - 初回は Steam Auto-Cloud の既定挙動に寄せる。
  - 仕様としては「最後に更新された `autoSave.json` を採用。競合 UI は初回体験版では持たない」と明記する。
  - 将来、複数スロットや明示的な競合 UI が必要なら別 issue に分ける。

## 体験版先行公開までの推奨順
1. #627 を実装・確認する。
   - Steamworks Demo App `4861250` で Steam Cloud を有効化する。
   - Auto-Cloud の root / path / pattern を設定する。
   - macOS build で save 作成、終了、再起動 load を確認する。
   - 可能なら別端末または別 OS で同期確認する。
2. Demo App ID `4861250` で Windows / macOS Steam Demo Release Build を作り直す。
   - `steam_appid.txt` はローカル確認用だけに使い、SteamPipe upload には含めない。
3. SteamPipe で upload する。
   - Steamworks SDK の `tools/ContentBuilder/scripts` に `Assets/DocsForAI/Plan/SteamPipe/*.vdf` をコピーする。
   - `content/windows` に Windows build 一式を配置する。
   - `content/macos` に `VlcnpStory.app` を配置する。
   - 実行例: `steamcmd +login <builder_account> +run_app_build ../scripts/app_build_demo_template.vdf +quit`
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
