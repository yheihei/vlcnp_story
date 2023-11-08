using System.Collections;
using System.Collections.Generic;
using Fungus;
using UnityEngine;
using VLCNP.Control;
using VLCNP.Core;

namespace VLCNP.Movie
{
    public class KowaseKowase : MonoBehaviour
    {
        [SerializeField] Flowchart flowChart;
        StoppableController stoppableController;
        FlagManager flagManager;

        private void Awake()
        {
            stoppableController = GetComponent<StoppableController>();
            flagManager = GameObject.FindWithTag("FlagManager").GetComponent<FlagManager>();
        }

        private void Start()
        {
            if (flagManager.GetFlag(Flag.LeeleeFukkatuDone)) return;
            StartCoroutine(Talk());
        }

        IEnumerator Talk() {
            stoppableController.StopAll();
            flowChart.ExecuteBlock("Message");
            yield return new WaitUntil(() => flowChart.GetExecutingBlocks().Count == 0);
            stoppableController.StartAll();
        }
    }
}
