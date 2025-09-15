# VLCNP Story - プロジェクト概要

## プロジェクトの目的
Unity 2021.3を使用した2Dメトロイドヴァニア型アクションゲーム。WebGL向けにビルドされ、年末までのリリースと月間アクティブユーザー1,000人を目標としている。

## 技術スタック
- **ゲームエンジン**: Unity 2021.3
- **ビルドターゲット**: WebGL  
- **言語**: C#
- **レンダリングパイプライン**: URP (Universal Render Pipeline) 12.1.8

## 主要パッケージ
- Cinemachine 2.8.9 - カメラ制御
- TextMeshPro 3.0.6 - テキスト表示
- PostProcessing 3.2.2 - ポストエフェクト
- Visual Scripting 1.7.8
- Sprite Glow - スプライトエフェクト
- Newtonsoft Json 3.2.1

## 開発環境
- IDE: Rider, Visual Studio, VS Code対応
- バージョン管理: Git (GitHub Actions統合)
- CI/CD: GitHub Actions (Claude PR Assistant設定済み)

## サウンドアセット
効果音とBGMは外部サイトから取得（README.mdに記載）