using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VLCNP.Control
{
    public class InvisibleWallController : MonoBehaviour
    {
        SpriteRenderer spriteRenderer;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.tag == "Player")
            {
                StartCoroutine(Fade(0.1f));
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.tag == "Player")
            {
                StartCoroutine(Fade(0));
            }
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
