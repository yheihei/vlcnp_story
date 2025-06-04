using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using VLCNP.Attributes;
using VLCNP.Core;
using VLCNP.Movement;

namespace VLCNP.Combat
{
    public class BouncingProjectile : MonoBehaviour, IStoppable, IProjectile
    {
        [SerializeField]
        float speed = 30;

        [SerializeField]
        float gravityScale = 2.0f;

        [SerializeField]
        float maxBounceHeight = 0.8f;

        [SerializeField]
        int maxBounceCount = 18;

        [SerializeField]
        GameObject hitEffect = null;

        [SerializeField]
        GameObject destroyEffect = null;

        [SerializeField]
        string targetTagName = "Enemy";

        [SerializeField]
        string groundTagName = "Ground";

        [SerializeField]
        float deleteTime = 10f;

        [SerializeField]
        private FrontCollisionDetector frontCollisionDetector = null;

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

        // IProjectile インターフェースの実装（このクラスでは使用しない）
        public bool IsStucking => false;

        private int bounceCount = 0;
        private float damage = 0;
        private Rigidbody2D rb;
        private List<GameObject> penetratedObjects = new List<GameObject>();
        private bool hasReversedDirection = false;
        
        [SerializeField]
        private UnityEvent<GameObject> onTargetHit = new UnityEvent<GameObject>();
        public UnityEvent<GameObject> OnTargetHit => onTargetHit;

        private void Start()
        {
            rb = GetComponent<Rigidbody2D>();
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody2D>();
            }

            // 重力設定
            rb.gravityScale = gravityScale;

            // 初期速度を設定
            int directionX = isLeft ? -1 : 1;
            rb.velocity = new Vector2(directionX * speed, 0);

            // 自動削除
            if (deleteTime > 0)
            {
                Destroy(gameObject, deleteTime);
            }
        }

        public void SetDirection(bool isLeft)
        {
            this.isLeft = isLeft;
            UpdateDirection();

            // 既にRigidbody2Dが存在する場合は速度を更新
            if (rb != null)
            {
                int directionX = isLeft ? -1 : 1;
                rb.velocity = new Vector2(directionX * speed, rb.velocity.y);
            }
        }

        public void SetDamage(float damage)
        {
            this.damage = damage;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (isStopped)
                return;

            // 地面との衝突でバウンド
            if (collision.gameObject.CompareTag(groundTagName))
            {
                if (bounceCount >= maxBounceCount)
                {
                    ImpactAndDestroy();
                    return;
                }

                bounceCount++;

                // バウンド後の速度を制限するコルーチンを開始
                StartCoroutine(LimitBounceHeightByVelocity());
            }
        }

        private IEnumerator LimitBounceHeightByVelocity()
        {
            // 物理エンジンのバウンド処理を1フレーム待つ
            yield return new WaitForFixedUpdate();

            // バウンド後の初速度から、maxBounceHeightに到達する速度を計算
            // v^2 = u^2 + 2as より、v=0（最高点）、a=-g、s=maxBounceHeight として
            // 0 = u^2 - 2gs → u = sqrt(2gs)
            float maxVelocityY = Mathf.Sqrt(
                2 * Physics2D.gravity.magnitude * gravityScale * maxBounceHeight
            );

            Vector2 velocity = rb.velocity;
            if (velocity.y > maxVelocityY)
            {
                velocity.y = maxVelocityY;
                rb.velocity = velocity;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (isStopped)
                return;

            if (other.gameObject.CompareTag(targetTagName))
            {
                // 既にヒットしたオブジェクトにはダメージを与えない
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

                // バウンドする弾は貫通しない
                ImpactAndDestroy();
            }
        }

        public void ImpactAndDestroy()
        {
            if (hitEffect != null)
            {
                // エフェクトを生成
                Vector3 effectPosition =
                    new(
                        transform.position.x + (isLeft ? -0.2f : 0.2f),
                        transform.position.y,
                        transform.position.z
                    );
                GameObject effect = Instantiate(
                    hitEffect,
                    effectPosition,
                    Quaternion.Euler(0, 0, isLeft ? -90 : 90)
                );
                Destroy(effect, 1);
            }

            // 消滅エフェクトを生成
            if (destroyEffect != null)
            {
                GameObject effect = Instantiate(
                    destroyEffect,
                    transform.position,
                    Quaternion.identity
                );
                Destroy(effect, 2f);
            }

            Destroy(gameObject);
        }

        private void FixedUpdate()
        {
            if (isStopped)
                return;

            // 壁に衝突した場合はX軸方向を反転（連続反転を防ぐ）
            if (
                frontCollisionDetector != null
                && frontCollisionDetector.IsColliding
                && !hasReversedDirection
            )
            {
                isLeft = !isLeft;
                SetDirection(isLeft);
                hasReversedDirection = true;
            }
            else if (frontCollisionDetector != null && !frontCollisionDetector.IsColliding)
            {
                hasReversedDirection = false;
            }

            // 水平速度を維持（重力による影響を受けないように）
            Vector2 velocity = rb.velocity;
            int directionX = isLeft ? -1 : 1;
            velocity.x = directionX * speed;
            rb.velocity = velocity;
        }

        private void UpdateDirection()
        {
            if (isLeft)
            {
                // x方向のscaleを反転させる
                transform.localScale = new Vector3(
                    -Mathf.Abs(transform.localScale.x),
                    transform.localScale.y,
                    transform.localScale.z
                );
            }
            else
            {
                transform.localScale = new Vector3(
                    Mathf.Abs(transform.localScale.x),
                    transform.localScale.y,
                    transform.localScale.z
                );
            }
        }
    }
}
