using UnityEngine;
using VLCNP.Attributes;
using VLCNP.Combat;

namespace VLCNP.Combat.EnemyAction
{
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(Shield))]
    public class OrbitingOnibiProjectile : MonoBehaviour
    {
        Transform orbitOwner = null;
        SpriteRenderer spriteRenderer = null;
        BoxCollider2D triggerCollider = null;
        Shield shield = null;
        Animator animator = null;

        Vector2 orbitCenterOffset = new Vector2(0f, 0.5f);
        float orbitRadius = 1.1f;
        float orbitSpeedDegreesPerSecond = 240f;
        float currentAngleDegrees = 0f;
        float selfRotationDegreesPerSecond = 180f;
        float damage = 1f;
        float moveSpeed = 6f;
        float lifetimeAfterLaunch = 4f;
        string targetTagName = "Player";
        Vector2 moveDirection = Vector2.zero;
        [SerializeField]
        float spawnFadeDuration = 0.25f;
        [SerializeField]
        float hitFadeDuration = 0.25f;
        Color targetColor = Color.white;
        bool isOrbiting = false;
        bool isLaunched = false;
        bool isLeft = false;
        bool isFadingOut = false;

        void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            triggerCollider = GetComponent<BoxCollider2D>();
            shield = GetComponent<Shield>();
            animator = GetComponent<Animator>();

            AudioSource audioSource = GetComponent<AudioSource>();
            if (audioSource != null)
            {
                audioSource.playOnAwake = false;
            }

            if (triggerCollider != null)
            {
                triggerCollider.isTrigger = true;
            }
        }

        public void InitializeOrbit(
            Transform owner,
            Sprite sprite,
            RuntimeAnimatorController animatorController,
            Color color,
            Vector3 projectileScale,
            Vector2 centerOffset,
            Vector2 colliderSize,
            float radius,
            float initialAngleDegrees,
            float orbitSpeed,
            float spinSpeed,
            float projectileDamage,
            string targetTag,
            int sortingLayerId,
            int sortingOrder
        )
        {
            orbitOwner = owner;
            orbitCenterOffset = centerOffset;
            orbitRadius = radius;
            currentAngleDegrees = initialAngleDegrees;
            orbitSpeedDegreesPerSecond = orbitSpeed;
            selfRotationDegreesPerSecond = spinSpeed;
            damage = projectileDamage;
            targetTagName = targetTag;
            targetColor = color;

            transform.SetParent(owner, false);
            transform.localScale = projectileScale;

            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = sprite;
                spriteRenderer.color = new Color(color.r, color.g, color.b, 0f);
                spriteRenderer.sortingLayerID = sortingLayerId;
                spriteRenderer.sortingOrder = sortingOrder;
            }

            if (animatorController != null)
            {
                if (animator == null)
                {
                    animator = gameObject.AddComponent<Animator>();
                }

                animator.runtimeAnimatorController = animatorController;
                animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            }

            if (triggerCollider != null)
            {
                triggerCollider.size = colliderSize;
                triggerCollider.offset = Vector2.zero;
                triggerCollider.enabled = true;
            }

            if (shield != null)
            {
                shield.enabled = true;
            }

            isOrbiting = true;
            isLaunched = false;
            moveDirection = Vector2.zero;
            UpdateOrbitPosition();
            StartCoroutine(FadeInOnSpawn());
        }

        public void LaunchTowards(Vector3 targetPosition, float speed, float lifetime)
        {
            Vector2 direction = ((Vector2)targetPosition - (Vector2)transform.position).normalized;
            if (direction.sqrMagnitude <= 0.0001f)
            {
                direction = Vector2.right;
            }

            transform.SetParent(null, true);

            moveDirection = direction;
            moveSpeed = speed;
            lifetimeAfterLaunch = lifetime;
            isLeft = moveDirection.x < 0f;
            isOrbiting = false;
            isLaunched = true;

            if (shield != null)
            {
                shield.enabled = false;
            }

            Destroy(gameObject, lifetimeAfterLaunch);
        }

        void Update()
        {
            if (isOrbiting)
            {
                currentAngleDegrees += orbitSpeedDegreesPerSecond * Time.deltaTime;
                UpdateOrbitPosition();
            }
            else if (isLaunched)
            {
                transform.position += (Vector3)(moveDirection * moveSpeed * Time.deltaTime);
            }

            if (Mathf.Abs(selfRotationDegreesPerSecond) > 0.001f)
            {
                transform.Rotate(0f, 0f, selfRotationDegreesPerSecond * Time.deltaTime);
            }
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            TryHitTarget(other);
        }

        void OnTriggerStay2D(Collider2D other)
        {
            TryHitTarget(other);
        }

        void TryHitTarget(Collider2D other)
        {
            if ((!isOrbiting && !isLaunched) || isFadingOut)
                return;

            if (!other.CompareTag(targetTagName))
                return;

            Health health = other.GetComponent<Health>();
            if (health == null)
                return;

            bool blowAwayLeft = isLaunched
                ? isLeft
                : other.transform.position.x < transform.position.x;
            health.TakeDamage(damage, blowAwayLeft);

            if (isLaunched)
            {
                StartCoroutine(FadeOutAndDestroy());
            }
        }

        void UpdateOrbitPosition()
        {
            if (orbitOwner == null)
                return;

            float angleRadians = currentAngleDegrees * Mathf.Deg2Rad;
            Vector3 offset =
                new Vector3(Mathf.Cos(angleRadians), Mathf.Sin(angleRadians), 0f) * orbitRadius;
            transform.localPosition = (Vector3)orbitCenterOffset + offset;
        }

        System.Collections.IEnumerator FadeOutAndDestroy()
        {
            isFadingOut = true;

            if (triggerCollider != null)
            {
                triggerCollider.enabled = false;
            }

            if (shield != null)
            {
                shield.enabled = false;
            }

            if (spriteRenderer == null || hitFadeDuration <= 0f)
            {
                Destroy(gameObject);
                yield break;
            }

            Color startColor = spriteRenderer.color;
            float elapsed = 0f;
            while (elapsed < hitFadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / hitFadeDuration);
                Color color = startColor;
                color.a = Mathf.Lerp(startColor.a, 0f, t);
                spriteRenderer.color = color;
                yield return null;
            }

            Destroy(gameObject);
        }

        System.Collections.IEnumerator FadeInOnSpawn()
        {
            if (spriteRenderer == null)
                yield break;

            if (spawnFadeDuration <= 0f)
            {
                spriteRenderer.color = targetColor;
                yield break;
            }

            float elapsed = 0f;
            while (elapsed < spawnFadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / spawnFadeDuration);
                Color color = targetColor;
                color.a = Mathf.Lerp(0f, targetColor.a, t);
                spriteRenderer.color = color;
                yield return null;
            }

            spriteRenderer.color = targetColor;
        }
    }
}
