using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VLCNP.Attributes;
using VLCNP.Combat;
using VLCNP.Effects;
using VLCNP.UI;

namespace VLCNP.Combat.EnemyAction
{
    public class VLMitamaCloneWeakAction : EnemyAction
    {
        [SerializeField]
        Transform leftPoint = null;

        [SerializeField]
        Transform rightPoint = null;

        [SerializeField]
        Transform middlePoint = null;

        [SerializeField]
        float disappearFadeDuration = 0.4f;

        [SerializeField]
        float appearFadeDuration = 0.4f;

        [SerializeField]
        float preMagicDuration = 2f;

        [SerializeField]
        float magicDuration = 0.2f;

        [SerializeField]
        float projectileLaunchDelay = 0.8f;

        [SerializeField]
        float cleanupFadeDuration = 0.3f;

        [SerializeField]
        AudioClip laughClip = null;

        [SerializeField]
        float laughVolume = 1f;

        [SerializeField]
        float decoyHitLaughVolume = 1.4f;

        [SerializeField]
        float laughPitch = 1f;

        [SerializeField]
        GameObject decoyPrefab = null;

        [SerializeField]
        GameObject castEffectPrefab = null;

        [SerializeField]
        Vector3 castEffectOffset = new Vector3(0f, -0.2f, 0f);

        [SerializeField]
        float castEffectFadeOutDuration = 0.5f;

        [SerializeField]
        Sprite projectileSprite = null;

        [SerializeField]
        RuntimeAnimatorController projectileAnimatorController = null;

        [SerializeField]
        OrbitingOnibiProjectile projectilePrefab = null;

        [SerializeField]
        Color projectileColor = Color.white;

        [SerializeField]
        Vector3 projectileScale = new Vector3(0.8f, 0.8f, 0.8f);

        [SerializeField]
        Vector2 projectileHeadOffset = new Vector2(0f, 0.9f);

        [SerializeField]
        Vector2 projectileColliderSize = new Vector2(0.4f, 0.8f);

        [SerializeField]
        float projectileSpeed = 12f;

        [SerializeField]
        float projectileLifetime = 4f;

        [SerializeField]
        float projectileDamage = 1f;

        [SerializeField]
        float projectileSelfRotationDegreesPerSecond = 0f;

        [SerializeField]
        string targetTagName = "Player";

        [SerializeField]
        string preMagicBoolName = "isPreMagic";

        [SerializeField]
        string magicBoolName = "isMagic";

        Coroutine actionRoutine = null;
        SpriteRenderer[] ownerRenderers = null;
        Color[] ownerInitialColors = null;
        Collider2D[] ownerColliders = null;
        bool[] ownerColliderStates = null;
        Animator ownerAnimator = null;
        Health ownerHealth = null;

        GameObject ownerCastEffect = null;
        OrbitingOnibiProjectile ownerOnibi = null;
        Transform cachedPlayerTransform = null;
        readonly List<CloneDecoyState> decoyStates = new List<CloneDecoyState>();

        void Awake()
        {
            ownerAnimator = GetComponent<Animator>();
            ownerHealth = GetComponent<Health>();
        }

        void OnDisable()
        {
            CleanupImmediate();
        }

        void OnDestroy()
        {
            CleanupImmediate();
        }

        public override void Execute()
        {
            if (IsExecuting || IsDone)
                return;

            if (leftPoint == null || rightPoint == null)
            {
                Debug.LogWarning($"VLMitamaCloneWeakAction: clone points are not set on {gameObject.name}");
                IsDone = true;
                return;
            }

            if (projectileSprite == null)
            {
                Debug.LogWarning($"VLMitamaCloneWeakAction: projectileSprite is not set on {gameObject.name}");
                IsDone = true;
                return;
            }

            IsExecuting = true;
            actionRoutine = StartCoroutine(ExecuteRoutine());
        }

        public override void Stop()
        {
            CleanupImmediate();
            IsExecuting = false;
            IsDone = true;
        }

