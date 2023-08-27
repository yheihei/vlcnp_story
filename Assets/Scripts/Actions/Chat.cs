using UnityEngine;
using Fungus;
using System.Threading;
using System.Collections;
using VLCNP.Core;

namespace VLCNP.Actions
{
    public class Chat : MonoBehaviour, ICollisionAction
    {
        [SerializeField]
        public Flowchart flowChart;
        [SerializeField]
        public string BlockName = "Message";
        
        [SerializeField]
        public string TargetTag = "Player";

        bool isAction = true;

        public bool IsAction { get => isAction; set => isAction = value; }

        public void Execute()
        {
            if (!isAction) return;
            if (flowChart.HasExecutingBlocks()) return;
            StartCoroutine(Talk());
        }

        IEnumerator Talk() {
            isAction = false;
            StopAll();
            flowChart.ExecuteBlock(BlockName);
            foreach (Variable variable in flowChart.Variables)
            {
                print(variable.Key + " : " + variable.GetValue());
            }
            yield return new WaitUntil(() => flowChart.GetExecutingBlocks().Count == 0);
            StartAll();
            isAction = true;
        }

        private void StopAll()
        {
            foreach (MonoBehaviour obj in FindObjectsOfType<MonoBehaviour>())
            {
                IStoppable stoppable = obj.GetComponent<IStoppable>();
                if (stoppable == null) continue;
                stoppable.IsStopped = true;
            }
        }

        private void StartAll()
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
