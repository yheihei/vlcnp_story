using UnityEngine;
using UnityEngine.UI;
using VLCNP.Stats;

namespace VLCNP.UI
{
    public class LevelDisplay : MonoBehaviour
    {
        private Text text;
        private BaseStats baseStats;

        void Awake()
        {
            text = GetComponent<Text>();
            baseStats = GameObject.FindWithTag("Player").GetComponent<BaseStats>();
        }

        void LateUpdate()
        {
            if (baseStats.isReachedMaxLevel()) {
                text.text = "MAX";
                return;
            }
            text.text = $"{baseStats.GetLevel()}";
        }
    }
}
