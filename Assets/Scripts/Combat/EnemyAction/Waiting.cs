using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

namespace VLCNP.Combat.EnemyAction
{
    public class Waiting : EnemyAction
    {
        [SerializeField] float waitTimeSecond = 3f;
        DamageStun damageStun;

        private void Awake()
        {
            damageStun = GetComponent<DamageStun>();
        }

        public override void Execute()
        {
            if (IsExecuting) return;
            if (IsDone) return;
            IsExecuting = true;
            StartCoroutine(Wait());
        }

        private IEnumerator Wait()
        {
            damageStun.ValidStan();
            yield return new WaitForSeconds(waitTimeSecond);
            IsDone = true;
        }
    }
}
