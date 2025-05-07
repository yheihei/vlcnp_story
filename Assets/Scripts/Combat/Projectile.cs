using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using VLCNP.Attributes;
using VLCNP.Core;

namespace VLCNP.Combat
{
    public class Projectile : MonoBehaviour, IStoppable
    {
        [SerializeField]
        float speed = 30;

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
        float deleteTime = 0.18f;

        [SerializeField]
        string targetTagName = "Enemy";

        [SerializeField]
        bool IsPenetration = false;

        [Header("地面に刺さるかどうか")]
        [SerializeField]
        bool isStuckInGround = false;
        private bool isStucking = false;
        public bool IsStucking
        {
            get => isStucking;
        }

        [SerializeField]
        string groundTagName = "Ground";

        [Header("地面にあたったら壊れるかどうか")]
        [SerializeField]
        bool isBreakOnGround = false;
        List<GameObject> penetratedObjects = new List<GameObject>();
        float damage = 0;
        private ParticleSystem particle;

        [SerializeField]
        bool isFadeOut = false;

        private void Start()
        {
            if (deleteTime < 0)
                return;
            if (isFadeOut)
                StartCoroutine(FadeOut(deleteTime - deleteTime / 5));
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

        private IEnumerator FadeOut(float waitTime)
        {
            // waitTime後に画像の透明度を0.5sかけて0にする
            yield return new WaitForSeconds(waitTime);
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
            int directionX = isLeft ? -1 : 1;
            transform.Translate(directionX * speed / 50, 0, 0);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // 地面に刺さっている場合は何もしない
            if (isStucking)
                return;
            // 地面に触れた&地面に刺さる設定が有効の場合
            if (isStuckInGround && other.tag.Equals("Ground"))
            {
                isStucking = true;
                StartCoroutine(StuckInGround());
            }
            // 地面に触れた&地面にあたったら壊れる設定が有効の場合
            if (isBreakOnGround && other.tag.Equals(groundTagName))
            {
                ImpactAndDestroy();
            }
            if (other.gameObject.CompareTag(targetTagName))
            {
                // 既にヒットしたオブジェクトにはダメージを与えない. 二重にダメージを与えることを防ぐ
                if (penetratedObjects.Contains(other.gameObject))
                    return;
                // ヒットしたオブジェクトを記録しておく
                penetratedObjects.Add(other.gameObject);
                Health health = other.gameObject.GetComponent<Health>();
                if (health != null)
                    health.TakeDamage(damage, isLeft);
                if (!IsPenetration)
                    ImpactAndDestroy();
            }
        }

        public void ImpactAndDestroy()
        {
            if (hitEffect != null)
            {
                // 90度回転させてからちょっと横にずらしてエフェクトを生成
                Vector3 _position =
                    new(
                        transform.position.x + (isLeft ? -0.2f : 0.2f),
                        transform.position.y,
                        transform.position.z
                    );
                GameObject effect = Instantiate(
                    hitEffect,
                    _position,
                    Quaternion.Euler(0, 0, isLeft ? -90 : 90)
                );
                Destroy(effect, 1);
            }
            Destroy(gameObject);
        }

        private IEnumerator StuckInGround()
        {
            // Spriteのsorting layerをDefault、sorting orderを1に変更して地面に刺さったように見せる
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.sortingLayerName = "Default";
                spriteRenderer.sortingOrder = 8;
            }
            // 刺さるところまでの時間を待つ
            yield return new WaitForSeconds(0.03f);
            isStopped = true;
            // 10s後に削除
            Destroy(gameObject, 10);
        }
    }
}