        IEnumerator ExecuteRoutine()
        {
            PrepareOwnerState();
            decoyStates.Clear();

            PlayLaugh(transform.position);
            yield return FadeRenderers(ownerRenderers, ownerInitialColors, 0f, disappearFadeDuration);
            RestoreOwnerColliders(false);

            Transform[] clonePoints = GetClonePoints();
            int ownerPointIndex = Random.Range(0, clonePoints.Length);
            Transform ownerPoint = clonePoints[ownerPointIndex];

            transform.position = ownerPoint.position;
            for (int i = 0; i < clonePoints.Length; i++)
            {
                if (i == ownerPointIndex)
                    continue;

                CreateDecoy(clonePoints[i].position, i);
            }

            LookAtPlayer(transform);
            LookAtDecoysAtPlayer();

            yield return FadeAllIn();
            RestoreOwnerColliders(true);
            SetDecoyCollidersEnabled(true);

            SetMagicState(true, false);
            StartCastEffects();
            ownerOnibi = SpawnHeadOnibi(transform, "VLMitamaWeak_Onibi_Main");
            SpawnDecoyOnibis();

            yield return WaitPreMagic();
            if (IsDone)
                yield break;

            if (!CanContinue())
            {
                AbortAction();
                yield break;
            }

            SetMagicState(false, true);
            yield return WaitInterruptible(projectileLaunchDelay);
            if (IsDone)
                yield break;

            if (!CanContinue())
            {
                AbortAction();
                yield break;
            }

            if (TryGetPlayer(out Transform playerTransform))
            {
                ownerOnibi?.LaunchTowards(playerTransform.position, projectileSpeed, projectileLifetime);
                ownerOnibi = null;
            }

            StartFadeOutDecoyOnibis();
            StartCoroutine(FadeOutAllDecoys());

            yield return WaitInterruptible(magicDuration);
            if (IsDone)
                yield break;

            SetMagicState(false, false);
            yield return FadeOutCastEffects();
            yield return FadeRenderers(ownerRenderers, ownerInitialColors, 0f, cleanupFadeDuration);
            CompleteAction();
        }

        IEnumerator WaitPreMagic()
        {
            float elapsed = 0f;
            while (elapsed < preMagicDuration)
            {
                if (!CanContinue())
                {
                    AbortAction();
                    yield break;
                }

                yield return HandlePendingDecoyHits();

                elapsed += Time.deltaTime;
                yield return null;
            }
        }

        IEnumerator WaitInterruptible(float seconds)
        {
            if (seconds <= 0f)
                yield break;

            float elapsed = 0f;
            while (elapsed < seconds)
            {
                if (!CanContinue())
                {
                    AbortAction();
                    yield break;
                }

                yield return HandlePendingDecoyHits();

                elapsed += Time.deltaTime;
                yield return null;
            }
        }

        bool CanContinue()
        {
            if (!isActiveAndEnabled || !gameObject.activeInHierarchy)
                return false;

            if (ownerHealth != null && ownerHealth.IsDead)
                return false;

            return TryGetPlayer(out _);
        }

        void PrepareOwnerState()
        {
            ownerRenderers = GetComponentsInChildren<SpriteRenderer>(true);
            ownerInitialColors = CaptureColors(ownerRenderers);
            ownerColliders = GetComponentsInChildren<Collider2D>(true);
            ownerColliderStates = CaptureColliderStates(ownerColliders);
            ownerAnimator = GetComponent<Animator>();
            ownerHealth = GetComponent<Health>();
            SetRenderersAlpha(ownerRenderers, ownerInitialColors, 1f);
            RestoreOwnerColliders(true);
            SetMagicState(false, false);
        }

        Transform[] GetClonePoints()
        {
            if (middlePoint == null)
                return new[] { leftPoint, rightPoint };

            return new[] { leftPoint, middlePoint, rightPoint };
        }

