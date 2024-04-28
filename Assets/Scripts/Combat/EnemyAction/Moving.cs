using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

namespace VLCNP.Combat.EnemyAction
{
    public class Moving : MonoBehaviour, IEnemyAction
    {
        bool isDone = false;
        bool isExecuting = false;
        public bool IsDone { get => isDone; set => isDone = value; }
        public bool IsExecuting { get => isExecuting; set => isExecuting = value; }
        [SerializeField] float speed = 4;
        [SerializeField] float moveX = 0;

        Rigidbody2D rbody;
        Animator animator;
        float vx = 0;

        private void Awake() {
            rbody = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();
        }

        public void Exeute()
        {
            if (isExecuting) return;
            if (isDone) return;
            isExecuting = true;
            Vector3 position = transform.position;
            Vector3 destinationPosition = new Vector3(position.x + moveX, position.y, position.z);
            StartCoroutine(MoveToPosition(destinationPosition));
        }

        private IEnumerator MoveToPosition(Vector3 position, float timeout = 0, bool keepDirection = false)
        {
            // 経過時間を格納する変数
            float elapsedTime = 0;
            // プレイヤーの位置と指定のx位置が特定の値以内になるまでループ
            while (Mathf.Abs(position.x - transform.position.x) > 0.05f)
            {
                // 経過時間加算
                elapsedTime += Time.deltaTime;
                // タイムアウト値になったらループを抜ける
                if (timeout > 0 && elapsedTime > timeout)
                {
                    break;
                }
                // 指定の位置に向かって移動
                UpdateMoveSpeed(position.x < transform.position.x ? -speed : speed);
                yield return null;
            }
            UpdateMoveSpeed(0);
            isDone = true;
        }

        private void UpdateMoveSpeed(float _vx)
        {
            vx = _vx;
            rbody.velocity = new Vector2(vx, rbody.velocity.y);
            animator?.SetFloat("vx", Mathf.Abs(vx));
        }

        /**
         * 行動実行後 再度実行可能にする
         */
        public void Reset()
        {
            isDone = false;
            isExecuting = false;
        }
    }
}
