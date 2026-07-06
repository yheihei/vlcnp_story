using System.Collections;
using UnityEngine;

namespace VLCNP.Combat
{
    /** 窓などに設置し、一定間隔で弾を発射し続ける銃 */
    public class WindowGun : MonoBehaviour
    {
        [SerializeField]
        Projectile projectilePrefab = null;

        [SerializeField]
        GameObject fireEffect = null;

        [SerializeField]
        Transform muzzleTransform = null;

        [SerializeField]
        float fireInterval = 3f;

        [Header("最初の発射までの待ち時間。複数設置時にずらす用")]
        [SerializeField]
        float firstDelay = 0f;

        [SerializeField]
        float damage = 1f;

        [SerializeField]
        bool isLeft = true;

        private void Start()
        {
            StartCoroutine(FireLoop());
        }

        private IEnumerator FireLoop()
        {
            yield return new WaitForSeconds(firstDelay);
            while (true)
            {
                Fire();
                yield return new WaitForSeconds(fireInterval);
            }
        }

        private void Fire()
        {
            if (projectilePrefab == null)
                return;
            Transform muzzle = muzzleTransform != null ? muzzleTransform : transform;
            Projectile projectile = Instantiate(
                projectilePrefab,
                muzzle.position,
                Quaternion.identity
            );
            projectile.SetDirection(isLeft);
            projectile.SetDamage(damage);
            if (fireEffect != null)
            {
                GameObject effect = Instantiate(fireEffect, muzzle.position, Quaternion.identity);
                Destroy(effect, 1f);
            }
        }
    }
}