        void CreateDecoy(Vector3 position, int pointIndex)
        {
            GameObject decoy = CreateDecoyObject(position);
            decoy.name = $"{gameObject.name}_WeakCloneDecoy_{pointIndex + 1}";
            decoy.transform.localScale = transform.localScale;

            ConfigureDecoyFromOwner(decoy);

            CloneDecoyState state = new CloneDecoyState
            {
                gameObject = decoy,
                animator = decoy.GetComponent<Animator>(),
                renderers = decoy.GetComponentsInChildren<SpriteRenderer>(true),
            };
            state.initialColors = BuildDecoyTargetColors(state.renderers);
            decoyStates.Add(state);

            VLMitamaCloneDecoyHitDetector hitDetector =
                decoy.AddComponent<VLMitamaCloneDecoyHitDetector>();
            hitDetector.Initialize(this, state);

            SetRenderersAlpha(state.renderers, state.initialColors, 0f);
            SetCollidersEnabled(decoy, false);
        }

        GameObject CreateDecoyObject(Vector3 position)
        {
            if (decoyPrefab != null)
            {
                return Instantiate(decoyPrefab, position, transform.rotation);
            }

            GameObject decoy = new GameObject();
            decoy.transform.SetPositionAndRotation(position, transform.rotation);
            decoy.AddComponent<SpriteRenderer>();
            decoy.AddComponent<Animator>();

            Rigidbody2D body = decoy.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.constraints = RigidbodyConstraints2D.FreezeAll;
            return decoy;
        }

        void ConfigureDecoyFromOwner(GameObject decoy)
        {
            CopyRootVisual(decoy);
            EnsureDecoyRigidbody(decoy);
            EnsureDecoyColliders(decoy);
        }

        void CopyRootVisual(GameObject decoy)
        {
            SpriteRenderer ownerRenderer = GetComponent<SpriteRenderer>();
            SpriteRenderer decoyRenderer = decoy.GetComponent<SpriteRenderer>();
            if (ownerRenderer != null && decoyRenderer == null)
                decoyRenderer = decoy.AddComponent<SpriteRenderer>();

            if (ownerRenderer != null && decoyRenderer != null)
            {
                decoyRenderer.sprite = ownerRenderer.sprite;
                decoyRenderer.color = ownerRenderer.color;
                decoyRenderer.flipX = ownerRenderer.flipX;
                decoyRenderer.flipY = ownerRenderer.flipY;
                decoyRenderer.drawMode = ownerRenderer.drawMode;
                decoyRenderer.size = ownerRenderer.size;
                decoyRenderer.tileMode = ownerRenderer.tileMode;
                decoyRenderer.maskInteraction = ownerRenderer.maskInteraction;
                decoyRenderer.spriteSortPoint = ownerRenderer.spriteSortPoint;
                decoyRenderer.sortingLayerID = ownerRenderer.sortingLayerID;
                decoyRenderer.sortingOrder = ownerRenderer.sortingOrder;
                decoyRenderer.sharedMaterial = ownerRenderer.sharedMaterial;
            }

            Animator decoyAnimator = decoy.GetComponent<Animator>();
            if (ownerAnimator != null && decoyAnimator == null)
                decoyAnimator = decoy.AddComponent<Animator>();

            if (ownerAnimator != null && decoyAnimator != null)
            {
                decoyAnimator.runtimeAnimatorController = ownerAnimator.runtimeAnimatorController;
                decoyAnimator.avatar = ownerAnimator.avatar;
                decoyAnimator.applyRootMotion = false;
                decoyAnimator.updateMode = ownerAnimator.updateMode;
                decoyAnimator.cullingMode = ownerAnimator.cullingMode;
            }
        }

        void EnsureDecoyRigidbody(GameObject decoy)
        {
            Rigidbody2D body = decoy.GetComponent<Rigidbody2D>();
            if (body == null)
                body = decoy.AddComponent<Rigidbody2D>();

            if (body != null)
            {
                body.velocity = Vector2.zero;
                body.angularVelocity = 0f;
                body.gravityScale = 0f;
                body.constraints = RigidbodyConstraints2D.FreezeAll;
            }
        }

