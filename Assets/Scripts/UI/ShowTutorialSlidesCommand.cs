using Fungus;
using UnityEngine;

namespace VLCNP.UI
{
    /** Fungusの会話中からチュートリアルスライドを表示し、閉じられるまで待つコマンド */
    [CommandInfo("Tutorial",
                 "Show Tutorial Slides",
                 "チュートリアルスライドを表示し、閉じられたら次のコマンドへ進む")]
    [AddComponentMenu("")]
    public class ShowTutorialSlidesCommand : Command
    {
        [Tooltip("表示に使うTutorialSlideShow。未設定ならシーンから検索する")]
        [SerializeField]
        protected TutorialSlideShow slideShow;

        [Tooltip("表示するスライド(順番通りに表示される)")]
        [SerializeField]
        protected TutorialSlide[] slides;

        public override void OnEnter()
        {
            TutorialSlideShow target = slideShow;
            if (target == null)
            {
                target = FindObjectOfType<TutorialSlideShow>(true);
            }
            if (target == null || slides == null || slides.Length == 0)
            {
                Debug.LogWarning("[ShowTutorialSlidesCommand] TutorialSlideShowが見つからないかスライドが未設定です。");
                Continue();
                return;
            }
            target.Show(slides, Continue);
        }

        public override string GetSummary()
        {
            int count = slides != null ? slides.Length : 0;
            return $"スライド {count} 枚";
        }

        public override Color GetButtonColor()
        {
            return new Color32(216, 191, 216, 255);
        }
    }
}
