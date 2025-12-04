using UnityEngine;
using VLCNP.Core;

namespace VLCNP.Movement
{
    public class Floating : MonoBehaviour, IStoppable
    {
        [SerializeField, Min(0f)]
        float amplitude = 0.25f;

        [SerializeField, Min(0.01f)]
        float cycleDuration = 2f;

        [SerializeField]
        float phaseOffset = 0f;

        [SerializeField]
        bool useUnscaledTime = false;

        float elapsedTime = 0f;
        float lastOffset = 0f;

        bool isStopped = false;
        public bool IsStopped
        {
            get => isStopped;
            set
            {
                if (isStopped == value)
                    return;
                isStopped = value;
                if (isStopped)
                {
                    ResetOffset();
                }
            }
        }

        private void OnEnable()
        {
            ResetOffset();
        }

        private void OnDisable()
        {
            ResetOffset();
        }

        private void LateUpdate()
        {
            if (isStopped)
                return;

            elapsedTime += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

            float angularFrequency = Mathf.PI * 2f / Mathf.Max(cycleDuration, 0.01f);
            float offset = Mathf.Sin(elapsedTime * angularFrequency + phaseOffset) * amplitude;

            ApplyOffset(offset);
        }

        private void ApplyOffset(float offset)
        {
            Vector3 position = transform.position;
            position.y -= lastOffset;
            position.y += offset;
            transform.position = position;
            lastOffset = offset;
        }

        private void ResetOffset()
        {
            if (Mathf.Abs(lastOffset) > 0.0001f)
            {
                Vector3 position = transform.position;
                position.y -= lastOffset;
                transform.position = position;
            }
            lastOffset = 0f;
        }
    }
}
