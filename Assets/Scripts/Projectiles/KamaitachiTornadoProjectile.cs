using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using VLCNP.Attributes;
using VLCNP.Combat;
using VLCNP.Core;

namespace VLCNP.Projectiles
{
    /**
     * 画面端から反対側へ直進するカマイタチの竜巻。
     */
    [RequireComponent(typeof(Rigidbody2D))]
    public class KamaitachiTornadoProjectile : MonoBehaviour, IProjectile, IStoppable
    {
        [SerializeField]
        [Min(0f)]
        private float speed = 8f;

        [SerializeField]
        [Min(0f)]
        private float maxLifetime = 8f;

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
        private bool isLeft;
        private float damage;
        private float endX;
        private bool hasEndX;
        private bool hasImpacted;

        public bool IsStopped { get; set; }
        public bool IsStucking => false;
        public UnityEvent<GameObject> OnTargetHit => onTargetHit;

        private void Awake()
        {
            rbody = GetComponent<Rigidbody2D>();
            hitCollider = GetComponent<Collider2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void Start()
        {
            if (maxLifetime > 0f)
            {
                Destroy(gameObject, maxLifetime);
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
                Destroy(gameObject);
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

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag(targetTagName))
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
            if (hasImpacted)
                return;

            hasImpacted = true;
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

            if (spriteRenderer == null || hitFadeDuration <= 0f)
            {
                Destroy(gameObject);
                return;
            }

            StartCoroutine(FadeOutAndDestroy());
        }

        private IEnumerator FadeOutAndDestroy()
        {
            Color startColor = spriteRenderer.color;
            Vector3 startScale = transform.localScale;
            Vector3 endScale = startScale * hitScaleMultiplier;
            float elapsedTime = 0f;

            while (elapsedTime < hitFadeDuration)
            {
                elapsedTime += Time.deltaTime;
                float progress = Mathf.Clamp01(elapsedTime / hitFadeDuration);
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
