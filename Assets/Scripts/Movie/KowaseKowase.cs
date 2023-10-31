using System.Collections;
using System.Collections.Generic;
using Fungus;
using UnityEngine;
using VLCNP.Control;

namespace VLCNP.Movie
{
    public class KowaseKowase : MonoBehaviour
    {
        [SerializeField] Flowchart flowChart;
        StoppableController stoppableController;

        private void Awake()
        {
            stoppableController = GetComponent<StoppableController>();
        }

        private void Start()
        {
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
