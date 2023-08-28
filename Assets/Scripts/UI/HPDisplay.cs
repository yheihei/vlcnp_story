using UnityEngine;
using UnityEngine.UI;
using VLCNP.Attributes;

namespace VLCNP.UI
{
    public class HPDisplay : MonoBehaviour
    {
        private Text text;
        private GameObject player;

        void Awake()
        {
            text = GetComponent<Text>();
            player = GameObject.FindWithTag("Player");
        }

        void LateUpdate()
        {
            float currentHP = player.GetComponent<Health>().GetHealthPoints();
            text.text = $"{currentHP}";
        }
    }
}
