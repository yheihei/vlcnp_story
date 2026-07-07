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

        [Header("プレイヤーが見つからないときの発射方向")]
        [SerializeField]
        bool isLeft = true;

        private Transform playerTransform;

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
            // 発射のたび(fireInterval ごと)にプレイヤーのいる側へ向きを変える
            if (TryGetPlayerTransform(out Transform player))
            {
                isLeft = player.position.x < transform.position.x;
            }
            UpdateFacing();
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

        private void UpdateFacing()
        {
            // スプライトは左向きが基準。右向きは localScale.x を反転して表現する
            Vector3 scale = transform.localScale;
            float scaleX = Mathf.Abs(scale.x);
            transform.localScale = new Vector3(isLeft ? scaleX : -scaleX, scale.y, scale.z);
        }

        private bool TryGetPlayerTransform(out Transform player)
        {
            if (
                playerTransform == null
                || !playerTransform.gameObject.activeInHierarchy
                || !playerTransform.CompareTag("Player")
            )
            {
                GameObject playerObject = GameObject.FindWithTag("Player");
                playerTransform = playerObject != null ? playerObject.transform : null;
            }
            player = playerTransform;
            return player != null;
        }
    }
}
