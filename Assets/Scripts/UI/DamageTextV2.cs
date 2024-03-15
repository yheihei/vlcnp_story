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
        // ダメージを受けた対象のキャラクターを入れる
        [SerializeField] Transform damagedCharacter;
        [SerializeField] Text damageText;
        [SerializeField] float showTimeDuration = 2.5f;
        float damageAmount = 0f;
        float showTime = 0f;

        private void Awake()
        {
            ResetDamageText();
        }

        private bool IsCharacterDirectionLeft()
        {
            return damagedCharacter.localScale.x > 0;
        }

        public void AddDamageText(float damagePoint)
        {
            print($"AddDamageText: {damagePoint}");
            showTime = 0f;
            // Text表示
            damageText.color = new Color(damageText.color.r, damageText.color.g, damageText.color.b, 1f);
            // ダメージを合算
            damageAmount += damagePoint;
            damageText.text = damageAmount.ToString();
        }

        void ResetDamageText()
        {
            // Textを透明にする
            damageText.color = new Color(damageText.color.r, damageText.color.g, damageText.color.b, 0f);
            damageAmount = 0f;
            damageText.text = damageAmount.ToString();
        }

        void FixedUpdate()
        {
            showTime += Time.deltaTime;
            if (showTime > showTimeDuration)
            {
                ResetDamageText();
            }
        }

        void Update()
        {
            UpdateDirection();
        }

        private void UpdateDirection()
        {
            if (IsCharacterDirectionLeft())
            {
                transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
            else
            {
                transform.localScale = new Vector3(-1 * Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
        }
    }
}
