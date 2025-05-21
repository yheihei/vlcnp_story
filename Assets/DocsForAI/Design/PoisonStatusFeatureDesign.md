# 毒ステータス機能 設計書

## 1. 目的

プレイヤーキャラクターがダメージを受けた際に毒状態を付与し、一定時間移動速度を低下させる機能を実装する。また、キャラクター切り替え時の毒状態の挙動を管理する。加えて、毒状態のキャラクターには専用の視覚エフェクトを表示する。

## 2. 主要コンポーネントと役割

### 2.1. `PoisonStatus.cs` (`VLCNP.Stats`)
-   キャラクターの毒状態（毒にかかっているか、治癒までの残り時間）を管理する。
-   毒にかかっている間、`Mover`コンポーネントと連携してキャラクターの移動速度を50%に低下させる。
-   毒の治癒時間をカウントダウンし、0になったら毒状態を解除し、移動速度を元に戻す。
-   **`OnPoisonStarted` イベント**: 毒状態が開始されたときに発火する。
-   **`OnPoisonCured` イベント**: 毒状態が治癒されたときに発火する。
-   アタッチされたGameObjectが無効化されると、Unityのライフサイクルにより`Update`メソッドが呼び出されなくなるため、実質的に治癒時間のカウントが停止する。
-   アタッチされたGameObjectが有効化されると、毒状態であれば`Update`メソッドの処理により治癒時間のカウントが再開される。

### 2.2. `PoisonAttacher.cs` (`VLCNP.Combat`)
-   `AttachPoison(GameObject target)` メソッドを提供する。
-   対象の `GameObject` に `PoisonStatus` コンポーネントが存在しない場合は追加し、その後 `PoisonStatus.ActivatePoison()` を呼び出して毒状態を開始させる。

### 2.3. `Mover.cs` (`VLCNP.Movement`)
-   `speedModifier` (float型) のメンバー変数を持ち、移動速度の計算時にこの値を乗算する。
-   `SetSpeedModifier(float modifier)` メソッドを提供し、外部（主に`PoisonStatus`）から `speedModifier` の値を変更できるようにする。

### 2.4. `PartyCongroller.cs` (`VLCNP.Control`)
-   キャラクター切り替え処理 (`SwitchToPlayer` メソッド内) で以下を行う。
    -   切り替え前キャラクターのGameObjectを非アクティブ化し、切り替え後キャラクターのGameObjectをアクティブ化する。これにより、`PoisonStatus`の治癒時間カウントはGameObjectの有効/無効状態に応じて自動的に停止/再開される。
-   毒状態はキャラクター固有であり、切り替え時に他のキャラクターには引き継がれない（`PoisonStatus`が各キャラクターに個別管理されるため）。

### 2.5. `DirectAttack.cs` (`VLCNP.Combat`)
-   `UnityEvent<GameObject> OnAttackSuccess` を持つ。
-   攻撃が成功した際 (`AttemptAttack` メソッド内) に、このイベントを発火し、攻撃対象の `GameObject` を引数として渡す。

### 2.6. `PoisonEffectController.cs` (`VLCNP.Effects`)
-   毒状態のエフェクト表示を管理する。
-   インスペクターで以下の設定が必要:
    -   `poisonEffectPrefab`: 表示する毒エフェクトのプレファブ。
    -   `effectParent`: エフェクトを追従させる親となる `Transform`（例: キャラクターの頭部付近の空GameObject）。
-   `PoisonStatus` の `OnPoisonStarted` イベントを購読し、エフェクトプレファブを `effectParent` の子としてインスタンス化して表示する。
-   `PoisonStatus` の `OnPoisonCured` イベントを購読し、表示中のエフェクトインスタンスを破棄する。
-   エフェクトの表示位置は、`effectParent` の Transform と、`poisonEffectPrefab` のローカル Transform に依存する。プレファブは原点に配置し、`effectParent` で表示位置を調整することを推奨。
-   アタッチされたGameObjectが無効化 (`OnDisable`) されると、表示中のエフェクトを非表示にし、有効化 (`OnEnable`) されると現在の毒状態に応じてエフェクトを再表示する機能を持つ（具体的な実装は`PoisonEffectController`自身に依存）。

## 3. 主要な動作フロー

