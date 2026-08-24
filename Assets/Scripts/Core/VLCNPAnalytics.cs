using Unity.Services.Analytics;
using Unity.Services.Core;
using UnityEngine;

namespace VLCNP.Core
{
    public class VLCNPAnalytics : MonoBehaviour
    {
        public void ReachedTrial1End()
        {
            // Developmentビルド・エディタでは収集を開始していないため送信しない
            if (Debug.isDebugBuild) return;
            AnalyticsService.Instance.RecordEvent("trial1End");
            Debug.Log("Trial completion event recorded");
        }
    }    
}
