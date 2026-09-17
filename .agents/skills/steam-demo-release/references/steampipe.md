# SteamPipe と認証

プロジェクトルートの既存スクリプトを使う。実行前に引数と処理範囲を確認する。`--help` を安全に扱うか不明なら、先にスクリプトを読む。

- `Assets/DocsForAI/Plan/SteamPipe/prepare_steam_demo_staging.sh`
- `Assets/DocsForAI/Plan/SteamPipe/upload_steam_demo_build.sh`

署名・staple 済み macOS app と Windows 出力を stage し、各出力が同じ更新に対応しているか確認する。配布内容に `steam_appid.txt` を含めない。

## アップロード

SteamCMD の場所は `STEAMCMD` 環境変数で指定する。未導入なら公式配布物 `https://steamcdn-a.akamaihd.net/client/installer/steamcmd_osx.tar.gz` を作業ディレクトリへ展開し、`steamcmd.sh +quit` で初期化する。

生成 VDF の `"SetLive" ""` を維持する。アップロード完了で公開されたと解釈せず、default 変更は [公開手順](publishing.md) で行う。

認証キャッシュを使える場合はそのまま進める。認証が必要になったときだけ、Steam アプリの対象アカウントや Steam Guard の状態を確認する。キャッシュの欠落を Steam アプリのログアウトと断定しない。

## 対話入力が必要な場合

ユーザーが直接入力できる Terminal を使う。Finder から一時 .command を開く方法を使えるが、パスワードや Steam Guard をファイルへ書かない。チャットでコードを収集しない。

起動前と、応答を失った後に `pgrep -fl 'steamcmd.*run_app_build'` とログで実行状態を確認する。Finder の再クリックなどで同じアップロードを並列起動しない。一時 .command は完了後に除く。

## 確認

SteamPipe ログの成功行と、BuildID、Windows Depot `4861251` / macOS Depot `4861252` の manifest ID を記録する。応答不明ならログと Steamworks の BuildID を確認してから再試行し、認証待ちと転送失敗を区別する。
