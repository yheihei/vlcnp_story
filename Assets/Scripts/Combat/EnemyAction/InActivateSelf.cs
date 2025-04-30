using System.Collections;
using UnityEngine;

namespace VLCNP.Combat.EnemyAction
{
    public class InActivateSelf : EnemyAction
    {
        private void Awake() { }

        public override void Execute()
        {
            if (IsExecuting)
                return;
            if (IsDone)
                return;
            IsExecuting = true;
            StartCoroutine(InActivateExecute());
        }

        private IEnumerator InActivateExecute()
        {
            // 0.1s後に非アクティブにする
            yield return new WaitForSeconds(0.1f);
            IsDone = true;
            gameObject.SetActive(false);
        }
    }
}
