using System.Collections;
using Fungus;
using UnityEngine;
using VLCNP.Core;

namespace VLCNP.Control
{
    public class StoppableController : MonoBehaviour
    {
        public void StopAll()
        {
            Debug.Log("Stop All Components Called");
            StartCoroutine(StopAllCoroutine());
        }

        private IEnumerator StopAllCoroutine()
        {
            // Loadが完了するのを待つ
            LoadCompleteManager loadCompleteManager = LoadCompleteManager.Instance;
            // LoadCompleteManagerが存在し、ロードが完了していない場合は待つ
            if (loadCompleteManager != null)
            {
                while (!loadCompleteManager.IsLoaded)
                {
                    yield return null;
                }
                Debug.Log("Stop All Components");
                // ゲームの中でIStoppableを実装しているものを全て取得する
                foreach (MonoBehaviour obj in FindObjectsOfType<MonoBehaviour>())
                {
                    IStoppable[] stoppables = obj.GetComponents<IStoppable>();
                    foreach (IStoppable stoppable in stoppables)
                    {
                        stoppable.IsStopped = true;
                    }
                }
            }
        }

        public void StartAll()
        {
            Debug.Log("Start All Components Called");
            StartCoroutine(StartAllCoroutine());
        }

        private IEnumerator StartAllCoroutine()
        {
            // Loadが完了するのを待つ
            LoadCompleteManager loadCompleteManager = LoadCompleteManager.Instance;
            if (loadCompleteManager != null)
            {
                while (!loadCompleteManager.IsLoaded)
                {
                    yield return null;
                }
                Debug.Log("Start All Coroutines");
                foreach (MonoBehaviour obj in FindObjectsOfType<MonoBehaviour>())
                {
                    IStoppable[] stoppables = obj.GetComponents<IStoppable>();
                    foreach (IStoppable stoppable in stoppables)
                    {
                        stoppable.IsStopped = false;
                    }
                }
            }
        }
    }
}
