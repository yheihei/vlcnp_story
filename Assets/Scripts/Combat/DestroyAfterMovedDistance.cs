using System.Collections;
using UnityEngine;

namespace VLCNP.Combat
{
    /** 生成位置から一定距離進んだら自身を削除する。飛距離制限のある弾に付ける */
    public class DestroyAfterMovedDistance : MonoBehaviour
    {
        [SerializeField]
        float maxDistance = 8f;
        public float MaxDistance
        {
            get => maxDistance;
            set => maxDistance = value;
        }

        [SerializeField]
        [Min(0f)]
        float fadeOutDuration = 0f;

        private Vector3 startPosition;
        private bool isDisappearing;

        private void Start()
        {
            startPosition = transform.position;
        }

        private void Update()
        {
            if (
                !isDisappearing
                && Vector3.Distance(startPosition, transform.position) >= maxDistance
            )
            {
                StartCoroutine(Disappear());
            }
        }

        private IEnumerator Disappear()
        {
            isDisappearing = true;
            if (TryGetComponent(out Collider2D collider))
            {
                collider.enabled = false;
            }

            if (fadeOutDuration <= 0f)
            {
                Destroy(gameObject);
                yield break;
            }

            SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>();
            Color[] startColors = new Color[renderers.Length];
            for (int i = 0; i < renderers.Length; i++)
            {
                startColors[i] = renderers[i].color;
            }

            float elapsed = 0f;
            while (elapsed < fadeOutDuration)
            {
                elapsed += Time.deltaTime;
                float alphaRate = 1f - Mathf.Clamp01(elapsed / fadeOutDuration);
                for (int i = 0; i < renderers.Length; i++)
                {
                    Color color = startColors[i];
                    color.a *= alphaRate;
                    renderers[i].color = color;
                }
                yield return null;
            }

            Destroy(gameObject);
        }
    }
}
