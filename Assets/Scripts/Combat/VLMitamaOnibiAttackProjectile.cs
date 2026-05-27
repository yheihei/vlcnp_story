using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using VLCNP.Attributes;
using VLCNP.Control;

namespace VLCNP.Combat
{
    public class VLMitamaOnibiAttackProjectile : MonoBehaviour, IProjectile, IProjectileOwnerReceiver
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
        float launchSpeed = 9f;

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
        UnityEvent<GameObject> onTargetHit = new UnityEvent<GameObject>();

        static readonly HashSet<int> activeOwnerIds = new HashSet<int>();

        readonly List<OnibiState> onibis = new List<OnibiState>();
        GameObject owner = null;
        Transform ownerTransform = null;
        IAttackAnimationController ownerAttackAnimationController = null;
        bool isLeft = false;
        bool ownerRegistered = false;
        bool isStucking = false;
        bool isImpacting = false;
        float damage = 1f;

        public bool IsStucking => isStucking;
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

        public void SetDirection(bool left)
        {
            isLeft = left;
        }

        public void SetDamage(float projectileDamage)
        {
            damage = projectileDamage;
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

            float elapsed = 0f;
            while (elapsed < orbitDuration)
            {
                elapsed += Time.deltaTime;
                UpdateOrbitingOnibis();
                UpdateSpriteAnimation();
                yield return null;
            }

            ownerAttackAnimationController?.TriggerAttackAnimation();

            for (int i = 0; i < onibis.Count; i++)
            {
                OnibiState state = onibis[i];
                if (state == null || state.transform == null)
                    continue;

                state.Launch(GetLaunchDirection());

                if (i < onibis.Count - 1 && launchInterval > 0f)
                    yield return new WaitForSeconds(launchInterval);
            }

            UnregisterOwner();

            bool hasAliveOnibi = true;
            while (hasAliveOnibi)
            {
                hasAliveOnibi = false;
                UpdateSpriteAnimation();

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

                yield return null;
            }

            Destroy(gameObject);
        }

        void SpawnOnibis()
        {
            CleanupOnibisImmediate();

            SpriteRenderer ownerRenderer = owner != null ? owner.GetComponent<SpriteRenderer>() : null;
            int sortingLayerId = GetOnibiSortingLayerId(ownerRenderer);
            int sortingOrder = onibiSortingOrder;
            int count = Mathf.Max(1, onibiCount);

            for (int i = 0; i < count; i++)
            {
                GameObject onibi = new GameObject($"VLMitamaOnibi_{i + 1}");
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

        int GetOnibiSortingLayerId(SpriteRenderer ownerRenderer)
        {
            if (!string.IsNullOrEmpty(onibiSortingLayerName))
            {
                int layerId = SortingLayer.NameToID(onibiSortingLayerName);
                if (SortingLayer.IsValid(layerId))
                    return layerId;
            }

            return ownerRenderer != null ? ownerRenderer.sortingLayerID : 0;
        }

        void UpdateOrbitingOnibis()
        {
            if (ownerTransform == null)
                return;

            Vector3 center = ownerTransform.position + (Vector3)orbitCenterOffset;
            for (int i = 0; i < onibis.Count; i++)
            {
                OnibiState state = onibis[i];
                if (state == null || state.transform == null || !state.IsOrbiting)
                    continue;

                state.AngleDegrees += orbitDegreesPerSecond * Time.deltaTime;
                float radians = state.AngleDegrees * Mathf.Deg2Rad;
                Vector3 offset = new Vector3(Mathf.Cos(radians), Mathf.Sin(radians), 0f) * orbitRadius;
                state.transform.position = center + offset;
                state.UpdateFadeIn(Time.deltaTime, onibiColor);
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

        Vector2 GetLaunchDirection()
        {
            float x = PlayerInputAdapter.GetMoveHorizontal();
            float y = 0f;

            if (PlayerInputAdapter.IsAimUpPressed())
                y += 1f;
            if (PlayerInputAdapter.IsAimDownPressed())
                y -= 1f;

            Vector2 direction = new Vector2(x, y);
            if (direction.sqrMagnitude <= 0.01f)
                direction = isLeft ? Vector2.left : Vector2.right;

            return direction.normalized;
        }

        void HandleOnibiHit(VLMitamaOnibiHitDetector hitDetector, Collider2D other)
        {
            if (isImpacting || hitDetector == null || other == null)
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
            for (int i = 0; i < onibis.Count; i++)
            {
                if (onibis[i] != null && onibis[i].transform != null)
                    StartCoroutine(FadeOutAndDestroyOnibi(onibis[i]));
            }

            yield return new WaitForSeconds(Mathf.Max(hitFadeDuration, 0.01f));
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
                transform.SetParent(null, true);
            }

            public bool UpdateLaunched(float deltaTime, float speed, float lifetime)
            {
                if (!IsLaunched || IsFadingOut || transform == null)
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
    }
}
