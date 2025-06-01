using UnityEngine;

namespace VLCNP.Combat
{
    /**
     * @brief あたったprojectileを破壊する
     */
    [RequireComponent(typeof(AudioSource)), RequireComponent(typeof(BoxCollider2D))]
    public class Shield : MonoBehaviour
    {
        [SerializeField]
        AudioClip blockSe = null;
        private AudioSource audioSource;

        void Awake()
        {
            audioSource = GetComponent<AudioSource>();
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Projectile"))
            {
                // 新しいIProjectile実装クラスに対応
                IProjectile iProjectile = other.GetComponent<IProjectile>();
                if (iProjectile != null)
                {
                    if (iProjectile.IsStucking) return;
                    if (blockSe != null && audioSource != null)
                    {
                        audioSource.PlayOneShot(blockSe, 2.0f);
                    }
                    iProjectile.ImpactAndDestroy();
                }
            }
        }
    }
}
