using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

namespace VLCNP.Combat.EnemyAction
{
    public class Waiting : MonoBehaviour, IEnemyAction
    {
        bool isDone = false;
        bool isExecuting = false;
        public bool IsDone { get => isDone; set => isDone = value; }
        public bool IsExecuting { get => isExecuting; set => isExecuting = value; }

        [SerializeField] float waitTimeSecond = 3f;
        [SerializeField] public uint priority = 1;
        public uint Priority { get => priority; }
        DamageStun damageStun;

        private void Awake()
        {
            damageStun = GetComponent<DamageStun>();
        }

        public void Execute()
        {
            if (isExecuting) return;
            if (isDone) return;
            isExecuting = true;
            StartCoroutine(Wait());
        }

        private IEnumerator Wait()
        {
            damageStun.ValidStan();
            yield return new WaitForSeconds(waitTimeSecond);
            isDone = true;
        }

        /**
         * 行動実行後 再度実行可能にする
         */
        public void Reset()
        {
            isDone = false;
            isExecuting = false;
        }

        public void Stop()
        {
            // 何もしない
        }
    }
}
