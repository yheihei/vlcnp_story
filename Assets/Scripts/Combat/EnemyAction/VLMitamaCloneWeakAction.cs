using System.Collections;
using UnityEngine;
using VLCNP.Attributes;
using VLCNP.Combat;
using VLCNP.Effects;

namespace VLCNP.Combat.EnemyAction
{
    public class VLMitamaCloneWeakAction : EnemyAction
    {
        [SerializeField]
        Transform leftPoint = null;

        [SerializeField]
        Transform rightPoint = null;

        [SerializeField]
        float disappearFadeDuration = 0.4f;

        [SerializeField]
        float appearFadeDuration = 0.4f;

        [SerializeField]
        float preMagicDuration = 2f;

        [SerializeField]
        float magicDuration = 0.2f;

        [SerializeField]
        float cleanupFadeDuration = 0.3f;

        [SerializeField]
        AudioClip laughClip = null;

        [SerializeField]
        float laughVolume = 1f;

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
        AudioClip generatedLaughClip = null;

        bool ownerWasHit = false;
        bool decoyWasHit = false;
        bool decoyHitHandled = false;
        bool ownerDamageSubscribed = false;

        GameObject decoy = null;
        SpriteRenderer[] decoyRenderers = null;
        Color[] decoyInitialColors = null;
        Animator decoyAnimator = null;
        GameObject ownerCastEffect = null;
        GameObject decoyCastEffect = null;
        OrbitingOnibiProjectile ownerOnibi = null;
        OrbitingOnibiProjectile decoyOnibi = null;

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
            SubscribeOwnerDamage();
            ownerWasHit = false;
            decoyWasHit = false;
            decoyHitHandled = false;

            PlayLaugh(transform.position);
            yield return FadeRenderers(ownerRenderers, ownerInitialColors, 0f, disappearFadeDuration);
            RestoreOwnerColliders(false);

            bool ownerOnLeft = Random.value < 0.5f;
            Transform ownerPoint = ownerOnLeft ? leftPoint : rightPoint;
            Transform decoyPoint = ownerOnLeft ? rightPoint : leftPoint;

            transform.position = ownerPoint.position;
            CreateDecoy(decoyPoint.position);
            LookAtPlayer(transform);
            LookAtPlayer(decoy != null ? decoy.transform : null);

            yield return FadeBothIn();
            RestoreOwnerColliders(true);
            SetCollidersEnabled(decoy, true);

            SetMagicState(true, false);
            StartCastEffects();
            ownerOnibi = SpawnHeadOnibi(transform, "VLMitamaWeak_Onibi_Main");
            decoyOnibi = SpawnHeadOnibi(decoy != null ? decoy.transform : null, "VLMitamaWeak_Onibi_Decoy");

            yield return WaitPreMagic();
            if (IsDone)
                yield break;

            if (!CanContinue())
            {
                AbortAction();
                yield break;
            }

            SetMagicState(false, true);
            if (TryGetPlayer(out GameObject player))
            {
                ownerOnibi?.LaunchTowards(player.transform.position, projectileSpeed, projectileLifetime);
                ownerOnibi = null;
            }

            if (decoyOnibi != null)
            {
                StartCoroutine(FadeOutAndDestroy(decoyOnibi.gameObject, cleanupFadeDuration));
                decoyOnibi = null;
            }

            StartCoroutine(FadeOutDecoy());

            yield return WaitInterruptible(magicDuration);
            if (IsDone)
                yield break;

            if (ownerWasHit)
            {
                yield return CompleteWithoutProjectile();
                yield break;
            }

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
                if (ownerWasHit)
                {
                    yield return CompleteWithoutProjectile();
                    yield break;
                }

                if (!CanContinue())
                {
                    AbortAction();
                    yield break;
                }

                if (decoyWasHit && !decoyHitHandled)
                    yield return HandleDecoyHit();

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
                if (ownerWasHit)
                    yield break;

                if (!CanContinue())
                {
                    AbortAction();
                    yield break;
                }

                if (decoyWasHit && !decoyHitHandled)
                    yield return HandleDecoyHit();

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

