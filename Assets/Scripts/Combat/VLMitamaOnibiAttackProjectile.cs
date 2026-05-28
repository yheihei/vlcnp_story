using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using VLCNP.Attributes;
using VLCNP.Control;
using VLCNP.Core;

namespace VLCNP.Combat
{
    public class VLMitamaOnibiAttackProjectile :
        MonoBehaviour,
        IProjectile,
        IProjectileOwnerReceiver,
        IProjectileLevelReceiver,
        IProjectileLaunchGate,
        IStoppable
    {
        [SerializeField]
        Sprite[] onibiSprites = null;

        [SerializeField]
        Color onibiColor = Color.white;

        [SerializeField]
        int onibiCount = 3;

        [SerializeField]
        Vector3 onibiScale = new Vector3(0.48f, 0.48f, 1f);

        [SerializeField]
        Vector2 onibiColliderSize = new Vector2(0.2f, 0.4f);

        [SerializeField]
        Vector2 orbitCenterOffset = new Vector2(0f, -0.3f);

        [SerializeField]
        float orbitRadius = 0.95f;

        [SerializeField]
        float orbitDuration = 1.6f;

        [SerializeField]
        float orbitHitInterval = 0.2f;

        [SerializeField]
        float orbitDegreesPerSecond = 360f;

        [SerializeField]
        float launchInterval = 0.12f;

        [SerializeField]
        float launchSpeed = 18f;

        [SerializeField]
        float lifetimeAfterLaunch = 3f;

        [SerializeField]
        float animationFrameDuration = 0.12f;

        [SerializeField]
        float spawnFadeDuration = 0.1f;

        [SerializeField]
        float hitFadeDuration = 0.15f;

        [SerializeField]
        string targetTagName = "Enemy";

        [SerializeField]
        string onibiSortingLayerName = "PlayerUpperObject";

        [SerializeField]
        int onibiSortingOrder = 0;

        [SerializeField]
        Sprite aimArrowSprite = null;

        [SerializeField]
        float aimArrowRadius = 2.2f;

        [SerializeField]
        Vector3 aimArrowScale = new Vector3(0.7f, 0.7f, 1f);

        [SerializeField]
        float aimArrowMoveDegreesPerSecond = 360f;

        [SerializeField]
        string aimArrowSortingLayerName = "PlayerUpperObject";

        [SerializeField]
        int aimArrowSortingOrder = 610;

        [SerializeField]
        string aimLockAttackButton = "x";

        [SerializeField]
        float aimArrowBlinkInterval = 0.12f;

        [SerializeField]
        float aimArrowBlinkMinAlpha = 0.35f;

        [SerializeField]
        float aimConvergenceDistance = 10f;

        [SerializeField]
        AudioClip onibiLaunchSe = null;

        [SerializeField]
        float onibiLaunchSeVolume = 0.2f;

        [SerializeField]
        UnityEvent<GameObject> onTargetHit = new UnityEvent<GameObject>();

        static readonly HashSet<int> activeOwnerIds = new HashSet<int>();

        readonly List<OnibiState> onibis = new List<OnibiState>();
        GameObject owner = null;
        Transform ownerTransform = null;
        IAttackAnimationController ownerAttackAnimationController = null;
        AudioSource launchAudioSource = null;
        Transform aimArrowTransform = null;
        SpriteRenderer aimArrowRenderer = null;
        Vector2 aimDirection = Vector2.right;
        float aimAngleDegrees = 0f;
        bool isAimArrowLocked = false;
        int aimArrowSpawnFrame = -1;
        bool isLeft = false;
        bool ownerRegistered = false;
        bool isStucking = false;
        bool isImpacting = false;
        bool isStopped = false;
        bool hasReceivedLevel = false;
        int attackLevel = 1;
        float damage = 1f;

        public bool IsStucking => isStucking;
        public bool IsStopped
        {
            get => isStopped;
            set => isStopped = value;
        }
        public UnityEvent<GameObject> OnTargetHit => onTargetHit;

        void Start()
        {
            if (owner == null)
            {
                Destroy(gameObject);
                return;
            }

            if (onibiSprites == null || onibiSprites.Length == 0)
            {
                Debug.LogWarning($"VLMitamaOnibiAttackProjectile: onibiSprites is not set on {gameObject.name}");
                Destroy(gameObject);
                return;
            }

            transform.SetParent(null, true);
            InitializeLaunchAudioSource();
            StartCoroutine(AttackRoutine());
        }

        void OnDestroy()
        {
            UnregisterOwner();
        }

        public void SetOwner(GameObject projectileOwner)
        {
            owner = projectileOwner;
            ownerTransform = owner != null ? owner.transform : null;
            ownerAttackAnimationController =
                owner != null ? owner.GetComponent<IAttackAnimationController>() : null;

            if (owner == null)
                return;

            int ownerId = owner.GetInstanceID();
            if (activeOwnerIds.Contains(ownerId))
            {
                Destroy(gameObject);
                return;
            }

            activeOwnerIds.Add(ownerId);
            ownerRegistered = true;
        }

        public bool CanLaunch(GameObject projectileOwner)
        {
            return projectileOwner != null &&
                !activeOwnerIds.Contains(projectileOwner.GetInstanceID());
        }

        public void SetDirection(bool left)
        {
            isLeft = left;
        }

        public void SetDamage(float projectileDamage)
        {
            damage = projectileDamage;
        }

        public void SetLevel(int level)
        {
            attackLevel = Mathf.Max(1, level);
            hasReceivedLevel = true;
        }

        public void ImpactAndDestroy()
        {
            if (isImpacting)
                return;

            isImpacting = true;
            StopAllCoroutines();
            StartCoroutine(FadeOutAllAndDestroy());
        }

        IEnumerator AttackRoutine()
        {
            SpawnOnibis();
            SpawnAimArrow();

            float elapsed = 0f;
            while (elapsed < orbitDuration)
            {
                if (isStopped)
                {
                    yield return null;
                    continue;
                }

                float deltaTime = Mathf.Min(Time.deltaTime, orbitDuration - elapsed);
                elapsed += deltaTime;
                UpdateOrbitingOnibis(deltaTime);
                UpdateAimArrow(deltaTime);
                UpdateSpriteAnimation();
                yield return null;
            }

            FreezeOrbitingOnibisForLaunch();
            ownerAttackAnimationController?.TriggerAttackAnimation();

            List<OnibiState> launchOrder = new List<OnibiState>(onibis);
            Vector3 launchTargetPosition = GetAimTargetPosition();
            for (int i = 0; i < launchOrder.Count; i++)
            {
                OnibiState state = launchOrder[i];
                if (state == null || state.transform == null)
                    continue;

                Vector3 launchPosition = state.transform.position;
                state.Launch(GetLaunchDirectionToTarget(launchPosition, launchTargetPosition));
                PlayOnibiLaunchSe(launchPosition);

                if (i < launchOrder.Count - 1 && launchInterval > 0f)
                    yield return WaitLaunchInterval();
            }

            CleanupAimArrowImmediate();
            UnregisterOwner();

            bool hasAliveOnibi = true;
            while (hasAliveOnibi)
            {
                if (isStopped)
                {
                    yield return null;
                    continue;
                }

                UpdateSpriteAnimation();
                hasAliveOnibi = UpdateLaunchedOnibis();
                yield return null;
            }

            Destroy(gameObject);
        }

        IEnumerator WaitLaunchInterval()
        {
            float elapsed = 0f;
            while (elapsed < launchInterval)
            {
                if (isStopped)
                {
                    yield return null;
                    continue;
                }

                elapsed += Time.deltaTime;
                UpdateAimArrow(Time.deltaTime);
                UpdateSpriteAnimation();
                UpdateLaunchedOnibis();
                yield return null;
            }
        }

        void SpawnOnibis()
        {
            CleanupOnibisImmediate();

            SpriteRenderer ownerRenderer = owner != null ? owner.GetComponent<SpriteRenderer>() : null;
            int sortingLayerId = GetOnibiSortingLayerId(ownerRenderer);
            int sortingOrder = onibiSortingOrder;
            int count = ResolveOnibiCount();

            for (int i = 0; i < count; i++)
            {
                GameObject onibi = new GameObject($"VLMitamaOnibi_{i + 1}");
                onibi.tag = "Projectile";
                onibi.transform.SetParent(transform, false);
                onibi.transform.localScale = onibiScale;

                SpriteRenderer spriteRenderer = onibi.AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = onibiSprites[0];
                spriteRenderer.color = new Color(onibiColor.r, onibiColor.g, onibiColor.b, 0f);
                spriteRenderer.sortingLayerID = sortingLayerId;
                spriteRenderer.sortingOrder = sortingOrder;

                Rigidbody2D body = onibi.AddComponent<Rigidbody2D>();
                body.bodyType = RigidbodyType2D.Kinematic;
                body.gravityScale = 0f;
                body.simulated = true;

                BoxCollider2D collider = onibi.AddComponent<BoxCollider2D>();
                collider.isTrigger = true;
                collider.size = onibiColliderSize;

                VLMitamaOnibiHitDetector hitDetector =
                    onibi.AddComponent<VLMitamaOnibiHitDetector>();
                hitDetector.Initialize(this);

                VLMitamaOnibiProjectileForwarder projectileForwarder =
                    onibi.AddComponent<VLMitamaOnibiProjectileForwarder>();
                projectileForwarder.Initialize(this);

                float angle = i * (360f / count);
                OnibiState state = new OnibiState(
                    onibi.transform,
                    spriteRenderer,
                    collider,
                    angle,
                    spawnFadeDuration
                );
                onibis.Add(state);
            }
        }

        int ResolveOnibiCount()
        {
            if (!hasReceivedLevel)
                return Mathf.Max(1, onibiCount);

            if (attackLevel <= 1)
                return 3;
            if (attackLevel == 2)
                return 5;

            return 7;
        }

        void SpawnAimArrow()
        {
            CleanupAimArrowImmediate();
            isAimArrowLocked = false;
            aimArrowSpawnFrame = Time.frameCount;

            aimDirection = GetCurrentInputDirection();
            if (aimDirection.sqrMagnitude <= 0.01f)
                aimDirection = isLeft ? Vector2.left : Vector2.right;
            aimDirection.Normalize();
            aimAngleDegrees = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;

            if (aimArrowSprite == null)
                return;

            GameObject aimArrow = new GameObject("VLMitamaOnibiAimArrow");
            aimArrow.transform.SetParent(transform, false);
            aimArrow.transform.localScale = aimArrowScale;

            SpriteRenderer aimArrowRenderer = aimArrow.AddComponent<SpriteRenderer>();
            aimArrowRenderer.sprite = aimArrowSprite;
            aimArrowRenderer.color = Color.white;
            aimArrowRenderer.sortingLayerID = GetSortingLayerId(aimArrowSortingLayerName, 0);
            aimArrowRenderer.sortingOrder = aimArrowSortingOrder;

            aimArrowTransform = aimArrow.transform;
            this.aimArrowRenderer = aimArrowRenderer;
            UpdateAimArrowPosition();
        }

        void UpdateAimArrow(float deltaTime)
        {
            if (!isAimArrowLocked && WasAimLockPressed())
            {
                isAimArrowLocked = true;
            }

            if (!isAimArrowLocked)
                UpdateAimArrowDirection(deltaTime);

            UpdateAimArrowPosition();
            UpdateAimArrowBlink();
        }

        void UpdateAimArrowDirection(float deltaTime)
        {
            Vector2 inputDirection = GetCurrentInputDirection();
            if (inputDirection.sqrMagnitude <= 0.01f)
                return;

            float targetAngle = Mathf.Atan2(inputDirection.y, inputDirection.x) * Mathf.Rad2Deg;
            aimAngleDegrees = Mathf.MoveTowardsAngle(
                aimAngleDegrees,
                targetAngle,
                Mathf.Max(0f, aimArrowMoveDegreesPerSecond) * deltaTime
            );
            aimDirection = AngleToDirection(aimAngleDegrees);
        }

        bool WasAimLockPressed()
        {
            if (Time.frameCount <= aimArrowSpawnFrame)
                return false;

            return PlayerInputAdapter.WasAttackPressed(aimLockAttackButton);
        }

        void UpdateAimArrowBlink()
        {
            if (aimArrowRenderer == null)
                return;

            Color color = aimArrowRenderer.color;
            if (!isAimArrowLocked || aimArrowBlinkInterval <= 0f)
            {
                aimArrowRenderer.color = new Color(color.r, color.g, color.b, 1f);
                return;
            }

            float alpha = Mathf.PingPong(Time.time / aimArrowBlinkInterval, 1f);
            float minAlpha = Mathf.Clamp01(aimArrowBlinkMinAlpha);
            aimArrowRenderer.color = new Color(
                color.r,
                color.g,
                color.b,
                Mathf.Lerp(minAlpha, 1f, alpha)
            );
        }

        void UpdateAimArrowPosition()
        {
            if (ownerTransform == null)
                return;

            Vector3 center = GetAimCenterPosition();
            Vector3 offset = (Vector3)(aimDirection.normalized * aimArrowRadius);

            if (aimArrowTransform != null)
            {
                aimArrowTransform.position = center + offset;
                aimArrowTransform.rotation = Quaternion.Euler(0f, 0f, aimAngleDegrees + 180f);
            }
        }

        Vector3 GetAimTargetPosition()
        {
            return GetAimCenterPosition()
                + (Vector3)(aimDirection.normalized * aimConvergenceDistance);
        }

        Vector3 GetAimCenterPosition()
        {
            return ownerTransform != null
                ? ownerTransform.position + (Vector3)orbitCenterOffset
                : transform.position;
        }

        Vector2 GetLaunchDirectionToTarget(Vector3 launchPosition, Vector3 targetPosition)
        {
            Vector2 direction = targetPosition - launchPosition;
            return direction.sqrMagnitude <= 0.01f ? aimDirection.normalized : direction.normalized;
        }

        void PlayOnibiLaunchSe(Vector3 position)
        {
            if (onibiLaunchSe == null || onibiLaunchSeVolume <= 0f)
                return;

            if (launchAudioSource == null)
                InitializeLaunchAudioSource();

            launchAudioSource.transform.position = position;
            launchAudioSource.PlayOneShot(onibiLaunchSe, onibiLaunchSeVolume);
        }

        void InitializeLaunchAudioSource()
        {
            if (launchAudioSource != null)
                return;

            launchAudioSource = gameObject.AddComponent<AudioSource>();
            launchAudioSource.playOnAwake = false;
            launchAudioSource.spatialBlend = 0f;
        }

        Vector2 GetCurrentInputDirection()
        {
            float y = 0f;

            if (PlayerInputAdapter.IsAimUpPressed())
                y += 1f;
            if (PlayerInputAdapter.IsAimDownPressed())
                y -= 1f;

            Vector2 direction = new Vector2(0f, y);
            return direction.sqrMagnitude > 0.01f ? direction.normalized : Vector2.zero;
        }

        static Vector2 AngleToDirection(float angleDegrees)
        {
            float radians = angleDegrees * Mathf.Deg2Rad;
            return new Vector2(Mathf.Cos(radians), Mathf.Sin(radians)).normalized;
        }

        int GetOnibiSortingLayerId(SpriteRenderer ownerRenderer)
        {
            int fallbackLayerId = ownerRenderer != null ? ownerRenderer.sortingLayerID : 0;
            return GetSortingLayerId(onibiSortingLayerName, fallbackLayerId);
        }

        int GetSortingLayerId(string sortingLayerName, int fallbackLayerId)
        {
            if (string.IsNullOrEmpty(sortingLayerName))
                return fallbackLayerId;

            int layerId = SortingLayer.NameToID(sortingLayerName);
            return SortingLayer.IsValid(layerId) ? layerId : fallbackLayerId;
        }

        void UpdateOrbitingOnibis(float deltaTime)
        {
            if (ownerTransform == null)
                return;

            Vector3 center = ownerTransform.position + (Vector3)orbitCenterOffset;
            for (int i = 0; i < onibis.Count; i++)
            {
                OnibiState state = onibis[i];
                if (state == null || state.transform == null || !state.IsOrbiting)
                    continue;

                state.AngleDegrees += orbitDegreesPerSecond * deltaTime;
                float radians = state.AngleDegrees * Mathf.Deg2Rad;
                Vector3 offset = new Vector3(Mathf.Cos(radians), Mathf.Sin(radians), 0f) * orbitRadius;
                state.transform.position = center + offset;
                state.UpdateFadeIn(deltaTime, onibiColor);
            }
        }

        void FreezeOrbitingOnibisForLaunch()
        {
            for (int i = 0; i < onibis.Count; i++)
            {
                OnibiState state = onibis[i];
                if (state == null || state.transform == null || !state.IsOrbiting)
                    continue;

                state.transform.SetParent(null, true);
            }
        }

        void UpdateSpriteAnimation()
        {
            if (onibiSprites == null || onibiSprites.Length == 0 || animationFrameDuration <= 0f)
                return;

            int frame = Mathf.FloorToInt(Time.time / animationFrameDuration) % onibiSprites.Length;
            Sprite sprite = onibiSprites[frame];
            for (int i = 0; i < onibis.Count; i++)
            {
                if (onibis[i]?.spriteRenderer != null)
                    onibis[i].spriteRenderer.sprite = sprite;
            }
        }

        bool UpdateLaunchedOnibis()
        {
            bool hasAliveOnibi = false;
            for (int i = onibis.Count - 1; i >= 0; i--)
            {
                OnibiState state = onibis[i];
                if (state == null || state.transform == null)
                {
                    onibis.RemoveAt(i);
                    continue;
                }

                if (state.UpdateLaunched(Time.deltaTime, launchSpeed, lifetimeAfterLaunch))
                {
                    DestroyOnibi(state);
                    onibis.RemoveAt(i);
                    continue;
                }

                hasAliveOnibi = true;
            }

            return hasAliveOnibi;
        }

        void HandleOnibiHit(VLMitamaOnibiHitDetector hitDetector, Collider2D other)
        {
            if (isStopped || isImpacting || hitDetector == null || other == null)
                return;

            if (!other.CompareTag(targetTagName))
                return;

            Health health = other.GetComponent<Health>();
            if (health == null)
                return;

            OnibiState state = FindState(hitDetector.transform);
            if (state == null || state.IsFadingOut)
                return;

            bool canRepeatHit = state.IsOrbiting;
            if (!state.TryMarkHit(other.gameObject, Time.time, canRepeatHit, orbitHitInterval))
                return;

            bool blowAwayLeft = state.IsLaunched
                ? state.MoveDirection.x < 0f
                : other.transform.position.x < state.transform.position.x;
            health.TakeDamage(damage, blowAwayLeft);
            onTargetHit?.Invoke(other.gameObject);

            if (state.IsLaunched)
                StartCoroutine(FadeOutAndDestroyOnibi(state));
        }

        void HandleOnibiBlocked(Transform onibiTransform)
        {
            if (isStopped)
                return;

            OnibiState state = FindState(onibiTransform);
            if (state == null || state.IsFadingOut)
                return;

            StartCoroutine(FadeOutAndDestroyOnibi(state));
        }

        OnibiState FindState(Transform onibiTransform)
        {
            for (int i = 0; i < onibis.Count; i++)
            {
                if (onibis[i]?.transform == onibiTransform)
                    return onibis[i];
            }

            return null;
        }

        IEnumerator FadeOutAndDestroyOnibi(OnibiState state)
        {
            if (state == null || state.transform == null || state.IsFadingOut)
                yield break;

            state.IsFadingOut = true;
            if (state.collider != null)
                state.collider.enabled = false;

            SpriteRenderer spriteRenderer = state.spriteRenderer;
            if (spriteRenderer == null || hitFadeDuration <= 0f)
            {
                DestroyOnibi(state);
                yield break;
            }

            Color startColor = spriteRenderer.color;
            float elapsed = 0f;
            while (elapsed < hitFadeDuration && spriteRenderer != null)
            {
                if (isStopped)
                {
                    yield return null;
                    continue;
                }

                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / hitFadeDuration);
                Color color = startColor;
                color.a = Mathf.Lerp(startColor.a, 0f, t);
                spriteRenderer.color = color;
                yield return null;
            }

            DestroyOnibi(state);
        }

