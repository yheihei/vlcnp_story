using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CLCNP.Core
{
    public class DisplayAnalyticsUserId : MonoBehaviour
    {
        Text text;
        void Awake()
        {
            text = GetComponent<Text>();
        }

        void Start()
        {
            text.text = Unity.Services.Analytics.AnalyticsService.Instance.GetAnalyticsUserID();
        }
    }
}
