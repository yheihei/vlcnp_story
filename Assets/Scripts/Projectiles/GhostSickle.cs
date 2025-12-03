using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using VLCNP.Attributes;
using VLCNP.Core;

namespace VLCNP.Combat
{
    public class GhostSickle : MonoBehaviour, IStoppable, IProjectile
    {
        [SerializeField]
        float speed = 180f; // 角速度[deg/s]

        [SerializeField]
        float radiusGrowthPerSecond = 1.5f;

        [SerializeField]
        float initialRadius = 0.3f;

        [SerializeField]
        float centerOffsetX = 0f;

        [SerializeField]
        float selfRotationSpeed = 360f; // 自身の自転角速度[deg/s]

        [SerializeField]
        GameObject hitEffect = null;
        bool isLeft = false;
        public bool IsLeft
        {
            get => isLeft;
            set => isLeft = value;
        }
        bool isStopped = false;
        public bool IsStopped
        {
            get => isStopped;
            set => isStopped = value;
        }

        [SerializeField]
        float deleteTime = 3f;

        [SerializeField]
        string targetTagName = "Player";

        private bool isStucking = false;
        public bool IsStucking
        {
            get => isStucking;
        }

        List<GameObject> penetratedObjects = new List<GameObject>();
        float damage = 0;
        private ParticleSystem particle;

        private float currentAngleRad = 0f;
        private float currentRadius = 0f;
        private Vector2 currentCenter = Vector2.zero;
        private Vector2 spawnPosition = Vector2.zero;

        [SerializeField]
        bool isFadeOut = true;

        [SerializeField]
        private UnityEvent<GameObject> onTargetHit = new UnityEvent<GameObject>();
        public UnityEvent<GameObject> OnTargetHit => onTargetHit;

        private void Start()
        {
            spawnPosition = transform.position;
            InitializeOrbit();

            if (deleteTime < 0)
                return;
            if (isFadeOut)
                StartCoroutine(FadeOut(deleteTime));
            Destroy(gameObject, deleteTime);
        }

        public void SetDirection(bool isLeft)
        {
            this.isLeft = isLeft;
            if (isLeft)
            {
                // x方向のscaleを反転させる
                transform.localScale = new Vector3(
                    -transform.localScale.x,
                    transform.localScale.y,
                    transform.localScale.z
                );
            }
        }

        public void SetDamage(float damage)
        {
            this.damage = damage;
        }

        private void InitializeOrbit()
        {
            currentCenter = spawnPosition + new Vector2(centerOffsetX, 0f);

            Vector2 toProjectile = (Vector2)transform.position - currentCenter;
            float magnitude = toProjectile.magnitude;
            bool hasInitialOffset = magnitude > 0.0001f;

            // 初期位置が中心と同じなら、向きに応じて少しずらして開始する
            if (!hasInitialOffset)
            {
                currentAngleRad = isLeft ? Mathf.PI : 0f;
                currentRadius = Mathf.Max(initialRadius, 0.01f);
            }
            else
            {
                currentAngleRad = Mathf.Atan2(toProjectile.y, toProjectile.x);
                currentRadius = Mathf.Max(magnitude, initialRadius);
            }
        }

        private IEnumerator FadeOut(float waitTime)
        {
            // waitTime後に画像の透明度を0.5sかけて0にする
            yield return new WaitForSeconds(waitTime - 0.5f);
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                Color color = spriteRenderer.color;
                float alpha = color.a;
                while (alpha > 0)
                {
                    alpha -= Time.deltaTime / 0.5f;
                    color.a = alpha;
                    spriteRenderer.color = color;
                    yield return null;
                }
            }
        }

        private void FixedUpdate()
        {
            if (isStopped)
                return;

            currentAngleRad += speed * Mathf.Deg2Rad * Time.fixedDeltaTime;
            currentRadius += radiusGrowthPerSecond * Time.fixedDeltaTime;

            Vector2 offset =
                new Vector2(Mathf.Cos(currentAngleRad), Mathf.Sin(currentAngleRad)) * currentRadius;

            transform.position = currentCenter + offset;

            if (Mathf.Abs(selfRotationSpeed) > 0.0001f)
            {
                float deltaAngle = selfRotationSpeed * Time.fixedDeltaTime;
                transform.Rotate(0f, 0f, deltaAngle, Space.Self);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.CompareTag(targetTagName))
            {
                // 既にヒットしたオブジェクトにはダメージを与えない. 二重にダメージを与えることを防ぐ
                if (penetratedObjects.Contains(other.gameObject))
                    return;
                // ヒットしたオブジェクトを記録しておく
                penetratedObjects.Add(other.gameObject);
                Health health = other.gameObject.GetComponent<Health>();
                if (health != null)
                {
                    health.TakeDamage(damage, isLeft);
                    // ダメージを与えた後にイベントを発火
                    onTargetHit?.Invoke(other.gameObject);
                }
            }
        }

        public void ImpactAndDestroy()
        {
            // 壊れない
        }
    }
}
