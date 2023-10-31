using UnityEngine;
using VLCNP.Core;

namespace VLCNP.Control
{
    public class StoppableController : MonoBehaviour
    {
        public void StopAll()
        {
            // ゲームの中でIStoppableを実装しているものを全て取得する
            foreach (MonoBehaviour obj in FindObjectsOfType<MonoBehaviour>())
            {
                IStoppable stoppable = obj.GetComponent<IStoppable>();
                if (stoppable == null) continue;
                stoppable.IsStopped = true;
            }
        }

        public void StartAll()
        {
            foreach (MonoBehaviour obj in FindObjectsOfType<MonoBehaviour>())
            {
                IStoppable stoppable = obj.transform.GetComponent<IStoppable>();
                if (stoppable == null) continue;
                stoppable.IsStopped = false;
            }
        }
    }
}
