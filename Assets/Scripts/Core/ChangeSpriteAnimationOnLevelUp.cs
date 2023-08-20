using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VLCNP.Stats;

namespace VLCNP.Core
{
    public class ChangeSpriteAnimationOnLevelUp : MonoBehaviour
    {
        // レベルごとに採用するAnimationをSerializeFieldで指定する
        [SerializeField] AnimatorOverrideController[] animatorOverrideControllers = null;
        [SerializeField] ColliderPerLevel[] colliderPerLevels = null;

        [System.Serializable]
        public class ColliderPerLevel
        {
            public float capsuleCollider2DYSize;
            public float capsuleCollider2DYOffset;
            public float boxCollider2DYOffset;
            public float handTransformY;
        }

        Animator animator;

        private void Awake() {
            animator = GetComponent<Animator>();
            ChangeAnimation(GetComponent<BaseStats>().GetLevel());
        }

        private void OnEnable() {
            GetComponent<BaseStats>().OnChangeLevel += ChangeAnimation;
        }

        private void OnDisable() {
            GetComponent<BaseStats>().OnChangeLevel -= ChangeAnimation;
        }

        public void ChangeAnimation(int level)
        {
            int levelIndex = level < animatorOverrideControllers.Length ? level - 1 : animatorOverrideControllers.Length - 1;
            animator.runtimeAnimatorController = animatorOverrideControllers[levelIndex];
            ChangeCollider(levelIndex);
        }

        private void ChangeCollider(int levelIndex)
        {
            CapsuleCollider2D playerCollider = GetComponent<CapsuleCollider2D>();
            BoxCollider2D playerGroundCollider = GetComponent<BoxCollider2D>();
            playerCollider.size = new Vector2(playerCollider.size.x, colliderPerLevels[levelIndex].capsuleCollider2DYSize);
            playerCollider.offset = new Vector2(playerCollider.offset.x, colliderPerLevels[levelIndex].capsuleCollider2DYOffset);
            playerGroundCollider.offset = new Vector2(playerGroundCollider.offset.x, colliderPerLevels[levelIndex].boxCollider2DYOffset);
            Transform hand = transform.Find("Hand");
            if (hand != null)
            {
                hand.localPosition = new Vector3(hand.localPosition.x, colliderPerLevels[levelIndex].handTransformY, hand.localPosition.z);
            }
        }
    }
}