### 3.1. 毒付与フロー
1.  `DirectAttack` がターゲットに攻撃を試みる (`AttemptAttack`)。
2.  攻撃が成功すると `DirectAttack.OnAttackSuccess` イベントが発火される (引数は攻撃対象の `GameObject`)。
3.  (Unity Editorでの設定により) `OnAttackSuccess` イベントが `PoisonAttacher.AttachPoison(GameObject target)` を呼び出す。
4.  `PoisonAttacher` は対象の `GameObject` に `PoisonStatus` を付与（または取得）し、`PoisonStatus.ActivatePoison()` を実行する。
5.  `PoisonStatus.ActivatePoison()` は自身の毒状態を `true` にし、治癒時間を設定後、`Mover.SetSpeedModifier(0.5f)` を呼び出して移動速度を50%にし、`OnPoisonStarted` イベントを発火する。

### 3.2. 毒エフェクト表示フロー
1.  `PoisonEffectController` は、アタッチされたキャラクターの `PoisonStatus.OnPoisonStarted` イベントを購読する。
2.  `PoisonStatus.OnPoisonStarted` イベントが発火すると、`PoisonEffectController.ShowEffect()` が呼び出される。
3.  `ShowEffect()` は、指定された `poisonEffectPrefab` を `effectParent` の子としてインスタンス化し、表示する。

### 3.3. 毒治癒フロー
1.  `PoisonStatus` が毒状態 (`isPoisoned == true`) の間、`Update()` メソッドで治癒残り時間 (`remainingTime`) を減少させる。
2.  `remainingTime` が0以下になると `Cure()` メソッドが呼び出される。
3.  `PoisonStatus.Cure()` は自身の毒状態を `false` にし、`Mover.SetSpeedModifier(1f)` を呼び出して移動速度を100%に戻し、`OnPoisonCured` イベントを発火する。

### 3.4. 毒エフェクト非表示フロー
1.  `PoisonEffectController` は、アタッチされたキャラクターの `PoisonStatus.OnPoisonCured` イベントを購読する。
2.  `PoisonStatus.OnPoisonCured` イベントが発火すると、`PoisonEffectController.HideEffect()` が呼び出される。
3.  `HideEffect()` は、表示中のエフェクトインスタンスを破棄する。

### 3.5. キャラクター切り替え時のフロー
1.  プレイヤーがキャラクター切り替え操作を行う。
2.  `PartyCongroller.SwitchToPlayer(GameObject nextPlayer)` が呼び出される。
3.  切り替え前のキャラクター (`previousPlayer`) の GameObject は非アクティブ化される。これにより、`previousPlayer` にアタッチされた `PoisonStatus` の `Update` が停止し、毒の治癒時間カウントも実質停止する。また、`PoisonEffectController` も非アクティブ化され、エフェクトが非表示になる（`OnDisable`の挙動による）。
4.  切り替え後のキャラクター (`currentPlayer`) の GameObject はアクティブ化される。これにより、`currentPlayer` にアタッチされた `PoisonStatus` が毒状態であれば `Update` により治癒時間カウントが進行する。`PoisonEffectController` も有効化され、現在の毒状態に応じてエフェクトが表示される（`OnEnable`の挙動による）。

## 4. Unity Editorでの設定

-   攻撃時に毒を付与したい `GameObject` にアタッチされている `DirectAttack` コンポーネントのインスペクターを開く。
    -   `On Attack Success (GameObject)` イベントスロットに、`PoisonAttacher` コンポーネントを持つ `GameObject` (例えば、攻撃を仕掛けるキャラクター自身や、シーン内の専用マネージャーなど) をドラッグ＆ドロップする。
    -   ドロップダウンメニューから `PoisonAttacher` -> `AttachPoison (GameObject)` を選択する。
-   毒エフェクトを表示したいキャラクターの `GameObject` (またはその子オブジェクト) に `PoisonEffectController.cs` をアタッチする。
    -   `PoisonEffectController` のインスペクターで以下を設定する:
        -   `Poison Effect Prefab`: 毒エフェクトとして使用するプレファブ (例: `Assets/Game/Effect/PoisonEffect.prefab`)。
        -   `Effect Parent`: エフェクトを表示する基準となる `Transform`。キャラクターの頭部付近に作成した空の `GameObject` の `Transform` などを指定する。 