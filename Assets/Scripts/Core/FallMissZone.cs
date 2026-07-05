using UnityEngine;
using VLCNP.Attributes;

namespace VLCNP.Core
{
    // ステージ下端より下に落ちたプレイヤーを即ミスにする
    [RequireComponent(typeof(BoxCollider2D))]
    public class FallMissZone : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (!collision.CompareTag("Player"))
                return;
            Health health = collision.GetComponent<Health>();
            if (health == null)
                return;
            health.Kill();
        }
    }
}
