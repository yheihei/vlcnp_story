using System.Collections.Generic;
using UnityEngine;
using VLCNP.Attributes;
using VLCNP.Core;

namespace VLCNP.SceneManagement
{
    public class SetFlagOnEnemiesDefeated : MonoBehaviour
    {
        [SerializeField]
        List<Health> enemyHealths = new List<Health>();

        [SerializeField]
        Flag flagToSet;

        [SerializeField]
        AudioClip setFlagSe = null;

        [SerializeField]
        [Range(0f, 1f)]
        float setFlagSeVolume = 0.5f;

        [SerializeField]
        AudioSource seAudioSource = null;

        FlagManager flagManager;

        readonly HashSet<Health> subscribedEnemyHealths = new HashSet<Health>();

        int remainingEnemies = 0;
        bool isAllDie = false;

        private void Awake()
        {
            ResolveFlagManager();

            // 初期起動時に既にFlagが立っていれば何もしない
            if (flagManager != null && flagManager.GetFlag(flagToSet))
            {
                isAllDie = true;
                enabled = false;
                return;
            }

            SubscribeEnemyHealths();

            if (remainingEnemies == 0)
            {
                OpenTargets();
            }
        }

        private void OnDestroy()
        {
            UnsubscribeEnemyHealths();
        }

        private void SubscribeEnemyHealths()
        {
            remainingEnemies = 0;
            subscribedEnemyHealths.Clear();

            if (enemyHealths == null)
                return;

            foreach (Health health in enemyHealths)
            {
                if (health == null)
                    continue;
                if (!subscribedEnemyHealths.Add(health))
                    continue;

                health.onDie += HandleEnemyDie;

                if (!health.IsDead)
                {
                    remainingEnemies++;
                }
            }
        }

        private void UnsubscribeEnemyHealths()
        {
            foreach (Health health in subscribedEnemyHealths)
            {
                if (health == null)
                    continue;
                health.onDie -= HandleEnemyDie;
            }

            subscribedEnemyHealths.Clear();
        }

        private void HandleEnemyDie()
        {
            if (isAllDie)
                return;

            if (remainingEnemies > 0)
            {
                remainingEnemies--;
            }

            if (remainingEnemies == 0)
            {
                OpenTargets();
            }
        }

        private void OpenTargets()
        {
            if (isAllDie)
                return;

            isAllDie = true;

            flagManager?.SetFlag(flagToSet, true);
            PlaySetFlagSe();

            UnsubscribeEnemyHealths();
            enabled = false;
        }

        private void ResolveFlagManager()
        {
            // FlagManagerをタグから取得
            GameObject flagManagerObject = GameObject.FindGameObjectWithTag("FlagManager");
            if (flagManagerObject != null)
            {
                flagManager = flagManagerObject.GetComponent<FlagManager>();
            }
        }

        private void PlaySetFlagSe()
        {
            if (setFlagSe == null)
                return;

            if (seAudioSource != null)
            {
                seAudioSource.PlayOneShot(setFlagSe, setFlagSeVolume);
                return;
            }

            AudioSource.PlayClipAtPoint(setFlagSe, transform.position, setFlagSeVolume);
        }
    }
}
