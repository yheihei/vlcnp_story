using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VLCNP.Effects;

namespace VLCNP.Combat.EnemyAction
{
    public class SummonGhostEnemyAction : EnemyAction
    {
        [Serializable]
        public class SummonSetting
        {
            public Transform spawnPoint = null;
            public GameObject enemyPrefab = null;
            public GameObject spawnEffectPrefab = null;
        }

        [SerializeField]
        List<SummonSetting> summonSettings = new List<SummonSetting>();

        [SerializeField]
        float preMagicDuration = 2f;

        [SerializeField]
        float spawnInterval = 0.25f;

        [SerializeField]
        float postMagicDuration = 0.2f;

        [SerializeField]
        AudioClip spawnSe = null;

        [SerializeField]
        float spawnSeVolume = 1f;

        [SerializeField]
        float spawnEffectLifetime = 1f;

        [SerializeField]
        float spawnFadeInDuration = 0.5f;

        [SerializeField]
        GameObject castEffectPrefab = null;

        [SerializeField]
        Vector3 castEffectOffset = new Vector3(0f, 0.5f, 0f);

        [SerializeField]
        float castEffectLingerDuration = 0.5f;

        [SerializeField]
        float castEffectFadeOutDuration = 0.5f;

        [SerializeField]
        string preMagicBoolName = "isPreMagic";

        [SerializeField]
        string magicBoolName = "isMagic";

        readonly List<GameObject> summonedEnemies = new List<GameObject>();
        Coroutine actionRoutine = null;
        Animator animator = null;
        GameObject activeCastEffect = null;

        void Awake()
        {
            animator = GetComponent<Animator>();
        }

        void OnDisable()
        {
            if (actionRoutine != null)
            {
                StopCoroutine(actionRoutine);
                actionRoutine = null;
            }

            ResetMagicStates();
            CleanupCastEffect(false);
            IsExecuting = false;
            IsDone = true;
        }

        public override void Execute()
        {
            if (IsExecuting || IsDone)
                return;

            RemoveDestroyedSummons();
            if (summonedEnemies.Count > 0)
            {
                CompleteAction();
                return;
            }

            if (!HasValidSummonSetting())
            {
                Debug.LogWarning($"SummonGhostEnemyAction: summonSettings is empty on {gameObject.name}");
                CompleteAction();
                return;
            }

            IsExecuting = true;
            actionRoutine = StartCoroutine(ExecuteRoutine());
        }

        public override void Stop()
        {
            if (actionRoutine != null)
            {
                StopCoroutine(actionRoutine);
                actionRoutine = null;
            }

            ResetMagicStates();
            CleanupCastEffect(true);
            IsExecuting = false;
            IsDone = true;
        }

        IEnumerator ExecuteRoutine()
        {
            SetPreMagic(true);
            StartCastEffect();
            yield return WaitForSecondsInterruptible(preMagicDuration);

            SetPreMagic(false);
            SetMagic(true);

            for (int i = 0; i < summonSettings.Count; i++)
            {
                SummonSetting setting = summonSettings[i];
                if (!IsValidSummonSetting(setting))
                    continue;

                Spawn(setting);

                if (spawnInterval > 0f && i < summonSettings.Count - 1)
                    yield return WaitForSecondsInterruptible(spawnInterval);
            }

            if (postMagicDuration > 0f)
                yield return WaitForSecondsInterruptible(postMagicDuration);

            if (castEffectLingerDuration > 0f)
                yield return WaitForSecondsInterruptible(castEffectLingerDuration);

            yield return FadeOutCastEffect();

            CompleteAction();
        }

        IEnumerator WaitForSecondsInterruptible(float seconds)
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

        void Spawn(SummonSetting setting)
        {
            Vector3 spawnPosition = setting.spawnPoint.position;

            if (setting.spawnEffectPrefab != null)
            {
                GameObject effect = Instantiate(
                    setting.spawnEffectPrefab,
                    spawnPosition,
                    Quaternion.identity
                );

                if (spawnEffectLifetime > 0f)
                    Destroy(effect, spawnEffectLifetime);
            }

            if (spawnSe != null)
            {
                AudioSource.PlayClipAtPoint(spawnSe, spawnPosition, spawnSeVolume);
            }

            GameObject summonedEnemy = Instantiate(
                setting.enemyPrefab,
                spawnPosition,
                setting.spawnPoint.rotation
            );
            StartCoroutine(FadeInSummonedEnemy(summonedEnemy));
            summonedEnemies.Add(summonedEnemy);
        }

        IEnumerator FadeInSummonedEnemy(GameObject summonedEnemy)
        {
            if (summonedEnemy == null || spawnFadeInDuration <= 0f)
                yield break;

            SpriteRenderer[] renderers = summonedEnemy.GetComponentsInChildren<SpriteRenderer>(true);
            if (renderers.Length == 0)
                yield break;

            Color[] initialColors = new Color[renderers.Length];
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] == null)
                    continue;

                initialColors[i] = renderers[i].color;
                SetRendererAlpha(renderers[i], 0f);
            }

            float elapsed = 0f;
            while (elapsed < spawnFadeInDuration)
            {
                elapsed += Time.deltaTime;
                float rate = Mathf.Clamp01(elapsed / spawnFadeInDuration);

                for (int i = 0; i < renderers.Length; i++)
                {
                    if (renderers[i] == null)
                        continue;

                    SetRendererAlpha(renderers[i], initialColors[i].a * rate);
                }

                yield return null;
            }

            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null)
                    renderers[i].color = initialColors[i];
            }
        }

        void SetRendererAlpha(SpriteRenderer spriteRenderer, float alpha)
        {
            Color color = spriteRenderer.color;
            color.a = alpha;
            spriteRenderer.color = color;
        }

        bool HasValidSummonSetting()
        {
            if (summonSettings == null)
                return false;

            for (int i = 0; i < summonSettings.Count; i++)
            {
                if (IsValidSummonSetting(summonSettings[i]))
                    return true;
            }

            return false;
        }

        bool IsValidSummonSetting(SummonSetting setting)
        {
            return setting != null
                && setting.spawnPoint != null
                && setting.enemyPrefab != null;
        }

        void RemoveDestroyedSummons()
        {
            summonedEnemies.RemoveAll(enemy => enemy == null);
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

        IEnumerator FadeOutCastEffect()
        {
            if (activeCastEffect == null)
                yield break;

            GameObject effect = activeCastEffect;
            activeCastEffect = null;
            yield return ParticleEffectFadeOut.FadeOutAndDestroy(effect, castEffectFadeOutDuration);
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
            if (animator != null)
                animator.SetBool(preMagicBoolName, value);
        }

        void SetMagic(bool value)
        {
            if (animator != null)
                animator.SetBool(magicBoolName, value);
        }

        void ResetMagicStates()
        {
            SetPreMagic(false);
            SetMagic(false);
        }
    }
}
