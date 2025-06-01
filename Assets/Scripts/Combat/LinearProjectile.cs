using UnityEngine;

namespace VLCNP.Combat
{
    public class LinearProjectile : ProjectileBase
    {
        protected override void HandleMovement()
        {
            int directionX = isLeft ? -1 : 1;
            transform.Translate(directionX * speed / 50, 0, 0);
        }

        protected override void HandleCollision(Collider2D other)
        {
            if (isStucking)
                return;

            HandleGroundCollision(other);

            if (other.gameObject.CompareTag(targetTagName))
            {
                HandleTargetCollision(other);
            }
        }
    }
}