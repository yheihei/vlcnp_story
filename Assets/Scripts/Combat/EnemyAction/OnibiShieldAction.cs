using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VLCNP.Attributes;

namespace VLCNP.Combat.EnemyAction
{
    public class OnibiShieldAction : EnemyAction
    {
        [SerializeField]
        Sprite projectileSprite = null;

        [SerializeField]
        RuntimeAnimatorController projectileAnimatorController = null;

        [SerializeField]
        Color projectileColor = Color.white;

        [SerializeField]
        Vector3 projectileScale = new Vector3(1.2f, 1.2f, 1f);

        [SerializeField]
        Vector2 orbitCenterOffset = new Vector2(0f, 0.5f);

        [SerializeField]
        Vector2 projectileColliderSize = new Vector2(0.28f, 0.28f);

        [SerializeField]
        int projectileCount = 4;

        [SerializeField]
        float orbitRadius = 1.1f;

        [SerializeField]
        float orbitDegreesPerSecond = 240f;

        [SerializeField]
        float orbitWarmupDuration = 2.4f;

        [SerializeField]
        float launchInterval = 0.2f;

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

        Coroutine actionRoutine = null;
        readonly List<OrbitingOnibiProjectile> activeProjectiles = new List<OrbitingOnibiProjectile>();
        SpriteRenderer ownerSpriteRenderer = null;
        Health ownerHealth = null;

        void Awake()
        {
            ownerSpriteRenderer = GetComponent<SpriteRenderer>();
            ownerHealth = GetComponent<Health>();
        }

        void OnDisable()
        {
            CleanupAndComplete();
        }

        void OnDestroy()
        {
            CleanupProjectiles();
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
            if (!TryGetPlayer(out _))
            {
                CleanupAndComplete();
                yield break;
            }

            SpawnProjectiles();

            float elapsed = 0f;
            while (elapsed < orbitWarmupDuration)
            {
                if (!CanContinue())
                {
                    CleanupAndComplete();
                    yield break;
                }

                elapsed += Time.deltaTime;
                yield return null;
            }

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

                if (!TryGetPlayer(out GameObject player))
                {
                    CleanupAndComplete();
                    yield break;
                }

                projectile.LaunchTowards(player.transform.position, projectileSpeed, projectileLifetime);
                yield return new WaitForSeconds(launchInterval);
            }

            while (HasLivingProjectiles())
            {
                if (!CanContinue())
                {
                    CleanupAndComplete();
                    yield break;
                }

                yield return null;
            }

            CompleteAction();
        }

        void SpawnProjectiles()
        {
            CleanupProjectiles();

            int sortingLayerId = ownerSpriteRenderer != null ? ownerSpriteRenderer.sortingLayerID : 0;
            int sortingOrder = ownerSpriteRenderer != null ? ownerSpriteRenderer.sortingOrder + 1 : 0;

            for (int i = 0; i < projectileCount; i++)
            {
                GameObject projectileObject = new GameObject($"Onibi_{i + 1}");
                OrbitingOnibiProjectile projectile =
                    projectileObject.AddComponent<OrbitingOnibiProjectile>();

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

                activeProjectiles.Add(projectile);
            }
        }

        bool TryGetPlayer(out GameObject player)
        {
            player = GameObject.FindWithTag(targetTagName);
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

        bool HasLivingProjectiles()
        {
            for (int i = 0; i < activeProjectiles.Count; i++)
            {
                if (activeProjectiles[i] != null)
                    return true;
            }

            return false;
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

        void CleanupAndComplete()
        {
            if (actionRoutine != null)
            {
                StopCoroutine(actionRoutine);
                actionRoutine = null;
            }

            CleanupProjectiles();
            IsExecuting = false;
            IsDone = true;
        }

        void CompleteAction()
        {
            actionRoutine = null;
            activeProjectiles.Clear();
            IsExecuting = false;
            IsDone = true;
        }
    }
}
