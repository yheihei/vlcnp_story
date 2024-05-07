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
        [SerializeField] float speed = 30;
        [SerializeField] GameObject hitEffect = null;
        bool isLeft = false;
        public bool IsLeft { get => isLeft; set => isLeft = value; }
        bool isStopped = false;
        public bool IsStopped { get => isStopped; set => isStopped = value; }

        [SerializeField] float deleteTime = 0.18f;
        [SerializeField] string targetTagName = "Enemy";
        [SerializeField] bool IsPenetration = false;
        [Header("地面に刺さるかどうか")]
        [SerializeField] bool isStuckInGround = false;
        private bool isStucking = false;
        List<GameObject> penetratedObjects = new List<GameObject>();
        float damage = 0;
        private ParticleSystem particle;

        private void Start()
        {
            Destroy(gameObject, deleteTime);
        }

        public void SetDirection(bool isLeft)
        {
            this.isLeft = isLeft;
            if (isLeft)
            {
                // x方向のscaleを反転させる
                transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
            }
        }

        public void SetDamage(float damage)
        {
            this.damage = damage;
        }

        private void FixedUpdate()
        {
            if (isStopped) return;
            int directionX = isLeft ? -1 : 1;
            transform.Translate(directionX * speed / 50, 0, 0);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // 地面に刺さっている場合は何もしない
            if (isStucking) return;
            // 地面に触れた&地面に刺さる設定が有効の場合
            if (isStuckInGround && other.tag.Equals("Ground"))
            {
                isStucking = true;
                StartCoroutine(StuckInGround());
            }
            if (other.gameObject.CompareTag(targetTagName))
            {
                // 既にヒットしたオブジェクトにはダメージを与えない. 二重にダメージを与えることを防ぐ
                if (penetratedObjects.Contains(other.gameObject)) return;
                // ヒットしたオブジェクトを記録しておく
                penetratedObjects.Add(other.gameObject);
                Health health = other.gameObject.GetComponent<Health>();
                if (health != null) health.TakeDamage(damage);
                if (!IsPenetration) Destroy(gameObject);
            }
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
