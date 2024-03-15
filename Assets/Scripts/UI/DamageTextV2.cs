using System.Collections;
using System.Collections.Generic;
using Fungus;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VLCNP.UI
{
    public class DamageTextV2 : MonoBehaviour
    {
        [SerializeField] Text damageText;
        [SerializeField] float showTimeDuration = 2.5f;
        float showTime = 0f;

        private void Awake()
        {
            ResetDamageText();
        }

        public void AddDamageText(float damagePoint)
        {
            showTime = 0f;
            // Text表示
            damageText.color = new Color(damageText.color.r, damageText.color.g, damageText.color.b, 1f);
            // ダメージを合算
            float currentDamageAmount = float.Parse(damageText.text) + damagePoint;
            damageText.text = currentDamageAmount.ToString();
        }

        void ResetDamageText()
        {
            // Textを透明にする
            damageText.color = new Color(damageText.color.r, damageText.color.g, damageText.color.b, 0f);
            damageText.text = "0";
        }

        void FixedUpdate()
        {
            showTime += Time.deltaTime;
            if (showTime > showTimeDuration)
            {
                ResetDamageText();
            }
        }
    }
}
