using UnityEngine;

namespace VLCNP.Core
{
    /**
     * 体験版完了挨拶シーン(TrialEnding_3)に置き、シーン到達を trialEnd として計測する
     */
    public class TrialEndAnalytics : MonoBehaviour
    {
        void Start()
        {
            VLCNPAnalytics.RecordTrialEnd();
        }
    }
}
