using UnityEngine;

namespace VLCNP.Combat
{
    public class BounceProjectile : ProjectileBase
    {
        [Header("バウンド設定")]
        [SerializeField]
        private float bounceHeight = 3f;
        
        [SerializeField]
        private float gravity = 9.8f;
        
        [SerializeField]
        private float bounceReduction = 0.8f;

        private Vector2 velocity;
        private bool hasHitGround = false;

        protected override void Start()
        {
            base.Start();
            int directionX = isLeft ? -1 : 1;
            velocity = new Vector2(directionX * speed / 50, 0);
        }

        protected override void HandleMovement()
        {
            velocity.y -= gravity * Time.fixedDeltaTime;
            transform.Translate(velocity.x, velocity.y, 0);
        }

        protected override void HandleCollision(Collider2D other)
        {
            if (isStucking)
                return;

            if (other.CompareTag(groundTagName))
            {
                if (isStuckInGround)
                {
                    isStucking = true;
                    StartCoroutine(StuckInGround());
                    return;
                }

                if (isBreakOnGround)
                {
                    ImpactAndDestroy();
                    return;
                }

                if (velocity.y < 0)
                {
                    velocity.y = Mathf.Abs(velocity.y) * bounceReduction;
                    if (velocity.y < 0.5f)
                    {
                        velocity.y = bounceHeight;
                    }
                    hasHitGround = true;
                }
            }

            if (other.CompareTag("Wall") || other.CompareTag("Ground"))
            {
                if (Mathf.Abs(velocity.x) > 0.1f)
                {
                    Vector2 boundsOther = other.bounds.size;
                    Vector2 boundsThis = GetComponent<Collider2D>().bounds.size;
                    
                    if (boundsOther.y > boundsOther.x)
                    {
                        velocity.x = -velocity.x;
                    }
                }
            }

            if (other.gameObject.CompareTag(targetTagName))
            {
                HandleTargetCollision(other);
            }
        }
    }
}