        void EnsureDecoyColliders(GameObject decoy)
        {
            if (decoy.GetComponentsInChildren<Collider2D>(true).Length > 0)
                return;

            Collider2D[] sourceColliders = ownerColliders != null && ownerColliders.Length > 0
                ? ownerColliders
                : GetComponentsInChildren<Collider2D>(true);

            for (int i = 0; i < sourceColliders.Length; i++)
            {
                Collider2D source = sourceColliders[i];
                if (source == null)
                    continue;

                GameObject colliderTarget = source.transform == transform
                    ? decoy
                    : CreateDecoyColliderChild(decoy, source.transform);

                CopyCollider(source, colliderTarget);
            }
        }

        GameObject CreateDecoyColliderChild(GameObject decoy, Transform sourceTransform)
        {
            GameObject child = new GameObject(sourceTransform.name);
            child.layer = sourceTransform.gameObject.layer;
            child.tag = sourceTransform.gameObject.tag;
            child.transform.SetParent(decoy.transform, false);
            child.transform.localPosition = sourceTransform.localPosition;
            child.transform.localRotation = sourceTransform.localRotation;
            child.transform.localScale = sourceTransform.localScale;
            return child;
        }

        void CopyCollider(Collider2D source, GameObject target)
        {
            target.layer = source.gameObject.layer;
            target.tag = source.gameObject.tag;

            if (source is BoxCollider2D sourceBox)
            {
                BoxCollider2D collider = target.AddComponent<BoxCollider2D>();
                collider.offset = sourceBox.offset;
                collider.size = sourceBox.size;
                collider.edgeRadius = sourceBox.edgeRadius;
                collider.isTrigger = sourceBox.isTrigger;
                collider.enabled = sourceBox.enabled;
                return;
            }

            if (source is CapsuleCollider2D sourceCapsule)
            {
                CapsuleCollider2D collider = target.AddComponent<CapsuleCollider2D>();
                collider.offset = sourceCapsule.offset;
                collider.size = sourceCapsule.size;
                collider.direction = sourceCapsule.direction;
                collider.isTrigger = sourceCapsule.isTrigger;
                collider.enabled = sourceCapsule.enabled;
                return;
            }

            if (source is CircleCollider2D sourceCircle)
            {
                CircleCollider2D collider = target.AddComponent<CircleCollider2D>();
                collider.offset = sourceCircle.offset;
                collider.radius = sourceCircle.radius;
                collider.isTrigger = sourceCircle.isTrigger;
                collider.enabled = sourceCircle.enabled;
            }
        }

        IEnumerator FadeAllIn()
        {
            float duration = Mathf.Max(0f, appearFadeDuration);
            if (duration <= 0f)
            {
                SetRenderersAlpha(ownerRenderers, ownerInitialColors, 1f);
                SetDecoyRenderersAlpha(1f);
                yield break;
            }

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Clamp01(elapsed / duration);
                SetRenderersAlpha(ownerRenderers, ownerInitialColors, alpha);
                SetDecoyRenderersAlpha(alpha);
                yield return null;
            }

            SetRenderersAlpha(ownerRenderers, ownerInitialColors, 1f);
            SetDecoyRenderersAlpha(1f);
        }

        IEnumerator HandlePendingDecoyHits()
        {
            for (int i = 0; i < decoyStates.Count; i++)
            {
                CloneDecoyState state = decoyStates[i];
                if (state != null && state.wasHit && !state.hitHandled)
                    yield return HandleDecoyHit(state);
            }
        }

