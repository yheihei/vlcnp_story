using UnityEngine;
using VLCNP.Stats;

namespace VLCNP.UI
{
    /**
     * 頭上にダメージ合計とプレイヤーのレベル変化を表示する。
     */
    public class DamageTextV2 : MonoBehaviour
    {
        // ダメージを受けた対象のキャラクターを入れる
        [SerializeField]
        Transform damagedCharacter;

        [SerializeField]
        UnityEngine.UI.Text damageText;

        [SerializeField]
        float showTimeDuration = 2.5f;

        [Header("プレイヤーのレベル変化表示")]
        [SerializeField, Min(0.1f)]
        private float levelChangeShowTimeDuration = 1.5f;

        [SerializeField, Min(0.1f), Tooltip("ダメージ数字に対する文字サイズの倍率")]
        private float levelChangeFontScale = 0.9f;

        [SerializeField, Min(0.1f), Tooltip("ダメージ数字の文字サイズを基準にした上方向の間隔")]
        private float levelChangeLineOffset = 1.4f;

        [SerializeField]
        private Color levelUpColor = new Color(1f, 0.85f, 0.15f, 1f);

        [SerializeField]
        private Color levelDownColor = new Color(1f, 0.15f, 0.1f, 1f);

        [SerializeField, Min(0.01f)]
        private float levelChangeEnterDuration = 0.22f;

        [SerializeField, Min(0.01f)]
        private float levelChangeFadeOutDuration = 0.4f;

        [SerializeField, Min(0f), Tooltip("消えるときに浮かぶ距離。文字の高さを基準にする")]
        private float levelChangeRiseHeight = 0.75f;

        float damageAmount = 0f;
        float showTime = 0f;
        bool isDestroy = false;
        Transform cachedTransform;
        Experience playerExperience;
        UnityEngine.UI.Text levelChangeText;
        float levelChangeShowTime;
        RectTransform levelChangeRect;
        Vector2 levelChangeBasePosition;
        Vector3 levelChangeBaseScale;
        Color levelChangeColor;
        float levelChangeLineHeight;

        private void Awake()
        {
            EnsureInitialized();
            ResetDamageText();
            enabled = false;

            if (damagedCharacter != null && damagedCharacter.CompareTag("Player"))
            {
                playerExperience = damagedCharacter.GetComponent<Experience>();
                if (playerExperience != null)
                {
                    // 非表示中はこのコンポーネントを無効化するため、購読は破棄まで維持する。
                    playerExperience.onLevelChanged += ShowLevelChange;
                }
            }
        }

        private void OnDisable()
        {
            // キャラクターを切り替えた後に古い通知が再表示されるのを防ぐ。
            if (levelChangeText != null)
                levelChangeText.enabled = false;
        }

        private void OnDestroy()
        {
            if (playerExperience != null)
                playerExperience.onLevelChanged -= ShowLevelChange;
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
        }

        void Update()
        {
            if (isDestroy)
                return;

            if (damageText.enabled)
            {
                showTime += Time.deltaTime;
                if (showTime > showTimeDuration)
                    ResetDamageText();
            }

            if (IsLevelChangeTextVisible())
            {
                levelChangeShowTime += Time.deltaTime;
                UpdateLevelChangeAnimation();
                if (levelChangeShowTime >= levelChangeShowTimeDuration)
                    levelChangeText.enabled = false;
            }

            if (!damageText.enabled && !IsLevelChangeTextVisible())
            {
                enabled = false;
                return;
            }

            UpdateDirection();
        }

        private bool IsLevelChangeTextVisible()
        {
            return levelChangeText != null && levelChangeText.enabled;
        }

        private void ShowLevelChange(int previousLevel, int currentLevel)
        {
            if (isDestroy || !gameObject.activeInHierarchy || damageText == null)
                return;

            EnsureLevelChangeText();
            bool isLevelUp = currentLevel > previousLevel;
            levelChangeText.text = isLevelUp ? "LevelUP" : "LevelDOWN";
            levelChangeColor = isLevelUp ? levelUpColor : levelDownColor;
            levelChangeShowTime = 0f;
            levelChangeText.enabled = true;
            enabled = true;
            UpdateLevelChangeAnimation();
            UpdateDirection();
        }

