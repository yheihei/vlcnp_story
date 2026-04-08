using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Projectiles.StatusEffects;
using UnityEngine;
using VLCNP.Stats;

namespace VLCNP.Combat.EnemyAction
{
    public class TeleportAction : EnemyAction
    {
        [SerializeField]
        List<Transform> teleportPoints = new List<Transform>();

        [SerializeField]
        SpriteRenderer[] targetRenderers = null;

        [SerializeField]
        Collider2D[] targetColliders = null;

        [SerializeField]
        float fadeOutDuration = 0.25f;

        [SerializeField]
        float hiddenWaitDuration = 0.2f;

        [SerializeField]
        float fadeInDuration = 0.25f;

        [SerializeField]
        float baseCooldownBeforeTeleport = 1.5f;

        [SerializeField]
        float poisonExtraDelay = 1.0f;

        [SerializeField]
        bool keepCurrentFacing = true;

        Coroutine teleportRoutine;
        Color[] initialColors = null;
        void Awake()
        {
            if (targetRenderers == null || targetRenderers.Length == 0)
            {
                targetRenderers = GetComponentsInChildren<SpriteRenderer>(true);
            }

            if (targetColliders == null || targetColliders.Length == 0)
            {
                targetColliders = GetComponentsInChildren<Collider2D>(true);
            }

            CacheInitialColors();
        }

        void OnEnable()
        {
            RestoreVisibleState();
        }

        public override void Execute()
        {
            if (IsExecuting || IsDone)
                return;

            if (teleportPoints == null || teleportPoints.Count == 0 || teleportPoints.All(point => point == null))
            {
                Debug.LogWarning($"TeleportAction: teleportPoints is empty on {gameObject.name}");
                IsDone = true;
                return;
            }

            IsExecuting = true;
            teleportRoutine = StartCoroutine(TeleportSequence());
        }

        public override void Stop()
        {
            if (teleportRoutine != null)
            {
                StopCoroutine(teleportRoutine);
                teleportRoutine = null;
            }

            RestoreVisibleState();
            IsExecuting = false;
            IsDone = true;
        }

        IEnumerator TeleportSequence()
        {
            yield return WaitInterruptible(baseCooldownBeforeTeleport);

            if (HasTeleportDelayStatus())
            {
                yield return WaitInterruptible(poisonExtraDelay);
            }

            yield return FadeToAlpha(0f, fadeOutDuration);
            SetCollidersEnabled(false);

            Transform destination = SelectDestination();
            if (destination != null)
            {
                Vector3 previousPosition = transform.position;
                transform.position = destination.position;

                if (!keepCurrentFacing)
                {
                    UpdateFacing(previousPosition.x, destination.position.x);
                }
            }

            yield return WaitKeepingAlpha(hiddenWaitDuration, 0f);

            yield return FadeToAlpha(1f, fadeInDuration);
            SetCollidersEnabled(true);

            teleportRoutine = null;
            IsExecuting = false;
            IsDone = true;
        }

        IEnumerator WaitInterruptible(float seconds)
        {
            if (seconds <= 0f)
                yield break;

            float elapsed = 0f;
            while (elapsed < seconds)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }
        }

        IEnumerator WaitKeepingAlpha(float seconds, float alpha)
        {
            if (seconds <= 0f)
            {
                SetAlpha(alpha);
                yield break;
            }

            float elapsed = 0f;
            while (elapsed < seconds)
            {
                SetAlpha(alpha);
                elapsed += Time.deltaTime;
                yield return null;
            }

            SetAlpha(alpha);
        }

        IEnumerator FadeToAlpha(float alpha, float duration)
        {
            CacheInitialColors();

            if (targetRenderers == null || targetRenderers.Length == 0)
                yield break;

            float clampedDuration = Mathf.Max(0f, duration);
            if (clampedDuration <= 0f)
            {
                SetAlpha(alpha);
                yield break;
            }

            float elapsed = 0f;
            float[] startAlphas = targetRenderers
                .Select(renderer => renderer != null ? renderer.color.a : alpha)
                .ToArray();

            while (elapsed < clampedDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / clampedDuration);

                for (int i = 0; i < targetRenderers.Length; i++)
                {
                    SpriteRenderer renderer = targetRenderers[i];
                    if (renderer == null)
                        continue;

                    Color color = renderer.color;
                    color.a = Mathf.Lerp(startAlphas[i], alpha, t);
                    renderer.color = color;
                }

                yield return null;
            }

            SetAlpha(alpha);
        }

        bool HasTeleportDelayStatus()
        {
            PoisonStatus poisonStatus = GetComponent<PoisonStatus>();
            if (poisonStatus != null && poisonStatus.IsPoisoned)
            {
                return true;
            }

            ParalysisStatusController paralysisStatus = GetComponent<ParalysisStatusController>();
            if (paralysisStatus != null && paralysisStatus.IsParalyzed)
            {
                return true;
            }

            return false;
        }

        void SetAlpha(float alpha)
        {
            if (targetRenderers == null)
                return;

            for (int i = 0; i < targetRenderers.Length; i++)
            {
                SpriteRenderer renderer = targetRenderers[i];
                if (renderer == null)
                    continue;

                Color baseColor = GetInitialColor(i, renderer);
                baseColor.a = alpha;
                renderer.color = baseColor;
            }
        }

        void SetCollidersEnabled(bool value)
        {
            if (targetColliders == null)
                return;

            foreach (Collider2D targetCollider in targetColliders)
            {
                if (targetCollider == null)
                    continue;

                targetCollider.enabled = value;
            }
        }

        Transform SelectDestination()
        {
            List<Transform> validPoints = teleportPoints.Where(point => point != null).ToList();
            if (validPoints.Count == 0)
            {
                Debug.LogWarning($"TeleportAction: valid teleport point is not found on {gameObject.name}");
                return null;
            }

            List<Transform> differentPoints = validPoints
                .Where(point => Vector3.Distance(point.position, transform.position) > 0.05f)
                .ToList();

            List<Transform> candidates = differentPoints.Count > 0 ? differentPoints : validPoints;
            int randomIndex = Random.Range(0, candidates.Count);
            return candidates[randomIndex];
        }

        void UpdateFacing(float previousX, float nextX)
        {
            float deltaX = nextX - previousX;
            if (Mathf.Abs(deltaX) < 0.01f)
                return;

            Vector3 scale = transform.localScale;
            scale.x = deltaX < 0f ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
            transform.localScale = scale;
        }

        void CacheInitialColors()
        {
            if (targetRenderers == null)
            {
                initialColors = null;
                return;
            }

            initialColors = new Color[targetRenderers.Length];
            for (int i = 0; i < targetRenderers.Length; i++)
            {
                SpriteRenderer renderer = targetRenderers[i];
                initialColors[i] = renderer != null ? renderer.color : Color.white;
            }
        }

        Color GetInitialColor(int index, SpriteRenderer fallbackRenderer)
        {
            if (initialColors == null || index < 0 || index >= initialColors.Length)
            {
                return fallbackRenderer != null ? fallbackRenderer.color : Color.white;
            }

            return initialColors[index];
        }

        void RestoreVisibleState()
        {
            CacheInitialColors();
            SetAlpha(1f);
            SetCollidersEnabled(true);
        }
    }
}
