using UnityEngine;

namespace VLCNP.Movie
{
    // 連番スプライトを一定間隔で切り替える簡易アニメ(Animator 不要)。
    // followParentAlpha を有効にすると親の SpriteRenderer の透明度に追従する(ManualFadeObject と併用する子レイヤー向け)
    [RequireComponent(typeof(SpriteRenderer))]
    public class SpriteFlipbook : MonoBehaviour
    {
        [SerializeField] Sprite[] frames;
        [SerializeField] float framesPerSecond = 8f;
        [SerializeField] bool randomOrder = true;
        [SerializeField] bool followParentAlpha = true;

        SpriteRenderer spriteRenderer;
        SpriteRenderer parentRenderer;
        float timer;
        int index;

        void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (transform.parent != null) parentRenderer = transform.parent.GetComponent<SpriteRenderer>();
        }

        void Update()
        {
            if (frames != null && frames.Length > 0 && framesPerSecond > 0)
            {
                timer += Time.deltaTime;
                if (timer >= 1f / framesPerSecond)
                {
                    timer = 0;
                    index = randomOrder && frames.Length > 1 ? NextRandomIndex() : (index + 1) % frames.Length;
                    spriteRenderer.sprite = frames[index];
                }
            }
            if (followParentAlpha && parentRenderer != null)
            {
                Color color = spriteRenderer.color;
                color.a = parentRenderer.color.a;
                spriteRenderer.color = color;
            }
        }

        // 同じコマが連続しないように選ぶ
        int NextRandomIndex()
        {
            int next = Random.Range(0, frames.Length - 1);
            return next >= index ? next + 1 : next;
        }
    }
}
