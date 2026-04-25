---
name: git-workflow
description: Git workflow rules for /Users/yhei/unity/vlcnpStory2022. Use when Codex works in this repository and needs to commit, push, choose whether to create a branch, write a commit message, or handle issue-linked work.
---

# Git Workflow

## Overview

この skill は `/Users/yhei/unity/vlcnpStory2022` の commit / push 方針を定義する。ユーザーへの応答は日本語で行う。

## Project Context

- Unity: 2021.3
- Target: WebGL
- Genre: Metroidvania-style 2D action
- Goal: year-end release and 1,000 monthly active users

## Directory Scope

実装作業では主に次のディレクトリを使う。

- `Assets/Scripts/`: gameplay logic
- `Assets/Game/`: prefabs
- `Assets/Scenes/`: scenes

ユーザーが明示した場合、または依頼を安全に完了するために必要な場合を除き、他のディレクトリは実装対象外として扱う。

## Git Rules

- ユーザーが明示的に依頼しない限り、feature ブランチを作らない。
- `main` に直接 commit して push してよい。
- commit 前に diff を確認し、無関係なユーザー変更を stage しない。
- 元の issue がある場合、commit message に issue 番号を `#618` の形式で含める。
- issue 番号がローカル文脈から分からない場合は、捏造せず、ユーザーに確認するか省略する。
- issue 番号が分かっている場合、commit message は `Fix player jump timing #618` のように簡潔にする。

## Validation

コード変更時は、利用可能なプロジェクトツールで Unity の検証を優先する。C# 編集では実行可能な範囲でコンパイルまたは関連 Unity テストを実行し、検証できなかった場合は明記する。
