using System.Collections;
using Fungus;
using UnityEngine;
using UnityAsyncAwaitUtil;
using VLCNP.Core;

namespace VLCNP.Control
{
    public class StoppableController : MonoBehaviour
    {
        public const string TagName = "StoppableController";

        public static StoppableController FindInScene()
        {
            GameObject taggedObject = GameObject.FindWithTag(TagName);
            if (taggedObject != null &&
                taggedObject.TryGetComponent(out StoppableController controller))
            {
                return controller;
            }

            StoppableController[] controllers = FindObjectsOfType<StoppableController>(true);
            foreach (StoppableController candidate in controllers)
            {
                if (candidate != null &&
                    candidate.CompareTag(TagName))
                {
                    return candidate;
                }
            }

            return controllers.Length > 0 ? controllers[0] : null;
        }

        public void StopAll()
        {
            PerfLog.Log("Stop All Components Called");
            StartSafeCoroutine(StopAllCoroutine());
        }

        private IEnumerator StopAllCoroutine()
        {
            LoadCompleteManager loadCompleteManager = LoadCompleteManager.Instance;
            bool hadManager = loadCompleteManager != null;
            // LoadCompleteManagerが存在し、ロードが完了していない場合は待つ
            while (loadCompleteManager != null && !loadCompleteManager.IsLoaded)
            {
                yield return null;
            }
            // 待機中にシーン遷移でLoadCompleteManagerが破棄された場合は、停止対象のシーンごと消えているため中止
            if (hadManager && loadCompleteManager == null) yield break;
            // LoadCompleteManagerが無い場合でも停止は必ず実行する（以前は何もせず終了し、イベント中に操作できてしまった）
            PerfLog.Log("Stop All Components");
            SetAllStoppables(true);
        }

        public void StartAll()
        {
            PerfLog.Log("Start All Components Called");
            StartSafeCoroutine(StartAllCoroutine());
        }

        private IEnumerator StartAllCoroutine()
        {
            LoadCompleteManager loadCompleteManager = LoadCompleteManager.Instance;
            bool hadManager = loadCompleteManager != null;
            // Loadが完了するのを待つ
            while (loadCompleteManager != null && !loadCompleteManager.IsLoaded)
            {
                yield return null;
            }
            // 待機中にシーン遷移でLoadCompleteManagerが破棄された場合は中止
            if (hadManager && loadCompleteManager == null) yield break;
            PerfLog.Log("Start All Coroutines");
            SetAllStoppables(false);
        }

        // ゲームの中で IStoppable を実装しているものを全て取得し停止状態を設定する（非アクティブ/子も含む）
        private void SetAllStoppables(bool isStopped)
        {
            var seen = new System.Collections.Generic.HashSet<IStoppable>();
            foreach (MonoBehaviour obj in FindObjectsOfType<MonoBehaviour>(true))
            {
                IStoppable stoppable = obj as IStoppable;
                if (stoppable == null) continue;
                if (!seen.Add(stoppable)) continue;
                stoppable.IsStopped = isStopped;
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
