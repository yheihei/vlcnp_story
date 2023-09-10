using System.Collections;
using System.Collections.Generic;
using Fungus;
using UnityEngine;
using VLCNP.Attributes;
using VLCNP.SceneManagement;

namespace VLCNP.Core
{
    public class GameOver : MonoBehaviour
    {
        Health playerHealth;
        [SerializeField] string autoSaveFileName = "autoSave";
        [SerializeField]
        public Flowchart flowChart;
        [SerializeField] float fadeOutTime = 3f;
        [SerializeField] float fadeWaitTime = 3f;

        void Awake()
        {
            // プレイヤーのHealthを取得する
            playerHealth = GameObject.FindWithTag("Player").GetComponent<Health>();
        }

        private void OnEnable()
        {
            if (playerHealth != null)
            {
                // プレイヤーの死亡Actionを監視する
                playerHealth.onDie += PlayerDie;
            }
        }

        private void OnDisable() {
            if (playerHealth != null)
            {
                playerHealth.onDie -= PlayerDie;
            }
        }

        void PlayerDie()
        {
            print("GameOver");
            flowChart.ExecuteBlock("GameOver");
        }

        public void Retry()
        {
            StartCoroutine(RestartSchene());
        }

        public IEnumerator RestartSchene()
        {
            print("RestoreSchene");
            DontDestroyOnLoad(gameObject);
            Fader fader = FindObjectOfType<Fader>();
            yield return fader.FadeOut(fadeOutTime);
            yield return new WaitForSeconds(fadeWaitTime);
            SavingWrapper wrapper = FindObjectOfType<SavingWrapper>();
            wrapper.Load(autoSaveFileName);
            Destroy(gameObject);
        }
    }    
}
