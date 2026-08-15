using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using VLCNP.Attributes;
using VLCNP.Combat;
using VLCNP.Core;

namespace VLCNP.Projectiles
{
    /**
     * くるくる自転しながら、慣性を持ってゆっくりプレイヤーを追尾するVLNarukamiの衝撃波。
     * 同一GameObjectのHealth(NarukamiShockwave: HP3)で破壊できる。
     * 地面に当たるとヒットエフェクトを残して消える。
     */
    [RequireComponent(typeof(Rigidbody2D))]
    public class NarukamiHomingShockwave : MonoBehaviour, IProjectile, IStoppable
    {
        [SerializeField]
        [Min(0f)]
        [Tooltip("発射直後の速度")]
        private float initialSpeed = 3f;

        [SerializeField]
        [Min(0f)]
        private float maxSpeed = 4.5f;

        [SerializeField]
        [Min(0f)]
        [Tooltip("プレイヤー方向への加速度。小さいほど曲がりが緩く慣性が強い")]
        private float homingAcceleration = 5f;

        [SerializeField]
        [Tooltip("見た目の自転速度[deg/s]")]
        private float selfRotationSpeed = 540f;

        [SerializeField]
        [Min(0f)]
        [Tooltip("追尾をやめるまでの時間。以降は直進する")]
        private float homingDuration = 5f;

        [SerializeField]
        [Min(0f)]
        private float maxLifetime = 8f;

        [SerializeField]
        [Min(0f)]
        [Tooltip("自然消滅時のフェードアウト時間")]
        private float fadeOutDuration = 0.5f;

        [SerializeField]
        private string targetTagName = "Player";

        [SerializeField]
        private string groundTagName = "Ground";

        [SerializeField]
        private GameObject hitEffect = null;

        [SerializeField]
        private UnityEvent<GameObject> onTargetHit = new UnityEvent<GameObject>();

        private Rigidbody2D rbody;
        private Transform playerTransform;
        private Vector2 velocity = Vector2.zero;
        private float elapsedTime = 0f;
        private bool hasImpacted = false;
        private float damage = 0;
        private RigidbodyInterpolation2D initialInterpolation;
        private bool hasRestoredInterpolation = false;

        public bool IsStopped { get; set; }
        public bool IsStucking => false;
        public UnityEvent<GameObject> OnTargetHit => onTargetHit;

        private void Awake()
        {
            rbody = GetComponent<Rigidbody2D>();
            // 生成直後は補間バッファが生成位置を知らず、原点付近に1フレーム描画されるため
            // 最初の物理ステップまで補間を無効にする
            initialInterpolation = rbody.interpolation;
            rbody.interpolation = RigidbodyInterpolation2D.None;
        }

        private void Start()
        {
            if (maxLifetime > 0f)
            {
                StartCoroutine(FadeOutAfterLifetime());
            }
        }

        private IEnumerator FadeOutAfterLifetime()
        {
            yield return new WaitForSeconds(Mathf.Max(0f, maxLifetime - fadeOutDuration));
            if (hasImpacted)
                yield break;

            // 消滅中は当たらないようにする。HealthはFixedUpdateでスプライトのalphaを
            // 毎回1に戻すため、フェードが上書きされないよう無効化する
            Collider2D hitCollider = GetComponent<Collider2D>();
            if (hitCollider != null)
                hitCollider.enabled = false;
            Health health = GetComponent<Health>();
            if (health != null)
                health.enabled = false;

            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null && fadeOutDuration > 0f)
            {
                Color color = spriteRenderer.color;
                float startAlpha = color.a;
                float elapsedTime = 0f;
                while (elapsedTime < fadeOutDuration)
                {
                    elapsedTime += Time.deltaTime;
                    color.a = Mathf.Lerp(
                        startAlpha,
                        0f,
                        Mathf.Clamp01(elapsedTime / fadeOutDuration)
                    );
                    spriteRenderer.color = color;
                    yield return null;
                }
            }

            Destroy(gameObject);
        }

        /** 発射方向を指定して初速を与える */
        public void Launch(Vector2 direction)
        {
            if (direction.sqrMagnitude < 0.0001f)
            {
                direction = Vector2.right;
            }
            velocity = direction.normalized * initialSpeed;
        }

        public void SetDirection(bool shouldFaceLeft)
        {
            // 自転するため向きの反転は不要
        }

        public void SetDamage(float projectileDamage)
        {
            damage = projectileDamage;
        }

        private void FixedUpdate()
        {
            if (!hasRestoredInterpolation)
            {
                // 物理ステップ開始後なら現在位置から補間が始まるため元の設定へ戻す
                rbody.interpolation = initialInterpolation;
                hasRestoredInterpolation = true;
            }

            if (IsStopped || hasImpacted)
                return;

            elapsedTime += Time.fixedDeltaTime;

            if (elapsedTime <= homingDuration && TryGetPlayerTransform(out Transform player))
            {
                Vector2 toPlayer = (Vector2)player.position - rbody.position;
                if (toPlayer.sqrMagnitude > 0.0001f)
                {
                    velocity += toPlayer.normalized * homingAcceleration * Time.fixedDeltaTime;
                }
            }

            velocity = Vector2.ClampMagnitude(velocity, maxSpeed);
            rbody.MovePosition(rbody.position + velocity * Time.fixedDeltaTime);

            if (Mathf.Abs(selfRotationSpeed) > 0.0001f)
            {
                transform.Rotate(0f, 0f, selfRotationSpeed * Time.fixedDeltaTime, Space.Self);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (hasImpacted || IsStopped)
                return;

            if (other.CompareTag(groundTagName))
            {
                ImpactAndDestroy();
                return;
            }

            if (other.CompareTag(targetTagName))
            {
                Health health = other.GetComponent<Health>();
                if (health != null)
                {
                    health.TakeDamage(damage, other.transform.position.x < rbody.position.x);
                    onTargetHit?.Invoke(other.gameObject);
                }
                ImpactAndDestroy();
            }
        }

        public void ImpactAndDestroy()
        {
            if (hasImpacted)
                return;

            hasImpacted = true;
            if (hitEffect != null)
            {
                GameObject effect = Instantiate(hitEffect, transform.position, Quaternion.identity);
                Destroy(effect, 1f);
            }
            Destroy(gameObject);
        }

        private bool TryGetPlayerTransform(out Transform player)
        {
            if (playerTransform == null || !playerTransform.gameObject.activeInHierarchy)
            {
                GameObject playerObject = GameObject.FindWithTag(targetTagName);
                playerTransform = playerObject != null ? playerObject.transform : null;
            }

            player = playerTransform;
            return player != null;
        }
    }
}
