using UnityEngine;
using UnityEngine.UI;
using VLCNP.Stats;

namespace VLCNP.UI
{
    public class LevelDisplay : MonoBehaviour
    {
        private Text text;
        private BaseStats baseStats;
        private int displayedLevel = int.MinValue;
        private bool displayedMax;

        void Awake()
        {
            text = GetComponent<Text>();
            GameObject player = GameObject.FindWithTag("Player");
            baseStats = player != null ? player.GetComponent<BaseStats>() : null;
        }

        void LateUpdate()
        {
            if (baseStats == null)
                return;

            if (baseStats.isReachedMaxLevel()) {
                if (!displayedMax)
                {
                    text.text = "MAX";
                    displayedMax = true;
                }
                return;
            }

            int level = baseStats.GetLevel();
            if (displayedMax || displayedLevel != level)
            {
                text.text = level.ToString();
                displayedLevel = level;
                displayedMax = false;
            }
        }

        public void SetBaseStats(BaseStats newBaseStats)
        {
            baseStats = newBaseStats;
            displayedLevel = int.MinValue;
            displayedMax = false;
        }
    }
}
