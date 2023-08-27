using UnityEngine;
using Fungus;
using System.Threading;
using System.Collections;
using VLCNP.Core;
using VLCNP.UI;

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
        [SerializeField]
        public string InformationText = null;

        bool isAction = true;

        InformationText informationTextObject = null;

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

        public void ShowInformation()
        {
            if (InformationText == null) return;
            InformationTextSpawner spawner = GetComponent<InformationTextSpawner>();
            if (spawner == null) return;
            if (informationTextObject != null) return;
            informationTextObject = spawner.Spawn(InformationText);
        }

        public void HideInformation()
        {
            if (informationTextObject == null) return;
            Destroy(informationTextObject.gameObject);
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
