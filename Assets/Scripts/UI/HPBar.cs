using UnityEngine;
using UnityEngine.UI;
using VLCNP.Attributes;
using VLCNP.Stats;

namespace VLCNP.UI
{
    public class HPBar : MonoBehaviour
    {
        private Slider slider;
        private GameObject player;
        private Health playerHealth;
        BaseStats baseStats;

        void Awake()
        {
            slider = GetComponent<Slider>();
            slider.value = 1;
            SetPlayer(GameObject.FindWithTag("Player"));
        }

        void LateUpdate()
        {
            if (playerHealth == null || baseStats == null)
            {
                slider.value = 0;
                return;
            }

            float hitPoints = playerHealth.GetHealthPoints();
            slider.value = (float) hitPoints / (float) baseStats.GetStat(Stat.Health);
        }

        public void SetPlayer(GameObject newPlayer)
        {
            player = newPlayer;
            playerHealth = player != null ? player.GetComponent<Health>() : null;
            baseStats = player != null ? player.GetComponent<BaseStats>() : null;
        }
    }
}