        IEnumerator HandleDecoyHit(CloneDecoyState state)
        {
            if (state == null || state.hitHandled)
                yield break;

            state.hitHandled = true;
            if (state.gameObject != null)
                PlayLaugh(state.gameObject.transform.position, decoyHitLaughVolume);

            if (state.onibi != null)
            {
                StartCoroutine(FadeOutAndDestroy(state.onibi.gameObject, cleanupFadeDuration));
                state.onibi = null;
            }

            if (state.castEffect != null)
            {
                GameObject effect = state.castEffect;
                state.castEffect = null;
                StartCoroutine(ParticleEffectFadeOut.FadeOutAndDestroy(
                    effect,
                    castEffectFadeOutDuration
                ));
            }

            yield return FadeOutDecoy(state);
        }

        IEnumerator FadeOutAllDecoys()
        {
            for (int i = 0; i < decoyStates.Count; i++)
            {
                CloneDecoyState state = decoyStates[i];
                if (state != null)
                    yield return FadeOutDecoy(state);
            }
        }

        IEnumerator FadeOutDecoy(CloneDecoyState state)
        {
            if (state == null || state.gameObject == null)
                yield break;

            state.hitHandled = true;
            SetCollidersEnabled(state.gameObject, false);
            yield return FadeRenderers(state.renderers, state.initialColors, 0f, cleanupFadeDuration);

            if (state.gameObject != null)
                Destroy(state.gameObject);

            state.gameObject = null;
            state.renderers = null;
            state.initialColors = null;
            state.animator = null;
        }

        OrbitingOnibiProjectile SpawnHeadOnibi(Transform owner, string objectName)
        {
            if (owner == null || projectileSprite == null)
                return null;

            SpriteRenderer ownerRenderer = owner.GetComponent<SpriteRenderer>();
            int sortingLayerId = ownerRenderer != null ? ownerRenderer.sortingLayerID : 0;
            int sortingOrder = ownerRenderer != null ? ownerRenderer.sortingOrder + 1 : 0;

            DamageTextV2 damageText = owner.GetComponentInChildren<DamageTextV2>(true);
            Canvas damageTextCanvas = damageText != null ? damageText.GetComponent<Canvas>() : null;
            if (damageTextCanvas != null)
            {
                sortingLayerId = damageTextCanvas.sortingLayerID;
                sortingOrder = damageTextCanvas.sortingOrder - 1;
            }

            OrbitingOnibiProjectile projectile = CreateOnibiProjectile(objectName);

            projectile.InitializeOrbit(
                owner,
                projectileSprite,
                projectileAnimatorController,
                projectileColor,
                projectileScale,
                projectileHeadOffset,
                projectileColliderSize,
                0f,
                0f,
                0f,
                projectileSelfRotationDegreesPerSecond,
                projectileDamage,
                targetTagName,
                sortingLayerId,
                sortingOrder
            );

            return projectile;
        }

        OrbitingOnibiProjectile CreateOnibiProjectile(string objectName)
        {
            OrbitingOnibiProjectile projectile = projectilePrefab != null
                ? Instantiate(projectilePrefab)
                : new GameObject(objectName).AddComponent<OrbitingOnibiProjectile>();

            projectile.gameObject.name = objectName;
            return projectile;
        }

        IEnumerator FadeOutAndDestroy(GameObject target, float duration)
        {
            if (target == null)
                yield break;

            SetCollidersEnabled(target, false);
            Shield shield = target.GetComponent<Shield>();
            if (shield != null)
                shield.enabled = false;

            SpriteRenderer[] renderers = target.GetComponentsInChildren<SpriteRenderer>(true);
            Color[] colors = CaptureColors(renderers);
            yield return FadeRenderers(renderers, colors, 0f, duration);

            if (target != null)
                Destroy(target);
        }

        void StartCastEffects()
        {
            ownerCastEffect = StartCastEffect(transform);
            for (int i = 0; i < decoyStates.Count; i++)
            {
                CloneDecoyState state = decoyStates[i];
                if (state != null && state.gameObject != null)
                    state.castEffect = StartCastEffect(state.gameObject.transform);
            }
        }

