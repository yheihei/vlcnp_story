using UnityEngine;
using UnityEngine.UI;
using VLCNP.Attributes;
using VLCNP.Stats;

namespace VLCNP.UI
{
    public class HPBar : MonoBehaviour
    {
        [SerializeField] private StatusValueFeedback feedback;

        private Slider slider;
        private GameObject player;
        private Health playerHealth;
        BaseStats baseStats;
        private float previousHP;
        private bool hasPreviousHP;

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
            float maximumHP = baseStats.GetStat(Stat.Health);
            slider.value = maximumHP > 0 ? hitPoints / maximumHP : 0;
            if (hasPreviousHP && !Mathf.Approximately(previousHP, hitPoints))
                feedback?.Play(hitPoints > previousHP);
            previousHP = hitPoints;
            hasPreviousHP = true;
        }

        public void SetPlayer(GameObject newPlayer)
        {
            feedback?.Cancel();
            hasPreviousHP = false;
            player = newPlayer;
            playerHealth = player != null ? player.GetComponent<Health>() : null;
            baseStats = player != null ? player.GetComponent<BaseStats>() : null;
        }
    }
}
