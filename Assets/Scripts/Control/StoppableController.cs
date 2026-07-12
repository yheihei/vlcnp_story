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

        // StopAll/StartAllの要求順序。ロード待ちでの適用順の入れ替わりを防ぎ、最後の要求だけを適用する
        private static long requestSequence = 0;

        public void StopAll()
        {
            PerfLog.Log("Stop All Components Called");
            StartSafeCoroutine(ApplyAllAfterLoad(true, ++requestSequence));
        }

        public void StartAll()
        {
            PerfLog.Log("Start All Components Called");
            StartSafeCoroutine(ApplyAllAfterLoad(false, ++requestSequence));
        }

        private IEnumerator ApplyAllAfterLoad(bool isStopped, long sequence)
        {
            LoadCompleteManager loadCompleteManager = LoadCompleteManager.Instance;
            bool hadManager = loadCompleteManager != null;
            // LoadCompleteManagerが存在し、ロードが完了していない場合は待つ
            while (loadCompleteManager != null && !loadCompleteManager.IsLoaded)
            {
                yield return null;
            }
            // 待機中にシーン遷移でLoadCompleteManagerが破棄された場合は、対象のシーンごと消えているため中止
            if (hadManager && loadCompleteManager == null) yield break;
            // 待機中に新しい要求が来ていたら古い要求は適用しない
            // （例: チェックポイントのStartAllが後発カットシーンのStopAllを上書きして、イベント中に操作できてしまう競合の防止）
            if (sequence != requestSequence) yield break;
            PerfLog.Log(isStopped ? "Stop All Components" : "Start All Coroutines");
            SetAllStoppables(isStopped);
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