        GameObject StartCastEffect(Transform target)
        {
            if (castEffectPrefab == null || target == null)
                return null;

            GameObject effect = Instantiate(castEffectPrefab, target);
            effect.transform.localPosition = castEffectOffset;
            effect.transform.localRotation = Quaternion.identity;
            effect.transform.localScale = Vector3.one;
            return effect;
        }

        IEnumerator FadeOutCastEffects()
        {
            if (ownerCastEffect != null)
            {
                GameObject effect = ownerCastEffect;
                ownerCastEffect = null;
                yield return ParticleEffectFadeOut.FadeOutAndDestroy(effect, castEffectFadeOutDuration);
            }

            for (int i = 0; i < decoyStates.Count; i++)
            {
                CloneDecoyState state = decoyStates[i];
                if (state == null || state.castEffect == null)
                    continue;

                GameObject effect = state.castEffect;
                state.castEffect = null;
                yield return ParticleEffectFadeOut.FadeOutAndDestroy(effect, castEffectFadeOutDuration);
            }
        }

        void LookAtDecoysAtPlayer()
        {
            for (int i = 0; i < decoyStates.Count; i++)
            {
                CloneDecoyState state = decoyStates[i];
                if (state != null && state.gameObject != null)
                    LookAtPlayer(state.gameObject.transform);
            }
        }

        void LookAtPlayer(Transform target)
        {
            if (target == null || !TryGetPlayer(out Transform player))
                return;

            Vector3 scale = target.localScale;
            float scaleX = Mathf.Abs(scale.x);
            scale.x = player.position.x < target.position.x ? scaleX : -scaleX;
            target.localScale = scale;
        }

        bool TryGetPlayer(out Transform player)
        {
            if (cachedPlayerTransform != null && cachedPlayerTransform.gameObject.activeInHierarchy)
            {
                player = cachedPlayerTransform;
                return true;
            }

            GameObject playerObject = GameObject.FindWithTag(targetTagName);
            cachedPlayerTransform = playerObject != null ? playerObject.transform : null;
            player = cachedPlayerTransform;
            return player != null;
        }

        void SetMagicState(bool preMagic, bool magic)
        {
            SetAnimatorBool(ownerAnimator, preMagicBoolName, preMagic);
            SetAnimatorBool(ownerAnimator, magicBoolName, magic);

            for (int i = 0; i < decoyStates.Count; i++)
            {
                CloneDecoyState state = decoyStates[i];
                if (state == null)
                    continue;

                SetAnimatorBool(state.animator, preMagicBoolName, preMagic);
                SetAnimatorBool(state.animator, magicBoolName, magic);
            }
        }

        void SetAnimatorBool(Animator animator, string parameterName, bool value)
        {
            if (animator == null || string.IsNullOrEmpty(parameterName))
                return;

            animator.SetBool(parameterName, value);
        }

        void NotifyDecoyHit(CloneDecoyState state)
        {
            if (state == null || state.gameObject == null || state.hitHandled)
                return;

            state.wasHit = true;
        }

        void PlayLaugh(Vector3 position)
        {
            PlayLaugh(position, laughVolume);
        }

        void PlayLaugh(Vector3 position, float volume)
        {
            if (laughClip == null)
                return;

            GameObject soundObject = new GameObject("VLMitamaLaughSound");
            soundObject.transform.position = position;

            float pitch = Mathf.Max(0.01f, laughPitch);
            AudioSource audioSource = soundObject.AddComponent<AudioSource>();
            audioSource.clip = laughClip;
            audioSource.volume = volume;
            audioSource.pitch = pitch;
            audioSource.Play();

            float destroyDelay = laughClip.length / pitch;
            Destroy(soundObject, destroyDelay);
        }

        Color[] CaptureColors(SpriteRenderer[] renderers)
        {
            if (renderers == null)
                return null;

            Color[] colors = new Color[renderers.Length];
            for (int i = 0; i < renderers.Length; i++)
            {
                colors[i] = renderers[i] != null ? renderers[i].color : Color.white;
            }

            return colors;
        }

