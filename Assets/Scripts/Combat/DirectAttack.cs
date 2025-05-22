using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events; // UnityEventを使用するために追加

namespace VLCNP.Combat
{
    public class DirectAttack : MonoBehaviour
    {
        [SerializeField]
        Fighter fighter;

        [SerializeField]
        Collider2D touchCollider; // PRDの指示に基づき追加しますが、通常はGetComponent<Collider2D>()で十分な場合もあります。

        [SerializeField]
        List<string> attackTargetTagNames = new List<string>();

        // 攻撃成功時に呼び出されるUnityEvent
        [SerializeField]
        UnityEvent<GameObject> OnAttackSuccess;

        private void AttemptAttack(GameObject targetObject)
        {
            if (fighter == null)
                return;

            foreach (string tagName in attackTargetTagNames)
            {
                if (targetObject.CompareTag(tagName))
                {
                    fighter.DirectAttack(targetObject);
                    OnAttackSuccess?.Invoke(targetObject); // イベントを発火
                    // 1回の呼び出しにつき1回攻撃を実行
                    break;
                }
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            AttemptAttack(other.gameObject);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            AttemptAttack(other.gameObject);
        }
    }
}