        IEnumerator FadeOutAllAndDestroy()
        {
            CleanupAimArrowImmediate();

            for (int i = 0; i < onibis.Count; i++)
            {
                if (onibis[i] != null && onibis[i].transform != null)
                    StartCoroutine(FadeOutAndDestroyOnibi(onibis[i]));
            }

            float elapsed = 0f;
            float waitTime = Mathf.Max(hitFadeDuration, 0.01f);
            while (elapsed < waitTime)
            {
                if (!isStopped)
                    elapsed += Time.deltaTime;

                yield return null;
            }

            Destroy(gameObject);
        }

        void DestroyOnibi(OnibiState state)
        {
            if (state?.transform != null)
                Destroy(state.transform.gameObject);
        }

        void CleanupOnibisImmediate()
        {
            for (int i = 0; i < onibis.Count; i++)
            {
                if (onibis[i]?.transform != null)
                    Destroy(onibis[i].transform.gameObject);
            }

            onibis.Clear();
        }

        void CleanupAimArrowImmediate()
        {
            if (aimArrowTransform != null)
                Destroy(aimArrowTransform.gameObject);

            aimArrowTransform = null;
            aimArrowRenderer = null;
            isAimArrowLocked = false;
            aimArrowSpawnFrame = -1;
        }

        void UnregisterOwner()
        {
            if (!ownerRegistered || owner == null)
                return;

            activeOwnerIds.Remove(owner.GetInstanceID());
            ownerRegistered = false;
        }

