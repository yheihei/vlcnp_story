# Windows / macOS のビルド

Unity 操作を行う段階で、公式の `unity-cli` と `unity-pipeline` と [プロジェクトの Editor 操作](../../unity-editor-automation/SKILL.md) を使う。Steam 認証とブラウザ操作はアップロード・公開の段階で扱う。

`git status --short` と `unity status --format json`、`editor_status` で対象・Play Mode・コンパイル・保存状態を確認する。自分の変更や依頼で保存対象が明らかな変更は保存する。未保存変更をビルドへ含めるべきか判断できない場合はその点だけ確認し、独立した出力先や設定の確認は進める。

ビルドメソッドの `eval` 呼び出しはモーダルで停止しやすいため、既存メニューを使う。対象platformへの切り替えは既存ビルド処理が行う。公式CLIの現在のコマンド定義を確認する。各 `unity command` に `--project-path <対象の絶対パス>` と `--caller plugin --skill steam-demo-release` を付ける。

以下は対象プロジェクトのルートで実行する。

```sh
unity command menu --project-path "$PWD" --path "Tools/VLCNP/Build/Steam Demo Windows Release Build" --caller plugin --skill steam-demo-release --format json --timeout 1800
```

Windows 完了後、macOS を必要とする場合に実行する。同じ Editor で両ビルドを並列起動しない。

```sh
unity command menu --project-path "$PWD" --path "Tools/VLCNP/Build/Steam Demo macOS Release Build" --caller plugin --skill steam-demo-release --format json --timeout 1800
```

長い処理は実行環境のジョブ監視で追う。`menu` が返らなければ Editor・ログ・成果物を確認し、処理中のビルドを重複起動しない。操作不能なら Editor 操作の復旧手順を使う。

## 成果物

| 対象 | 出力と確認 |
| --- | --- |
| Windows | `Builds/SteamDemo/Windows/VlcnpStory.exe`、`UnityPlayer.dll`、`VlcnpStory_Data/`、`steam_api64.dll` |
| macOS | `Builds/SteamDemo/Mac/VlcnpStory.app` の Info.plist、MacOS 配下の実行ファイル、`steam_api.bundle` |

ビルド結果・時刻・ログを確認し、以前からあるファイルを今回の成功と取り違えない。macOS の実行ファイルは `lipo -archs` で `x86_64 arm64` を確認する。Release と Development を取り違えない。
