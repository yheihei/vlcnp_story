# コードベース構造

## プロジェクトルート構造
```
.
├── Assets/                 # Unityアセット
├── Packages/              # Unityパッケージ設定
├── ProjectSettings/       # Unityプロジェクト設定
├── .github/               # GitHub Actions設定
├── .vscode/              # VS Code設定
├── CLAUDE.md             # Claude AI用ガイドライン
└── README.md             # サウンドアセット情報

```

## 重要：有効なアセットディレクトリ
**開発で使用するディレクトリ（Assets配下）**:
- `Assets/Scripts/` - ゲームプレイロジック
- `Assets/Game/` - 各種Prefab
- `Assets/Scenes/` - シーン

**その他のAssetsフォルダは試作レベルで開発には使用しない**

## Scripts構造（Assets/Scripts/）
- `UI/` - UI関連のコンポーネント
- `Movie/` - カットシーン、演出関連
- `Core/` - ゲームのコアシステム（フラグ管理、BGM、Analytics等）
- `Combat/` - 戦闘システム（武器、ダメージ、プロジェクタイル）
- `Movement/` - 移動システム（ジャンプ、ダッシュ、水中移動）
- `Control/` - コントローラー（プレイヤー、エネミー、トラップ）
- `Attributes/` - 属性システム（HP、ステータス）
- `SceneManagement/` - シーン遷移、ポータル
- `Actions/` - インタラクション、チャット
- `Saving/` - セーブシステム
- `Projectiles/` - 投射物
- `Stats/` - ステータス、経験値、レベル
- `Pickups/` - アイテム取得
- `DebugScript/` - デバッグ用スクリプト
- `Effects/` - エフェクト、状態異常表示
- `Editor/` - エディター拡張

## ドキュメント
- `Assets/DocsForAI/Design/` - AI用設計書保存先（実装後に作成）