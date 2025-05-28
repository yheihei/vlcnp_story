# オロチ攻撃時アニメーション機能設計書

## 概要
オロチキャラクターのみが攻撃時にアニメーションを再生し、他のキャラクター（Leelee、Akim）は攻撃アニメーションを再生しない機能の実装。

## コンポーネント設計

### 1. IAttackAnimationController インターフェース
**ファイル**: `Assets/Scripts/Combat/IAttackAnimationController.cs`

**目的**: 攻撃アニメーション制御の共通インターフェース

**メソッド**:
- `TriggerAttackAnimation()`: 攻撃アニメーション開始

### 2. OrochiAttackAnimationController クラス
**ファイル**: `Assets/Scripts/Combat/OrochiAttackAnimationController.cs`

**目的**: オロチ専用の攻撃アニメーション制御

**機能**:
- Animatorコンポーネントから`isAttack`パラメータをtrueに設定
- Any Stateから`PlayerAttack`アニメーションへの遷移をトリガー

### 3. DefaultAttackAnimationController クラス
**ファイル**: `Assets/Scripts/Combat/DefaultAttackAnimationController.cs`

**目的**: デフォルト実装（何もしない）

**機能**:
- 攻撃アニメーション無しキャラクター用
- `TriggerAttackAnimation()`は空実装

### 4. Fighter クラス拡張
**ファイル**: `Assets/Scripts/Combat/Fighter.cs`

**変更点**:
- `IAttackAnimationController attackAnimationController`フィールド追加
- `Awake()`で`GetComponent<IAttackAnimationController>()`取得
- `Attack()`で`attackAnimationController?.TriggerAttackAnimation()`呼び出し

## Unity Editor上での設定手順

### 1. オロチキャラクター設定
**対象プレハブ**: `Assets/Game/Characters/OrochiPlayerVariant.prefab`

1. プレハブを開く
2. `OrochiAttackAnimationController`コンポーネントをアタッチ
3. Animatorコンポーネントが存在することを確認
4. `OrochiLevel1.overrideController`が設定されていることを確認

### 2. Leeleeキャラクター設定
**対象プレハブ**: `Assets/Game/Characters/LeeleePlayerVariant.prefab`

1. プレハブを開く
2. `DefaultAttackAnimationController`コンポーネントをアタッチ

### 3. Akimキャラクター設定
**対象プレハブ**: `Assets/Game/Characters/Player.prefab`

1. プレハブを開く
2. `DefaultAttackAnimationController`コンポーネントをアタッチ

## 技術的特徴

### 単一責任の原則
- アニメーション制御ロジックを専用クラスに分離
- `Fighter`クラスは武器制御に専念
- アニメーション制御は`IAttackAnimationController`実装クラスが担当

### 拡張性
- 新しいキャラクターは適切な`IAttackAnimationController`実装をアタッチするだけ
- 異なるアニメーション制御ロジックも容易に追加可能

### 安全性
- null安全演算子（`?.`）使用でコンポーネント未アタッチ時も動作

## 動作フロー

1. プレイヤーが攻撃ボタン押下
2. `PlayerController.AttackBehaviour()`が`Fighter.Attack()`呼び出し
3. `Fighter.Attack()`が武器のプロジェクタイル発射
4. `Fighter.Attack()`が`attackAnimationController?.TriggerAttackAnimation()`呼び出し
5. オロチの場合：`OrochiAttackAnimationController.TriggerAttackAnimation()`が`isAttack=true`設定
6. その他の場合：`DefaultAttackAnimationController.TriggerAttackAnimation()`は何もしない

## 注意事項

- アニメーションの終了処理（`isAttack=false`設定）は既存のAnimation Event等で対応
- この実装では攻撃開始時のトリガーのみ提供
- プレハブへのコンポーネントアタッチ作業はユーザーが実施する必要がある