        Color[] BuildDecoyTargetColors(SpriteRenderer[] renderers)
        {
            if (renderers == null)
                return null;

            Color[] colors = new Color[renderers.Length];
            for (int i = 0; i < renderers.Length; i++)
            {
                if (ownerInitialColors != null && i < ownerInitialColors.Length)
                {
                    colors[i] = ownerInitialColors[i];
                }
                else
                {
                    colors[i] = renderers[i] != null ? renderers[i].color : Color.white;
                    colors[i].a = 1f;
                }
            }

            return colors;
        }

        bool[] CaptureColliderStates(Collider2D[] colliders)
        {
            if (colliders == null)
                return null;

            bool[] states = new bool[colliders.Length];
            for (int i = 0; i < colliders.Length; i++)
            {
                states[i] = colliders[i] != null && colliders[i].enabled;
            }

            return states;
        }

        IEnumerator FadeRenderers(
            SpriteRenderer[] renderers,
            Color[] baseColors,
            float targetAlphaRate,
            float duration
        )
        {
            if (renderers == null || renderers.Length == 0)
                yield break;

            float clampedDuration = Mathf.Max(0f, duration);
            Color[] startColors = CaptureColors(renderers);
            if (clampedDuration <= 0f)
            {
                SetRenderersAlpha(renderers, baseColors, targetAlphaRate);
                yield break;
            }

            float elapsed = 0f;
            while (elapsed < clampedDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / clampedDuration);
                for (int i = 0; i < renderers.Length; i++)
                {
                    SpriteRenderer renderer = renderers[i];
                    if (renderer == null)
                        continue;

                    Color baseColor = GetBaseColor(baseColors, i, renderer.color);
                    Color startColor = GetBaseColor(startColors, i, renderer.color);
                    Color color = baseColor;
                    color.a = Mathf.Lerp(startColor.a, baseColor.a * targetAlphaRate, t);
                    renderer.color = color;
                }

                yield return null;
            }

