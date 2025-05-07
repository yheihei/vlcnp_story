using System.Collections;
using System.Collections.Generic;
using TNRD;
using UnityEngine;
using VLCNP.Projectiles;

namespace VLCNP.Combat.EnemyAction
{
    public class Fire : EnemyAction
    {
        Animator animator;

        [SerializeField]
        private List<SerializableInterface<IFire>> firePrefabs;

        [SerializeField]
        private Transform spawnPoint;

        [SerializeField]
        private float fireInterval = 1f;

        [SerializeField]
        private float animationOffsetWaitTime = 0.417f;

        private void Awake()
        {
            animator = GetComponent<Animator>();
        }

        public override void Execute()
        {
            if (IsExecuting)
                return;
            if (IsDone)
                return;
            IsExecuting = true;
            StartCoroutine(FireCoroutine());
        }

        private IEnumerator FireCoroutine()
        {
            animator?.SetBool("isMagic", true);
            // ちょっと待つ
            yield return new WaitForSeconds(animationOffsetWaitTime);
            foreach (var target in firePrefabs)
            {
                if (target == null)
                    continue;
                GameObject fire = Instantiate(
                    target.Value.GetGameObject(),
                    spawnPoint.position,
                    Quaternion.identity
                );
                fire.GetComponent<IFire>()?.Fire(new Vector3(IsLeft() ? -1 : 1, 0, 0));
                yield return new WaitForSeconds(fireInterval);
            }
            animator?.SetBool("isMagic", false);
            IsDone = true;
        }

        private bool IsLeft()
        {
            return transform.localScale.x > 0;
        }
    }
}
