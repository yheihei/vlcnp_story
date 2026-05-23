using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

namespace VLCNP.Combat.EnemyAction
{
    public class LookAtPlayer : EnemyAction
    {
        [SerializeField] float afterWaitTimeSecond = 1f;
        DamageStun damageStun;
        Transform cachedTransform;
        Transform playerTransform;

        public enum Direction
        {
            Left,
            Right
        }

        Direction direction = Direction.Left;

        private void Awake()
        {
            damageStun = GetComponent<DamageStun>();
            cachedTransform = transform;
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
            if (!TryGetPlayerTransform(out Transform player))
            {
                IsDone = true;
                yield break;
            }
            if (player.position.x < cachedTransform.position.x)
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
            Vector3 localScale = cachedTransform.localScale;
            if (direction == Direction.Left)
            {
                cachedTransform.localScale = new Vector3(Mathf.Abs(localScale.x), localScale.y, localScale.z);
            }
            else
            {
                cachedTransform.localScale = new Vector3(-1 * Mathf.Abs(localScale.x), localScale.y, localScale.z);
            }
        }

        private bool TryGetPlayerTransform(out Transform player)
        {
            if (
                playerTransform == null
                || !playerTransform.gameObject.activeInHierarchy
                || !playerTransform.CompareTag("Player")
            )
            {
                GameObject playerObject = GameObject.FindWithTag("Player");
                playerTransform = playerObject != null ? playerObject.transform : null;
            }

            player = playerTransform;
            return player != null;
        }
    }
}
