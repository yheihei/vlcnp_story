---
name: next-fest-analytics
description: Next Fest 中の体験版の遊ばれ方を Unity Analytics(Unity Gaming Services)で読む手順。Unity Dashboard の Event Manager / Event Browser / Data Explorer / Funnels の場所、エリア到達ファネル(永遠→土→闇→風→trialEnd)の組み方、gameOver のシーン別集計、結果を #635 にコメントで記録する定型。「Next Fest の数字を見て」「どこまで遊ばれたか」「どこで死んでいるか」「計測結果を記録して」と言われたときに使う。
---

# Next Fest 計測の閲覧と記録

体験版がどのエリアまで遊ばれ、どこで死に、どこで離脱するかを Unity Dashboard で読み、
結果を GitHub issue #635 にコメントで残す。難易度と導線の調整(#600)の根拠にする。

## 送っているイベント(#663)

送信元は `Assets/Scripts/Core/VLCNPAnalytics.cs`。エディタと Development Build では送らない(#645)。
エディタでは `[Analytics] <イベント名> (skipped: debug build)` のログだけ出る。

| イベント | パラメータ | 発火点 |
| --- | --- | --- |
| `areaEntered` | `areaName` | エリア名バナー表示時(`AreaNameShow.Show`)。同一エリア内の部屋移動では出ない |
| `bossDefeated` | `bossName` | `defeatedFlag` を持つ `Health` の死亡時、`BossDefeated` の死亡時、`BossDefeatedAnalytics` を付けたボスの死亡時 |
| `gameOver` | `sceneName`, `areaName` | `GameOver.Execute`。sceneName はシーン名、areaName はその時のバナー文字列 |
| `trialEnd` | なし | 体験版完了挨拶シーン `TrialEnding_3` の開始時 |
| `trial1End` | なし | 旧体験版の終了。到達経路は #662 で断ってあり、今は送られない |

`areaName` の値は Core の AreaName バナーの文字列そのもの。

| エリア | areaName |
| --- | --- |
| 永遠 | `おひるねべや(永遠)` |
| 土 | `おひるねべや(土)` |
| 闇 | `おひるねべや(闇)` |
| 風 | `おひるねべや(風)` |
| 拠点 | `ベリーロングCNPファーム` |

`bossName` の値は次のとおり。

| bossName | ボス | シーン | 送り方 |
| --- | --- | --- | --- |
| `VeryEnemyAnimalsBossDefeated` | 永遠の最終ボス Very Enemy Animals | Ohirunebeya_5_boss | `BossDefeated` |
| `VLOrochiBossDefeated` | 土の最終ボス VLヤマタノオロチ | Ohirunebeya_tuti_6_boss_2 | `BossDefeatedAnalytics` |
| `VLMitamaBossDefeated` | 闇の最終ボス ダークミタマ | Yami5F-2 | `BossDefeatedAnalytics` |
| `VLKamaitachi1Defeated` | 風のかまいたち 1 | Kaze1 | `Health.defeatedFlag` |
| `VLKamaitachi1Defeated2` | 風のかまいたち 2 | Kaze2 | `Health.defeatedFlag` |
| `VLNarukamiBossDefeated` | 風の最終ボス VLナルカミ | Kaze3_boss_2 | `BossDefeatedAnalytics` |

ドラァグクイーン、闇のスケルトン系は送られない。新しいボスを計測に足すときは、ボスの Health と同じオブジェクトに
`BossDefeatedAnalytics`(`Assets/Scripts/Combat/BossDefeatedAnalytics.cs`)を付けて `bossName` を入れる。

## 反映の遅れ

- Event Browser は 1〜2 分で出る。ここでは「届いているか」だけを見る
- Data Explorer は 2026-09-07 の確認では 10 分ほどで当日分が出たが、公式には数時間〜1 日の遅れがある
- Funnels は集計テーブルを読むので、当日分は出ない(確認時は完了率 0% のまま)。翌日に見直す
- Next Fest 期間(10/19〜26)の集計は 10/27 以降に確定値を取る

## Unity Dashboard の場所

プロジェクトは `vlcnpStory`(組織 `yhei`、組織 ID `3770273`)。ログインは Unity ID。環境は `production`。

- 入口: https://cloud.unity.com/home → 左の **Projects** → `vlcnpStory` → 左の Shortcuts **Analytics**
- 直接開く URL(`<page>` を差し替える):
  `https://cloud.unity.com/home/organizations/3770273/projects/350f81b2-bbff-4c8b-adab-976470923dcb/environments/ca25b9ee-f5a5-4d72-9c69-ee12ece4758b/analytics/v2/<page>`
- Analytics の左メニュー
  - **Manage → Event Manager**(`<page>` = `events`): イベントとパラメータの定義。未登録・無効のイベントは Invalid に落ちて捨てられる
  - **Manage → Event Browser**(`eventBrowser`): 直近 48 時間・最大 100 件の生イベント。届いているかの確認用
  - **Analyze → Data Explorer**(`data-explorer`): イベント数の日別グラフ。パラメータで Group by できる
  - **Analyze → Funnels**(`funnels`): イベントを段に並べ、各段の到達ユーザー数と離脱率を出す

Chrome の MCP タブで開くときは、上の URL か Projects から辿る。`/projects/<id>/analytics` のような短い URL は 404 になる。

## 手順 0: 新しいイベントを足したとき(Event Manager)

1. Event Manager → **Add New → Custom Parameter** で、まずパラメータを作る(name / description / type=String)
2. **Add New → Custom Event** で name / description を入れ、**Assign Parameter** で検索してチェック
3. **Enable event** にチェックを付けてから **Confirm**(付け忘れると Invalid に落ちる)
4. 登録前に届いた分は後から Valid にならない。登録後にもう一度送って確認する

## 手順 1: 届いているかを確認する(Event Browser)

1. Event Browser を開く(Valid Events タブ)
2. **Add filters → Event Name** で `areaEntered` などを選ぶ
3. 数件でも並べば OK。行末の `<>` で JSON を開くとパラメータの値(`areaName` など)が読める
4. 0 件なら **Invalid Events** タブを見る。そこに出ていれば Event Manager の定義(名前・パラメータ・Enable)がずれている
5. Invalid にも無ければ、ビルドが Development になっていないか(`Data collection started` が Player.log に出るか)を疑う

## 手順 2: エリア到達ファネルを読む(Funnels)

保存済みのファネル **`NextFest エリア到達`** を開き、右上の期間を Next Fest の開催期間に合わせる。
段の構成は次のとおり(作り直すときも同じ)。

| 段 | イベント | Add Parameter の条件 |
| --- | --- | --- |
| 1 | `areaEntered` | `areaName` Equal To `おひるねべや(永遠)` |
| 2 | `areaEntered` | `areaName` Equal To `おひるねべや(土)` |
| 3 | `areaEntered` | `areaName` Equal To `おひるねべや(闇)` |
| 4 | `areaEntered` | `areaName` Equal To `おひるねべや(風)` |
| 5 | `trialEnd` | なし |

作り直すとき: Funnels → **New Funnel** → Step ごとに Select Event(入力して Enter で確定)→ **Add Parameter** → `areaName` → 値を入力 → **Add Step** で段を増やす → **Apply** → **Save Funnel** で名前を付ける。

読むもの: 各段の「Users」と、段の間の離脱率。グラフ上部の completion rate が全体の到達率。

## 手順 3: gameOver をシーン別に見る(Data Explorer)

保存済みのレポート **`NextFest gameOver x sceneName`** を開き、期間を合わせて **Apply**。

作り直すとき(旧 Data Explorer を使う。New Experience はカスタムイベントが集計されるまで一覧に出ない):

1. Data Explorer → **New Report** → ダイアログで **Old Experience**
2. **Add Event** → `gameOver`
3. **Group by** → `Event Parameter` → `sceneName`(`areaName` にするとエリア単位)
4. **Apply**。グラフ種別は右の **Line** を **Column** や **Stacked Bar** にすると比べやすい
5. 上の「New Report」欄に名前を入れて **Save Report**

`+ Add parameter` は Group by ではなくフィルタ(`sceneName` Equal To 値)なので、特定シーンだけ見たいときに使う。

死亡数の多いシーンは、そのエリアの `areaEntered` ユーザー数(ファネルの段の Users)で割って「1 人あたり死亡回数」にする。

## 手順 4: #635 にコメントで記録する

`gh issue comment 635 --body-file <ファイル>` で、次の定型で残す。数値が 0 でも書く(反映待ちの記録になる)。

```markdown
## 計測記録 YYYY-MM-DD(集計期間 MM/DD〜MM/DD)

### 届いているか(Event Browser、Valid Events)

| イベント | 件数 |
| --- | --- |
| areaEntered | 0 |
| bossDefeated | 0 |
| gameOver | 0 |
| trialEnd | 0 |

### エリア到達(Funnels: NextFest エリア到達)

| 段 | ユーザー数 | 前段からの離脱 |
| --- | --- | --- |
| 永遠 | 0 | - |
| 土 | 0 | 0% |
| 闇 | 0 | 0% |
| 風 | 0 | 0% |
| trialEnd | 0 | 0% |

### ゲームオーバー(Data Explorer: gameOver × sceneName)

| シーン | 回数 |
| --- | --- |
| (上位 5 つ) | 0 |

### 所見

- 1〜3 行。数字から言えることだけ書く。対策は #600 側に書く

### 注意

- Data Explorer / Funnels は反映に数時間〜1 日。YYYY-MM-DD HH:MM UTC 時点の値
```

## 注意

- イベント名やパラメータ名をコードで変えたら、必ず Event Manager も合わせる。ずれた分は静かに捨てられる
- 自分の動作確認プレイも本番の数字に混ざる。Next Fest の集計は期間で区切って除外する
- 数字の解釈で迷ったら、Event Browser で 1 人の生イベントの並びを数件読むと早い
- 動作確認の手順(リリースビルドでセーブを差し替えて任意シーンから始める)は #663 のコメントを参照
