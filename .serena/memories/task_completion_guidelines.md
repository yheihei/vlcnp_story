# タスク完了時のガイドライン

## 実装完了時の確認事項

### 1. コード品質確認
- 既存のコードスタイルに従っているか
- 適切な名前空間（VLCNP.*）を使用しているか
- 不要なコメントを追加していないか

### 2. Unity Editor上での確認
- **コンソールエラー確認**: Window > General > Console でエラーがないことを確認
- **動作確認**: Play モードで実際の動作を確認
- **Prefab更新**: 必要に応じてPrefabを更新

### 3. 設計書作成
実装完了後は必ず`Assets/DocsForAI/Design/`配下に設計書（.md）を作成：
- コンポーネントの役割と変更箇所
- Unity Editor上での操作マニュアル
- 実装した機能の概要

### 4. Git管理
- 変更内容を`git status`で確認
- `git diff`で詳細な変更を確認
- **注意**: コミットはユーザーが明示的に指示した場合のみ実行

### 5. 制限事項
- 編集可能ファイル: `Assets/Scripts/`配下のみ
- その他のAssetsフォルダは試作レベルのため編集禁止
- プルリクエストのマージはユーザーが実施

## 現在の制約
- **テストツール**: 未実装（Unity Test Frameworkは導入済みだが未使用）
- **Linter/Formatter**: 未設定
- **CI/CD**: GitHub Actions（Claude PR Assistant）のみ設定済み