        private void UpdateLevelChangeAnimation()
        {
            float duration = Mathf.Max(0.02f, levelChangeShowTimeDuration);
            float enterDuration = Mathf.Clamp(levelChangeEnterDuration, 0.01f, duration * 0.5f);
            float exitDuration = Mathf.Clamp(levelChangeFadeOutDuration, 0.01f, duration * 0.5f);
            float enter = Mathf.Clamp01(levelChangeShowTime / enterDuration);
            float exit = Mathf.SmoothStep(0f, 1f,
                (levelChangeShowTime - (duration - exitDuration)) / exitDuration);

            // 少し小さく現れ、一度だけ軽く膨らんで通常の大きさに落ち着く。
            float scale = enter < 0.6f
                ? Mathf.Lerp(0.8f, 1.08f, Mathf.SmoothStep(0f, 1f, enter / 0.6f))
                : Mathf.Lerp(1.08f, 1f, Mathf.SmoothStep(0f, 1f, (enter - 0.6f) / 0.4f));
            levelChangeRect.localScale = levelChangeBaseScale * scale;

            float rise = -0.2f * (1f - Mathf.SmoothStep(0f, 1f, enter))
                + levelChangeRiseHeight * exit;
            levelChangeRect.anchoredPosition = levelChangeBasePosition
                + Vector2.up * (levelChangeLineHeight * rise);

            Color color = levelChangeColor;
            color.a *= Mathf.SmoothStep(0f, 1f, enter / 0.4f) * (1f - exit);
            levelChangeText.color = color;
        }

        private void EnsureLevelChangeText()
        {
            if (levelChangeText != null)
                return;

            // フォント・描画レイヤー・縮尺を既存のダメージ数字から引き継ぐ。
            levelChangeText = Instantiate(damageText, damageText.transform.parent, false);
            levelChangeText.gameObject.name = "LevelChangeText";
            levelChangeText.fontSize = Mathf.Max(1, Mathf.RoundToInt(damageText.fontSize * levelChangeFontScale));
            levelChangeText.alignment = TextAnchor.MiddleCenter;
            levelChangeText.resizeTextForBestFit = false;
            levelChangeText.horizontalOverflow = HorizontalWrapMode.Overflow;
            levelChangeText.verticalOverflow = VerticalWrapMode.Overflow;
            levelChangeText.raycastTarget = false;

            RectTransform damageRect = damageText.rectTransform;
            RectTransform levelRect = levelChangeText.rectTransform;
            // World Space Canvas ではフォントのピクセル数を UI の座標単位へ変換する。
            float damageLineHeight = damageText.fontSize / damageText.pixelsPerUnit;
            float levelLineHeight = levelChangeText.fontSize / levelChangeText.pixelsPerUnit;
            levelRect.sizeDelta = new Vector2(levelLineHeight * 10f, levelLineHeight * 2f);
            levelRect.anchoredPosition = damageRect.anchoredPosition + Vector2.up
                * (damageLineHeight * Mathf.Abs(damageRect.localScale.y) * levelChangeLineOffset);
            levelChangeRect = levelRect;
            levelChangeBasePosition = levelRect.anchoredPosition;
            levelChangeBaseScale = levelRect.localScale;
            levelChangeLineHeight = levelLineHeight * Mathf.Abs(levelRect.localScale.y);

            UnityEngine.UI.Outline outline = levelChangeText.GetComponent<UnityEngine.UI.Outline>();
            if (outline == null)
                outline = levelChangeText.gameObject.AddComponent<UnityEngine.UI.Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.9f);
            outline.useGraphicAlpha = true;
            float outlineSize = levelLineHeight * 0.04f;
            outline.effectDistance = new Vector2(outlineSize, -outlineSize);
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
            KeepUpright();
        }

        // キャラクターが回転しても表記は正立を維持する
        // 左右反転はX負スケールで行われるため、反転時は補正角の符号が逆になる
        private void KeepUpright()
        {
            Transform parent = cachedTransform.parent;
            if (parent == null)
                return;
            float parentAngle = parent.eulerAngles.z;
            float localAngle = IsCharacterDirectionLeft() ? -parentAngle : parentAngle;
            cachedTransform.localRotation = Quaternion.Euler(0f, 0f, localAngle);
        }

        private void EnsureInitialized()
        {
            if (cachedTransform == null)
                cachedTransform = transform;
        }
    }
}
