# DirectAttackとPoisonAttacherの連携プラン

1.  **`DirectAttack.cs` の修正**:
    *   `UnityEngine.Events.UnityEvent<GameObject>` 型のシリアライズ可能なメンバー変数を追加します。これを `OnAttackSuccess` のような名前にしましょう。このイベントは、攻撃が成功した対象の `GameObject` を引数として渡せるようにします。
    *   `AttemptAttack` メソッド内で、`fighter.DirectAttack(targetObject);` が呼び出された直後（つまり攻撃が成功したと判断できる箇所）で、この `OnAttackSuccess` イベントを発火 (Invoke) させ、引数として `targetObject` を渡します。
2.  **Unity Editor上での設定**:
    *   `DirectAttack` コンポーネントがアタッチされている GameObject のインスペクターで、上記で追加した `OnAttackSuccess` イベントのスロットが表示されるようになります。
    *   このスロットに、`PoisonAttacher` コンポーネントがアタッチされている GameObject をドラッグ＆ドロップします。
    *   ドロップダウンメニューから `PoisonAttacher` -> `AttachPoison (GameObject)` を選択します。これにより、`DirectAttack` の `OnAttackSuccess` イベントが発火した際に、`PoisonAttacher` の `AttachPoison` メソッドが、攻撃対象の `GameObject` を引数として呼び出されるようになります。 