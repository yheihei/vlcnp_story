# オロチ攻撃時アニメーションシステム実装計画

## 概要
オロチキャラクターのみが攻撃時にアニメーションを再生し、LeeleeやAkimは攻撃アニメーションを再生しない仕組みを実装する。

## 現状分析

### 現在の実装
- `PlayerController.cs`: 攻撃入力を検知し、`Fighter.Attack()`を呼び出し
- `Fighter.cs`: 武器のプロジェクタイル発射のみ実装、アニメーション制御なし
- アニメーション: `OrochiLevel1.overrideController`の`PlayerAttack`、`isAttack`パラメータで制御

### 課題
- 攻撃アニメーションの制御ロジックが存在しない
- キャラクター固有の動作を判別する仕組みが必要

## 実装計画

### 1. アニメーション制御インターフェース作成
- `IAttackAnimationController`インターフェース
  - `TriggerAttackAnimation()`メソッド定義

### 2. 具象クラス実装
- `OrochiAttackAnimationController`: オロチ用、アニメーション再生
- `DefaultAttackAnimationController`: その他キャラクター用、何もしない

### 3. Fighter クラス拡張
- `IAttackAnimationController`の参照を追加
- `Attack()`メソッドでアニメーション制御を呼び出し

### 4. 既存プレハブ対応
- `OrochiPlayerVariant.prefab`: `OrochiAttackAnimationController`をアタッチ
- `LeeleePlayerVariant.prefab`: `DefaultAttackAnimationController`をアタッチ
- `Player.prefab`: `DefaultAttackAnimationController`をアタッチ

## 技術的制約への対応
- 単一責任の原則: アニメーション制御を専用クラスに分離
- Unity Editor作業: コンポーネントのアタッチはユーザーが実施

## ファイル構成
- `Assets/Scripts/Combat/IAttackAnimationController.cs`
- `Assets/Scripts/Combat/OrochiAttackAnimationController.cs`
- `Assets/Scripts/Combat/DefaultAttackAnimationController.cs`
- `Assets/Scripts/Combat/Fighter.cs`（修正）