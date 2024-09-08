using Unity.Services.Analytics;
using Unity.Services.Core;
using UnityEngine;

namespace VLCNP.Core
{
    public class VLCNPAnalytics : MonoBehaviour
    {
        async void Awake()
        {
            try
            {
                await UnityServices.InitializeAsync();
                Debug.Log("Unity Services initialized successfully");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error initializing Unity Services: {e.Message}");
            }
        }

        public void ReachedTrial1End()
        {
            AnalyticsService.Instance.RecordEvent("trial1End");
        }

    }    
}
