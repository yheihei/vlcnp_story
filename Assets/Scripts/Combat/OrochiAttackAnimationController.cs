using UnityEngine;

namespace VLCNP.Combat
{
    public class OrochiAttackAnimationController : MonoBehaviour, IAttackAnimationController
    {
        private Animator animator;

        private void Awake()
        {
            animator = GetComponent<Animator>();
        }

        public void TriggerAttackAnimation()
        {
            if (animator != null)
            {
                animator.SetBool("isAttack", true);
            }
        }

        // Animationイベントから呼び出される
        public void OnFinishAttackAnimation()
        {
            if (animator != null)
            {
                animator.SetBool("isAttack", false);
            }
        }
    }
}
