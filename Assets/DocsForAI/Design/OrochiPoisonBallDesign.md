# オロチ毒玉実装設計書

## 概要
プレイヤーが特定の武器（毒玉）で敵にダメージを与えた時、敵に毒状態を付与する機能を実装しました。

## 実装したコンポーネント

### 1. EnemyPoisonStatus（敵用毒ステータス）
**場所**: `Assets/Scripts/Stats/EnemyPoisonStatus.cs`

**役割**:
- 敵キャラクターの毒状態を管理
- 一定時間ごとに毒ダメージを与える（HPは1まで）
- 毒の開始・終了イベントを発行

**主要パラメータ**:
- `poisonDuration`: 毒の継続時間（デフォルト: 5秒）
- `poisonDamage`: 毒ダメージ量（デフォルト: 1）
- `poisonInterval`: ダメージ間隔（デフォルト: 1秒）

### 2. IPoisonous（毒付与インターフェース）
**場所**: `Assets/Scripts/Combat/IPoisonous.cs`

**役割**:
- 毒を付与できるProjectileを識別するためのインターフェース
- 毒関連のパラメータを提供

### 3. PoisonProjectile（毒付与Projectile）
**場所**: `Assets/Scripts/Combat/PoisonProjectile.cs`

**役割**:
- 通常のProjectile機能に加えて毒付与機能を実装
- 敵にヒットした時、EnemyPoisonStatusを通じて毒を付与

**主要パラメータ**:
- `isPoisonous`: 毒付与の有効/無効
- `poisonDamage`: 付与する毒のダメージ量
- `poisonDuration`: 付与する毒の継続時間
- `poisonInterval`: 付与する毒のダメージ間隔

### 4. EnemyPoisonEffectController（毒エフェクト制御）
**場所**: `Assets/Scripts/Effects/EnemyPoisonEffectController.cs`

**役割**:
- EnemyPoisonStatusのイベントを監視
- 毒状態時にエフェクトを表示/非表示

## 変更箇所

### Projectile.cs
- `OnTriggerEnter2D`メソッドを`protected virtual`に変更
- 継承クラスでオーバーライド可能に

## Unity Editor上での設定手順

### 1. 毒玉Projectileの作成
1. 空のGameObjectを作成
2. `PoisonProjectile`コンポーネントをアタッチ
3. 必要なパラメータを設定:
   - Speed: 弾速
   - Target Tag Name: "Enemy"
   - Is Poisonous: ✓（チェック）
   - Poison Damage: 1
   - Poison Duration: 5
   - Poison Interval: 1
4. SpriteRendererやCollider2Dなど必要なコンポーネントを追加
5. Prefab化して保存

### 2. 敵キャラクターへの設定
1. 敵キャラクターのGameObjectを選択
2. `EnemyPoisonStatus`コンポーネントをアタッチ
3. `EnemyPoisonEffectController`コンポーネントをアタッチ
4. EnemyPoisonEffectControllerの設定:
   - Poison Effect Prefab: 毒エフェクトのPrefabを設定
   - Effect Parent: エフェクトの親Transform（省略可）

### 3. 武器への設定
1. WeaponConfig（ScriptableObject）を作成または編集
2. Weapon Levelsに毒玉Projectileを設定:
   - Projectile Prefab: 作成したPoisonProjectileのPrefab
   - Damage: 通常ダメージ量

### 4. 毒エフェクトPrefabの作成
1. パーティクルシステムまたはアニメーションを含むGameObjectを作成
2. 毒を表現するビジュアルエフェクトを設定
3. Prefab化して保存

## 動作確認手順
1. プレイヤーが毒玉武器を装備
2. 敵キャラクターに毒玉を発射
3. 以下を確認:
   - 敵に毒が付与される
   - 毒エフェクトが表示される
   - 一定間隔でダメージが入る（HPは1まで）
   - 時間経過で毒が治る
   - エフェクトが消える

## 注意事項
- 敵キャラクターには必ず`Health`コンポーネントが必要
- 毒は重複しない（既に毒状態の敵には新たに毒を付与しない）
- 敵が死亡すると毒処理も停止する