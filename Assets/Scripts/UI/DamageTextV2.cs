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

        [Header("ダメージ数字の演出")]
        [SerializeField, Tooltip("初回表示時にダメージ数字の基本色を下の色へ置き換える。Prefab側の色を使う場合はオフ")]
        private bool applyDamageColor = true;

        [SerializeField, Tooltip("落ち着いた後のダメージ数字の色")]
        private Color damageColor = new Color(1f, 0.96f, 0.86f, 1f);

        [SerializeField, Tooltip("ヒット直後に基本色へ乗算する色味。短時間で基本色へ戻る")]
        private Color damagePopTint = new Color(1f, 0.8f, 0.45f, 1f);

        [SerializeField, Min(1f), Tooltip("最初のヒットで現れるときの拡大率")]
        private float damageFirstPopScale = 1.45f;

        [SerializeField, Min(1f), Tooltip("表示中の追加ヒットで膨らむ最大の拡大率")]
        private float damageRehitPopScale = 1.16f;

        [SerializeField, Min(0.01f), Tooltip("拡大と跳ね上がりが落ち着くまでの秒数")]
        private float damagePopDuration = 0.18f;

        [SerializeField, Min(0.01f), Tooltip("ヒット直後の色味が基本色へ戻るまでの秒数")]
        private float damageTintDuration = 0.22f;

        [SerializeField, Tooltip("最初に現れる位置。文字の高さを基準にする（負で下）")]
        private float damageEntryOffset = -0.1f;

        [SerializeField, Tooltip("跳ね上がって静止する位置。文字の高さを基準にする")]
        private float damageSettleOffset = 0.2f;

        [SerializeField, Min(0f), Tooltip("追加ヒットで一瞬沈む深さ。文字の高さを基準にする")]
        private float damageRehitDip = 0.05f;

        [SerializeField, Range(0f, 1f), Tooltip("追加ヒットで戻す色味の強さ")]
        private float damageRehitTintStrength = 0.45f;

        [SerializeField, Min(0.01f), Tooltip("この秒数より短い間隔の追加ヒットほど再反応を弱める")]
        private float damageRehitRecoveryTime = 0.2f;

        [SerializeField, Range(0f, 1f), Tooltip("ヒット間隔が短いときにも残す再反応の割合")]
        private float damageRehitMinStrength = 0.35f;

        [SerializeField, Min(0.01f), Tooltip("表示の終わりに消えていく秒数。表示時間の4割を上限とする")]
        private float damageFadeOutDuration = 0.32f;

        [SerializeField, Min(0f), Tooltip("消えるときに浮かぶ距離。文字の高さを基準にする")]
        private float damageFadeRise = 0.3f;

        [SerializeField, Range(0f, 0.5f), Tooltip("消えるときに縮む割合")]
        private float damageFadeShrink = 0.06f;

        [SerializeField, Tooltip("ダメージ数字に落ち影を付ける。既存のShadow/Outlineがあれば再利用する")]
        private bool useDamageShadow = true;

        [SerializeField]
        private Color damageShadowColor = new Color(0.06f, 0.05f, 0.12f, 0.85f);

        [SerializeField, Tooltip("落ち影のずれ。文字の高さを基準にする")]
        private Vector2 damageShadowDistance = new Vector2(0.05f, -0.09f);

        // 死亡後、表示が消えるより先に保険の破棄が走らないための余裕
        const float WithdrawDestroyMargin = 0.25f;

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
        bool isDamageStyleReady;
        RectTransform damageRect;
        CanvasRenderer damageRenderer;
        Vector2 damageBasePosition;
        Vector3 damageBaseScale;
        float damageLineHeight;
        bool isDamagePopping;
        float damagePopTime;
        float damagePopStartScale;
        float damagePopStartOffset;
        float damagePopStartTint;
        float damageCurrentScale = 1f;
        float damageCurrentOffset;
        float damageCurrentTint;

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

            EnsureDamageStyle();

            bool wasVisible = damageText.enabled;
            float timeSinceLastHit = showTime;
            enabled = true;
            showTime = 0f;
            // ダメージを合算
            damageAmount += damagePoint;
            damageText.text = damageAmount.ToString();
            damageText.enabled = true;
            StartDamagePop(wasVisible, timeSinceLastHit);
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
            // 表示中は Update で最後まで消してから破棄する。こちらは Update が動かない場合の保険。
            Destroy(gameObject, showTimeDuration + WithdrawDestroyMargin);
        }

        void ResetDamageText()
        {
            damageText.enabled = false;
            damageAmount = 0f;

            if (isDamageStyleReady)
            {
                // 次の表示と LevelChange の複製が基準の状態から始まるようにする。
                isDamagePopping = false;
                ApplyDamageVisual(1f, 0f, 0f, 1f);
            }
        }

        void Update()
        {
            float deltaTime = Time.deltaTime;
            // 一時停止中は演出を止め、同じ値の書き込みもしない。
            if (deltaTime <= 0f)
                return;

            if (damageText.enabled)
            {
                showTime += deltaTime;
                if (showTime > showTimeDuration)
                    ResetDamageText();
                else
                    UpdateDamageAnimation(deltaTime);
            }

            if (IsLevelChangeTextVisible())
            {
                levelChangeShowTime += deltaTime;
                UpdateLevelChangeAnimation();
                if (levelChangeShowTime >= levelChangeShowTimeDuration)
                    levelChangeText.enabled = false;
            }

            if (!damageText.enabled && !IsLevelChangeTextVisible())
            {
                enabled = false;
                // 死亡後は表示が消えきった時点で破棄する。
                if (isDestroy)
                    Destroy(gameObject);
                return;
            }

            UpdateDirection();
        }

        private void StartDamagePop(bool wasVisible, float timeSinceLastHit)
        {
            if (!wasVisible)
            {
                damagePopStartScale = damageFirstPopScale;
                damagePopStartOffset = damageEntryOffset;
                damagePopStartTint = 1f;
            }
            else
            {
                // 間隔の短い連続ヒットほど控えめに反応させる。
                // 現在値との大きい方を取るだけなので、何度当たっても最初のポップを超えて膨らまない。
                float strength = Mathf.Lerp(damageRehitMinStrength, 1f,
                    Mathf.Clamp01(timeSinceLastHit / damageRehitRecoveryTime));
                damagePopStartScale = Mathf.Max(damageCurrentScale,
                    1f + (damageRehitPopScale - 1f) * strength);
                damagePopStartOffset = Mathf.Max(damageCurrentOffset - damageRehitDip * strength,
                    damageEntryOffset);
                damagePopStartTint = Mathf.Max(damageCurrentTint, damageRehitTintStrength * strength);
            }

            damagePopTime = 0f;
            isDamagePopping = true;
            // ヒットしたフレームから開始姿勢を見せる。
            ApplyDamageVisual(damagePopStartScale, damagePopStartOffset, damagePopStartTint, 1f);
        }

        private void UpdateDamageAnimation(float deltaTime)
        {
            float duration = Mathf.Max(0.02f, showTimeDuration);
            float fadeDuration = Mathf.Clamp(damageFadeOutDuration, 0.01f, duration * 0.4f);
            float fadeStart = duration - fadeDuration;
            bool isFading = showTime > fadeStart;

            // 静止中は Rect も色も書き込まず、Canvas の再構築を起こさない。
            if (!isDamagePopping && !isFading)
                return;

            float scale = 1f;
            float offset = damageSettleOffset;
            float tint = 0f;
            if (isDamagePopping)
            {
                damagePopTime += deltaTime;
                // 大きく現れて縮みながら跳ね上がり、行き過ぎずに止まる。
                float pop = Mathf.Clamp01(damagePopTime / damagePopDuration);
                float popRemain = 1f - pop;
                float popEase = 1f - popRemain * popRemain * popRemain;
                scale = Mathf.LerpUnclamped(damagePopStartScale, 1f, popEase);
                offset = Mathf.LerpUnclamped(damagePopStartOffset, damageSettleOffset, popEase);

                float tintRemain = 1f - Mathf.Clamp01(damagePopTime / damageTintDuration);
                tint = damagePopStartTint * tintRemain * tintRemain;

                if (pop >= 1f && tintRemain <= 0f)
                    isDamagePopping = false;
            }

            float alpha = 1f;
            if (isFading)
            {
                float fade = Mathf.SmoothStep(0f, 1f, (showTime - fadeStart) / fadeDuration);
                alpha = 1f - fade;
                offset += damageFadeRise * fade;
                scale *= 1f - damageFadeShrink * fade;
            }

            ApplyDamageVisual(scale, offset, tint, alpha);
        }

        private void ApplyDamageVisual(float scale, float offset, float tint, float alpha)
        {
            damageCurrentScale = scale;
            damageCurrentOffset = offset;
            damageCurrentTint = tint;

            // 毎回基準値から求めるので、連続ヒットでも位置や大きさがずれていかない。
            damageRect.localScale = damageBaseScale * scale;
            damageRect.anchoredPosition = damageBasePosition + Vector2.up * (damageLineHeight * offset);

            // CanvasRenderer の色は頂点を作り直さずに反映される。
            Color color = Color.LerpUnclamped(Color.white, damagePopTint, tint);
            color.a = alpha;
            damageRenderer.SetColor(color);
        }

        private void EnsureDamageStyle()
        {
            if (isDamageStyleReady || damageText == null)
                return;
            isDamageStyleReady = true;

            damageRect = damageText.rectTransform;
            damageRenderer = damageText.canvasRenderer;
            damageBasePosition = damageRect.anchoredPosition;
            damageBaseScale = damageRect.localScale;
            // CanvasScaler 適用後の値が必要なため、Awake ではなく初回表示時に測る。
            float pixelsPerUnit = damageText.pixelsPerUnit > 0f ? damageText.pixelsPerUnit : 1f;
            float localLineHeight = damageText.fontSize / pixelsPerUnit;
            damageLineHeight = localLineHeight * Mathf.Abs(damageBaseScale.y);

            if (applyDamageColor)
                damageText.color = damageColor;

            UnityEngine.UI.Shadow shadow = damageText.GetComponent<UnityEngine.UI.Shadow>();
            if (!useDamageShadow)
                return;
            // Outline は三角形5倍になるため、新規には三角形2倍の落ち影だけを付ける。
            // UGUI が三角形ストリームへ展開するため、mesh vertexCount は1文字4→12になる。
            if (shadow == null)
                shadow = damageText.gameObject.AddComponent<UnityEngine.UI.Shadow>();
            shadow.effectColor = damageShadowColor;
            shadow.useGraphicAlpha = true;
            Vector2 distance = damageShadowDistance * localLineHeight;
            if (shadow is UnityEngine.UI.Outline)
            {
                float outlineSize = Mathf.Min(Mathf.Abs(distance.x), Mathf.Abs(distance.y));
                distance = new Vector2(outlineSize, -outlineSize);
            }
            shadow.effectDistance = distance;
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

            // ダメージ数字の基準位置・大きさを先に確定させる。
            EnsureDamageStyle();

            // フォント・描画レイヤー・縮尺を既存のダメージ数字から引き継ぐ。
            levelChangeText = Instantiate(damageText, damageText.transform.parent, false);
            levelChangeText.gameObject.name = "LevelChangeText";
            levelChangeText.fontSize = Mathf.Max(1, Mathf.RoundToInt(damageText.fontSize * levelChangeFontScale));
            levelChangeText.alignment = TextAnchor.MiddleCenter;
            levelChangeText.resizeTextForBestFit = false;
            levelChangeText.horizontalOverflow = HorizontalWrapMode.Overflow;
            levelChangeText.verticalOverflow = VerticalWrapMode.Overflow;
            levelChangeText.raycastTarget = false;

            // ダメージ数字の落ち影と演出中の色は引き継がない。Outline は下で設定し直す。
            UnityEngine.UI.Shadow[] clonedShadows = levelChangeText.GetComponents<UnityEngine.UI.Shadow>();
            for (int i = 0; i < clonedShadows.Length; i++)
            {
                if (!(clonedShadows[i] is UnityEngine.UI.Outline))
                    clonedShadows[i].enabled = false;
            }
            levelChangeText.canvasRenderer.SetColor(Color.white);

            RectTransform levelRect = levelChangeText.rectTransform;
            // 演出中に複製されても、ダメージ数字の基準の大きさ・位置から配置する。
            levelRect.localScale = damageBaseScale;
            // World Space Canvas ではフォントのピクセル数を UI の座標単位へ変換する。
            float levelLineHeight = levelChangeText.fontSize / levelChangeText.pixelsPerUnit;
            levelRect.sizeDelta = new Vector2(levelLineHeight * 10f, levelLineHeight * 2f);
            levelRect.anchoredPosition = damageBasePosition + Vector2.up
                * (damageLineHeight * levelChangeLineOffset);
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
            float scaleX;
            if (IsCharacterDirectionLeft())
            {
                scaleX = Mathf.Abs(scale.x);
            }
            else
            {
                scaleX = -1 * Mathf.Abs(scale.x);
            }
            // 向きが変わったときだけ書き込み、Canvas の再計算を避ける。
            if (scale.x != scaleX)
            {
                scale.x = scaleX;
                cachedTransform.localScale = scale;
            }
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
            Quaternion rotation = Quaternion.Euler(0f, 0f, localAngle);
            if (cachedTransform.localRotation != rotation)
                cachedTransform.localRotation = rotation;
        }

        private void EnsureInitialized()
        {
            if (cachedTransform == null)
                cachedTransform = transform;
        }
    }
}
