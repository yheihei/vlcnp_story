using UnityEngine;
using Fungus;
using System.Threading;
using System.Collections;
using VLCNP.Core;
using VLCNP.UI;
using VLCNP.Saving;
using Newtonsoft.Json.Linq;

namespace VLCNP.Actions
{
    public class Chat : MonoBehaviour, ICollisionAction, IJsonSaveable
    {
        [SerializeField]
        public Flowchart flowChart;
        [SerializeField]
        public string BlockName = "Message";
        
        [SerializeField]
        public string TargetTag = "Player";
        [SerializeField]
        public string InformationText = null;

        [SerializeField]
        public bool IsOnce = false;

        bool isOnceDone = false;

        bool isAction = true;

        InformationText informationTextObject = null;

        public bool IsAction { get => isAction; set => isAction = value; }

        public void Execute()
        {
            if (!isAction) return;
            if (flowChart == null) return;
            if (isOnceDone) return;
            if (flowChart.HasExecutingBlocks()) return;
            StartCoroutine(Talk());
        }

        public void OnTriggerExit2D(Collider2D other)
        {
            if (other.gameObject.tag != TargetTag) return;
            if (IsOnce)
            {
                if (isOnceDone) return;
                isOnceDone = true;
            }
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
            if (isOnceDone) return;
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

        public JToken CaptureAsJToken()
        {
            return JToken.FromObject(isOnceDone);
        }

        public void RestoreFromJToken(JToken state)
        {
            isOnceDone = state.ToObject<bool>();
        }
    }
}
