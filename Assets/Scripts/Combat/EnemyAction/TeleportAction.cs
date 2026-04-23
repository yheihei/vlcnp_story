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
        CanvasGroup[] targetCanvasGroups = null;

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
        Color[] initialRendererColors = null;
        float[] initialCanvasGroupAlphas = null;
        void Awake()
        {
            if (targetRenderers == null || targetRenderers.Length == 0)
            {
                targetRenderers = GetComponentsInChildren<SpriteRenderer>(true);
            }

            if (targetCanvasGroups == null || targetCanvasGroups.Length == 0)
            {
                targetCanvasGroups = GetComponentsInChildren<Canvas>(true)
                    .Select(GetOrAddCanvasGroup)
                    .Where(canvasGroup => canvasGroup != null)
                    .Distinct()
                    .ToArray();
            }

            if (targetColliders == null || targetColliders.Length == 0)
            {
                targetColliders = GetComponentsInChildren<Collider2D>(true);
            }

            CacheInitialVisualState();
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

            CacheInitialVisualState();

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
            if (!HasFadeTargets())
                yield break;

            float clampedDuration = Mathf.Max(0f, duration);
            if (clampedDuration <= 0f)
            {
                SetAlpha(alpha);
                yield break;
            }

            float elapsed = 0f;
            float[] startRendererAlphas = (targetRenderers ?? new SpriteRenderer[0])
                .Select(renderer => renderer != null ? renderer.color.a : alpha)
                .ToArray();
            float[] startCanvasGroupAlphas = (targetCanvasGroups ?? new CanvasGroup[0])
                .Select(canvasGroup => canvasGroup != null ? canvasGroup.alpha : alpha)
                .ToArray();

            while (elapsed < clampedDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / clampedDuration);

                if (targetRenderers != null)
                {
                    for (int i = 0; i < targetRenderers.Length; i++)
                    {
                        SpriteRenderer renderer = targetRenderers[i];
                        if (renderer == null)
                            continue;

                        Color color = renderer.color;
                        color.a = Mathf.Lerp(startRendererAlphas[i], alpha, t);
                        renderer.color = color;
                    }
                }

                if (targetCanvasGroups != null)
                {
                    for (int i = 0; i < targetCanvasGroups.Length; i++)
                    {
                        CanvasGroup canvasGroup = targetCanvasGroups[i];
                        if (canvasGroup == null)
                            continue;

                        float targetAlpha = GetInitialCanvasGroupAlpha(i, canvasGroup) * alpha;
                        canvasGroup.alpha = Mathf.Lerp(startCanvasGroupAlphas[i], targetAlpha, t);
                    }
                }

                yield return null;
            }

            SetAlpha(alpha);
        }

        bool HasFadeTargets()
        {
            bool hasRendererTargets = targetRenderers != null && targetRenderers.Any(renderer => renderer != null);
            bool hasCanvasGroupTargets = targetCanvasGroups != null && targetCanvasGroups.Any(canvasGroup => canvasGroup != null);
            return hasRendererTargets || hasCanvasGroupTargets;
        }

        CanvasGroup GetOrAddCanvasGroup(Canvas canvas)
        {
            if (canvas == null)
                return null;

            CanvasGroup canvasGroup = canvas.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
                return canvasGroup;

            canvasGroup = canvas.gameObject.AddComponent<CanvasGroup>();
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            return canvasGroup;
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
            if (targetRenderers != null)
            {
                for (int i = 0; i < targetRenderers.Length; i++)
                {
                    SpriteRenderer renderer = targetRenderers[i];
                    if (renderer == null)
                        continue;

                    Color baseColor = GetInitialRendererColor(i, renderer);
                    baseColor.a = alpha;
                    renderer.color = baseColor;
                }
            }

            if (targetCanvasGroups != null)
            {
                for (int i = 0; i < targetCanvasGroups.Length; i++)
                {
                    CanvasGroup canvasGroup = targetCanvasGroups[i];
                    if (canvasGroup == null)
                        continue;

                    canvasGroup.alpha = GetInitialCanvasGroupAlpha(i, canvasGroup) * alpha;
                }
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

        void CacheInitialVisualState()
        {
            if (targetRenderers == null)
            {
                initialRendererColors = null;
            }
            else
            {
                initialRendererColors = new Color[targetRenderers.Length];
                for (int i = 0; i < targetRenderers.Length; i++)
                {
                    SpriteRenderer renderer = targetRenderers[i];
                    initialRendererColors[i] = renderer != null ? renderer.color : Color.white;
                }
            }

            if (targetCanvasGroups == null)
            {
                initialCanvasGroupAlphas = null;
            }
            else
            {
                initialCanvasGroupAlphas = new float[targetCanvasGroups.Length];
                for (int i = 0; i < targetCanvasGroups.Length; i++)
                {
                    CanvasGroup canvasGroup = targetCanvasGroups[i];
                    initialCanvasGroupAlphas[i] = canvasGroup != null ? canvasGroup.alpha : 1f;
                }
            }
        }

        Color GetInitialRendererColor(int index, SpriteRenderer fallbackRenderer)
        {
            if (initialRendererColors == null || index < 0 || index >= initialRendererColors.Length)
            {
                return fallbackRenderer != null ? fallbackRenderer.color : Color.white;
            }

            return initialRendererColors[index];
        }

        float GetInitialCanvasGroupAlpha(int index, CanvasGroup fallbackCanvasGroup)
        {
            if (initialCanvasGroupAlphas == null || index < 0 || index >= initialCanvasGroupAlphas.Length)
            {
                return fallbackCanvasGroup != null ? fallbackCanvasGroup.alpha : 1f;
            }

            return initialCanvasGroupAlphas[index];
        }

        void RestoreVisibleState()
        {
            SetAlpha(1f);
            SetCollidersEnabled(true);
        }
    }
}
