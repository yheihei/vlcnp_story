# イベント対応表

送信名・パラメータの正は `Assets/Scripts/Core/VLCNPAnalytics.cs` と呼び出し元。下表は既存の計測構成。イベント追加・不一致調査では現在のコードと scene / Prefab を確認する。

| イベント | パラメータ | 発火点 |
| --- | --- | --- |
| `areaEntered` | `areaName` | `AreaNameShow.Show` のエリア名バナー表示。同一エリア内の部屋移動では表示されない |
| `bossDefeated` | `bossName` | `Health.defeatedFlag`、`BossDefeated`、`BossDefeatedAnalytics` によるボス死亡 |
| `gameOver` | `sceneName`、`areaName` | `GameOver.Execute` |
| `trialEnd` | なし | 体験版完了挨拶 `TrialEnding_3` の開始 |
| `trial1End` | なし | 旧体験版終了。メソッドは残っているため現在の到達経路は呼び出し元で確認する |

Editor / Development Build は送信をスキップする。新しいイベントの名前・型・Enable 状態と Dashboard の定義を一致させる。閲覧依頼だけでは定義を変更しない。

## areaName

Core の AreaName バナーの文字列をそのまま使う。

| エリア | 値 |
| --- | --- |
| 永遠 | `おひるねべや(永遠)` |
| 土 | `おひるねべや(土)` |
| 闇 | `おひるねべや(闇)` |
| 風 | `おひるねべや(風)` |
| 拠点 | `ベリーロングCNPファーム` |

## bossName

| 値 | シーン | 送り方 |
| --- | --- | --- |
| `VeryEnemyAnimalsBossDefeated` | `Ohirunebeya_5_boss` | `BossDefeated` |
| `VLOrochiBossDefeated` | `Ohirunebeya_tuti_6_boss_2` | `BossDefeatedAnalytics` |
| `VLMitamaBossDefeated` | `Yami5F-2` | `BossDefeatedAnalytics` |
| `VLKamaitachi1Defeated` | `Kaze1` | `Health.defeatedFlag` |
| `VLKamaitachi1Defeated2` | `Kaze2` | `Health.defeatedFlag` |
| `VLNarukamiBossDefeated` | `Kaze3_boss_2` | `BossDefeatedAnalytics` |

既存表ではドラァグクイーン・闇のスケルトン系は計測対象外。追加の依頼では、既存の送信経路がないことを確かめてから `Assets/Scripts/Combat/BossDefeatedAnalytics.cs` を検討する。Health と同じオブジェクトに付け、二重送信を避ける。
