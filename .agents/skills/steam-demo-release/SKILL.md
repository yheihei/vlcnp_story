---
name: steam-demo-release
description: vlcnpStory2022 のSteam体験版をWindows/macOS向けに再ビルドし、macOS版をDeveloper IDで署名・Apple公証し、SteamPipeへアップロードしてSteamworksのdefaultブランチへ公開・更新する。Steam体験版のビルド、Mac公証、SteamCMD認証、デポ更新、BuildIDの公開、SMS確認、公開後検証を依頼されたときに使う。
---

# Steam体験版リリース

`/Users/yhei/unity/vlcnpStory2022` の現在の保存済み状態から、Windows/macOS両方のSteam体験版を安全に更新する。

## 固定情報

- Demo AppID: `4861250`
- Windows Depot: `4861251`
- macOS Depot: `4861252`
- Windows出力: `Builds/SteamDemo/Windows/VlcnpStory.exe`
- macOS出力: `Builds/SteamDemo/Mac/VlcnpStory.app`
- Windowsビルド: `Tools/VLCNP/Build/Steam Demo Windows Release Build`
- macOSビルド: `Tools/VLCNP/Build/Steam Demo macOS Release Build`
- 署名Identity: `Developer ID Application: Yohei Kokubo (U8E796F5BN)`
- Entitlements: `/Users/yhei/tool/iOSApp/steam-macos-entitlements.plist`
- notarytoolプロファイル: `vlcnp-notary`
- ステージング: `Assets/DocsForAI/Plan/SteamPipe/prepare_steam_demo_staging.sh`
- アップロード: `Assets/DocsForAI/Plan/SteamPipe/upload_steam_demo_build.sh`

資格情報、パスワード、Steam Guardコード、SMSコードをチャットやスクリプトへ記録しない。`/Users/yhei/tool/iOSApp` の手順書は資格情報を含む可能性があるため、全文表示・全文検索・転載をしない。上記の既知のIdentity、Entitlements、Keychainプロファイルだけを使う。

## 1. 事前確認

1. `computer-use` と `unity-development`、プロジェクト側の該当スキルを読む。
2. `git status --short` で作業中の変更を把握し、勝手に破棄しない。
3. Steamデスクトップアプリへ対象アカウントでログイン済みか確認する。初回SteamCMD認証ではSteamアプリのログインとSteam Guardが必要になることがある。
4. Steamworks公開時にSMS確認を求められることがあるため、ユーザーが確認コードを直接Chromeへ入力できる状態か確認する。
5. Unityの状態を確認する。

```sh
unicli check
unicli exec Editor.Status --json
```

Dirty Sceneがあればビルド前に止め、ユーザーが「保存した」と明示している場合のみ次で保存する。保存後にDirty Sceneが消えたことを再確認する。

```sh
unicli exec Scene.Save --all --json
unicli exec Editor.Status --json
```

## 2. Windows/macOSをビルド

`unicli eval` からビルドメソッドを直接呼ぶと、未保存シーンのモーダルなどで停止しやすい。必ずコンパイル後にメニュー項目を実行する。

```sh
unicli exec BuildPlayer.Compile --target StandaloneWindows64 --json
unicli exec Menu.Execute --menuItemPath "Tools/VLCNP/Build/Steam Demo Windows Release Build" --json --timeout 1800000

unicli exec BuildPlayer.Compile --target StandaloneOSX --json
unicli exec Menu.Execute --menuItemPath "Tools/VLCNP/Build/Steam Demo macOS Release Build" --json --timeout 1800000
```

`Menu.Execute` が出力を返さなくても、すぐ失敗と判断しない。Editorの状態と生成物を直接確認する。

Windows版では最低限、次を確認する。

- `VlcnpStory.exe`
- `UnityPlayer.dll`
- `VlcnpStory_Data/`
- `steam_api64.dll`

macOS版では次を確認する。

- `VlcnpStory.app/Contents/Info.plist`
- `VlcnpStory.app/Contents/MacOS/` 内の実行ファイル
- `steam_api.bundle`
- `lipo -archs` が `x86_64 arm64`

## 3. macOS版を署名・公証

Steam版はApp Sandboxを付けない。Entitlementsには次が必要。

- `com.apple.security.cs.allow-jit`
- `com.apple.security.cs.disable-library-validation`
- `com.apple.security.cs.allow-dyld-environment-variables`

Entitlementsの内容を確認し、ネストしたコードから外側へ署名する。各コマンドではIdentity、`--options runtime`、`--timestamp` を使う。

署名順序:

1. `Contents/PlugIns/steam_api.bundle/Contents/MacOS/libsteam_api.dylib`
2. `Contents/PlugIns/steam_api.bundle`
3. `lib_burst_generated.bundle`
4. Frameworks内の `libMonoPosixHelper.dylib`、`libmono-native.dylib`、`libmonobdwgc-2.0.dylib`、`UnityPlayer.dylib`
5. 最後に `.app` 全体をEntitlements付きで署名

