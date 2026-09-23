using System.Collections;
using UnityEngine;
using VLCNP.Attributes;
using VLCNP.Core;
using VLCNP.Movement;

namespace VLCNP.Combat
{
    /**
     * 天井からしたたり落ちる水滴。天井で膨らんでから落下し、プレイヤーに当たるとダメージを与える。
     * 地面や水面に触れると弾けて消える。弾けたしぶきにダメージはない。
     * スプライトの pivot は上端中央にする(天井から垂れ下がるように膨らむ)
     */
    [RequireComponent(typeof(Rigidbody2D))]
    public class FallingDrop : MonoBehaviour, IStoppable
    {
        [SerializeField]
        string targetTagName = "Player";

        [SerializeField]
        string groundTagName = "Ground";

        [Header("膨らみはじめの大きさ(最終の大きさに対する倍率)")]
        [SerializeField]
        Vector2 formStartScale = new Vector2(0.4f, 0.15f);

        [Header("弾けたときのしぶき")]
        [SerializeField]
        GameObject splashEffect = null;

        [SerializeField]
        float splashEffectLifetime = 0.3f;

        [Header("何にも当たらなかったときに消えるまでの時間")]
        [SerializeField]
        float maxFallTime = 6f;

        Rigidbody2D rbody;
        Collider2D dropCollider;
        Vector3 baseScale;
        float damage = 0f;
        bool isFalling = false;
        bool isSplashed = false;
        Vector2 velocityBeforeStop = Vector2.zero;

        bool isStopped = false;
        public bool IsStopped
        {
            get => isStopped;
            set
            {
                if (isStopped == value)
                    return;
                isStopped = value;
                if (rbody == null)
                    return;
                if (isStopped)
                {
                    velocityBeforeStop = rbody.linearVelocity;
                    rbody.simulated = false;
                }
                else if (isFalling)
                {
                    rbody.simulated = true;
                    rbody.linearVelocity = velocityBeforeStop;
                }
            }
        }

        private void Awake()
        {
            rbody = GetComponent<Rigidbody2D>();
            dropCollider = GetComponent<Collider2D>();
            baseScale = transform.localScale;
            // 膨らんでいる間は天井に留める
            rbody.simulated = false;
        }

        /** formDuration 秒かけて天井で膨らませてから落とす */
        public void Drip(float damage, float formDuration)
        {
            this.damage = damage;
            StartCoroutine(FormAndFall(formDuration));
        }

        private IEnumerator FormAndFall(float formDuration)
        {
            float elapsed = 0f;
            while (elapsed < formDuration)
            {
                if (!isStopped)
                    elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / formDuration);
                // 溢れる直前に少し縦へ伸びて震える
                float wobble = t > 0.7f ? Mathf.Sin(elapsed * 40f) * 0.06f * (t - 0.7f) / 0.3f : 0f;
                transform.localScale = new Vector3(
                    baseScale.x * Mathf.Lerp(formStartScale.x, 1f, t) * (1f - wobble),
                    baseScale.y * Mathf.Lerp(formStartScale.y, 1f, t * t) * (1f + wobble),
                    baseScale.z
                );
                yield return null;
            }
            transform.localScale = baseScale;
            isFalling = true;
            if (!isStopped)
                rbody.simulated = true;

            float fallTime = 0f;
            while (fallTime < maxFallTime)
            {
                if (!isStopped)
                    fallTime += Time.deltaTime;
                yield return null;
            }
            Destroy(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!isFalling || isSplashed)
                return;
            if (other.CompareTag(targetTagName))
            {
                Health health = other.GetComponent<Health>();
                if (health != null)
                    health.TakeDamage(damage, other.transform.position.x < transform.position.x);
                Splash(GetBottomY());
                return;
            }
            if (other.TryGetComponent(out Water _))
            {
                // 水面で弾ける
                Splash(other.bounds.max.y);
                return;
            }
            if (other.CompareTag(groundTagName))
            {
                // 落下が速いと地面にめり込んでから検知されるため、上から見た地面の表面で弾けさせる
                Vector2 above = new Vector2(transform.position.x, transform.position.y + 0.5f);
                Splash(other.ClosestPoint(above).y);
            }
        }

        private float GetBottomY()
        {
            return dropCollider != null ? dropCollider.bounds.min.y : transform.position.y;
        }

        /** しぶきの pivot は下端中央。y に接地面の高さを渡す */
        private void Splash(float y)
        {
            isSplashed = true;
            if (splashEffect != null)
            {
                GameObject effect = Instantiate(
                    splashEffect,
                    new Vector3(transform.position.x, y, transform.position.z),
                    Quaternion.identity
                );
                Destroy(effect, splashEffectLifetime);
            }
            Destroy(gameObject);
        }
    }
}
