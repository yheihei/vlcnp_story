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
    }
}