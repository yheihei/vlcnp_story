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

        [SerializeField]
        bool playTeleportSound = false;

        [SerializeField]
        AudioClip teleportSoundClip = null;

        [SerializeField]
        float teleportSoundVolume = 1f;

        Coroutine teleportRoutine;
        Color[] initialRendererColors = null;
        float[] initialCanvasGroupAlphas = null;
        ParticleSystemRenderer[] targetParticleRenderers = null;
        Color[] initialParticleColors = null;
        Color[] initialParticleTintColors = null;
        bool[] initialParticleRendererEnabled = null;
        MaterialPropertyBlock particlePropertyBlock = null;

        static readonly int ColorPropertyId = Shader.PropertyToID("_Color");
        static readonly int TintColorPropertyId = Shader.PropertyToID("_TintColor");

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

            PlayTeleportSound();
            yield return FadeToAlpha(0f, fadeOutDuration);
            SetCollidersEnabled(false);

            Transform destination = SelectDestination();
            if (destination != null)
            {
                Vector3 previousPosition = transform.position;
                transform.position = destination.position;

                if (!TryFacePlayer() && !keepCurrentFacing)
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
            bool hasParticleTargets =
                targetParticleRenderers != null && targetParticleRenderers.Any(renderer => renderer != null);
            return hasRendererTargets || hasCanvasGroupTargets || hasParticleTargets;
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

        void PlayTeleportSound()
        {
            if (!playTeleportSound)
                return;

            VLMitamaLaughSound.Play(teleportSoundClip, transform.position, teleportSoundVolume);
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

            SetParticleAlpha(alpha);
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

        bool TryFacePlayer()
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player == null)
                return false;

            FaceTarget(player.transform.position.x);
            return true;
        }

        void FaceTarget(float targetX)
        {
            Vector3 scale = transform.localScale;
            scale.x = targetX < transform.position.x ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
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

            CacheParticleRenderers();
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

        void CacheParticleRenderers()
        {
            targetParticleRenderers = GetComponentsInChildren<ParticleSystemRenderer>(true)
                .Where(renderer => renderer != null)
                .Distinct()
                .ToArray();

            if (targetParticleRenderers.Length == 0)
            {
                initialParticleColors = null;
                initialParticleTintColors = null;
                initialParticleRendererEnabled = null;
                return;
            }

            initialParticleColors = new Color[targetParticleRenderers.Length];
            initialParticleTintColors = new Color[targetParticleRenderers.Length];
            initialParticleRendererEnabled = new bool[targetParticleRenderers.Length];

            for (int i = 0; i < targetParticleRenderers.Length; i++)
            {
                ParticleSystemRenderer particleRenderer = targetParticleRenderers[i];
                initialParticleColors[i] = GetMaterialColor(particleRenderer, ColorPropertyId, Color.white);
                initialParticleTintColors[i] =
                    GetMaterialColor(particleRenderer, TintColorPropertyId, initialParticleColors[i]);
                initialParticleRendererEnabled[i] = particleRenderer != null && particleRenderer.enabled;
            }
        }

        Color GetMaterialColor(Renderer renderer, int propertyId, Color fallback)
        {
            if (renderer == null || renderer.sharedMaterial == null)
                return fallback;

            return renderer.sharedMaterial.HasProperty(propertyId)
                ? renderer.sharedMaterial.GetColor(propertyId)
                : fallback;
        }

        void SetParticleAlpha(float alpha)
        {
            if (targetParticleRenderers == null)
                return;

            if (particlePropertyBlock == null)
            {
                particlePropertyBlock = new MaterialPropertyBlock();
            }

            float clampedAlpha = Mathf.Clamp01(alpha);
            for (int i = 0; i < targetParticleRenderers.Length; i++)
            {
                ParticleSystemRenderer particleRenderer = targetParticleRenderers[i];
                if (particleRenderer == null)
                    continue;

                bool initiallyEnabled = GetInitialParticleRendererEnabled(i, particleRenderer);
                if (clampedAlpha > 0f)
                {
                    particleRenderer.enabled = initiallyEnabled;
                }

                particleRenderer.GetPropertyBlock(particlePropertyBlock);

                Color color = GetInitialParticleColor(i, particleRenderer);
                color.a *= clampedAlpha;
                particlePropertyBlock.SetColor(ColorPropertyId, color);

                Color tintColor = GetInitialParticleTintColor(i, particleRenderer);
                tintColor.a *= clampedAlpha;
                particlePropertyBlock.SetColor(TintColorPropertyId, tintColor);

                particleRenderer.SetPropertyBlock(particlePropertyBlock);

                if (clampedAlpha <= 0f)
                {
                    particleRenderer.enabled = false;
                }
            }
        }

        Color GetInitialParticleColor(int index, ParticleSystemRenderer fallbackRenderer)
        {
            if (initialParticleColors == null || index < 0 || index >= initialParticleColors.Length)
            {
                return GetMaterialColor(fallbackRenderer, ColorPropertyId, Color.white);
            }

            return initialParticleColors[index];
        }

        Color GetInitialParticleTintColor(int index, ParticleSystemRenderer fallbackRenderer)
        {
            if (initialParticleTintColors == null || index < 0 || index >= initialParticleTintColors.Length)
            {
                return GetMaterialColor(
                    fallbackRenderer,
                    TintColorPropertyId,
                    GetInitialParticleColor(index, fallbackRenderer)
                );
            }

            return initialParticleTintColors[index];
        }

        bool GetInitialParticleRendererEnabled(int index, ParticleSystemRenderer fallbackRenderer)
        {
            if (
                initialParticleRendererEnabled == null
                || index < 0
                || index >= initialParticleRendererEnabled.Length
            )
            {
                return fallbackRenderer != null && fallbackRenderer.enabled;
            }

            return initialParticleRendererEnabled[index];
        }

        void RestoreVisibleState()
        {
            SetAlpha(1f);
            SetCollidersEnabled(true);
        }
    }
}