        class OnibiState
        {
            public readonly Transform transform;
            public readonly SpriteRenderer spriteRenderer;
            public readonly BoxCollider2D collider;
            public float AngleDegrees;
            public bool IsOrbiting = true;
            public bool IsLaunched = false;
            public bool IsFadingOut = false;
            public Vector2 MoveDirection = Vector2.zero;

            readonly float fadeDuration;
            readonly Dictionary<GameObject, float> nextHitTimes = new Dictionary<GameObject, float>();
            float fadeElapsed = 0f;
            float lifetimeElapsed = 0f;
            int launchFrame = -1;

            public OnibiState(
                Transform transform,
                SpriteRenderer spriteRenderer,
                BoxCollider2D collider,
                float angleDegrees,
                float fadeDuration
            )
            {
                this.transform = transform;
                this.spriteRenderer = spriteRenderer;
                this.collider = collider;
                AngleDegrees = angleDegrees;
                this.fadeDuration = Mathf.Max(0f, fadeDuration);
            }

            public void Launch(Vector2 direction)
            {
                IsOrbiting = false;
                IsLaunched = true;
                MoveDirection = direction.sqrMagnitude <= 0.01f ? Vector2.right : direction.normalized;
                launchFrame = Time.frameCount;
                transform.SetParent(null, true);
            }

