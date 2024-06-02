using UnityEngine;
using UnityEngine.UI;
using VLCNP.Attributes;
using VLCNP.Stats;

namespace VLCNP.UI
{
    public class BossHPBar : MonoBehaviour
    {
        private Slider slider;
        [SerializeField]
        private GameObject targetCharacter;
        BaseStats baseStats;

        void Awake()
        {
            slider = GetComponent<Slider>();
            slider.value = 1;
            baseStats = targetCharacter.GetComponent<BaseStats>();
        }

        void LateUpdate()
        {
            if (targetCharacter == null)
            {
                slider.value = 0;
                return;
            }
            float hitPoints = targetCharacter.GetComponent<Health>().GetHealthPoints();
            slider.value = (float) hitPoints / (float) baseStats.GetStat(Stat.Health);
        }
    }
}
