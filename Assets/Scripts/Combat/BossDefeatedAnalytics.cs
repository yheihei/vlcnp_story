using UnityEngine;
using VLCNP.Attributes;
using VLCNP.Core;

namespace VLCNP.Combat
{
    /**
     * defeatedFlag を持たないボスの撃破を計測する。
     * ボスの Health と同じオブジェクトに付け、死亡確定時に bossDefeated(bossName)を送る
     */
    [RequireComponent(typeof(Health))]
    public class BossDefeatedAnalytics : MonoBehaviour
    {
        [SerializeField]
        string bossName = "";

        Health health;

        void Awake()
        {
            health = GetComponent<Health>();
            health.onDieStarted += HandleDieStarted;
        }

        void OnDestroy()
        {
            if (health != null)
                health.onDieStarted -= HandleDieStarted;
        }

        void HandleDieStarted()
        {
            VLCNPAnalytics.RecordBossDefeated(string.IsNullOrEmpty(bossName) ? gameObject.name : bossName);
        }
    }
}
