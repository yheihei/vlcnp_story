using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VLCNP.Core
{
    public class VisibilityFlagManager : MonoBehaviour
    {
        [SerializeField]
        VisibilityFlag[] visibilityFlags = null;

        [System.Serializable]
        class VisibilityFlag
        {
            public Flag flag;
            // AND条件の追加フラグ。全てtrueのときだけこのエントリが有効(未設定なら flag 単独で判定)
            public Flag[] additionalFlags;
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
                if (IsConditionMatched(visibilityFlags[i]))
                {
                    gameObject.SetActive(visibilityFlags[i].isVisible);
                    // 子要素すべてをActiveにする
                    foreach (Transform child in transform)
                    {
                        child.gameObject.SetActive(visibilityFlags[i].isVisible);
                    }
                    return;
                }
            }
        }

        bool IsConditionMatched(VisibilityFlag visibilityFlag)
        {
            if (!flagManager.GetFlag(visibilityFlag.flag)) return false;
            if (visibilityFlag.additionalFlags == null) return true;
            foreach (Flag additional in visibilityFlag.additionalFlags)
            {
                if (!flagManager.GetFlag(additional)) return false;
            }
            return true;
        }

        void OnChangeFlag(Flag flag, bool value)
        {
            // 変化のあったflagがvisibilityFlagsに含まれていなければ何もしない 配列のfindで探す
            if (System.Array.Find(visibilityFlags, x => x.flag == flag
                || (x.additionalFlags != null && System.Array.IndexOf(x.additionalFlags, flag) >= 0)) == null)
                return;
            CheckVisibility();
        }
    }
}
