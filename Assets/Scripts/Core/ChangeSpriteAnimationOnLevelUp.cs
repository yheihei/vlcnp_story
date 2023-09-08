using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VLCNP.Stats;

namespace VLCNP.Core
{
    public class ChangeSpriteAnimationOnLevelUp : MonoBehaviour
    {
        [SerializeField] AnimationsPerLevel[] animationsPerLevels = null;

        [System.Serializable]
        public class AnimationsPerLevel
        {
            [SerializeField]
            public AnimatorOverrideController animatorOverrideController;
            [SerializeField]
            public ColliderPerLevel colliderPerLevel;
            [SerializeField]
            public GameObject auraEffect;
        }

        [System.Serializable]
        public class ColliderPerLevel
        {
            public float capsuleCollider2DYSize;
            public float capsuleCollider2DYOffset;
            public float boxCollider2DYOffset;
            public float handTransformY;
            public float legTransformY;
        }

        Animator animator;
        GameObject aura;
        Transform groundTransform;

        private void Awake() {
            animator = GetComponent<Animator>();
            ChangeAnimation(GetComponent<BaseStats>().GetLevel());
            groundTransform = GetComponent<BoxCollider2D>() ? GetComponent<BoxCollider2D>().transform : null;
        }

        private void OnEnable() {
            GetComponent<BaseStats>().OnChangeLevel += ChangeAnimation;
        }

        private void OnDisable() {
            GetComponent<BaseStats>().OnChangeLevel -= ChangeAnimation;
        }

        public void ChangeAnimation(int level)
        {
            int levelIndex = level < animationsPerLevels.Length ? level - 1 : animationsPerLevels.Length - 1;
            animator.runtimeAnimatorController = animationsPerLevels[levelIndex].animatorOverrideController;
            ChangeCollider(levelIndex);
            UpdateAuraEffect(levelIndex);
        }

        private void ChangeCollider(int levelIndex)
        {
            CapsuleCollider2D playerCollider = GetComponent<CapsuleCollider2D>();
            BoxCollider2D playerGroundCollider = GetComponent<BoxCollider2D>();
            ColliderPerLevel colliderPerLevel = animationsPerLevels[levelIndex].colliderPerLevel;
            playerCollider.size = new Vector2(playerCollider.size.x, colliderPerLevel.capsuleCollider2DYSize);
            playerCollider.offset = new Vector2(playerCollider.offset.x, colliderPerLevel.capsuleCollider2DYOffset);
            playerGroundCollider.offset = new Vector2(playerGroundCollider.offset.x, colliderPerLevel.boxCollider2DYOffset);
            Transform hand = transform.Find("Hand");
            if (hand != null)
            {
                hand.localPosition = new Vector3(hand.localPosition.x, colliderPerLevel.handTransformY, hand.localPosition.z);
            }
            Transform leg = transform.Find("Leg");
            if (leg != null)
            {
                leg.localPosition = new Vector3(leg.localPosition.x, colliderPerLevel.legTransformY, leg.localPosition.z);
            }
        }

        private void UpdateAuraEffect(int levelIndex)
        {
            GameObject auraEffect = animationsPerLevels[levelIndex].auraEffect;
            if (auraEffect != null && aura == null)
            {
                Transform leg = transform.Find("Leg");
                aura = Instantiate(auraEffect, leg.position, Quaternion.identity, leg);
            }
            else if (auraEffect == null && aura != null)
            {
                Destroy(aura);
            }
        }
    }
}