        IEnumerator CompleteWithoutProjectile()
        {
            SetMagicState(false, false);
            if (ownerOnibi != null)
            {
                yield return FadeOutAndDestroy(ownerOnibi.gameObject, cleanupFadeDuration);
                ownerOnibi = null;
            }

            if (decoyOnibi != null)
            {
                yield return FadeOutAndDestroy(decoyOnibi.gameObject, cleanupFadeDuration);
                decoyOnibi = null;
            }

            yield return FadeOutCastEffects();
            yield return FadeOutDecoy();
            yield return FadeRenderers(ownerRenderers, ownerInitialColors, 0f, cleanupFadeDuration);
            CompleteAction();
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

        void CreateDecoy(Vector3 position)
        {
            decoy = Instantiate(gameObject, position, transform.rotation);
            decoy.name = $"{gameObject.name}_WeakCloneDecoy";
            decoy.transform.localScale = transform.localScale;

            DisableDecoyBehaviours(decoy);
            RemoveDecoyDamageReceivers(decoy);
            VLMitamaCloneDecoyHitDetector hitDetector =
                decoy.AddComponent<VLMitamaCloneDecoyHitDetector>();
            hitDetector.Initialize(this);

            decoyAnimator = decoy.GetComponent<Animator>();
            decoyRenderers = decoy.GetComponentsInChildren<SpriteRenderer>(true);
            decoyInitialColors = BuildDecoyTargetColors(decoyRenderers);
            SetRenderersAlpha(decoyRenderers, decoyInitialColors, 0f);
            SetCollidersEnabled(decoy, false);
        }

        void DisableDecoyBehaviours(GameObject target)
        {
            EnemyV2Controller controller = target.GetComponent<EnemyV2Controller>();
            if (controller != null)
                controller.enabled = false;

            EnemyAction[] actions = target.GetComponents<EnemyAction>();
            for (int i = 0; i < actions.Length; i++)
            {
                if (actions[i] != null)
                    actions[i].enabled = false;
            }

            MonoBehaviour[] behaviours = target.GetComponents<MonoBehaviour>();
            for (int i = 0; i < behaviours.Length; i++)
            {
                MonoBehaviour behaviour = behaviours[i];
                if (behaviour == null)
                    continue;

                string namespaceName = behaviour.GetType().Namespace;
                if (namespaceName == "VLCNP.Pickups")
                    behaviour.enabled = false;
            }

            Rigidbody2D body = target.GetComponent<Rigidbody2D>();
            if (body != null)
            {
                body.velocity = Vector2.zero;
                body.angularVelocity = 0f;
                body.gravityScale = 0f;
            }
        }

        void RemoveDecoyDamageReceivers(GameObject target)
        {
            if (target == null)
                return;

            Health health = target.GetComponent<Health>();
            if (health != null)
                Destroy(health);

            TakeDamageSe takeDamageSe = target.GetComponent<TakeDamageSe>();
            if (takeDamageSe != null)
                Destroy(takeDamageSe);

            DamageStun damageStun = target.GetComponent<DamageStun>();
            if (damageStun != null)
                Destroy(damageStun);
        }

        IEnumerator FadeBothIn()
        {
            float duration = Mathf.Max(0f, appearFadeDuration);
            if (duration <= 0f)
            {
                SetRenderersAlpha(ownerRenderers, ownerInitialColors, 1f);
                SetRenderersAlpha(decoyRenderers, decoyInitialColors, 1f);
                yield break;
            }

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Clamp01(elapsed / duration);
                SetRenderersAlpha(ownerRenderers, ownerInitialColors, alpha);
                SetRenderersAlpha(decoyRenderers, decoyInitialColors, alpha);
                yield return null;
            }

            SetRenderersAlpha(ownerRenderers, ownerInitialColors, 1f);
            SetRenderersAlpha(decoyRenderers, decoyInitialColors, 1f);
        }

        IEnumerator HandleDecoyHit()
        {
            decoyHitHandled = true;
            if (decoy != null)
                PlayLaugh(decoy.transform.position);

            if (decoyOnibi != null)
            {
                StartCoroutine(FadeOutAndDestroy(decoyOnibi.gameObject, cleanupFadeDuration));
                decoyOnibi = null;
            }

            if (decoyCastEffect != null)
            {
                GameObject effect = decoyCastEffect;
                decoyCastEffect = null;
                StartCoroutine(ParticleEffectFadeOut.FadeOutAndDestroy(
                    effect,
                    castEffectFadeOutDuration
                ));
            }

            yield return FadeOutDecoy();
        }

        IEnumerator FadeOutDecoy()
        {
            if (decoy == null)
                yield break;

            SetCollidersEnabled(decoy, false);
            yield return FadeRenderers(decoyRenderers, decoyInitialColors, 0f, cleanupFadeDuration);

            if (decoy != null)
                Destroy(decoy);

            decoy = null;
            decoyRenderers = null;
            decoyInitialColors = null;
            decoyAnimator = null;
        }

