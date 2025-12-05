## Plan: FuwaFuwaMoveToPlayer

1. PRDの要件整理: ふわふわ上下動しつつプレイヤーへ接近する新EnemyActionの振る舞い・パラメータ（速度・時間）を確認する。
2. 設計方針策定: RandomFlyToPlayer/Floatingの実装を確認し、移動アルゴリズム・IEnemyActionライフサイクル・SerializeField項目を決める。
3. 実装: EnemyActionクラスとして新スクリプトをAssets/Scripts/Combat/EnemyActionに追加し、PRD要件を反映した移動・時間制御を実装する。
4. 簡易テスト: プレイヤーへの接近・制限時間経過後の完了・上下動が行われるかエディタ上で想定確認（実機テストは別途）。
