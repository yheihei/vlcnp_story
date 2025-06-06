# PRD - Projectile状態効果システム

## 目的

- Projectileが敵にダメージを与えた際に、様々な状態効果を柔軟に付与できるシステムを構築したい
- 特定のProjectileだけでなく、既存の全てのProjectileタイプ（通常弾、バウンド弾など）に状態効果を追加できるようにしたい
- 複数の状態効果を1つのProjectileに組み合わせて適用できるようにしたい
- 新しい状態効果の追加が既存コードの変更なしに行えるようにしたい

## 機能要件

### 基本システム
- Projectileがターゲットにダメージを与えた後、設定された状態効果が自動的に適用される
- UnityEventによる疎結合な設計で、Projectileと状態効果システムが独立して動作する
- Unity Editorで視覚的に状態効果を設定・管理できる

### 麻痺効果（初期実装）
- 麻痺を受けた敵は速度が0.5倍になる(可変)
- 麻痺は一定時間で自動的に治る
- 麻痺の重複付与は発生しない（既に麻痺状態の敵には新たに麻痺を付与しない）
- 麻痺状態の敵にはエフェクトが表示される

### 状態効果の管理
- ScriptableObjectとして状態効果を作成・管理
- 状態効果のパラメータ（速度の倍数、継続時間、間隔など）をUnity Editorで設定可能
- 同じ状態効果を複数のProjectileで共有可能

## 技術的制約

- 単一責任の原則を守り、各クラスが明確な役割を持つこと
- 既存のProjectileクラス（Projectile、BouncingProjectile）への変更は最小限に留める
- UnityEventを活用した疎結合な設計とする
- SerializableInterfaceパッケージを使用してインターフェースをUnity Editorで扱えるようにする
-「速度のmultipleを管理するコンポーネント」を用意し敵キャラにアタッチする想定。麻痺時はその値を変更すると良い
  - `@Assets/Scripts/Combat/EnemyAction/EnemyAction.cs` 配下の移動に絡む速度変数に掛け算させるとよさそう。「速度のmultipleを管理するコンポーネント」がない場合は1倍に

## 開発ガイドライン

### アーキテクチャ
- IProjectileStatusEffectインターフェースによる状態効果の抽象化
- ProjectileStatusEffectApplierコンポーネントによるイベント処理
- ScriptableObjectによる状態効果データの管理

### 拡張性
- 新しい状態効果はIProjectileStatusEffectを実装するだけで追加可能
- CreateAssetMenuを使用してUnity Editorから状態効果アセットを作成
- 複数の状態効果を1つのProjectileに設定可能

### イベント駆動設計
- IProjectileインターフェースにOnTargetHitイベントを追加
- ダメージ処理が成功した後にイベントを発火
- ProjectileStatusEffectApplierがイベントを購読し、状態効果を適用

## Unity Editor上での使用方法

### 状態効果アセットの作成
1. Projectメニューから「Create > Projectile Status Effects > [効果名]」を選択
2. 作成されたアセットでパラメータを設定
3. 分かりやすい名前を付けて保存

### Projectileへの設定
1. ProjectileプレファブにProjectileStatusEffectApplierコンポーネントを追加
2. Status Effectsリストに使用したい状態効果アセットを追加
3. 複数の効果を組み合わせる場合はリストに複数のアセットを設定

### 敵キャラクターへの設定
1. 状態効果を受ける敵キャラクターに対応するStatusコンポーネントをアタッチ
2. エフェクト表示用のControllerコンポーネントをアタッチ
3. エフェクトPrefabを設定

## 期待される成果物

- 柔軟で拡張性の高い状態効果システム
- 既存のProjectileシステムとの完全な互換性
- Unity Editorでの直感的な設定・管理機能
- 新しい状態効果の容易な追加
- 麻痺の実装

## 将来の拡張予定

- 毒効果（継続ダメージ）
- 凍結効果（一時的な行動停止）
- 燃焼効果（継続ダメージ + 炎エフェクト）
- 回復効果（味方への適用）
- バフ/デバフ効果（攻撃力・防御力の変更）