        OrbitingOnibiProjectile SpawnHeadOnibi(Transform owner, string objectName)
        {
            if (owner == null || projectileSprite == null)
                return null;

            SpriteRenderer ownerRenderer = owner.GetComponent<SpriteRenderer>();
            int sortingLayerId = ownerRenderer != null ? ownerRenderer.sortingLayerID : 0;
            int sortingOrder = ownerRenderer != null ? ownerRenderer.sortingOrder + 1 : 0;

            GameObject projectileObject = new GameObject(objectName);
            OrbitingOnibiProjectile projectile =
                projectileObject.AddComponent<OrbitingOnibiProjectile>();

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
            decoyCastEffect = StartCastEffect(decoy != null ? decoy.transform : null);
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

            if (decoyCastEffect != null)
            {
                GameObject effect = decoyCastEffect;
                decoyCastEffect = null;
                yield return ParticleEffectFadeOut.FadeOutAndDestroy(effect, castEffectFadeOutDuration);
            }
        }

        void LookAtPlayer(Transform target)
        {
            if (target == null || !TryGetPlayer(out GameObject player))
                return;

            Vector3 scale = target.localScale;
            float scaleX = Mathf.Abs(scale.x);
            scale.x = player.transform.position.x < target.position.x ? scaleX : -scaleX;
            target.localScale = scale;
        }

        bool TryGetPlayer(out GameObject player)
        {
            player = GameObject.FindWithTag(targetTagName);
            return player != null;
        }

        void SetMagicState(bool preMagic, bool magic)
        {
            SetAnimatorBool(ownerAnimator, preMagicBoolName, preMagic);
            SetAnimatorBool(ownerAnimator, magicBoolName, magic);
            SetAnimatorBool(decoyAnimator, preMagicBoolName, preMagic);
            SetAnimatorBool(decoyAnimator, magicBoolName, magic);
        }

        void SetAnimatorBool(Animator animator, string parameterName, bool value)
        {
            if (animator == null || string.IsNullOrEmpty(parameterName))
                return;

            animator.SetBool(parameterName, value);
        }

        void SubscribeOwnerDamage()
        {
            if (ownerHealth == null || ownerDamageSubscribed)
                return;

            ownerHealth.takeDamage.AddListener(OnOwnerTakeDamage);
            ownerDamageSubscribed = true;
        }

        void UnsubscribeOwnerDamage()
        {
            if (ownerHealth == null || !ownerDamageSubscribed)
                return;

            ownerHealth.takeDamage.RemoveListener(OnOwnerTakeDamage);
            ownerDamageSubscribed = false;
        }

        void OnOwnerTakeDamage(float damage)
        {
            if (damage > 0f)
                ownerWasHit = true;
        }

        void NotifyDecoyHit()
        {
            if (decoy == null || decoyHitHandled)
                return;

            decoyWasHit = true;
        }

        void PlayLaugh(Vector3 position)
        {
            AudioClip clip = laughClip != null ? laughClip : GetGeneratedLaughClip();
            if (clip != null)
                AudioSource.PlayClipAtPoint(clip, position, laughVolume);
        }

        AudioClip GetGeneratedLaughClip()
        {
            if (generatedLaughClip != null)
                return generatedLaughClip;

            const int frequency = 44100;
            const float duration = 0.35f;
            int sampleCount = Mathf.CeilToInt(frequency * duration);
            float[] samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / frequency;
                float envelope = Mathf.Sin(Mathf.Clamp01(t / duration) * Mathf.PI);
                float wave =
                    Mathf.Sin(2f * Mathf.PI * 720f * t)
                    + 0.45f * Mathf.Sin(2f * Mathf.PI * 980f * t);
                samples[i] = wave * envelope * 0.18f;
            }

            generatedLaughClip = AudioClip.Create(
                "VLMitamaBoss_TempLaugh",
                sampleCount,
                1,
                frequency,
                false
            );
            generatedLaughClip.SetData(samples, 0);
            return generatedLaughClip;
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

            if (decoyOnibi != null)
            {
                Destroy(decoyOnibi.gameObject);
                decoyOnibi = null;
            }

            if (ownerCastEffect != null)
            {
                Destroy(ownerCastEffect);
                ownerCastEffect = null;
            }

            if (decoyCastEffect != null)
            {
                Destroy(decoyCastEffect);
                decoyCastEffect = null;
            }

            if (decoy != null)
            {
                Destroy(decoy);
                decoy = null;
            }

            RestoreOwnerForNextAction();
        }

        void RestoreOwnerForNextAction()
        {
            UnsubscribeOwnerDamage();
            SetMagicState(false, false);

            if (ownerRenderers != null)
                SetRenderersAlpha(ownerRenderers, ownerInitialColors, 1f);

            RestoreOwnerColliders(true);
            ownerWasHit = false;
            decoyWasHit = false;
            decoyHitHandled = false;
        }

        class VLMitamaCloneDecoyHitDetector : MonoBehaviour
        {
            VLMitamaCloneWeakAction owner = null;
            bool isHit = false;

            public void Initialize(VLMitamaCloneWeakAction action)
            {
                owner = action;
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
                owner?.NotifyDecoyHit();
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
