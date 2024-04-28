using UnityEngine;
using VLCNP.Combat.EnemyAction;

public class EnemyV2Controller : MonoBehaviour
{
    IEnemyAction[] enemyActions;
    // 現在の行動のインデックス
    int currentActionIndex = 0;

    private void Awake()
    {
        // IEnemyActionのインターフェースを持つコンポーネントを取得する
        enemyActions = GetComponents<IEnemyAction>();
    }

    // 現在の行動を取得する
    public IEnemyAction? GetCurrentAction()
    {
        // 現在の行動の配列の長さが0の場合はnullを返す
        if (enemyActions.Length == 0) return null;
        return enemyActions[currentActionIndex];
    }

    // 次の行動に進む
    public void NextAction()
    {
        // 現在の行動のインデックスをインクリメントする
        currentActionIndex++;
        // 現在の行動のインデックスが行動の配列の長さ以上の場合は0に戻す
        if (currentActionIndex >= enemyActions.Length) currentActionIndex = 0;
    }

    void FixedUpdate()
    {
        // 現在の行動を取得する
        IEnemyAction? currentAction = GetCurrentAction();
        if (currentAction == null) return;
        // 現在の行動を実行する
        if (!currentAction.IsExecuting)
        {
            currentAction.Exeute();
        }
        // 現在の行動が完了した場合は次の行動に進む
        if (currentAction.IsDone)
        {
            currentAction.Reset();
            NextAction();
        }
    }
}
