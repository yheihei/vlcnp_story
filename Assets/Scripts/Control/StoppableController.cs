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
                IStoppable[] stoppables = obj.GetComponents<IStoppable>();
                foreach (IStoppable stoppable in stoppables)
                {
                    stoppable.IsStopped = true;
                }
            }
        }

        public void StartAll()
        {
            foreach (MonoBehaviour obj in FindObjectsOfType<MonoBehaviour>())
            {
                IStoppable[] stoppables = obj.transform.GetComponents<IStoppable>();
                foreach (IStoppable stoppable in stoppables)
                {
                    stoppable.IsStopped = false;
                }
            }
        }
    }
}
