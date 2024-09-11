using Unity.Services.Analytics;
using Unity.Services.Core;
using UnityEngine;

namespace VLCNP.Core
{
    public class VLCNPAnalytics : MonoBehaviour
    {
        public void ReachedTrial1End()
        {
            AnalyticsService.Instance.RecordEvent("trial1End");
            Debug.Log("Trial completion event recorded");
        }
    }    
}
