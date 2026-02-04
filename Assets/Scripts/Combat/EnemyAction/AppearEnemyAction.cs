using System.Collections;
using UnityEngine;

namespace VLCNP.Combat.EnemyAction
{
    public class AppearEnemyAction : EnemyAction
    {
        [SerializeField]
        Animator animator = null;

        [SerializeField]
        Collider2D hurtboxCollider = null;

        [SerializeField]
        Renderer visualRenderer = null;

        [SerializeField]
        string hideBoolParam = "isHide";

        [SerializeField]
        string appearBoolParam = "isAppear";

        bool IsAppeared = false;

        private void Awake()
        {
            if (animator == null)
                animator = GetComponent<Animator>();
            if (visualRenderer == null)
                visualRenderer = GetComponent<Renderer>();
            if (hurtboxCollider == null)
                hurtboxCollider = GetComponent<Collider2D>();

            if (hurtboxCollider == null)
                Debug.LogWarning("AppearEnemyAction: hurtboxCollider is not set.");
            if (visualRenderer == null)
                Debug.LogWarning("AppearEnemyAction: visualRenderer is not set.");
            if (animator == null)
                Debug.LogWarning("AppearEnemyAction: animator is not set.");
        }

        private void Start()
        {
            animator.SetBool(hideBoolParam, true);
            SetVisible(false);
            SetHurtboxEnabled(false);
        }

        public override void Execute()
        {
            if (IsExecuting || IsDone)
                return;
            IsExecuting = true;

            if (IsAppeared)
            {
                IsDone = true;
                IsExecuting = false;
                return;
            }

            animator.SetBool(hideBoolParam, false);
            animator.SetBool(appearBoolParam, true);
            
            SetVisible(true);
            SetHurtboxEnabled(true);

            if (animator == null)
            {
                IsDone = true;
                IsExecuting = false;
                return;
            }
        }

        public void OnAppearAnimationFinished()
        {
            if (animator != null)
                animator.SetBool(appearBoolParam, false);
            IsDone = true;
            IsExecuting = false;
            IsAppeared = true;
        }

        private void SetVisible(bool value)
        {
            if (visualRenderer != null)
                visualRenderer.enabled = value;
        }

        private void SetHurtboxEnabled(bool value)
        {
            if (hurtboxCollider != null)
                hurtboxCollider.enabled = value;
        }
    }
}
