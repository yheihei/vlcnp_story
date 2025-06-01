using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VLCNP.Attributes;
using VLCNP.Core;

namespace VLCNP.Combat
{
    public abstract class ProjectileBase : MonoBehaviour, IStoppable
    {
        [SerializeField]
        protected float speed = 30f;

        [SerializeField]
        protected GameObject hitEffect = null;

        [SerializeField]
        protected float deleteTime = 0.18f;

        [SerializeField]
        protected string targetTagName = "Enemy";

        [SerializeField]
        protected string groundTagName = "Ground";

        [SerializeField]
        protected bool IsPenetration = false;

        [Header("地面に刺さるかどうか")]
        [SerializeField]
        protected bool isStuckInGround = false;

        [Header("地面にあたったら壊れるかどうか")]
        [SerializeField]
        protected bool isBreakOnGround = false;

        [SerializeField]
        protected bool isFadeOut = false;

        protected bool isLeft = false;
        protected bool isStopped = false;
        protected bool isStucking = false;
        protected float damage = 0;
        protected List<GameObject> penetratedObjects = new List<GameObject>();

        public bool IsLeft
        {
            get => isLeft;
            set => isLeft = value;
        }

        public bool IsStopped
        {
            get => isStopped;
            set => isStopped = value;
        }

        public bool IsStucking
        {
            get => isStucking;
        }

        protected virtual void Start()
        {
            if (deleteTime < 0)
                return;
            if (isFadeOut)
                StartCoroutine(FadeOut(deleteTime - deleteTime / 5));
            Destroy(gameObject, deleteTime);
        }

        protected virtual void FixedUpdate()
        {
            if (!isStopped && !isStucking)
                HandleMovement();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            HandleCollision(other);
        }

        protected abstract void HandleMovement();
        protected abstract void HandleCollision(Collider2D other);

        public virtual void SetDirection(bool isLeft)
        {
            this.isLeft = isLeft;
            if (isLeft)
            {
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

        public virtual void SetDamage(float damage)
        {
            this.damage = damage;
        }

        public virtual void ImpactAndDestroy()
        {
            if (hitEffect != null)
            {
                Vector3 _position = new(
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

        protected virtual IEnumerator FadeOut(float waitTime)
        {
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

        protected virtual IEnumerator StuckInGround()
        {
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.sortingLayerName = "Default";
                spriteRenderer.sortingOrder = 8;
            }
            yield return new WaitForSeconds(0.03f);
            isStopped = true;
            Destroy(gameObject, 10);
        }

        protected virtual void HandleTargetCollision(Collider2D other)
        {
            if (penetratedObjects.Contains(other.gameObject))
                return;

            penetratedObjects.Add(other.gameObject);
            Health health = other.gameObject.GetComponent<Health>();
            if (health != null)
                health.TakeDamage(damage, isLeft);

            if (!IsPenetration)
                ImpactAndDestroy();
        }

        protected virtual void HandleGroundCollision(Collider2D other)
        {
            if (isStuckInGround && other.CompareTag("Ground"))
            {
                isStucking = true;
                StartCoroutine(StuckInGround());
            }

            if (isBreakOnGround && other.CompareTag(groundTagName))
            {
                ImpactAndDestroy();
            }
        }
    }
}