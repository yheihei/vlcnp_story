using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VLCNP.Attributes;
using VLCNP.Effects;

namespace VLCNP.Combat.EnemyAction
{
    public class OnibiShieldAction : EnemyAction
    {
        [SerializeField]
        Sprite projectileSprite = null;

        [SerializeField]
        RuntimeAnimatorController projectileAnimatorController = null;

        [SerializeField]
        OrbitingOnibiProjectile projectilePrefab = null;

        [SerializeField]
        Color projectileColor = Color.white;

        [SerializeField]
        Vector3 projectileScale = new Vector3(1.2f, 1.2f, 1f);

        [SerializeField]
        Vector2 orbitCenterOffset = new Vector2(0f, 0.5f);

        [SerializeField]
        Vector2 projectileColliderSize = new Vector2(0.28f, 0.28f);

        [SerializeField]
        int projectileCount = 6;

        [SerializeField]
        float orbitRadius = 1.1f;

        [SerializeField]
        float orbitDegreesPerSecond = 240f;

        [SerializeField]
        float orbitWarmupDuration = 2.4f;

        [SerializeField]
        float spawnDelayAfterCastStart = 1f;

        [SerializeField]
        float orbitRadiusExpandDuration = 1f;

        [SerializeField]
        float projectileLaunchDelay = 0.5f;

        [SerializeField]
        float launchInterval = 0.2f;

        [SerializeField]
        float postLaunchDelay = 0f;

        [SerializeField]
        float projectileSpeed = 6f;

        [SerializeField]
        float projectileLifetime = 4f;

        [SerializeField]
        float projectileDamage = 1f;

        [SerializeField]
        float selfRotationDegreesPerSecond = 0f;

        [SerializeField]
        string targetTagName = "Player";

        [SerializeField]
        GameObject castEffectPrefab = null;

        [SerializeField]
        Vector3 castEffectOffset = new Vector3(0f, 0.5f, 0f);

        [SerializeField]
        float castEffectFadeOutDuration = 0.5f;

        [SerializeField]
        AudioClip onibiSpawnSe = null;

        [SerializeField]
        float onibiSpawnSeVolume = 1f;

        [SerializeField]
        AudioClip onibiLaunchSe = null;

        [SerializeField]
        float onibiLaunchSeVolume = 1f;

        Coroutine actionRoutine = null;
        readonly List<OrbitingOnibiProjectile> activeProjectiles = new List<OrbitingOnibiProjectile>();
        SpriteRenderer ownerSpriteRenderer = null;
        Health ownerHealth = null;
        Animator animator = null;
        GameObject activeCastEffect = null;
        Transform cachedPlayerTransform = null;
        AudioSource audioSource = null;

        const string PreMagicParameterName = "isPreMagic";
        const string MagicParameterName = "isMagic";

        void Awake()
        {
            ownerSpriteRenderer = GetComponent<SpriteRenderer>();
            ownerHealth = GetComponent<Health>();
            animator = GetComponent<Animator>();
            audioSource = GetComponent<AudioSource>();
        }

        void OnDisable()
        {
            CleanupAndComplete(false);
        }

        void OnDestroy()
        {
            CleanupProjectiles();
            CleanupCastEffect(false);
        }

        public override void Execute()
        {
            if (IsExecuting || IsDone)
                return;

            if (projectileSprite == null)
            {
                Debug.LogWarning($"OnibiShieldAction: projectileSprite is not set on {gameObject.name}");
                IsDone = true;
                return;
            }

            if (projectileCount <= 0)
            {
                IsDone = true;
                return;
            }

            IsExecuting = true;
            actionRoutine = StartCoroutine(ExecuteRoutine());
        }

        public override void Stop()
        {
            CleanupAndComplete();
        }

        IEnumerator ExecuteRoutine()
        {
            if (!TryGetPlayer(out Transform playerTransform))
            {
                CleanupAndComplete();
                yield break;
            }

            FaceTarget(playerTransform.position.x);
            SetPreMagic(true);
            StartCastEffect();

            yield return WaitWhileCanContinue(spawnDelayAfterCastStart);
            if (!CanContinue())
            {
                CleanupAndComplete();
                yield break;
            }

            SpawnProjectiles();

            yield return WaitWhileCanContinue(orbitWarmupDuration);
            if (!CanContinue())
            {
                CleanupAndComplete();
                yield break;
            }

            SetPreMagic(false);
            SetMagic(true);

            yield return WaitWhileCanContinue(projectileLaunchDelay);
            if (!CanContinue())
            {
                CleanupAndComplete();
                yield break;
            }

            bool hasLaunchedProjectile = false;
            for (int i = 0; i < activeProjectiles.Count; i++)
            {
                if (!CanContinue())
                {
                    CleanupAndComplete();
                    yield break;
                }

                OrbitingOnibiProjectile projectile = activeProjectiles[i];
                if (projectile == null)
                    continue;

                if (hasLaunchedProjectile && launchInterval > 0f)
                    yield return new WaitForSeconds(launchInterval);

                if (!CanContinue())
                {
                    CleanupAndComplete();
                    yield break;
                }

                if (!TryGetPlayer(out playerTransform))
                {
                    CleanupAndComplete();
                    yield break;
                }

                FaceTarget(playerTransform.position.x);
                PlayOnibiLaunchSe();
                projectile.LaunchTowards(playerTransform.position, projectileSpeed, projectileLifetime);
                hasLaunchedProjectile = true;
            }

            SetMagic(false);

            float clampedPostLaunchDelay = Mathf.Max(0f, postLaunchDelay);
            float postLaunchElapsed = 0f;
            while (postLaunchElapsed < clampedPostLaunchDelay)
            {
                if (!CanContinue())
                {
                    CleanupAndComplete();
                    yield break;
                }

                postLaunchElapsed += Time.deltaTime;
                yield return null;
            }

            CleanupCastEffect(true);
            CompleteAction();
        }

