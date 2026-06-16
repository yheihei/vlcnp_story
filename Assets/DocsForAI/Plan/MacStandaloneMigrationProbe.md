# Mac Standalone 移行事前検証 Plan

## 背景と目的
- 対象 issue: #636
- 親 issue: #603
- 関連 issue: #635
- Windows 実機がない状態でも、WebGL から Standalone へ移行する前に Mac で確認できる問題を先に洗い出す。
- Windows 環境検証に入る前の前処理として、プラットフォーム依存のコンパイルエラー、WebGL 依存、不要になったウォレット系実装の整理対象を明確にする。

## 実施日と環境
- 実施日: 2026-06-16
- Unity: 2022.3.62f3
- 実行環境: macOS / Apple M2
- 検証ブランチ: `codex/issue-636-mac-standalone-probe`
- Mac Standalone 出力先: `/tmp/vlcnpStory_MacProbe/VlcnpStory.app`

## 検証結果
- 通常 C# コンパイル: 成功
  - エラー: 0
  - 警告: 254
- `StandaloneOSX` 向けスクリプトコンパイル: 成功
  - エラー: 0
  - 警告: 39
  - 対象 assembly: 50
- `StandaloneWindows64` 向けスクリプトコンパイル: 成功
  - エラー: 0
  - 警告: 76
  - 対象 assembly: 50
- Mac Standalone Development Build: 成功
  - 出力: `/tmp/vlcnpStory_MacProbe/VlcnpStory.app`
  - サイズ: 445 MiB
  - ビルド時間: 26.36 秒
  - エラー: 0
  - 警告: 110
- 起動確認: 成功
  - 実行ファイル: `/tmp/vlcnpStory_MacProbe/VlcnpStory.app/Contents/MacOS/VlcnpStory`
  - 10 秒以上プロセスが継続稼働することを確認
  - Player.log の `exception|error|crash|failed|dllnotfound|missingmethod|nullreference|abort|fatal` 検索はヒットなし
- 基本操作スモーク確認: 成功
  - windowed 起動後にタイトル画面を確認し、`Return` でゲーム画面へ遷移することを確認
  - ゲーム画面で `Right` 入力によりキャラクター位置が変わることを画面確認
  - `Left` / `Space` / `X` / `Z` 相当のキー入力後も Player プロセスが継続稼働することを確認
  - Player.log の `exception|error|crash|failed|dllnotfound|missingmethod|nullreference|abort|fatal` 検索はヒットなし

## WebGL 依存コード
- `Assets/Scripts/Saving/JsonSavingSystem.cs`
  - `Application.ExternalEval("FS.syncfs...")` がある。
  - `#if UNITY_WEBGL && !UNITY_EDITOR` ガード済みのため StandaloneOSX コンパイルには影響しない。
  - Steam / Standalone 専用方針では WebGL 永続化同期処理として整理対象。
- `Assets/Plugin/thirdweb.jslib`
  - WebGL 向け Thirdweb bridge。
  - Standalone 移行後にウォレット機能を使わないなら削除候補。
- `Assets/WebGLTemplates/Thirdweb`
  - Thirdweb 用 WebGL template。
  - Steam / Standalone では不要候補。
- `Assets/WebGLTemplates/StaticAspect`
  - 汎用 WebGL template。
  - WebGL 配信を完全に終了する方針なら削除候補。WebGL を残すなら維持。

## Thirdweb / WalletConnect 削除候補
- `Assets/Core/Wallet/WalletConnectSample.cs`
  - `ThirdwebSDK` を直接参照している。
  - `Assets/Scenes/SampleScene.unity` に `WalletConnectSample` 参照あり。
  - 削除時はシーン上の該当コンポーネント参照も同時に外す必要がある。
- `Assets/ExtraPackage/Thirdweb`
  - Thirdweb SDK 本体、Examples、WalletConnectSharp、Nethereum、AsyncAwaitUtil などを含む。
  - 通常 C# コンパイル警告 254 件の多くがこの配下から発生している。
  - StandaloneOSX スクリプトコンパイル警告 39 件、Mac build 警告 110 件にも Thirdweb / WalletConnect / AsyncAwaitUtil 系が含まれる。
- `Assets/Plugin/thirdweb.jslib`
  - WebGL 専用 bridge なので Standalone 移行では削除優先度が高い。
- `Assets/WebGLTemplates/Thirdweb`
  - Thirdweb WebGL template なので Standalone 移行では削除優先度が高い。

## 現時点の判断
- Mac Standalone のコンパイル、ビルド、短時間起動では致命的な問題は確認されなかった。
- Standalone 移行で最初に整理すべき対象は Thirdweb / WalletConnect 一式。
- `JsonSavingSystem` の WebGL 同期処理はガード済みなので、削除は必須ではない。ただし Steam / Standalone 専用化するなら不要コードとして後続整理対象。
- WebGL template はビルドターゲットを Standalone に寄せるだけなら残っていてもビルド失敗要因ではないが、配布対象を Steam に絞るなら削除候補。

## Windows 実機検証へ渡す未確認事項
- `StandaloneWindows64` 向けスクリプトコンパイルは成功している。
- `StandaloneWindows64` の実機起動確認。
- キーボード / ゲームパッドでの基本操作確認。
- セーブ / ロードが `Application.persistentDataPath` 配下で正常に動くか。
- 画面解像度、フルスクリーン、ウィンドウ切り替えの挙動。
- BGM / SE / Movie 再生の挙動。
- Thirdweb / WalletConnect 削除後に Windows build が通るか。
- WebGL template 削除後に WebGL を残す必要がないかの方針確認。

## 完了条件
- `StandaloneOSX` 向けスクリプトコンパイルが成功している。
- Mac Standalone Development Build が成功している。
- 起動確認できる範囲で致命的な Player.log エラーがない。
- タイトルからゲーム画面へ遷移でき、基本キー入力後も Player が継続稼働し、致命的な Player.log エラーがない。
- WebGL 依存コードと Thirdweb / WalletConnect / WebGL Template の削除候補が整理されている。
- Windows 実機検証へ渡す未確認事項が整理されている。
