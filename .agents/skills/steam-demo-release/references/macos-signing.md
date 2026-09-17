# macOS の署名と公証

## 使用する設定

- App: `Builds/SteamDemo/Mac/VlcnpStory.app`
- Identity: `Developer ID Application: Yohei Kokubo (U8E796F5BN)`
- Entitlements: `/Users/yhei/tool/iOSApp/steam-macos-entitlements.plist`
- notarytool profile: `vlcnp-notary`

Steam 版は App Sandbox を付けない。このビルドで使う entitlement は `com.apple.security.cs.allow-jit`、`com.apple.security.cs.disable-library-validation`、`com.apple.security.cs.allow-dyld-environment-variables`。指定ファイルと成果物に照らして確認し、一般アプリ向けの設定と混同しない。

## 署名

ネストしたコードから外側へ、Identity、`--options runtime`、`--timestamp` を付けて署名する。実際に含まれるコードを確認し、過去の一覧だけで追加プラグインを見落とさない。

既知の順序は次のとおり。

1. `Contents/PlugIns/steam_api.bundle/Contents/MacOS/libsteam_api.dylib`
2. `Contents/PlugIns/steam_api.bundle`
3. `lib_burst_generated.bundle`
4. Frameworks の `libMonoPosixHelper.dylib`、`libmono-native.dylib`、`libmonobdwgc-2.0.dylib`、`UnityPlayer.dylib`
5. 最後に .app 全体を Entitlements 付きで署名。

```sh
codesign --force --options runtime --timestamp \
  --entitlements /Users/yhei/tool/iOSApp/steam-macos-entitlements.plist \
  --sign "Developer ID Application: Yohei Kokubo (U8E796F5BN)" \
  Builds/SteamDemo/Mac/VlcnpStory.app
codesign --verify --deep --strict --verbose=2 Builds/SteamDemo/Mac/VlcnpStory.app
codesign -d --entitlements :- Builds/SteamDemo/Mac/VlcnpStory.app | plutil -p -
```

## 公証

作業用 ZIP は他の実行と区別できるパスを選ぶ。例は一回分のパスを指定する。

```sh
notary_zip="/absolute/path/work/VlcnpStory_Notarization.zip"
ditto -c -k --keepParent Builds/SteamDemo/Mac/VlcnpStory.app "$notary_zip"
xcrun notarytool submit "$notary_zip" \
  --keychain-profile vlcnp-notary --output-format json
```

返された Submission ID を控え、同 ID の info で完了を確認する。応答を失った場合は history と送信時刻・ファイルを照合し、再送前に既存 submission を特定する。

```sh
xcrun notarytool history --keychain-profile vlcnp-notary --output-format json
xcrun notarytool info SUBMISSION_ID --keychain-profile vlcnp-notary --output-format json
```

`Accepted` の後だけ staple する。`Invalid` ならログから原因を直し、同じ成果物を繰り返し送信しない。

```sh
xcrun stapler staple Builds/SteamDemo/Mac/VlcnpStory.app
xcrun stapler validate Builds/SteamDemo/Mac/VlcnpStory.app
spctl --assess --type execute --verbose=4 Builds/SteamDemo/Mac/VlcnpStory.app
```

`spctl` の `accepted` と `source=Notarized Developer ID` を確認する。staple 後の app から完成 ZIP を作り、既定の `/Users/yhei/tool/iOSApp/VlcnpStory_Notarized_YYYYMMDD.zip` または依頼先へ保存し SHA-256 を記録する。同名の既存成果物を不用意に上書きせず、公証前 ZIP を完成品として渡さない。
