using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VLCNP.Core
{
    public class FlagSetter : MonoBehaviour
    {
        [SerializeField]
        Flag[] flags = null;
        FlagManager flagManager = null;

        private void Start()
        {
            flagManager = GameObject.FindWithTag("FlagManager").GetComponent<FlagManager>();
        }

        public void SetNextFlag()
        {
            // flagsを順番に見て言って、まだFalseのものをTrueにして終了
            foreach (Flag flag in flags)
            {
                if (!flagManager.GetFlag(flag))
                {
                    flagManager.SetFlag(flag, true);
                    return;
                }
            }
        }

        public void SetFlag(string flagString)
        {
            // flagの文字列をENUMに変換して、そのフラグをTrueにする
            Flag flag = (Flag)System.Enum.Parse(typeof(Flag), flagString);
            flagManager.SetFlag(flag, true);
        }
    }
}
