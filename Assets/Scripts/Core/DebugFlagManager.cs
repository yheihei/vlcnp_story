using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VLCNP.Core;

namespace VLCNP.Core
{
    public class DebugFlagManager : MonoBehaviour
    {
        FlagManager flagManager;

        // デバッグ時のみONにするフラグのリストをUIから設定させる
        [SerializeField]
        public Flag[] trueFlags;

#if DEVELOPMENT_BUILD || UNITY_EDITOR
        void Awake()
        {
            // FlagManagerをタグから取得
            flagManager = GameObject
                .FindGameObjectWithTag("FlagManager")
                .GetComponent<FlagManager>();
        }

        void Start()
        {
            foreach (var flag in trueFlags)
            {
                flagManager.SetFlag(flag, true);
            }
        }
#endif
    }
}
