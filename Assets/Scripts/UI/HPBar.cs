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
        BaseStats baseStats;

        void Awake()
        {
            slider = GetComponent<Slider>();
            slider.value = 1;
            player = GameObject.FindWithTag("Player");
            baseStats = player.GetComponent<BaseStats>();
        }

        void LateUpdate()
        {
            float hitPoints = player.GetComponent<Health>().GetHealthPoints();
            slider.value = (float) hitPoints / (float) baseStats.GetStat(Stat.Health);
        }
    }
}
