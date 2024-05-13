using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VLCNP.Combat.EnemyAction
{
    public class Gatotu : MonoBehaviour, IEnemyAction
    {
        bool isDone = false;
        bool isExecuting = false;
        public bool IsDone { get => isDone; set => isDone = value; }
        public bool IsExecuting { get => isExecuting; set => isExecuting = value; }

        [SerializeField] WeaponConfig weaponConfig = null;
        [SerializeField] Transform handTransform = null;
        [SerializeField] Transform legTransform = null;
        [SerializeField] float animationOffsetWaitTime = 0.917f;
        [SerializeField] private uint priority = 1;
        public uint Priority { get => priority; }
        private Animator animator;
        [SerializeField] GameObject auraEffect;
        GameObject aura;
        Rigidbody2D rbody;
        [SerializeField] float speed = 12;
        [SerializeField] float moveX = 0;
        [SerializeField] GameObject weaponPrefab = null;
        float vx = 0;

        public enum Direction
        {
            Left,
            Right
        }

        Direction direction = Direction.Left;

        private void Awake()
        {
            rbody = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();
        }

        public void Execute()
        {
            if (isExecuting) return;
            if (isDone) return;
            isExecuting = true;
            StartCoroutine(Throw());
        }

        private IEnumerator Throw()
        {
            // プレイヤーの方向を向く
            GameObject player = GameObject.FindWithTag("Player");
            if (player == null)
            {
                isDone = true;
                yield break;
            }
            if (player.transform.position.x < transform.position.x)
            {
                SetDirection(Direction.Left);
            }
            else
            {
                SetDirection(Direction.Right);
            }

            // 武器を表示
            weaponPrefab.SetActive(true);
            // auraを2s間表示
            if (auraEffect != null)
            {
                aura = Instantiate(auraEffect, legTransform.position, Quaternion.identity, legTransform);
                yield return new WaitForSeconds(2f);
                Destroy(aura);
            }
            // 1s待つ
            yield return new WaitForSeconds(1f);
            // animatorのトリガーを発動する
            if (animator != null)
            {
                animator.SetTrigger("special1");
            }
            // 突撃
            Vector3 position = transform.position;
            float _moveX = moveX;
            Vector3 destinationPosition = new Vector3(position.x + _moveX, position.y, position.z);
            yield return MoveToPosition(destinationPosition, animationOffsetWaitTime);
            // 武器を非表示
            weaponPrefab.SetActive(false);
            isDone = true;
        }

        private IEnumerator MoveToPosition(Vector3 position, float timeout = 0)
        {
            // プレイヤーの位置が指定のx位置より左にある場合は左を向く
            if (position.x < transform.position.x)
            {
                SetDirection(Direction.Left);
            }
            else
            {
                SetDirection(Direction.Right);
            }
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
        }

        private void UpdateMoveSpeed(float _vx)
        {
            vx = _vx;
            rbody.velocity = new Vector2(vx, 0.1f);
        }

        public void SetDirection(Direction _direction)
        {
            direction = _direction;
            UpdateCharacterDirection();
        }

        private void UpdateCharacterDirection()
        {
            if (direction == Direction.Left)
            {
                transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
            else
            {
                transform.localScale = new Vector3(-1 * Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
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
