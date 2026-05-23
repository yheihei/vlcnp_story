using UnityEngine;
using UnityEngine.UI;

namespace VLCNP.UI
{
    public class DamageTextV2 : MonoBehaviour
    {
        // ダメージを受けた対象のキャラクターを入れる
        [SerializeField]
        Transform damagedCharacter;

        [SerializeField]
        Text damageText;

        [SerializeField]
        float showTimeDuration = 2.5f;
        float damageAmount = 0f;
        float showTime = 0f;
        bool isDestroy = false;
        Transform cachedTransform;

        private void Awake()
        {
            EnsureInitialized();
            ResetDamageText();
        }

        private bool IsCharacterDirectionLeft()
        {
            return damagedCharacter.localScale.x > 0;
        }

        public void AddDamageText(float damagePoint)
        {
            EnsureInitialized();

            if (isDestroy)
                return;

            enabled = true;
            showTime = 0f;
            // ダメージを合算
            damageAmount += damagePoint;
            damageText.color = new Color(
                damageText.color.r,
                damageText.color.g,
                damageText.color.b,
                1f
            );
            damageText.text = damageAmount.ToString();
            damageText.enabled = true;
            UpdateDirection();
        }

        // 死んだときダメージキャラクターの子オブジェクトから離脱し、その座標にとどまるようにする
        public void WithDrawlFromCharacterAndDestroy()
        {
            EnsureInitialized();

            Vector3 currentPos = cachedTransform.position;
            cachedTransform.SetParent(null);
            cachedTransform.position = currentPos;
            isDestroy = true;
            // 一定時間後に消滅
            Destroy(gameObject, showTimeDuration);
        }

        void ResetDamageText()
        {
            damageText.enabled = false;
            damageAmount = 0f;
            if (!isDestroy)
                enabled = false;
        }

        void Update()
        {
            if (isDestroy)
                return;

            showTime += Time.deltaTime;
            if (showTime > showTimeDuration)
            {
                ResetDamageText();
                return;
            }

            UpdateDirection();
        }

        private void UpdateDirection()
        {
            EnsureInitialized();

            if (isDestroy)
                return;
            if (damagedCharacter == null)
                return;

            Vector3 scale = cachedTransform.localScale;
            if (IsCharacterDirectionLeft())
            {
                scale.x = Mathf.Abs(scale.x);
            }
            else
            {
                scale.x = -1 * Mathf.Abs(scale.x);
            }
            cachedTransform.localScale = scale;
        }

        private void EnsureInitialized()
        {
            if (cachedTransform == null)
                cachedTransform = transform;
        }
    }
}
