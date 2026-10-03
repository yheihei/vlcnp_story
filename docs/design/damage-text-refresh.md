# ダメージ表記改善 — Claude制作・ローカルQA

実Claude Code 2.1.286の既存Claude Max認証を使用し、**claude-opus-5-5** に実ゲーム画像と実装を渡してデザイン仕様・完全なC#クラスを制作してもらった。制作セッション `015ac2ac-fdb6-4758-be15-feabe360a4e1` のinit/model、Read/Write、result success/is_error=falseを記録。実装後の720p/1080p画像もClaudeが確認し、「意図どおり、必須修正なし、現在値で確定」と最終レビューした。

Codexは調査、公式Unity CLI/Pipelineへの適用、技術レビュー、QAと性能測定を担当。デザインの変更は行わず、Shadowの頂点数に関するコメントだけ実測に合わせて訂正した。

## 変更

- 初回ヒットは1.45倍で現れ、0.18秒で通常サイズに落ち着く。0.22秒で琥珀色から象牙色へ変化。
- 表示中の追加hitは最大1.16倍の控えめな再反応。短い間隔ほど弱め、最大サイズと位置を固定範囲に保つ。
- 表示の中盤は静止、最後の0.32秒は少し浮いてフェード。死亡後もフェード完了まで残す。
- 既存font/UGUI Text/World Space Canvasを使用し、Shadowを初回のみ用意。色とalphaはCanvasRendererで変更。
- 対象ごとの既存数字1枚を再利用。新しいpoolやグローバル上限による数字の欠落は導入していない。同時数は各対象1枚＋既存レベル通知1枚まで。死亡後の孤立数字は有限時間で消える。
- 合算、表示期間、書式、Healthイベント、レベル通知、戦闘計算・頻度は維持。新しい戦闘ルール・素材・packageは追加していない。

## QA

Unity 6000.3.24f1、墓地の実Play Modeで以下を通過。

- 実Health.TakeDamage(1)から同じ値の数字へ到達。
- 0表示、同フレーム100hit合算、数字/Shadow/Materialの再利用、描画レイヤー・順序保持、拡大上限。
- 初回pixelsPerUnit=1000、Canvas再構築後の色/alpha、左右反転と親30度回転時の正立。
- LevelUP/LevelDOWN、ダメージpopのサイズ/色/Shadowがレベル通知へ漏れないこと、非表示partyの古い通知消去。
- timeScale=0を2.2実秒保持、timeScale=.25、実Updateのpop→静止→fade→1.5秒後reset/Update停止、死亡後fade破棄、次windowの合計リセット。
- Editor PauseでtimeScale=1でもshowTimeが0のまま。
- QA専用シーンのアンロードで孤立数字・親を破棄し、実プレイヤーExperience購読数が1→2→1に戻る。
- 変更後の新規Console Error/Exceptionは0。再コンパイルで既存の他クラス/third-party警告が出るが、DamageTextV2由来の警告はない。

## 性能（実測と制約）

同じEditor Mono、凍結world、offscreenの実UGUIを用い、1/32/128対象を毎フレームhit、Canvas.ForceUpdateCanvasesを含めて各18frame計測。setup・初回style/font warmupを除外した。Standaloneフレーム時間やGPU時間ではない。

| 対象数 | 変更前 hit+Canvas平均 | 変更後平均（再測定） | 増分 |
|---:|---:|---:|---:|
| 1 | 0.142ms | 0.160ms | +0.019ms |
| 32 | 0.415ms | 0.722ms | +0.307ms |
| 128 | 1.257ms | 1.751ms | +0.494ms |

最初のafter測定は1対象に2.619msの単発スパイクがあり、再測定でばらつきを確認した。追加コストは存在するため「負荷ゼロ」「軽い」とは断定しない。

ProfilerのGC Allocated In Frameで100KB既知割当を捕捉する正の対照を通過。フレーム全体のGCなのでEditor背景割当を含む。各測定のidle最小値を引いたhitフレーム最小値の増分は**24 / 770 / 3136 bytes**でbeforeとafter再測定が一致。これは専用のコード単位GC測定ではない。GC.GetAllocatedBytesForCurrentThreadは既知割当にも0を返し、結果に使用しなかった。既存ToString割当は残る。

128対象を連続hitしてもText数128→128、Material数増加0。1桁のmeshは4→12頂点、2→4三角形。Shadowの三角形数は2倍、indexed meshの頂点数は3倍になる。

別の同期計測で、文字内容を変えず初動の姿勢を動かす処理＋Canvas更新は1/32/128対象で0.006/0.011/0.026ms（各100回）。数字のDirtyVerticesコールバックは0回。GPUのDrawCall/overdraw、Standalone FPS、他ハードウェア、初回表示のstyle生成コストは未計測。

## 見た目の証拠・環境保護

`task-6/evidence/before-after.png` は左before/右afterの初動。`damage-after.mp4` は実墓地シーンの数字を24→36→48と合算し、最後までfadeする2.65秒動画。worldを固定し、実DamageTextV2を0.05秒刻みでサンプリングしたGame camera描画で、ライブ戦闘録画ではない。720p/1080p静止画像も保存。

MacロックでCUAの画面操作は使用できなかったが、公式Unity CLIとUnity Game camera描画で実画面・動作を検証した。作業前はmain 4edf43de、差分なし、既存Editorは墓地シーン・dirtyなし。独立branch `feature/damage-text-claude` に適用し、複数Editorは起動していない。QA用のscene/object/Assets一時出力を清掃し、元の墓地シーンを保存せず保持。セーブ2ファイルは事前退避し、終了時に2/2とも元のSHA-256と一致した（変更されておらず、復元コピー不要）。mainへのpush/merge、Steam更新、新規issueは行っていない。

比較画像 `before-after.png` はLibraryにも保存済み（836,609 bytes、1920×540）。native Library ID: `libfile_708ddf04331881918b776bd3638cb909`、file ID: `file_00000000373081f687cdf5a5c3b77e77`。動画 `damage-after.mp4` はH.264/960×540/2.65秒/99,388 bytes。ローカルプレーヤーで再生でき、0秒の初動・0.3秒の静止・0.35/0.70秒の累積・後半のfadeを確認する。

生ログ・再実行可能なQA C#は作業workspaceの`evidence/`と`qa/`。Claude仕様は`claude-design.md`、最終レビューは`claude-visual-review.md`。
