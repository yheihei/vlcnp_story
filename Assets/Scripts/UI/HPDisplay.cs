using UnityEngine;
using UnityEngine.UI;
using VLCNP.Attributes;

namespace VLCNP.UI
{
    public class HPDisplay : MonoBehaviour
    {
        private Text text;
        private GameObject player;
        private Health playerHealth;
        private float displayedHP = float.NaN;

        void Awake()
        {
            text = GetComponent<Text>();
            SetPlayer(GameObject.FindWithTag("Player"));
        }

        void LateUpdate()
        {
            if (playerHealth == null)
            {
                SetDisplayHP(0);
                return;
            }

            SetDisplayHP(playerHealth.GetHealthPoints());
        }

        public void SetPlayer(GameObject newPlayer)
        {
            player = newPlayer;
            playerHealth = player != null ? player.GetComponent<Health>() : null;
            displayedHP = float.NaN;
        }

        private void SetDisplayHP(float currentHP)
        {
            if (Mathf.Approximately(displayedHP, currentHP))
                return;

            displayedHP = currentHP;
            text.text = currentHP.ToString();
        }
    }
}
