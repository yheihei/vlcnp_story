using UnityEngine;
using UnityEngine.Events;

namespace VLCNP.UI
{
    /** 発売案内の選択肢1つ。選択中は拡大して明るく、非選択は暗くする。 */
    public class ReleaseGuideItem : MonoBehaviour
    {
        [SerializeField] CanvasGroup group;
        [SerializeField] UnityEvent onSubmit;
        [SerializeField] float selectedScale = 1.1f;
        [SerializeField] float unselectedAlpha = 0.45f;
        [SerializeField] float lerpSpeed = 12f;

        bool selected;

        public UnityEvent OnSubmit => onSubmit;

        public void SetSelected(bool value, bool immediate = false)
        {
            selected = value;
            if (immediate) Apply(1f);
        }

        public void Submit()
        {
            onSubmit?.Invoke();
        }

        void Update()
        {
            Apply(Time.deltaTime * lerpSpeed);
        }

        void Apply(float t)
        {
            float scale = selected ? selectedScale : 1f;
            float alpha = selected ? 1f : unselectedAlpha;
            transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one * scale, t);
            if (group != null) group.alpha = Mathf.Lerp(group.alpha, alpha, t);
        }
    }
}
