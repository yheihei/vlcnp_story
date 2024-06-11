using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

namespace VLCNP.Combat.EnemyAction
{
    public class LookAtPlayer : EnemyAction
    {
        [SerializeField] float afterWaitTimeSecond = 1f;
        DamageStun damageStun;
        public enum Direction
        {
            Left,
            Right
        }

        Direction direction = Direction.Left;

        private void Awake()
        {
            damageStun = GetComponent<DamageStun>();
        }

        public override void Execute()
        {
            if (IsExecuting) return;
            if (IsDone) return;
            IsExecuting = true;
            StartCoroutine(Look());
        }

        private IEnumerator Look()
        {
            damageStun.ValidStan();
            // プレイヤーの方向を向く
            GameObject player = GameObject.FindWithTag("Player");
            if (player == null)
            {
                IsDone = true;
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
            yield return new WaitForSeconds(afterWaitTimeSecond);
            IsDone = true;
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
    }
}
