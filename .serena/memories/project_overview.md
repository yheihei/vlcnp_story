# VLCNP Story - プロジェクト概要

## プロジェクトの目的
Unity 2021.3 を使用した 2D メトロイドヴァニア型アクションゲーム。WebGL 向けにビルドされ、年末までのリリースと月間アクティブユーザー 1,000 人を目標とする。

## 主要機能 / 開発体制
- Core: フラグ・セーブ・BGM など共通基盤
- Combat: 武器・投射物・状態異常システム
- Movement/Control: プレイヤー移動や入力制御
- UI/Movie: 演出、UI 表示
- DocsForAI フォルダで PRD / Plan / Design ドキュメントを管理（Plan→実装→Design の順で作成）。

## 技術スタック
- エンジン: Unity 2021.3 (URP 12.1.8)
- 言語: C# (Assembly-CSharp)
- ターゲット: WebGL
- 主なパッケージ: Cinemachine 2.8.9、TextMeshPro 3.0.6、PostProcessing 3.2.2、Newtonsoft JSON 3.2.1、Unity Test Framework 1.1.31、Visual Scripting 1.7.8

## 開発環境
- IDE: Rider / Visual Studio / VS Code
- バージョン管理: Git + GitHub
- CI: `.github/workflows/claude.yml` (Claude PR Action)

## サウンド/アセット
- 効果音・BGM は README.md に出典を記載
- Assets/ 以下に UI、Characters、Map など各種リソースフォルダが存在

## ドキュメンテーション
- PRD: `Assets/DocsForAI/Prd`
- Plan: `Assets/DocsForAI/Plan`
- 設計書: `Assets/DocsForAI/Design`
- 実装時は `Assets/Scripts/` のみ編集可能