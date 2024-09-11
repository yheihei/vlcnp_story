using Unity.Services.Analytics;
using Unity.Services.Core;
using UnityEngine;

namespace VLCNP.Core
{
    public class VLCNPAnalyticsInit : MonoBehaviour
    {
        async void Awake()
        {
            try
            {
                await UnityServices.InitializeAsync();
                Debug.Log("Unity Services initialized successfully");
                // データ収集を開始
                AnalyticsService.Instance.StartDataCollection();
                Debug.Log("Data collection started");
                Debug.Log($"SessionID: {AnalyticsService.Instance.SessionID}");
                Debug.Log($"AnalyticsUserID: {AnalyticsService.Instance.GetAnalyticsUserID()}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error initializing Unity Services: {e.Message}");
            }
        }
    }    
}
