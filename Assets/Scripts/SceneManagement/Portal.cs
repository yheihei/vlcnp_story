using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace VLCNP.SceneManagement
{
    public class Portal : MonoBehaviour
    {
        [SerializeField] int sceneToLoad = -1;
        [SerializeField] Transform spawnPoint;
        [SerializeField] bool isPlayerDirectionLeft;

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
            DontDestroyOnLoad(gameObject);
            yield return SceneManager.LoadSceneAsync(sceneToLoad);
            print("Scene Loaded");
            Portal otherPortal = GetOtherPortal();
            UpdatePlayer(otherPortal);
            Destroy(gameObject);
        }

        private void UpdatePlayer(Portal otherPortal)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            player.transform.position = otherPortal.spawnPoint.position;
            // 向きを変える
            player.GetComponent<Movement.Mover>().IsLeft = otherPortal.isPlayerDirectionLeft;
        }

        private Portal GetOtherPortal()
        {
            foreach (Portal portal in FindObjectsOfType<Portal>())
            {
                if (portal == this) continue;
                return portal;
            }
            return null;
        }
    }    
}
