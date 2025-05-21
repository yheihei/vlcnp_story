# 毒ステータス機能 設計書

## 1. 目的

プレイヤーキャラクターがダメージを受けた際に毒状態を付与し、一定時間移動速度を低下させる機能を実装する。また、キャラクター切り替え時の毒状態の挙動を管理する。

## 2. 主要コンポーネントと役割

### 2.1. `PoisonStatus.cs` (`VLCNP.Stats`)
-   キャラクターの毒状態（毒にかかっているか、治癒までの残り時間）を管理する。
-   毒にかかっている間、`Mover`コンポーネントと連携してキャラクターの移動速度を50%に低下させる。
-   毒の治癒時間をカウントダウンし、0になったら毒状態を解除し、移動速度を元に戻す。
-   アタッチされたGameObjectが無効化 (`OnDisable`) されると、実質的に治癒時間のカウントが停止する。
-   アタッチされたGameObjectが有効化 (`OnEnable`) されると、毒状態であれば治癒時間のカウントが再開される（`Update`処理による）。
-   `PauseTimer()` / `ResumeTimer()`: 外部から明示的にタイマーの挙動を制御するためのメソッド（現在は`PartyCongroller`からキャラクターの有効/無効状態の変更と合わせて呼び出される）。

### 2.2. `PoisonAttacher.cs` (`VLCNP.Combat`)
-   `AttachPoison(GameObject target)` メソッドを提供する。
-   対象の `GameObject` に `PoisonStatus` コンポーネントが存在しない場合は追加し、その後 `PoisonStatus.ActivatePoison()` を呼び出して毒状態を開始させる。

### 2.3. `Mover.cs` (`VLCNP.Movement`)
-   `speedModifier` (float型) のメンバー変数を持ち、移動速度の計算時にこの値を乗算する。
-   `SetSpeedModifier(float modifier)` メソッドを提供し、外部（主に`PoisonStatus`）から `speedModifier` の値を変更できるようにする。

### 2.4. `PartyCongroller.cs` (`VLCNP.Control`)
-   キャラクター切り替え処理 (`SwitchToPlayer` メソッド内) で以下を行う。
    -   切り替え前のキャラクターの `PoisonStatus` を取得し、存在すれば `PauseTimer()` を呼び出す。
    -   切り替え後のキャラクターの `PoisonStatus` を取得し、存在すれば `ResumeTimer()` を呼び出す。
-   毒状態はキャラクター固有であり、切り替え時に他のキャラクターには引き継がれない（`PoisonStatus`が各キャラクターに個別管理されるため）。

### 2.5. `DirectAttack.cs` (`VLCNP.Combat`)
-   `UnityEvent<GameObject> OnAttackSuccess` を持つ。
-   攻撃が成功した際 (`AttemptAttack` メソッド内) に、このイベントを発火し、攻撃対象の `GameObject` を引数として渡す。

## 3. 主要な動作フロー

### 3.1. 毒付与フロー
1.  `DirectAttack` がターゲットに攻撃を試みる (`AttemptAttack`)。
2.  攻撃が成功すると `DirectAttack.OnAttackSuccess` イベントが発火される (引数は攻撃対象の `GameObject`)。
3.  (Unity Editorでの設定により) `OnAttackSuccess` イベントが `PoisonAttacher.AttachPoison(GameObject target)` を呼び出す。
4.  `PoisonAttacher` は対象の `GameObject` に `PoisonStatus` を付与（または取得）し、`PoisonStatus.ActivatePoison()` を実行する。
5.  `PoisonStatus.ActivatePoison()` は自身の毒状態を `true` にし、治癒時間を設定後、`Mover.SetSpeedModifier(0.5f)` を呼び出して移動速度を50%にする。

### 3.2. 毒治癒フロー
1.  `PoisonStatus` が毒状態 (`isPoisoned == true`) の間、`Update()` メソッドで治癒残り時間 (`remainingTime`) を減少させる。
2.  `remainingTime` が0以下になると `Cure()` メソッドが呼び出される。
3.  `PoisonStatus.Cure()` は自身の毒状態を `false` にし、`Mover.SetSpeedModifier(1f)` を呼び出して移動速度を100%に戻す。

### 3.3. キャラクター切り替え時のフロー
1.  プレイヤーがキャラクター切り替え操作を行う。
2.  `PartyCongroller.SwitchToPlayer(GameObject nextPlayer)` が呼び出される。
3.  切り替え前のキャラクター (`previousPlayer`) の `PoisonStatus` が取得され、存在すれば `PauseTimer()` が呼び出される。`previousPlayer` の GameObject は非アクティブ化されるため、`Update` が停止し、タイマーも実質停止する。
4.  切り替え後のキャラクター (`currentPlayer`) の `PoisonStatus` が取得され、存在すれば `ResumeTimer()` が呼び出される。`currentPlayer` の GameObject はアクティブ化されるため、毒状態であれば `Update` によりタイマーが進行する。

## 4. Unity Editorでの設定

-   攻撃時に毒を付与したい `GameObject` にアタッチされている `DirectAttack` コンポーネントのインスペクターを開く。
-   `On Attack Success (GameObject)` イベントスロットに、`PoisonAttacher` コンポーネントを持つ `GameObject` (例えば、攻撃を仕掛けるキャラクター自身や、シーン内の専用マネージャーなど) をドラッグ＆ドロップする。
-   ドロップダウンメニューから `PoisonAttacher` -> `AttachPoison (GameObject)` を選択する。 