using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using VLCNP.Attributes;
using VLCNP.Combat;
using VLCNP.Core;

namespace VLCNP.Projectiles
{
    /**
     * カマイタチの後方から前方へ直進する竜巻。
     */
    [RequireComponent(typeof(Rigidbody2D))]
    public class KamaitachiTornadoProjectile : MonoBehaviour, IProjectile, IStoppable
    {
        [SerializeField]
        [Min(0f)]
        private float speed = 8f;

        [SerializeField]
        [Min(0f)]
        private float maxLifetime = 16f;

        [SerializeField]
        [Min(0f)]
        private float spawnFadeDuration = 0.3f;

        [SerializeField]
        [Min(0f)]
        private float lifetimeFadeDuration = 0.3f;

        [SerializeField]
        [Min(0f)]
        private float endFadeDuration = 0.3f;

        [SerializeField]
        private string targetTagName = "Player";

        [SerializeField]
        private GameObject hitEffect = null;

        [SerializeField]
        [Min(0f)]
        private float hitFadeDuration = 0.3f;

        [SerializeField]
        [Min(1f)]
        private float hitScaleMultiplier = 1.35f;

        [SerializeField]
        private UnityEvent<GameObject> onTargetHit = new UnityEvent<GameObject>();

        private Rigidbody2D rbody;
        private Collider2D hitCollider;
        private SpriteRenderer spriteRenderer;
        private Color spawnTargetColor;
        private bool isLeft;
        private float damage;
        private float endX;
        private bool hasEndX;
        private bool hasImpacted;
        private bool isFadingOut;
        private Coroutine fadeInCoroutine;
        private Coroutine lifetimeCoroutine;

        public bool IsStopped { get; set; }
        public bool IsStucking => false;
        public UnityEvent<GameObject> OnTargetHit => onTargetHit;

        private void Awake()
        {
            rbody = GetComponent<Rigidbody2D>();
            hitCollider = GetComponent<Collider2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spawnTargetColor = spriteRenderer.color;
                if (spawnFadeDuration > 0f)
                {
                    Color transparentColor = spawnTargetColor;
                    transparentColor.a = 0f;
                    spriteRenderer.color = transparentColor;
                }
            }
        }

        private void Start()
        {
            if (spriteRenderer != null && spawnFadeDuration > 0f)
            {
                fadeInCoroutine = StartCoroutine(FadeIn());
            }

            if (maxLifetime > 0f)
            {
                lifetimeCoroutine = StartCoroutine(FadeOutAfterLifetime());
            }
        }

        private void FixedUpdate()
        {
            if (IsStopped)
                return;

            float directionX = isLeft ? -1f : 1f;
            Vector2 nextPosition = rbody.position
                + Vector2.right * (directionX * speed * Time.fixedDeltaTime);
            rbody.MovePosition(nextPosition);

            if (hasEndX && (isLeft ? nextPosition.x <= endX : nextPosition.x >= endX))
            {
                FadeOutAtDestination();
            }
        }

        public void SetDirection(bool shouldMoveLeft)
        {
            isLeft = shouldMoveLeft;
        }

        public void SetDamage(float projectileDamage)
        {
            damage = projectileDamage;
        }

        public void SetEndX(float destinationX)
        {
            endX = destinationX;
            hasEndX = true;
        }

        private IEnumerator FadeIn()
        {
            Color color = spriteRenderer.color;
            float elapsedTime = 0f;

            while (elapsedTime < spawnFadeDuration)
            {
                elapsedTime += Time.deltaTime;
                color.a = spawnTargetColor.a
                    * Mathf.Clamp01(elapsedTime / spawnFadeDuration);
                spriteRenderer.color = color;
                yield return null;
            }

            spriteRenderer.color = spawnTargetColor;
            fadeInCoroutine = null;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (hasImpacted || isFadingOut || !other.CompareTag(targetTagName))
                return;

            Health health = other.GetComponent<Health>();
            if (health != null)
            {
                health.TakeDamage(damage, isLeft);
                onTargetHit?.Invoke(other.gameObject);
            }

            ImpactAndDestroy();
        }

        public void ImpactAndDestroy()
        {
            if (hasImpacted || isFadingOut)
                return;

            hasImpacted = true;
            if (lifetimeCoroutine != null)
            {
                StopCoroutine(lifetimeCoroutine);
                lifetimeCoroutine = null;
            }

            IsStopped = true;
            if (hitCollider != null)
            {
                hitCollider.enabled = false;
            }

            if (rbody != null)
            {
                rbody.velocity = Vector2.zero;
                rbody.simulated = false;
            }

            if (hitEffect != null)
            {
                GameObject effect = Instantiate(hitEffect, transform.position, Quaternion.identity);
                Destroy(effect, 1f);
            }

            StartFadeOut(hitFadeDuration, hitScaleMultiplier);
        }

        private IEnumerator FadeOutAfterLifetime()
        {
            yield return new WaitForSeconds(maxLifetime);
            lifetimeCoroutine = null;
            if (hasImpacted)
                yield break;

            if (hitCollider != null)
            {
                hitCollider.enabled = false;
            }
            StartFadeOut(lifetimeFadeDuration, 1f);
        }

        private void FadeOutAtDestination()
        {
            if (isFadingOut)
                return;

            if (lifetimeCoroutine != null)
            {
                StopCoroutine(lifetimeCoroutine);
                lifetimeCoroutine = null;
            }

            IsStopped = true;
            if (hitCollider != null)
            {
                hitCollider.enabled = false;
            }

            if (rbody != null)
            {
                rbody.velocity = Vector2.zero;
                rbody.simulated = false;
            }

            StartFadeOut(endFadeDuration, 1f);
        }

        private void StartFadeOut(float duration, float scaleMultiplier)
        {
            if (isFadingOut)
                return;

            isFadingOut = true;
            if (fadeInCoroutine != null)
            {
                StopCoroutine(fadeInCoroutine);
                fadeInCoroutine = null;
            }

            if (spriteRenderer == null || duration <= 0f)
            {
                Destroy(gameObject);
                return;
            }

            StartCoroutine(FadeOutAndDestroy(duration, scaleMultiplier));
        }

        private IEnumerator FadeOutAndDestroy(float duration, float scaleMultiplier)
        {
            Color startColor = spriteRenderer.color;
            Vector3 startScale = transform.localScale;
            Vector3 endScale = startScale * scaleMultiplier;
            float elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float progress = Mathf.Clamp01(elapsedTime / duration);
                Color color = startColor;
                color.a = startColor.a * (1f - progress);
                spriteRenderer.color = color;
                transform.localScale = Vector3.Lerp(startScale, endScale, progress);
                yield return null;
            }

            Destroy(gameObject);
        }
    }
}
