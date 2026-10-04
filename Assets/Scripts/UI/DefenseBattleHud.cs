using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace VLCNP.UI
{
    /**
     * 防衛戦の目標と、倒した数のメーターを画面上部の中央に出す。
     * 倒すたびにメーターを伸ばして数字を光らせ、守りきったら文言を変えてから消える。
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

        int requiredKills = 1;
        string defaultTitle;
        Coroutine fadeRoutine;
        Coroutine meterRoutine;

        void Awake()
        {
            defaultTitle = titleText.text;
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
                return;
            }
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }

        public void Show(int required)
        {
            gameObject.SetActive(true);
            requiredKills = Mathf.Max(1, required);
            titleText.text = defaultTitle;
            countText.text = FormatCount(0);
            SetMeter(0f);
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

        public void ShowCleared()
        {
            if (!isActiveAndEnabled)
                return;
            titleText.text = clearedTitle;
            countText.text = FormatCount(requiredKills);
            AnimateMeter(1f);
            StartCoroutine(HideAfter(clearedHoldTime));
        }

        // 守りきれずに終わったとき(操作キャラが倒れたとき)は、文言を変えずにすぐ消す
        public void Hide()
        {
            if (!isActiveAndEnabled)
                return;
            Fade(0f, true);
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