            public bool UpdateLaunched(float deltaTime, float speed, float lifetime)
            {
                if (!IsLaunched || IsFadingOut || transform == null)
                    return false;

                if (Time.frameCount <= launchFrame)
                    return false;

                transform.position += (Vector3)(MoveDirection * speed * deltaTime);
                lifetimeElapsed += deltaTime;
                return lifetimeElapsed >= lifetime;
            }

            public void UpdateFadeIn(float deltaTime, Color targetColor)
            {
                if (spriteRenderer == null)
                    return;

                if (fadeDuration <= 0f)
                {
                    spriteRenderer.color = targetColor;
                    return;
                }

                fadeElapsed += deltaTime;
                float t = Mathf.Clamp01(fadeElapsed / fadeDuration);
                spriteRenderer.color = new Color(
                    targetColor.r,
                    targetColor.g,
                    targetColor.b,
                    Mathf.Lerp(0f, targetColor.a, t)
                );
            }

            public bool TryMarkHit(
                GameObject target,
                float currentTime,
                bool canRepeatHit,
                float repeatInterval
            )
            {
                if (target == null)
                    return false;

                if (!canRepeatHit)
                {
                    if (nextHitTimes.ContainsKey(target))
                        return false;

                    nextHitTimes[target] = float.PositiveInfinity;
                    return true;
                }

                if (nextHitTimes.TryGetValue(target, out float nextHitTime) && currentTime < nextHitTime)
                    return false;

                nextHitTimes[target] = currentTime + Mathf.Max(0.02f, repeatInterval);
                return true;
            }
        }

