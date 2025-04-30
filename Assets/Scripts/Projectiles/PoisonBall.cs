using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VLCNP.Core;

namespace VLCNP.Projectiles
{
    public class PoisonBall : MonoBehaviour, IStoppable
    {
        [SerializeField]
        float speed = 5f;

        [SerializeField]
        float deleteTime = 5f;

        bool isStopped = false;
        public bool IsStopped
        {
            get => isStopped;
            set => isStopped = value;
        }

        [SerializeField]
        Vector3 direction = Vector3.zero;

        private void Start()
        {
            if (direction != Vector3.zero)
            {
                Fire(direction);
            }

            StartCoroutine(FadeOutLater(deleteTime));
        }

        private IEnumerator FadeOutLater(float seconds, float fadeTime = 0.5f)
        {
            yield return new WaitForSeconds(seconds);

            float startTime = Time.time;
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            Color startColor = spriteRenderer.color;
            Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0);
            while (Time.time < startTime + fadeTime)
            {
                float t = (Time.time - startTime) / fadeTime;
                spriteRenderer.color = Color.Lerp(startColor, endColor, t);
                yield return null;
            }
            spriteRenderer.color = endColor;
            // フェードアウト後にオブジェクトを削除
            Destroy(gameObject);
        }

        public void Fire(Vector3 _direction)
        {
            direction = _direction.normalized;
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            rb.velocity = direction * speed;
        }
    }
}
