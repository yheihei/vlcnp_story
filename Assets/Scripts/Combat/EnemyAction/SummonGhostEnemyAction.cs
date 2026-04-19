using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        string preMagicBoolName = "isPreMagic";

        [SerializeField]
        string magicBoolName = "isMagic";

        readonly List<GameObject> summonedEnemies = new List<GameObject>();
        Coroutine actionRoutine = null;
        Animator animator = null;

        void Awake()
        {
            animator = GetComponent<Animator>();
        }

        void OnDisable()
        {
            Stop();
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
            IsExecuting = false;
            IsDone = true;
        }

        IEnumerator ExecuteRoutine()
        {
            SetPreMagic(true);
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
            summonedEnemies.Add(summonedEnemy);
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
