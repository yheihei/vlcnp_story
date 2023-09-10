using System;
using System.Collections;
using Unity.VisualScripting;
using VLCNP.Control;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace VLCNP.SceneManagement
{
    public class Portal : MonoBehaviour
    {
        enum DestinationIdentifier
        {
            A, B, C, D, E
        }

        [SerializeField] int sceneToLoad = -1;
        [SerializeField] Transform spawnPoint;
        [SerializeField] bool isPlayerDirectionLeft;
        [SerializeField] DestinationIdentifier destination;
        [SerializeField] float fadeOutTime = 1f;
        [SerializeField] float fadeWaitTime = 0.2f;
        [SerializeField] float fadeInTime = 1f;
        [SerializeField] string autoSaveFileName = "autoSave";

        bool isTransitioning = false;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (isTransitioning) return;
            if (other.gameObject.tag == "Player")
            {
                print("Portal");
                isTransitioning = true;
                StartCoroutine(Transition());
            }
        }

        private IEnumerator Transition()
        {
            if (sceneToLoad < 0)
            {
                Debug.LogError("Scene to load not set");
                yield break;
            }
            DontDestroyOnLoad(gameObject);
            DisableControl();

            Fader fader = FindObjectOfType<Fader>();
            yield return fader.FadeOut(fadeOutTime);

            // キャラたちの状態保存
            SavingWrapper wrapper = FindObjectOfType<SavingWrapper>();
            wrapper.Save(autoSaveFileName);

            yield return SceneManager.LoadSceneAsync(sceneToLoad);
            print("Scene Loaded");

            // キャラたちの状態復元
            wrapper.LoadOnlyState(autoSaveFileName);

            Portal otherPortal = GetOtherPortal();
            UpdatePlayerPosition(otherPortal);

            DisableControl();

            yield return new WaitForSeconds(fadeWaitTime);
            yield return fader.FadeIn(fadeInTime);

            EnableControl();

            Destroy(gameObject);
        }

        private void UpdatePlayerPosition(Portal otherPortal)
        {
            print("UpdatePlayerPosition");
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            player.transform.position = otherPortal.spawnPoint.position;
            // 向きを変える
            player.GetComponent<Movement.Mover>().IsLeft = otherPortal.isPlayerDirectionLeft;
        }

        void DisableControl()
        {
            GameObject player = GameObject.FindWithTag("Player");
            player.GetComponent<PlayerController>().enabled = false;
            player.GetComponent<Movement.Mover>().Stop();
        }

        void EnableControl()
        {
            GameObject player = GameObject.FindWithTag("Player");
            player.GetComponent<PlayerController>().enabled = true;
        }

        private Portal GetOtherPortal()
        {
            foreach (Portal portal in FindObjectsOfType<Portal>())
            {
                if (portal == this) continue;
                if (portal.destination != destination) continue;
                return portal;
            }
            return null;
        }
    }    
}
