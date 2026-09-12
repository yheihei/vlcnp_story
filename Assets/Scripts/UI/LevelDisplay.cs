using UnityEngine;
using UnityEngine.UI;
using VLCNP.Stats;

namespace VLCNP.UI
{
    public class LevelDisplay : MonoBehaviour
    {
        [SerializeField] private StatusValueFeedback feedback;

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

            int level = baseStats.GetLevel();
            bool isMax = baseStats.isReachedMaxLevel();
            if (displayedLevel == level && displayedMax == isMax)
                return;

            if (displayedLevel != int.MinValue && level > displayedLevel)
                feedback?.Play(true);

            text.text = isMax ? "MAX" : level.ToString();
            displayedLevel = level;
            displayedMax = isMax;
        }

        public void SetBaseStats(BaseStats newBaseStats)
        {
            feedback?.Cancel();
            baseStats = newBaseStats;
            displayedLevel = int.MinValue;
            displayedMax = false;
        }
    }
}
