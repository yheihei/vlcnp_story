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
        bool isDestroy = false;

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
            showTime = 0f;
            // Text表示
            damageText.color = new Color(damageText.color.r, damageText.color.g, damageText.color.b, 1f);
            // ダメージを合算
            damageAmount += damagePoint;
            damageText.text = damageAmount.ToString();
        }

        // 死んだときダメージキャラクターの子オブジェクトから離脱し、その座標にとどまるようにする
        public void WithDrawlFromCharacterAndDestroy()
        {
            Vector3 currentPos = transform.position;
            transform.SetParent(null);
            transform.position = currentPos;
            isDestroy = true;
            // 一定時間後に消滅
            Destroy(gameObject, showTimeDuration);
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
            if (isDestroy) return;
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
            if (isDestroy) return;
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
