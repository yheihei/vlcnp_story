using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using VLCNP.Control;

namespace VLCNP.UI
{
    /** 一枚絵(動画/静止画)をスライド形式で順送り表示するUI。左右で送り/戻し、最終ページで決定すると閉じる */
    public class TutorialSlideShow : MonoBehaviour
    {
        [SerializeField]
        private GameObject panelRoot;

        [SerializeField]
        private VideoPlayer videoPlayer;

        [SerializeField]
        private RawImage videoImage;

        [SerializeField]
        private Image spriteImage;

        [SerializeField]
        private Text titleText;

        [SerializeField]
        private Text captionText;

        [SerializeField]
        private Text pageText;

        [SerializeField]
        private Text guideText;

        [SerializeField]
        private GameObject leftArrow;

        [SerializeField]
        private GameObject rightArrow;

        // メニュー選択の決定入力を拾ってしまわないための入力受付待ち時間
        const float InputWaitSeconds = 0.25f;

        IList<TutorialSlide> slides;
        int currentIndex;
        float openedTime;
        Action onClosed;

        public bool IsOpen { get; private set; }

        void Awake()
        {
            if (panelRoot != null) panelRoot.SetActive(false);
        }

        public void Show(IList<TutorialSlide> slides, Action onClosed = null)
        {
            if (IsOpen) return;
            if (slides == null || slides.Count == 0)
            {
                onClosed?.Invoke();
                return;
            }
            this.slides = slides;
            this.onClosed = onClosed;
            currentIndex = 0;
            openedTime = Time.unscaledTime;
            IsOpen = true;
            panelRoot.SetActive(true);
            ShowSlide(currentIndex);
        }

        void Update()
        {
            if (!IsOpen) return;
            if (Time.unscaledTime - openedTime < InputWaitSeconds) return;

            if (PlayerInputAdapter.WasMenuLeftPressed())
            {
                if (currentIndex > 0) ShowSlide(--currentIndex);
                return;
            }
            if (PlayerInputAdapter.WasMenuRightPressed())
            {
                if (currentIndex < slides.Count - 1) ShowSlide(++currentIndex);
                return;
            }
            if (PlayerInputAdapter.WasMenuSubmitPressed())
            {
                if (currentIndex < slides.Count - 1)
                {
                    ShowSlide(++currentIndex);
                }
                else
                {
                    Close();
                }
            }
        }

        void ShowSlide(int index)
        {
            TutorialSlide slide = slides[index];
            if (titleText != null) titleText.text = slide.Title;
            if (captionText != null) captionText.text = slide.Caption;
            if (pageText != null) pageText.text = $"{index + 1} / {slides.Count}";
            if (leftArrow != null) leftArrow.SetActive(index > 0);
            if (rightArrow != null) rightArrow.SetActive(index < slides.Count - 1);
            if (guideText != null)
            {
                guideText.text = index < slides.Count - 1 ? "決定: つぎへ" : "決定: とじる";
            }

            bool isVideo = slide.VideoClip != null;
            if (videoImage != null) videoImage.gameObject.SetActive(isVideo);
            if (spriteImage != null) spriteImage.gameObject.SetActive(!isVideo && slide.Sprite != null);

            if (videoPlayer != null)
            {
                videoPlayer.Stop();
                if (isVideo)
                {
                    ClearVideoTexture();
                    videoPlayer.clip = slide.VideoClip;
                    videoPlayer.isLooping = true;
                    videoPlayer.Play();
                }
            }
            if (!isVideo && spriteImage != null && slide.Sprite != null)
            {
                spriteImage.sprite = slide.Sprite;
                spriteImage.preserveAspect = true;
            }
        }

        // 直前のクリップの最終フレームが一瞬映らないようRenderTextureをクリアする
        void ClearVideoTexture()
        {
            if (videoPlayer == null || videoPlayer.targetTexture == null) return;
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = videoPlayer.targetTexture;
            GL.Clear(true, true, Color.black);
            RenderTexture.active = previous;
        }

        void Close()
        {
            if (videoPlayer != null) videoPlayer.Stop();
            panelRoot.SetActive(false);
            IsOpen = false;
            slides = null;
            Action callback = onClosed;
            onClosed = null;
            callback?.Invoke();
        }
    }
}
