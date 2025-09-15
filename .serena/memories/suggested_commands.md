# 開発用コマンド一覧

## Unityプロジェクトの特性
このプロジェクトはUnityプロジェクトのため、通常のCLIコマンドではなく、Unity Editor上での操作が中心となります。

## Git関連コマンド
```bash
# ブランチ確認
git status
git branch

# 変更確認
git diff

# コミット履歴
git log

# プルリクエスト作成（GitHub CLI使用時）
gh pr create
```

## ファイル操作（macOS/Darwin）
```bash
# ディレクトリ表示
ls -la

# ファイル検索
find . -name "*.cs"

# テキスト検索（ripgrep推奨）
rg "search_term"

# ディレクトリ移動
cd path/to/directory
```

## Unity特有の操作
**Unity Editorでの作業**:
1. **ビルド**: File > Build Settings > Build
2. **実行**: Play ボタン
3. **テスト**: Window > General > Test Runner（ただし現在テストは未実装）
4. **コンソール確認**: Window > General > Console

## プロジェクト固有の注意事項
- Unity 2021.3 を使用
- WebGLビルドのみ対応
- テストフレームワークは未実装
- linter/formatterツールは未設定

## CLAUDE.mdの規約
- 実装時は`Assets/Scripts/`配下のみ編集可能
- 実装後は`Assets/DocsForAI/Design/`に設計書を保存