        void SpawnProjectiles()
        {
            CleanupProjectiles();

            int sortingLayerId = ownerSpriteRenderer != null ? ownerSpriteRenderer.sortingLayerID : 0;
            int sortingOrder = ownerSpriteRenderer != null ? ownerSpriteRenderer.sortingOrder + 1 : 0;

            for (int i = 0; i < projectileCount; i++)
            {
                OrbitingOnibiProjectile projectile = CreateProjectile($"Onibi_{i + 1}");

                projectile.InitializeOrbit(
                    transform,
                    projectileSprite,
                    projectileAnimatorController,
                    projectileColor,
                    projectileScale,
                    orbitCenterOffset,
                    projectileColliderSize,
                    orbitRadius,
                    i * (360f / projectileCount),
                    orbitDegreesPerSecond,
                    selfRotationDegreesPerSecond,
                    projectileDamage,
                    targetTagName,
                    sortingLayerId,
                    sortingOrder
                );

                projectile.StartOrbitRadiusExpansion(0f, orbitRadiusExpandDuration);
                activeProjectiles.Add(projectile);
            }

            PlayOnibiSpawnSe();
        }

        OrbitingOnibiProjectile CreateProjectile(string objectName)
        {
            OrbitingOnibiProjectile projectile = projectilePrefab != null
                ? Instantiate(projectilePrefab)
                : new GameObject(objectName).AddComponent<OrbitingOnibiProjectile>();

            projectile.gameObject.name = objectName;
            return projectile;
        }

        IEnumerator WaitWhileCanContinue(float seconds)
        {
            float clampedSeconds = Mathf.Max(0f, seconds);
            float elapsed = 0f;
            while (elapsed < clampedSeconds)
            {
                if (!CanContinue())
                    yield break;

                elapsed += Time.deltaTime;
                yield return null;
            }
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

        bool CanContinue()
        {
            if (!isActiveAndEnabled || !gameObject.activeInHierarchy)
                return false;

            if (ownerHealth != null && ownerHealth.IsDead)
                return false;

            return TryGetPlayer(out _);
        }

        void FaceTarget(float targetX)
        {
            Vector3 scale = transform.localScale;
            scale.x = targetX < transform.position.x ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
            transform.localScale = scale;
        }

        void CleanupProjectiles()
        {
            for (int i = 0; i < activeProjectiles.Count; i++)
            {
                if (activeProjectiles[i] != null)
                {
                    Destroy(activeProjectiles[i].gameObject);
                }
            }

            activeProjectiles.Clear();
        }

        void StartCastEffect()
        {
            CleanupCastEffect(false);

            if (castEffectPrefab == null)
                return;

            activeCastEffect = Instantiate(castEffectPrefab, transform);
            activeCastEffect.transform.localPosition = castEffectOffset;
            activeCastEffect.transform.localRotation = Quaternion.identity;
            activeCastEffect.transform.localScale = Vector3.one;
        }

        void CleanupCastEffect(bool fadeOut)
        {
            if (activeCastEffect == null)
                return;

            GameObject effect = activeCastEffect;
            activeCastEffect = null;

            if (fadeOut && isActiveAndEnabled && gameObject.activeInHierarchy)
                StartCoroutine(ParticleEffectFadeOut.FadeOutAndDestroy(effect, castEffectFadeOutDuration));
            else
                Destroy(effect);
        }

        void CleanupAndComplete(bool fadeOutCastEffect = true)
        {
            if (actionRoutine != null)
            {
                StopCoroutine(actionRoutine);
                actionRoutine = null;
            }

            ResetMagicStates();
            CleanupProjectiles();
            CleanupCastEffect(fadeOutCastEffect);
            IsExecuting = false;
            IsDone = true;
        }

        void CompleteAction()
        {
            actionRoutine = null;
            ResetMagicStates();
            IsExecuting = false;
            IsDone = true;
        }

        void SetPreMagic(bool value)
        {
            animator?.SetBool(PreMagicParameterName, value);
        }

        void SetMagic(bool value)
        {
            animator?.SetBool(MagicParameterName, value);
        }

        void PlayOnibiSpawnSe()
        {
            PlaySe(onibiSpawnSe, onibiSpawnSeVolume);
        }

        void PlayOnibiLaunchSe()
        {
            PlaySe(onibiLaunchSe, onibiLaunchSeVolume);
        }

        void PlaySe(AudioClip clip, float volume)
        {
            if (clip == null)
                return;

            audioSource?.PlayOneShot(clip, volume);
        }

        void ResetMagicStates()
        {
            SetPreMagic(false);
            SetMagic(false);
        }
    }
}
