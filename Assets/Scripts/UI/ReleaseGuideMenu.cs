using System.Collections;
using UnityEngine;
using VLCNP.Control;
using VLCNP.Core;

namespace VLCNP.UI
{
    /**
     * 発売案内。体験版完了挨拶の暗転後に、X・ウィッシュリスト・タイトルの三択を黒背景で出す。
     * 左右で選択、決定で実行。マウスは使わない。
     */
    public class ReleaseGuideMenu : MonoBehaviour
    {
        [SerializeField] CanvasGroup background;
        [SerializeField] CanvasGroup content;
        [SerializeField] ReleaseGuideItem[] items;
        [SerializeField] int initialIndex = 1;
        [SerializeField] float blackoutDuration = 2f;
        [SerializeField] float holdDuration = 2f;
        [SerializeField] float fadeInDuration = 0.5f;
        [SerializeField] float inputLockDuration = 0.5f;
        [SerializeField] BGMWrapper bgmWrapper;
        [SerializeField] AudioClip guideBgm;
        [SerializeField] float guideBgmVolume = 0.4f;
        [SerializeField] float guideBgmPitch = 1f;
        [SerializeField] TrialEndCtaActions ctaActions;
        [SerializeField] float exitFadeDuration = 1f;

        int index;
        bool ready;
        float readyTime;

        public bool IsReady => ready && Time.time >= readyTime;
        public int SelectedIndex => index;

        /** 暗転と BGM フェードアウトから始めて、発売案内を表示する。Fungus から呼ぶ。 */
        public void Begin()
        {
            StopAllCoroutines();
            StartCoroutine(BeginRoutine());
        }

        IEnumerator BeginRoutine()
        {
            ready = false;
            background.alpha = 0f;
            content.alpha = 0f;
            if (bgmWrapper != null) bgmWrapper.FadeOut(blackoutDuration);
            yield return Fade(background, 1f, blackoutDuration);
            yield return new WaitForSeconds(holdDuration);
            if (bgmWrapper != null && guideBgm != null) bgmWrapper.Play(guideBgm, guideBgmVolume, guideBgmPitch);
            Select(initialIndex, true);
            yield return Fade(content, 1f, fadeInDuration);
            readyTime = Time.time + inputLockDuration;
            ready = true;
        }

        static IEnumerator Fade(CanvasGroup group, float target, float duration)
        {
            float start = group.alpha;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                group.alpha = Mathf.Lerp(start, target, Mathf.Clamp01(elapsed / duration));
                yield return null;
            }
            group.alpha = target;
        }

        void Update()
        {
            if (!IsReady) return;
            if (PlayerInputAdapter.WasMenuLeftPressed()) MoveSelection(-1);
            else if (PlayerInputAdapter.WasMenuRightPressed()) MoveSelection(1);
            else if (PlayerInputAdapter.WasMenuSubmitPressed()) SubmitCurrent();
        }

        public void MoveSelection(int direction)
        {
            if (items == null || items.Length == 0) return;
            Select((index + direction + items.Length) % items.Length, false);
        }

        public void SubmitCurrent()
        {
            if (items == null || items.Length == 0) return;
            items[index].Submit();
        }

        /** タイトルへ戻る。BGM をフェードアウトして止めてから遷移する。 */
        public void ReturnToTitle()
        {
            if (!ready) return;
            ready = false;
            StartCoroutine(ReturnToTitleRoutine());
        }

        IEnumerator ReturnToTitleRoutine()
        {
            if (bgmWrapper != null)
            {
                bgmWrapper.FadeOut(exitFadeDuration);
                yield return new WaitForSeconds(exitFadeDuration);
                bgmWrapper.Stop();
            }
            Debug.Log("[ReleaseGuide] BGM を停止してタイトルへ戻ります。");
            if (ctaActions != null) ctaActions.BackToTitle();
        }

        void Select(int target, bool immediate)
        {
            index = Mathf.Clamp(target, 0, items.Length - 1);
            for (int i = 0; i < items.Length; i++)
            {
                items[i].SetSelected(i == index, immediate);
            }
        }
    }
}
