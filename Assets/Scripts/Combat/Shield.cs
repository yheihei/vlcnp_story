using UnityEngine;

namespace VLCNP.Combat
{
    /**
     * @brief あたったprojectileを破壊する
     */
    [RequireComponent(typeof(AudioSource)), RequireComponent(typeof(BoxCollider2D))]
    public class Shield : MonoBehaviour
    {
        [SerializeField] AudioClip blockSe = null;
        private AudioSource audioSource;

        void Awake()
        {
            audioSource = GetComponent<AudioSource>();
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Projectile"))
            {
                Projectile projectile = other.GetComponent<Projectile>();
                if (ShouldIgnoreProjectile(projectile)) return;
                if (blockSe != null && audioSource != null)
                {
                    audioSource.PlayOneShot(blockSe, 2.0f);
                }
                projectile.ImpactAndDestory();
            }
        }

        private bool ShouldIgnoreProjectile(Projectile projectile)
        {
            return projectile != null && projectile.IsStucking;
        }
    }
}
