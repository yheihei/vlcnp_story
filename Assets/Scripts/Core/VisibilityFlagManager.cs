using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VLCNP.Core
{
    public class VisibilityFlagManager : MonoBehaviour
    {

        [SerializeField] VisibilityFlag[] visibilityFlags = null;

        [System.Serializable]
        class VisibilityFlag
        {
            public Flag flag;
            public bool isVisible;
        }

        FlagManager flagManager;

        void Awake()
        {
            flagManager = GameObject.FindWithTag("FlagManager").GetComponent<FlagManager>();
        }

        void Start()
        {
            flagManager.OnChangeFlag += OnChangeFlag;
            CheckVisibility();
        }

        void CheckVisibility()
        {
            // 後ろから見ていって、最初に見つかったもので動作させる
            for (int i = visibilityFlags.Length - 1; i >= 0; i--)
            {
                if (flagManager.GetFlag(visibilityFlags[i].flag))
                {
                    gameObject.SetActive(visibilityFlags[i].isVisible);
                    return;
                }
            }
        }

        void OnChangeFlag(Flag flag)
        {
            CheckVisibility();
        }
    }
}
