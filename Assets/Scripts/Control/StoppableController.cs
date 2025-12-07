using System.Collections;
using Fungus;
using UnityEngine;
using UnityAsyncAwaitUtil;
using VLCNP.Core;

namespace VLCNP.Control
{
    public class StoppableController : MonoBehaviour
    {
        public void StopAll()
        {
            Debug.Log("Stop All Components Called");
            StartSafeCoroutine(StopAllCoroutine());
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
                // ゲームの中で IStoppable を実装しているものを全て取得する（非アクティブ/子も含む）
                var seen = new System.Collections.Generic.HashSet<IStoppable>();
                foreach (MonoBehaviour obj in FindObjectsOfType<MonoBehaviour>(true))
                {
                    foreach (IStoppable stoppable in obj.GetComponentsInChildren<IStoppable>(true))
                    {
                        if (!seen.Add(stoppable)) continue;
                        stoppable.IsStopped = true;
                    }
                }
            }
        }

        public void StartAll()
        {
            Debug.Log("Start All Components Called");
            StartSafeCoroutine(StartAllCoroutine());
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
                var seen = new System.Collections.Generic.HashSet<IStoppable>();
                foreach (MonoBehaviour obj in FindObjectsOfType<MonoBehaviour>(true))
                {
                    foreach (IStoppable stoppable in obj.GetComponentsInChildren<IStoppable>(true))
                    {
                        if (!seen.Add(stoppable)) continue;
                        stoppable.IsStopped = false;
                    }
                }
            }
        }

        /// <summary>
        /// 自分のGameObjectが非アクティブでもコルーチンを実行するためのラッパー。
        /// 非アクティブ時は AsyncCoroutineRunner (DontDestroyOnLoad) に委譲する。
        /// </summary>
        private Coroutine StartSafeCoroutine(IEnumerator routine)
        {
            if (isActiveAndEnabled)
            {
                return StartCoroutine(routine);
            }

            Debug.LogWarning("[StoppableController] inactive GameObject detected. Running coroutine via AsyncCoroutineRunner.");
            return AsyncCoroutineRunner.Instance.StartCoroutine(routine);
        }
    }
}
