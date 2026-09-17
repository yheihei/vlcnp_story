# Skill 監査記録 2026-09-12

対象はプロジェクトの12件と個人グローバルの3件。短い適用条件、必要時だけ読む詳細、依頼範囲に応じた完了条件を軸に修正した。モデル名や固定の確認回数に依存する運用を減らし、プロジェクト固有の制約を残した。

## 対象別の変更

| 配置 | Skill | 変更 |
| --- | --- | --- |
| プロジェクト | [implementation-workflow](../.agents/skills/implementation-workflow/SKILL.md) | 関係する規約だけを読む構成に変更。重複コンパイル、固定回数での中断、文書へのUnity検証を除いた |
| プロジェクト | [unity-project-conventions](../.agents/skills/unity-project-conventions/SKILL.md) | 配置・namespace・マップ構成に絞り、規約合わせだけの既存API変更を避けるよう整理 |
| プロジェクト | [git-workflow](../.agents/skills/git-workflow/SKILL.md) | ゲーム概要や実装手順の重複を削除。main・日本語メッセージ・既知issue番号を維持 |
| プロジェクト | [unity-editor-automation](../.agents/skills/unity-editor-automation/SKILL.md) | 保存・差分更新・Eval制約を本体に残し、残留ダイアログと復旧を条件付き参照へ分離 |
| プロジェクト | [unity-playmode-verification](../.agents/skills/unity-playmode-verification/SKILL.md) | 観察する変更に適用を限定。Play Modeの調整値を記録し、終了後に反映する手順を明確化 |
| プロジェクト | [unity-performance-optimizer](../.agents/skills/unity-performance-optimizer/SKILL.md) | 全面スキャンを任意化。実測と静的推定を区別し、移行や設定変更を一律に推奨しない形に変更 |
| プロジェクト | [create-enemy-character](../.agents/skills/create-enemy-character/SKILL.md) | Prefab・Health、Action・検知、描画・物理を分離。RangeDetectを現行コードへ合わせた |
| プロジェクト | [generate-2d-sprite](../.agents/skills/generate-2d-sprite/SKILL.md) | 直接生成とCLI委譲を分離。固定effort・sandbox無効化の指示を削除し、透過Propsと不透明タイルを区別 |
| プロジェクト | [building-tilemap-builder](../.agents/skills/building-tilemap-builder/SKILL.md) | 壁の調整とUnityアセット化を分離。単一修正に素材一式の再生成を要求しない形に変更 |
| プロジェクト | [stage-map-assets](../.agents/skills/stage-map-assets/SKILL.md) | 必須質問票・固定枚数・ビルダーのコピー指示を除去。生成例と後処理を分離し、取り込み手順は建物素材側と共有 |
| プロジェクト | [steam-demo-release](../.agents/skills/steam-demo-release/SKILL.md) | ビルド・公証・SteamPipe・公開へ分離。許可済み公開の再確認と、ビルドだけの依頼への公開工程追加を避ける形に変更 |
| プロジェクト | [next-fest-analytics](../.agents/skills/next-fest-analytics/SKILL.md) | イベント表・Dashboard・記録例を分離。分析からの自動投稿、未取得の0埋め、母集団の異なる率の計算を修正 |
| グローバル | [unity-performance-optimizer](/Users/yhei/.codex/skills/unity-performance-optimizer/SKILL.md) | Unity全般向けに整理し、WebGL前提とプロジェクト固有の実装範囲を除去 |
| グローバル | [building-tilemap-builder](/Users/yhei/.codex/skills/building-tilemap-builder/SKILL.md) | 汎用の建物素材手順に変更。配置・PPU・Palette layerは対象プロジェクトへ合わせる |
| グローバル | [steam-demo-release](/Users/yhei/.codex/skills/steam-demo-release/SKILL.md) | プロジェクト版を参照する入口に集約。AppIDや署名・公開手順の二重管理を解消 |

## 個別に修正した問題

- 敵の検知設定にあった `undetectedAnimationName` は現行コードに存在しない。`enableUndetectedAnimation`、Animatorの `isUndetected`、`OnUndetectedAnimationFinished()` の接続へ修正した。
- Play Mode終了後に調整値が残る前提を除き、終了前の記録とEdit Modeでの再適用を記載した。
- 生成ログに `generated_images` があるかだけで画像生成の実施を判定する指示を除いた。成果物・寸法・透明度・実画像を確認する。
- Steamの未保存シーンについて「ユーザーが保存したと言った場合だけ保存する」という停止条件を除き、依頼の保存対象と他の作業の未保存変更を区別した。
- Event Browserの表示件数を全体件数と混同しないようにした。シーンの死亡回数とファネルのエリア到達者数は、同じ母集団と確認できない限り率にしない。
- Next Festの日付、DashboardのUIと遅延時間を恒久的な仕様から外し、実行時の確認または日付付きの過去観測として扱った。

## 維持した制約

Play Mode中のEval禁止、手動のシーン調整と既存GUIDの保護、一時Editorスクリプトの後片付け、StatClassの末尾追加、左向き素材の規約を維持した。

SteamのAppID・Depot、署名IdentityとEntitlements、内側からの署名、Accepted後のstaple、認証コードの直接入力、SetLiveを空にしたアップロード、公開後のBuildID照合を維持した。

## 読み込み量

SKILL.md本体15件の合計は97,678 bytesから26,675 bytesへ、72.7%減少した。詳細の一部は条件付きの参照へ移しているため、これは全ファイル容量やトークン数の削減率ではない。

descriptionは15件とも44〜64文字に収めた。既存のUI設定10件を更新し、既存の暗黙起動ポリシーと依存設定を維持した。

## 検証

- skill-creatorの `quick_validate.py` で15件すべて成功。
- UI設定10件のYAML、説明の長さ、スキル名を含む起動プロンプトを確認。
- Skill内のローカルリンク41件について参照先の存在を確認。
- 対象Markdownの空白とコードフェンス、対象差分の `git diff --check` を確認。
- 既存Pythonスクリプト5ファイルはSHA-256で変更なしを確認。
- 実コードのRangeDetect・EnemyAction・VLCNPAnalyticsと、既存のビルダー・SteamPipe設定を必要箇所で照合。

文面の適用範囲も、既知の性能問題、単一の壁修正、ビルドのみ、許可済み公開、分析のみ、未取得データという依頼例で点検した。これは文面の静的な点検であり、別エージェントの実行評価ではない。

今回の変更はSkillと監査文書。Unityの実行、画像生成、Steam・Unity Dashboardへの書き込みを伴う実地検証は行っていない。
