# コードベース構造

## プロジェクトルート構造
```
.
├── Assets/                 # Unityアセット一式
├── Packages/              # Unityパッケージ設定
├── ProjectSettings/       # Unityプロジェクト設定
├── .github/               # GitHub Actions設定（Claude PR Action）
├── .vscode/               # VS Code設定
├── CLAUDE.md, AGENTS.md   # AI/開発ガイドライン
└── README.md              # サウンドアセット情報など
```

## Assets配下の主要ディレクトリ
- `Assets/Scripts/` – ゲームプレイロジック（実装時に編集可能）
- `Assets/Game/` – Prefab類
- `Assets/Scenes/` – シーンデータ
- `Assets/DocsForAI/` – AI支援用ドキュメント
  - `Plan/` – タスクごとのPlan文書
  - `Design/` – 実装後の設計書出力先
  - `Prd/` – 要件・PRD類
- その他（`Assets/UI`, `Assets/Characters`, `Assets/Map` など）は主にアート/リソース。通常は編集しない。

## Scripts構造（Assets/Scripts/）
- `Core/` – フラグ管理、BGM、Analytics等のコアシステム
- `Combat/` – 武器、ダメージ、投射物
- `Movement/` – プレイヤーの移動・ジャンプ・水中挙動
- `Control/` – プレイヤーや敵の入力制御
- `Attributes/` – ステータス/HP等
- `SceneManagement/` – シーン遷移、ポータル
- `Actions/` – インタラクション、チャット関連
- `Saving/` – セーブ/ロード、`IJsonSaveable` 実装
- `Projectiles/` – 投射物と状態異常システム
- `Stats/` – レベル/経験値
- `Pickups/` – アイテム取得
- `Effects/` – ビジュアル/状態異常表示
- `UI/`, `Movie/`, `DebugScript/`, `Editor/` – UIや演出、ツール群

## その他
- `Packages/manifest.json` に URP 12.1.8、Cinemachine 2.8.9、TextMeshPro 3.0.6 等が記載。
- GitHub Actions (`.github/workflows/claude.yml`) は Claude PR Action のみ。