            SetRenderersAlpha(renderers, baseColors, targetAlphaRate);
        }

        void SetRenderersAlpha(SpriteRenderer[] renderers, Color[] baseColors, float alphaRate)
        {
            if (renderers == null)
                return;

            for (int i = 0; i < renderers.Length; i++)
            {
                SpriteRenderer renderer = renderers[i];
                if (renderer == null)
                    continue;

                Color color = GetBaseColor(baseColors, i, renderer.color);
                color.a *= alphaRate;
                renderer.color = color;
            }
        }

        Color GetBaseColor(Color[] baseColors, int index, Color fallback)
        {
            if (baseColors == null || index < 0 || index >= baseColors.Length)
                return fallback;

            return baseColors[index];
        }

        void RestoreOwnerColliders(bool restore)
        {
            if (ownerColliders == null)
                return;

            for (int i = 0; i < ownerColliders.Length; i++)
            {
                Collider2D ownerCollider = ownerColliders[i];
                if (ownerCollider == null)
                    continue;

                ownerCollider.enabled = restore
                    ? ownerColliderStates == null
                        || i >= ownerColliderStates.Length
                        || ownerColliderStates[i]
                    : false;
            }
        }

        void SetCollidersEnabled(GameObject target, bool value)
        {
            if (target == null)
                return;

            Collider2D[] colliders = target.GetComponentsInChildren<Collider2D>(true);
            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i] != null)
                    colliders[i].enabled = value;
            }
        }

        void SetDecoyCollidersEnabled(bool value)
        {
            for (int i = 0; i < decoyStates.Count; i++)
            {
                CloneDecoyState state = decoyStates[i];
                if (state != null)
                    SetCollidersEnabled(state.gameObject, value);
            }
        }

        void SetDecoyRenderersAlpha(float alphaRate)
        {
            for (int i = 0; i < decoyStates.Count; i++)
            {
                CloneDecoyState state = decoyStates[i];
                if (state != null)
                    SetRenderersAlpha(state.renderers, state.initialColors, alphaRate);
            }
        }

        void SpawnDecoyOnibis()
        {
            for (int i = 0; i < decoyStates.Count; i++)
            {
                CloneDecoyState state = decoyStates[i];
                if (state == null || state.gameObject == null)
                    continue;

                state.onibi = SpawnHeadOnibi(
                    state.gameObject.transform,
                    $"VLMitamaWeak_Onibi_Decoy_{i + 1}"
                );
            }
        }

        void StartFadeOutDecoyOnibis()
        {
            for (int i = 0; i < decoyStates.Count; i++)
            {
                CloneDecoyState state = decoyStates[i];
                if (state == null || state.onibi == null)
                    continue;

                StartCoroutine(FadeOutAndDestroy(state.onibi.gameObject, cleanupFadeDuration));
                state.onibi = null;
            }
        }

        void CompleteAction()
        {
            actionRoutine = null;
            RestoreOwnerForNextAction();
            IsExecuting = false;
            IsDone = true;
        }

        void AbortAction()
        {
            CleanupImmediate(false);
            IsExecuting = false;
            IsDone = true;
        }

        void CleanupImmediate(bool stopRoutine = true)
        {
            if (stopRoutine && actionRoutine != null)
            {
                StopCoroutine(actionRoutine);
            }
            actionRoutine = null;

            if (ownerOnibi != null)
            {
                Destroy(ownerOnibi.gameObject);
                ownerOnibi = null;
            }

            if (ownerCastEffect != null)
            {
                Destroy(ownerCastEffect);
                ownerCastEffect = null;
            }

            for (int i = 0; i < decoyStates.Count; i++)
            {
                CloneDecoyState state = decoyStates[i];
                if (state == null)
                    continue;

                if (state.onibi != null)
                {
                    Destroy(state.onibi.gameObject);
                    state.onibi = null;
                }

                if (state.castEffect != null)
                {
                    Destroy(state.castEffect);
                    state.castEffect = null;
                }

                if (state.gameObject != null)
                {
                    Destroy(state.gameObject);
                    state.gameObject = null;
                }
            }

            RestoreOwnerForNextAction();
            decoyStates.Clear();
        }

        void RestoreOwnerForNextAction()
        {
            SetMagicState(false, false);

            if (ownerRenderers != null)
                SetRenderersAlpha(ownerRenderers, ownerInitialColors, 1f);

            RestoreOwnerColliders(true);
        }

        class CloneDecoyState
        {
            public GameObject gameObject = null;
            public SpriteRenderer[] renderers = null;
            public Color[] initialColors = null;
            public Animator animator = null;
            public GameObject castEffect = null;
            public OrbitingOnibiProjectile onibi = null;
            public bool wasHit = false;
            public bool hitHandled = false;
        }

        class VLMitamaCloneDecoyHitDetector : MonoBehaviour
        {
            VLMitamaCloneWeakAction owner = null;
            CloneDecoyState state = null;
            bool isHit = false;

            public void Initialize(VLMitamaCloneWeakAction action, CloneDecoyState decoyState)
            {
                owner = action;
                state = decoyState;
            }

            void OnTriggerEnter2D(Collider2D other)
            {
                TryNotifyHit(other);
            }

            void OnTriggerStay2D(Collider2D other)
            {
                TryNotifyHit(other);
            }

            void TryNotifyHit(Collider2D other)
            {
                if (isHit || other == null)
                    return;

                if (!IsPlayerAttack(other))
                    return;

                isHit = true;
                owner?.NotifyDecoyHit(state);
            }

            bool IsPlayerAttack(Collider2D other)
            {
                if (other.CompareTag("Projectile"))
                    return true;

                if (other.GetComponent<DirectAttack>() != null)
                    return true;

                return other.GetComponentInParent<DirectAttack>() != null;
            }
        }
    }
}
