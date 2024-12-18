using System.Collections.Generic;
using TNRD;
using UnityEngine;
using VLCNP.Combat;
using VLCNP.Combat.EnemyAction;
using VLCNP.Core;

public class EnemyV2Controller : MonoBehaviour, IStoppable
{
    // 現在の行動のインデックス
    int currentActionIndex = 0;

    [SerializeField]
    public List<SerializableInterface<IEnemyAction>> enemyActions;
    Animator animator;

    [SerializeField]
    string attackTargetTagName = "Player";
    Fighter fighter;

    private bool isStopped;
    public bool IsStopped
    {
        get => isStopped;
        set => isStopped = value;
    }

    // 暗転あけ後の待ち時間
    [SerializeField]
    float StartWaitTime = 1.0f;

    [SerializeField]
    SerializableInterface<IDetect> detect = null;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        fighter = GetComponent<Fighter>();
    }

    // 現在の行動を取得する
    public IEnemyAction GetCurrentAction()
    {
        // 現在の行動の配列の長さが0の場合はnullを返す
        if (enemyActions.Count == 0)
            return null;
        return enemyActions[currentActionIndex].Value;
    }

    // 次の行動に進む
    public void NextAction()
    {
        // 現在の行動のインデックスをインクリメントする
        currentActionIndex++;
        // 現在の行動のインデックスが行動の配列の長さ以上の場合は0に戻す
        if (currentActionIndex >= enemyActions.Count)
            currentActionIndex = 0;
    }

    void FixedUpdate()
    {
        if (isStopped)
        {
            GetCurrentAction()?.Stop();
            return;
        }
        StartWaitTime -= Time.deltaTime;
        if (StartWaitTime > 0f)
            return;
        // プレイヤーを発見していなければ何もしない
        if (detect != null && !detect.Value.IsDetect())
            return;
        // 現在の行動を取得する
        IEnemyAction currentAction = GetCurrentAction();
        if (currentAction == null)
            return;
        // 現在の行動を実行する
        if (!currentAction.IsExecuting)
        {
            currentAction.Execute();
        }
        if (!currentAction.IsDone)
            return;
        // 現在の行動が終了した場合はリセットして次の行動に進む
        currentAction.Reset();
        NextAction();
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (animator.GetBool("isGround"))
            return;
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

    private void OnCollisionStay2D(Collision2D other)
    {
        AttackBehavior(other);
    }

    private void AttackBehavior(Collision2D other)
    {
        if (other.gameObject.tag != attackTargetTagName)
            return;
        fighter.DirectAttack(other.gameObject);
    }
}
