using UnityEngine;
using VLCNP.Core;
using VLCNP.Stats;

namespace VLCNP.Stats
{
    // Akimのレベルを監視し、レベルが変更されたときに AkimVeryLong フラグをOn/Offする
    public class AkimVeryLongFlagObserver : MonoBehaviour
    {
        FlagManager flagManager;
        [SerializeField] BaseStats akimBaseStats;

        private void Start()
        {
            flagManager = GameObject.FindWithTag("FlagManager").GetComponent<FlagManager>();
            print(flagManager);
        }

        private void OnEnable()
        {
            akimBaseStats.OnChangeLevel += ChangeAkimVeryLongFlag;
        }

        private void OnDisable()
        {
            akimBaseStats.OnChangeLevel -= ChangeAkimVeryLongFlag;
        }

        private void ChangeAkimVeryLongFlag(int level)
        {
            if (flagManager == null) return;
            if (level > 1)
            {
                flagManager.SetFlag(Flag.AkimVeryLong, true);
            }
            else
            {
                flagManager.SetFlag(Flag.AkimVeryLong, false);
            }
        }
    }    
}
