using UnityEngine;
using VLCNP.Combat.EnemyAction;
using VLCNP.Core;

public class EnemyV2Controller : MonoBehaviour, IStoppable
{
    IEnemyAction[] enemyActions;
    // 現在の行動のインデックス
    int currentActionIndex = 0;
    Animator animator;

    private bool isStopped;
    public bool IsStopped { get => isStopped; set => isStopped = value; }

    private void Awake()
    {
        // IEnemyActionのインターフェースを持つコンポーネントを取得する
        enemyActions = GetComponents<IEnemyAction>();
        // enemyActionsをPriorityでソートする
        System.Array.Sort(enemyActions, (a, b) => (int)(a.Priority - b.Priority));
        animator = GetComponent<Animator>();
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
        if (isStopped)
        {
            GetCurrentAction()?.Stop();
            return;
        }
        // 現在の行動を取得する
        IEnemyAction currentAction = GetCurrentAction();
        if (currentAction == null) return;
        // 現在の行動を実行する
        if (!currentAction.IsExecuting)
        {
            currentAction.Execute();
        }
        if (!currentAction.IsDone) return;
        // 現在の行動が終了した場合はリセットして次の行動に進む
        currentAction.Reset();
        NextAction();
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (animator.GetBool("isGround")) return;
        if (other.gameObject.CompareTag("Ground"))
        {
            animator.SetBool("isGround", true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Ground"))
        {
            animator.SetBool("isGround", false);
        }
    }

    public void Stop()
    {
        isStopped = true;
    }

    public void Resume()
    {
        isStopped = false;
    }
}
