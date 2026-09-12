using UnityEngine;
using UnityEngine.UI;
using VLCNP.Attributes;
using VLCNP.Stats;

namespace VLCNP.UI
{
    public class HPDisplay : MonoBehaviour
    {
        [SerializeField] private bool showMaximum;
        [SerializeField] private string prefix = "";

        private Text text;
        private GameObject player;
        private Health playerHealth;
        private BaseStats baseStats;
        private float displayedHP = float.NaN;
        private float displayedMaximumHP = float.NaN;

        void Awake()
        {
            text = GetComponent<Text>();
            SetPlayer(GameObject.FindWithTag("Player"));
        }

        void LateUpdate()
        {
            if (playerHealth == null)
            {
                SetDisplayHP(0, 0);
                return;
            }

            float maximumHP = showMaximum && baseStats != null ? baseStats.GetStat(Stat.Health) : 0;
            SetDisplayHP(playerHealth.GetHealthPoints(), maximumHP);
        }

        public void SetPlayer(GameObject newPlayer)
        {
            player = newPlayer;
            playerHealth = player != null ? player.GetComponent<Health>() : null;
            baseStats = player != null ? player.GetComponent<BaseStats>() : null;
            displayedHP = float.NaN;
            displayedMaximumHP = float.NaN;
        }

        private void SetDisplayHP(float currentHP, float maximumHP)
        {
            if (Mathf.Approximately(displayedHP, currentHP)
                && Mathf.Approximately(displayedMaximumHP, maximumHP))
                return;

            displayedHP = currentHP;
            displayedMaximumHP = maximumHP;
            text.text = showMaximum
                ? $"{prefix}{currentHP:0.##}/{maximumHP:0.##}"
                : prefix + currentHP.ToString();
        }
    }
}