外側の署名例:

```sh
codesign --force --options runtime --timestamp \
  --entitlements /Users/yhei/tool/iOSApp/steam-macos-entitlements.plist \
  --sign "Developer ID Application: Yohei Kokubo (U8E796F5BN)" \
  Builds/SteamDemo/Mac/VlcnpStory.app
```

署名を検証する。

```sh
codesign --verify --deep --strict --verbose=2 Builds/SteamDemo/Mac/VlcnpStory.app
codesign -d --entitlements :- Builds/SteamDemo/Mac/VlcnpStory.app | plutil -p -
```

公証用ZIPは`ditto`で作る。

```sh
ditto -c -k --keepParent Builds/SteamDemo/Mac/VlcnpStory.app /tmp/VlcnpStory_Notarization.zip
xcrun notarytool submit /tmp/VlcnpStory_Notarization.zip \
  --keychain-profile vlcnp-notary --wait --timeout 30m --output-format json
```

呼び出し側がタイムアウトして結果を失っても、再送信する前に履歴を確認する。

```sh
xcrun notarytool history --keychain-profile vlcnp-notary --output-format json
xcrun notarytool info SUBMISSION_ID --keychain-profile vlcnp-notary --output-format json
```

状態が`Accepted`であることを確認してからstapleする。

```sh
xcrun stapler staple Builds/SteamDemo/Mac/VlcnpStory.app
xcrun stapler validate Builds/SteamDemo/Mac/VlcnpStory.app
spctl --assess --type execute --verbose=4 Builds/SteamDemo/Mac/VlcnpStory.app
```

`spctl`が`accepted`かつ`source=Notarized Developer ID`になることを確認する。完成ZIPを`/Users/yhei/tool/iOSApp/VlcnpStory_Notarized_YYYYMMDD.zip`へ保存し、SHA-256を記録する。

## 4. SteamPipeへステージング・アップロード

署名・staple済みの`.app`とWindows出力をステージングする。

```sh
Assets/DocsForAI/Plan/SteamPipe/prepare_steam_demo_staging.sh
```

スクリプトの引数や出力先が変更されている可能性があるため、実行前に`--help`または先頭部分を確認する。ステージングに`steam_appid.txt`が含まれていないことを確認する。

SteamCMDがなければ公式配布物を一時ディレクトリへ展開し、最初に`steamcmd.sh +quit`で初期化する。

```text
https://steamcdn-a.akamaihd.net/client/installer/steamcmd_osx.tar.gz
```

アップロードスクリプトへ`STEAMCMD`環境変数を明示する。VDFの`"SetLive" ""`は維持し、アップロードと公開を分離する。

初回認証で対話入力が必要な場合、Codex内蔵ターミナルではなく、Finderから一時的な`.command`を開いてmacOS Terminalを使う。次を守る。

- パスワードやSteam Guardコードはユーザー本人がTerminalへ直接入力する。
- 認証情報を`.command`へ書かない。
- 起動前後に`pgrep -fl 'steamcmd.*run_app_build'`で重複プロセスがないことを確認する。
- Finderで連打せず、アップロードは1プロセスだけにする。
- 完了後、一時`.command`を削除する。

アップロード完了後、SteamPipeのログから次を記録する。

- BuildID
- Windows Depotのmanifest ID
- macOS Depotのmanifest ID
- `Successfully finished AppID ... build` の成功行

認証済みキャッシュがあれば、次回はパスワードなしで通ることがある。キャッシュがない場合はSteamアプリへのログイン状態を最初に疑う。

## 5. Steamworksでdefaultへ公開

Chromeで次を開く。

```text
https://partner.steamgames.com/apps/builds/4861250
```

1. 新しいBuildIDにWindows/macOS両方のDepotと期待したmanifest IDが含まれることを確認する。
2. ブランチ`default`を選び、変更プレビューで旧BuildIDから新BuildIDへの差分を確認する。
3. 「今すぐビルドをライブに設定」を押す直前に、即時公開の確認をユーザーから得る。
4. ブラウザの確認ダイアログを承認する。
5. SMSコード入力が出たら、コードをチャットで受け取らず、ユーザー本人にChromeへ直接入力してもらう。
6. 「ビルドの変更を確認」で公開を完了する。

## 6. 公開後の検証と報告

公開後にビルドページを再読み込みし、次を確認する。

- `default`が新しいBuildIDを指している。
- 公開履歴にdefaultブランチへの変更が残っている。
- Windows/macOS両Depotが同じBuildIDに含まれる。

最後に、BuildID、両manifest ID、公証Submission ID、公証済みZIPとSHA-256、default公開確認を簡潔に報告する。ユーザー入力待ちや手動操作が残っている場合は、公開完了とは報告しない。
