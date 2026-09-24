# Dashboard 操作

UI と反映時間の記述は既存運用の手掛かり。現在の画面と集計状態を確認して使う。

## 場所

- 組織 `yhei`、organization ID `3770273`
- プロジェクト `vlcnpStory`、project ID `350f81b2-bbff-4c8b-adab-976470923dcb`
- 環境 `production`、environment ID `ca25b9ee-f5a5-4d72-9c69-ee12ece4758b`
- 入口は [Unity Dashboard](https://cloud.unity.com/home) の Projects → vlcnpStory → Analytics。
- 既存 URL の形は `https://cloud.unity.com/home/organizations/3770273/projects/350f81b2-bbff-4c8b-adab-976470923dcb/environments/ca25b9ee-f5a5-4d72-9c69-ee12ece4758b/analytics/v2/<page>`。リンクが変わっていれば入口から辿る。

| 画面 | 既存 page 値 | 用途 |
| --- | --- | --- |
| Manage → Event Manager | `events` | イベントとパラメータの定義 |
| Manage → Event Browser | `eventBrowser` | 最近の生イベントの受信確認 |
| Analyze → Data Explorer | `data-explorer` | 件数・パラメータ別集計 |
| Analyze → Funnels | `funnels` | 順序付きの到達ユーザー数 |

## 受信の確認

Event Browser で Event Name を絞り、Valid Events の JSON からパラメータを確認する。0件なら Invalid Events の理由、定義名・型・Enable、環境を照合する。Invalid にもなければ、Release / Development、送信初期化、Player.log を調べる。

反映の目安は Event Browser が数分、Data Explorer が10分程度で、Funnels は当日分が出ないことがある。現画面の更新時刻と期間を確認し、反映待ちを判別できなければ未確定と報告する。自動の再確認・通知は依頼された場合にだけ設定する。

## エリア到達

保存済みファネル名は `NextFest エリア到達`。期間とタイムゾーン、完了までの条件を確認する。

| 段 | イベント | 条件 |
| --- | --- | --- |
| 永遠 | `areaEntered` | `areaName` = `おひるねべや(永遠)` |
| 土 | `areaEntered` | `areaName` = `おひるねべや(土)` |
| 闇 | `areaEntered` | `areaName` = `おひるねべや(闇)` |
| 風 | `areaEntered` | `areaName` = `おひるねべや(風)` |
| 完了 | `trialEnd` | なし |

Users、段間の離脱、全体の completion rate を読む。開催日はその回の依頼・開催情報に合わせ、固定日付を転用しない。

新規作成が依頼されている場合は New Funnel で各 Step の Event と Parameter を設定し、Apply 後に保存する。閲覧のためだけに共有の保存条件を書き換えない。

## シーン別 gameOver

既存レポートは `NextFest gameOver x sceneName`。Event を `gameOver`、Group by を Event Parameter の `sceneName` にする。`areaName` ならエリア別集計となる。

`Add parameter` は絞り込みであり Group by ではない。イベント回数とユーザー数を区別する。死亡回数の多さだけで難易度を断定しない。

New Experience の一覧にイベントが出ない場合は、Old Experience への切り替えがあれば使う。新レポートの保存は依頼範囲に含まれる場合に行う。

## イベントの登録・修正を依頼された場合

Event Manager で既存の定義を確認し、足りない Custom Parameter を作成してから Custom Event へ割り当て、Enable を確認する。登録前の無効イベントが遡及修復されることを前提にせず、必要なら登録後の送信で確認する。コードのイベント名を変える場合も Dashboard との対応を検証する。