        class VLMitamaOnibiHitDetector : MonoBehaviour
        {
            VLMitamaOnibiAttackProjectile owner = null;

            public void Initialize(VLMitamaOnibiAttackProjectile projectile)
            {
                owner = projectile;
            }

            void OnTriggerEnter2D(Collider2D other)
            {
                owner?.HandleOnibiHit(this, other);
            }

            void OnTriggerStay2D(Collider2D other)
            {
                owner?.HandleOnibiHit(this, other);
            }
        }

        class VLMitamaOnibiProjectileForwarder : MonoBehaviour, IProjectile
        {
            VLMitamaOnibiAttackProjectile owner = null;
            readonly UnityEvent<GameObject> onTargetHit = new UnityEvent<GameObject>();

            public bool IsStucking => owner != null && owner.IsStucking;
            public UnityEvent<GameObject> OnTargetHit => onTargetHit;

            public void Initialize(VLMitamaOnibiAttackProjectile projectile)
            {
                owner = projectile;
            }

            public void SetDirection(bool left)
            {
            }

            public void SetDamage(float projectileDamage)
            {
            }

            public void ImpactAndDestroy()
            {
                if (owner != null && owner.IsStopped)
                    return;

                owner?.HandleOnibiBlocked(transform);
            }
        }
    }
}
