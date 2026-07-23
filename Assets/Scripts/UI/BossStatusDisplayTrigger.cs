using UnityEngine;

namespace VLCNP.UI
{
    /**
     * プレイヤーが接触したときに BossStatus を表示する
     */
    [RequireComponent(typeof(BoxCollider2D))]
    public class BossStatusDisplayTrigger : MonoBehaviour
    {
        private int playerLayer;

        private void Awake()
        {
            playerLayer = LayerMask.NameToLayer("Player");
            GetComponent<BoxCollider2D>().isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.layer != playerLayer)
                return;

            if (BossStatusVisibility.SetVisible(true))
            {
                gameObject.SetActive(false);
                return;
            }

            Debug.LogWarning(
                $"[{nameof(BossStatusDisplayTrigger)}] BossStatus が見つからないため、表示できません。",
                this
            );
        }
    }
}
