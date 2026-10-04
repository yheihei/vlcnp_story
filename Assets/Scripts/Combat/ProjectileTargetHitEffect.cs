using UnityEngine;

namespace VLCNP.Combat
{
    /**
     * 弾が相手にダメージを与えたとき、弾の先端に命中の演出を出す。
     * 貫通して消えない弾でも、どこに当たったかが分かるようにする
     */
    public class ProjectileTargetHitEffect : MonoBehaviour
    {
        [SerializeField]
        GameObject effectPrefab = null;

        [SerializeField]
        [Tooltip("演出を出す位置。弾のローカル座標で、+xが進む向き(左向きは弾の左右反転に合わせて反転する)")]
        Vector2 localOffset = Vector2.zero;

        [SerializeField]
        float effectLifetime = 1f;

        IProjectile projectile;

        void Awake()
        {
            projectile = GetComponent<IProjectile>();
            projectile?.OnTargetHit.AddListener(SpawnEffect);
        }

        void OnDestroy()
        {
            projectile?.OnTargetHit.RemoveListener(SpawnEffect);
        }

        void SpawnEffect(GameObject target)
        {
            if (effectPrefab == null)
                return;
            GameObject effect = Instantiate(
                effectPrefab,
                transform.TransformPoint(localOffset),
                Quaternion.identity
            );
            Destroy(effect, effectLifetime);
        }
    }
}
