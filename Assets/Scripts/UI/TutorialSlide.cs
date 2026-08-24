using System;
using UnityEngine;
using UnityEngine.Video;

namespace VLCNP.UI
{
    /** チュートリアルスライド1枚分のデータ。動画か静止画のどちらかを設定する */
    [Serializable]
    public class TutorialSlide
    {
        [SerializeField]
        private VideoClip videoClip;

        [SerializeField]
        private Sprite sprite;

        [SerializeField]
        private string title = "";

        [TextArea]
        [SerializeField]
        private string caption = "";

        public VideoClip VideoClip => videoClip;
        public Sprite Sprite => sprite;
        public string Title => title;
        public string Caption => caption;
    }
}
