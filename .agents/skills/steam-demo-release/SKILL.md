---
name: steam-demo-release
description: vlcnpStory2022 の Steam体験版のビルド、公証、アップロード、公開を依頼されたときに使う。
---

# Steam 体験版リリース

対象はこのリポジトリの Windows / macOS 体験版。依頼された工程を完了まで進める。ビルドだけの依頼で公開を追加しない。すでに依頼・許可された保存、アップロード、公開を同じ理由で再確認しない。

Demo AppID は `4861250`、Windows Depot は `4861251`、macOS Depot は `4861252`。別アプリへこの設定を流用しない。

## 工程を選ぶ

| 依頼・必要な処理 | 読む手順 |
| --- | --- |
| Windows / macOS の再ビルド | [ビルド](references/build.md) |
| macOS の署名・Apple 公証 | [署名と公証](references/macos-signing.md) |
| SteamPipe の準備・アップロード・認証 | [SteamPipe](references/steampipe.md) |
| BuildID の公開・公開状態の確認 | [公開](references/publishing.md) |

両 OS の体験版更新では、ビルド、macOS 公証、ステージング、アップロード、default 公開の順に進める。一部工程だけなら関係する手順だけ読む。

## 共通の制約

- ユーザーの作業中の差分を破棄しない。対象と保存状態を確認し、依頼の変更を保存する。所有者や保存意図が不明な変更を一括保存・破棄しない。
- パスワード、Steam Guard、SMS コードをチャットやスクリプトへ記録しない。必要になったときにユーザー本人が認証 UI へ直接入力する。
- `/Users/yhei/tool/iOSApp` の手順書は資格情報を含む可能性がある。全文表示・全文検索をせず、署名手順の既知の Identity・Entitlements・Keychain profile を使う。
- 外部処理のタイムアウトは失敗確定ではない。公証 Submission ID、アップロードプロセス・ログ、公開状態を照会してから再試行する。
- 公開が依頼・許可されているなら、新 BuildID と manifest を照合して公開する。公開が必要な依頼でも対象や時期が不明なら、公開可能な成果物と変更プレビューを用意してその点だけ確認する。公開を求められていなければ、依頼された工程で完了する。

完了した工程と確認結果を伝える。公開まで完了した場合は BuildID、両 Depot の manifest ID、公証 Submission ID、ZIP と SHA-256、default の公開状態を報告する。入力待ちや未実施の工程を完了扱いにしない。
