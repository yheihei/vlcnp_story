using System.Collections.Generic;
using UnityEngine;

namespace VLCNP.Movement
{
    /**
     * 生成地点から上方向へ一定速度で移動し、終端付近で点滅して消える雲リフト。
     */
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(BoxCollider2D))]
    [RequireComponent(typeof(SpriteRenderer))]
    public class RisingCloudLift : MonoBehaviour
    {
        /**
         * スポーナーごとに指定する雲リフトの移動設定。
         */
        [System.Serializable]
        public class Settings
        {
            [Min(0.01f)] public float upwardSpeed = 1.25f;
            [Min(0.01f)] public float travelDistance = 6f;
            [Min(0f)] public float floatAmplitude = 0.08f;
            [Min(0f)] public float floatFrequency = 1.5f;
            [Range(0f, 1f)] public float blinkStartRate = 0.95f;
            [Min(0.01f)] public float blinkInterval = 0.08f;
            public bool carryRiders = true;
        }

        [SerializeField] float upwardSpeed = 1.25f;
        [SerializeField] float travelDistance = 6f;
        [SerializeField] float floatAmplitude = 0.08f;
        [SerializeField] float floatFrequency = 1.5f;
        [SerializeField, Range(0f, 1f)] float blinkStartRate = 0.95f;
        [SerializeField] float blinkInterval = 0.08f;
        [SerializeField] bool carryRiders = true;

        readonly HashSet<Rigidbody2D> riderBodies = new HashSet<Rigidbody2D>();

        Rigidbody2D body;
        SpriteRenderer spriteRenderer;
        BoxCollider2D platformCollider;
        Vector2 startPosition;
        Vector2 previousPosition;
        float elapsed;

        /**
         * スポーナーから指定された設定を、この生成済みの雲リフトに反映する。
         */
        public void ApplySettings(Settings settings)
        {
            if (settings == null)
            {
                return;
            }

            upwardSpeed = Mathf.Max(0.01f, settings.upwardSpeed);
            travelDistance = Mathf.Max(0.01f, settings.travelDistance);
            floatAmplitude = Mathf.Max(0f, settings.floatAmplitude);
            floatFrequency = Mathf.Max(0f, settings.floatFrequency);
            blinkStartRate = Mathf.Clamp01(settings.blinkStartRate);
            blinkInterval = Mathf.Max(0.01f, settings.blinkInterval);
            carryRiders = settings.carryRiders;
        }

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            platformCollider = GetComponent<BoxCollider2D>();

            body.bodyType = RigidbodyType2D.Kinematic;
            body.gravityScale = 0f;
            body.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        private void OnEnable()
        {
            elapsed = 0f;
            startPosition = body != null ? body.position : (Vector2)transform.position;
            previousPosition = startPosition;
            riderBodies.Clear();

            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = true;
            }
        }

        private void FixedUpdate()
        {
            elapsed += Time.fixedDeltaTime;

            float upwardDistance = upwardSpeed * elapsed;
            if (upwardDistance >= travelDistance)
            {
                Destroy(gameObject);
                return;
            }

            float bob = Mathf.Sin(elapsed * floatFrequency * Mathf.PI * 2f) * floatAmplitude;
            var nextPosition = startPosition + Vector2.up * (upwardDistance + bob);
            var delta = nextPosition - previousPosition;

            body.MovePosition(nextPosition);
            if (carryRiders && delta.y > 0f)
            {
                MoveRiders(delta);
            }

            previousPosition = nextPosition;
        }

        private void Update()
        {
            if (spriteRenderer == null)
            {
                return;
            }

            float progress = travelDistance <= 0f ? 1f : Mathf.Clamp01(upwardSpeed * elapsed / travelDistance);
            if (progress < blinkStartRate)
            {
                spriteRenderer.enabled = true;
                return;
            }

            int blinkStep = Mathf.FloorToInt(Time.time / Mathf.Max(0.01f, blinkInterval));
            spriteRenderer.enabled = blinkStep % 2 == 0;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            AddRiderIfOnTop(collision);
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            AddRiderIfOnTop(collision);
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            if (collision.rigidbody != null)
            {
                riderBodies.Remove(collision.rigidbody);
            }
        }

        private void AddRiderIfOnTop(Collision2D collision)
        {
            if (collision.rigidbody == null || platformCollider == null)
            {
                return;
            }

            float platformTop = platformCollider.bounds.max.y;
            float riderBottom = collision.collider.bounds.min.y;
            if (riderBottom >= platformTop - 0.12f)
            {
                riderBodies.Add(collision.rigidbody);
            }
        }

        private void MoveRiders(Vector2 delta)
        {
            riderBodies.RemoveWhere(rider => rider == null || !rider.simulated);

            foreach (var rider in riderBodies)
            {
                rider.MovePosition(rider.position + delta);
            }
        }
    }
}
