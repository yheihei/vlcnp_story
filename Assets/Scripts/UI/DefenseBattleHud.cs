using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VLCNP.UI
{
    /**
     * 防衛戦の目標と、倒した数のメーター、守る相手の残りを画面上部の中央に出す。
     * 倒すたびにメーターを伸ばして数字を光らせ、守りきったら文言を変えてから消える。
     * 守る相手が被弾するたびに顔を1つ灰色にして行を揺らし、画面の縁を赤く光らせる。
     * 残りが少なくなったら見出しを警告に変えて赤く点滅させ、顔を震わせ、画面の縁を速い鼓動で脈打たせる。
     * HUD と同じく UICamera に描く(見つからなければ画面に直接描く)。
     */
    public class DefenseBattleHud : MonoBehaviour
    {
        const string UICameraTag = "UICamera";

        [SerializeField]
        Canvas canvas;

        [SerializeField, Tooltip("HUD(UI の 1)の上、会話やフェード(画面に直接描く)の下に出す")]
        string sortingLayerName = "UI";

        [SerializeField]
        int sortingOrder = 2;

        [SerializeField]
        CanvasGroup canvasGroup;

        [SerializeField]
        RectTransform titleRow;

        [SerializeField]
        Text titleText;

        [SerializeField]
        Text countText;

        [SerializeField]
        Slider meter;

        [SerializeField]
        StatusValueFeedback countFeedback;

        [SerializeField]
        string clearedTitle = "まもりきった！";

        [SerializeField, Min(0.01f)]
        float fadeTime = 0.4f;

        [SerializeField, Min(0.01f)]
        float meterTime = 0.25f;

        [SerializeField, Min(0f), Tooltip("守りきったあと消えるまでの時間")]
        float clearedHoldTime = 3f;

        [Header("守る相手の残り")]
        [SerializeField, Tooltip("守る相手の顔を並べる行")]
        RectTransform targetLifeRow;

        [SerializeField, Tooltip("並べる顔の見本(非表示にしておく)。被弾できる回数だけ複製する")]
        Image targetLifeIconTemplate;

        [SerializeField]
        Sprite targetLifeSprite;

        [SerializeField, Tooltip("被弾して失った分の顔")]
        Sprite targetLostSprite;

        [SerializeField, Min(0), Tooltip("残りがこの数以下になったら危険の表示にする")]
        int dangerRemaining = 3;

        [SerializeField]
        string dangerTitle = "ミタマがあぶない！";

        [SerializeField]
        Color dangerColor = new Color(1f, 0.22f, 0.18f);

        [SerializeField, Min(0.02f), Tooltip("危険なあいだ、見出しと顔が赤く点滅する間隔")]
        float dangerBlinkPeriod = 0.3f;

        [SerializeField, Min(0f), Tooltip("危険なあいだ、見出しと顔の行が震える幅(px)")]
        float dangerShakeWidth = 2f;

        [SerializeField, Min(0f), Tooltip("被弾したときに顔の行を揺らす幅(px)")]
        float hitShakeWidth = 8f;

        [SerializeField, Min(0.01f)]
        float hitShakeTime = 0.35f;

        [Header("画面の縁")]
        [SerializeField, Tooltip("画面の縁を赤くする。被弾した瞬間と危険なあいだ光る")]
        CanvasGroup dangerVignette;

        [SerializeField, Tooltip("縁は HUD(UI の 1)より奥に描く")]
        Canvas dangerVignetteCanvas;

        [SerializeField]
        int dangerVignetteSortingOrder = 0;

        [SerializeField, Range(0f, 1f)]
        float hitFlashAlpha = 0.8f;

        [SerializeField, Min(0.01f)]
        float hitFlashTime = 0.45f;

        [SerializeField, Range(0f, 1f)]
        float dangerVignetteMinAlpha = 0.4f;

        [SerializeField, Range(0f, 1f)]
        float dangerVignetteMaxAlpha = 1f;

        [SerializeField, Min(0.05f), Tooltip("危険なあいだの鼓動の間隔。操作キャラのHPが少ないとき(1.2秒)より速くする")]
        float dangerPulsePeriod = 0.55f;

        int requiredKills = 1;
        string defaultTitle;
        Color titleColor;
        Vector2 titleRowPosition;
        Vector2 targetLifeRowPosition;
        Coroutine fadeRoutine;
        Coroutine meterRoutine;
        readonly List<Image> targetLifeIcons = new List<Image>();
        int targetMaxHits;
        int targetRemaining;
        bool isDanger;
        float hitFlashTimer;
        float hitShakeTimer;
        float dangerPhase;

        void Awake()
        {
            defaultTitle = titleText.text;
            titleColor = titleText.color;
            if (titleRow != null)
                titleRowPosition = titleRow.anchoredPosition;
            if (targetLifeRow != null)
                targetLifeRowPosition = targetLifeRow.anchoredPosition;
            if (targetLifeIconTemplate != null)
                targetLifeIconTemplate.gameObject.SetActive(false);
            AttachUICamera();
        }

        // カメラ未設定の Screen Space - Camera は Overlay として扱われるので、描画モードは見ずに付け直す
        void AttachUICamera()
        {
            if (canvas == null)
                return;
            GameObject uiCamera = GameObject.FindWithTag(UICameraTag);
            if (uiCamera != null && uiCamera.TryGetComponent(out Camera camera))
            {
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = camera;
                // 並び順はカメラが付いてからでないと設定できない
                canvas.sortingLayerName = sortingLayerName;
                canvas.sortingOrder = sortingOrder;
                if (dangerVignetteCanvas != null)
                {
                    dangerVignetteCanvas.overrideSorting = true;
                    dangerVignetteCanvas.sortingLayerName = sortingLayerName;
                    dangerVignetteCanvas.sortingOrder = dangerVignetteSortingOrder;
                }
                return;
            }
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }

        public void Show(int required, int targetMaxHits = 0)
        {
            gameObject.SetActive(true);
            requiredKills = Mathf.Max(1, required);
            titleText.text = defaultTitle;
            countText.text = FormatCount(0);
            SetMeter(0f);
            SetupTargetLife(targetMaxHits);
            canvasGroup.alpha = 0f;
            Fade(1f, false);
        }

        public void SetKillCount(int kills)
        {
            if (!isActiveAndEnabled)
                return;
            countText.text = FormatCount(kills);
            AnimateMeter((float)kills / requiredKills);
            if (countFeedback != null)
                countFeedback.Play(true);
        }

        // 守る相手の残りの被弾回数。減ったら顔を灰色にして揺らし、少なくなったら危険の表示にする
        public void SetTargetRemaining(int remaining)
        {
            if (!isActiveAndEnabled || targetMaxHits <= 0)
                return;
            remaining = Mathf.Clamp(remaining, 0, targetMaxHits);
            if (remaining < targetRemaining)
            {
                hitFlashTimer = hitFlashTime;
                hitShakeTimer = hitShakeTime;
            }
            targetRemaining = remaining;
            for (int i = 0; i < targetMaxHits && i < targetLifeIcons.Count; i++)
            {
                bool isLeft = i < remaining;
                targetLifeIcons[i].sprite = isLeft ? targetLifeSprite : targetLostSprite;
                // 失った顔には点滅の赤を残さない
                if (!isLeft)
                    targetLifeIcons[i].color = Color.white;
            }
            SetDanger(remaining <= dangerRemaining);
        }

        public void ShowCleared()
        {
            if (!isActiveAndEnabled)
                return;
            SetDanger(false);
            titleText.text = clearedTitle;
            countText.text = FormatCount(requiredKills);
            AnimateMeter(1f);
            StartCoroutine(HideAfter(clearedHoldTime));
        }

        // 守りきれずに終わったとき(操作キャラか守る相手が倒れたとき)は、文言を変えずにすぐ消す
        public void Hide()
        {
            if (!isActiveAndEnabled)
                return;
            Fade(0f, true);
        }

        // 時間を止めるヒットストップ中も動かすため、実時間で進める
        void Update()
        {
            float deltaTime = Time.unscaledDeltaTime;
            hitFlashTimer = Mathf.Max(0f, hitFlashTimer - deltaTime);
            hitShakeTimer = Mathf.Max(0f, hitShakeTimer - deltaTime);
            if (isDanger)
                dangerPhase = Mathf.Repeat(dangerPhase + deltaTime / dangerPulsePeriod, 1f);
            UpdateVignette();
            UpdateShake();
            UpdateDangerBlink();
        }

        void SetupTargetLife(int maxHits)
        {
            targetMaxHits = Mathf.Max(0, maxHits);
            targetRemaining = targetMaxHits;
            hitFlashTimer = 0f;
            hitShakeTimer = 0f;
            SetDanger(false);
            if (targetLifeRow == null || targetLifeIconTemplate == null)
                return;
            targetLifeRow.gameObject.SetActive(targetMaxHits > 0);
            while (targetLifeIcons.Count < targetMaxHits)
            {
                Image icon = Instantiate(targetLifeIconTemplate, targetLifeRow);
                icon.gameObject.SetActive(true);
                targetLifeIcons.Add(icon);
            }
            for (int i = 0; i < targetLifeIcons.Count; i++)
            {
                targetLifeIcons[i].gameObject.SetActive(i < targetMaxHits);
                targetLifeIcons[i].sprite = targetLifeSprite;
                targetLifeIcons[i].color = Color.white;
            }
        }

        void SetDanger(bool danger)
        {
            if (danger == isDanger)
                return;
            isDanger = danger;
            dangerPhase = 0f;
            titleText.text = danger ? dangerTitle : defaultTitle;
            if (danger)
                return;
            titleText.color = titleColor;
            foreach (Image icon in targetLifeIcons)
                icon.color = Color.white;
        }

        // 被弾した瞬間の光と、危険なあいだの鼓動の明るいほうを使う
        void UpdateVignette()
        {
            if (dangerVignette == null)
                return;
            float flash = hitFlashAlpha * hitFlashTimer / hitFlashTime;
            float pulse = isDanger
                ? Mathf.Lerp(dangerVignetteMinAlpha, dangerVignetteMaxAlpha, EvaluateHeartbeat(dangerPhase))
                : 0f;
            dangerVignette.alpha = Mathf.Max(flash, pulse);
        }

        // ドクッ、ドクッと2拍打って残りは休む心拍波形 (0〜1)。LowHealthScreenEffect と同じ形
        static float EvaluateHeartbeat(float phase)
        {
            float firstBeat = Mathf.Exp(-Square((phase - 0.1f) / 0.09f));
            float secondBeat = 0.5f * Mathf.Exp(-Square((phase - 0.32f) / 0.1f));
            return Mathf.Clamp01(firstBeat + secondBeat);
        }

        static float Square(float value) => value * value;

        // 拡大・縮小せず、整数の座標差だけで動かしてドットの形を保つ
        void UpdateShake()
        {
            float time = Time.unscaledTime;
            float tremble = isDanger ? Mathf.Round(Mathf.Sin(time * 47f) * dangerShakeWidth) : 0f;
            float hitShake = hitShakeTimer > 0f
                ? Mathf.Round(Mathf.Sin(time * 70f) * hitShakeWidth * hitShakeTimer / hitShakeTime)
                : 0f;
            if (targetLifeRow != null)
                SetOffset(targetLifeRow, targetLifeRowPosition, tremble + hitShake);
            if (titleRow != null)
                SetOffset(titleRow, titleRowPosition, tremble);
        }

        static void SetOffset(RectTransform target, Vector2 restPosition, float offsetX)
        {
            Vector2 position = restPosition + new Vector2(offsetX, 0f);
            if (target.anchoredPosition != position)
                target.anchoredPosition = position;
        }

        void UpdateDangerBlink()
        {
            if (!isDanger)
                return;
            bool isRed = Mathf.Repeat(Time.unscaledTime, dangerBlinkPeriod) < dangerBlinkPeriod * 0.5f;
            titleText.color = isRed ? dangerColor : titleColor;
            // 残っている顔は見出しと逆の拍で赤くする
            Color iconColor = isRed ? Color.white : dangerColor;
            for (int i = 0; i < targetRemaining && i < targetLifeIcons.Count; i++)
                targetLifeIcons[i].color = iconColor;
        }

        string FormatCount(int kills)
        {
            return $"{Mathf.Clamp(kills, 0, requiredKills)}/{requiredKills}";
        }

        void SetMeter(float value)
        {
            if (meterRoutine != null)
                StopCoroutine(meterRoutine);
            meterRoutine = null;
            meter.value = value;
        }

        void AnimateMeter(float target)
        {
            if (meterRoutine != null)
                StopCoroutine(meterRoutine);
            meterRoutine = StartCoroutine(MeterRoutine(Mathf.Clamp01(target)));
        }

        IEnumerator MeterRoutine(float target)
        {
            float start = meter.value;
            for (float t = 0f; t < meterTime; t += Time.unscaledDeltaTime)
            {
                meter.value = Mathf.Lerp(start, target, t / meterTime);
                yield return null;
            }
            meter.value = target;
            meterRoutine = null;
        }

        IEnumerator HideAfter(float delay)
        {
            yield return new WaitForSecondsRealtime(delay);
            Fade(0f, true);
        }

        void Fade(float target, bool deactivateAfter)
        {
            if (fadeRoutine != null)
                StopCoroutine(fadeRoutine);
            fadeRoutine = StartCoroutine(FadeRoutine(target, deactivateAfter));
        }

        IEnumerator FadeRoutine(float target, bool deactivateAfter)
        {
            float start = canvasGroup.alpha;
            for (float t = 0f; t < fadeTime; t += Time.unscaledDeltaTime)
            {
                canvasGroup.alpha = Mathf.Lerp(start, target, t / fadeTime);
                yield return null;
            }
            canvasGroup.alpha = target;
            fadeRoutine = null;
            if (deactivateAfter)
                gameObject.SetActive(false);
        }
    }
}
