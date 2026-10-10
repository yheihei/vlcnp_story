using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VLCNP.Control
{
    /**
     * 見えない壁。操作キャラがトリガーに触れると薄く見せ、離れると消す。
     */
    public class InvisibleWallController : MonoBehaviour
    {
        SpriteRenderer spriteRenderer;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        // 無効化で止まったフェードの濃さを残さず、次に有効化したときは消えた状態から始める
        private void OnDisable()
        {
            StopAllCoroutines();
            spriteRenderer.color = new Color(1, 1, 1, 0);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Player"))
            {
                StartFade(0.1f);
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.CompareTag("Player"))
            {
                StartFade(0);
            }
        }

        // 触れたまま無効化されると、Physics2D の Callbacks On Disable で OnTriggerExit2D が呼ばれる。
        // 無効な GameObject ではコルーチンを始められないので、このときは何もしない。
        // この呼び出しは SetActive(false) の途中、OnDisable より前に来る。
        // isActiveAndEnabled はまだ true を返すため、GameObject の状態で判定する。
        void StartFade(float alpha)
        {
            if (!enabled || !gameObject.activeInHierarchy)
                return;
            StartCoroutine(Fade(alpha));
        }

        IEnumerator Fade(float alpha)
        {
            float time = 0;
            while (time < 0.3f)
            {
                time += Time.deltaTime;
                spriteRenderer.color = new Color(1, 1, 1, Mathf.Lerp(spriteRenderer.color.a, alpha, time));
                yield return null;
            }
        }
    }
}
