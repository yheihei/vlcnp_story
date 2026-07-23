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

        [Header("弾が消えるまでの飛距離")]
        [SerializeField]
        float bulletMaxDistance = 8f;

        [Header("プレイヤーが見つからないときの発射方向")]
        [SerializeField]
        bool isLeft = true;

        [Header("プレイヤー位置を見ず、下方向へ固定して発射する")]
        [SerializeField]
        bool isDownward = false;

        private Transform playerTransform;
        private Coroutine fireLoopCoroutine;

        private void OnEnable()
        {
            fireLoopCoroutine = StartCoroutine(FireLoop());
        }

        private void OnDisable()
        {
            if (fireLoopCoroutine == null)
                return;

            StopCoroutine(fireLoopCoroutine);
            fireLoopCoroutine = null;
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
            // 横向きの場合は、発射のたび(fireInterval ごと)にプレイヤーのいる側へ向きを変える
            if (!isDownward && TryGetPlayerTransform(out Transform player))
            {
                isLeft = player.position.x < transform.position.x;
            }
            UpdateFacing();
            Transform muzzle = muzzleTransform != null ? muzzleTransform : transform;
            Projectile projectile = Instantiate(
                projectilePrefab,
                muzzle.position,
                isDownward ? Quaternion.Euler(0f, 0f, 90f) : Quaternion.identity
            );
            // Projectile はローカルX方向へ進む。90度回転 + 左向きでワールド下方向になる。
            projectile.SetDirection(isDownward || isLeft);
            projectile.SetDamage(damage);
            if (
                projectile.TryGetComponent(
                    out DestroyAfterMovedDistance destroyAfterMovedDistance
                )
            )
            {
                destroyAfterMovedDistance.MaxDistance = bulletMaxDistance;
            }
            if (fireEffect != null)
            {
                GameObject effect = Instantiate(fireEffect, muzzle.position, Quaternion.identity);
                Destroy(effect, 1f);
            }
        }

        private void UpdateFacing()
        {
            if (isDownward)
                return;

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
