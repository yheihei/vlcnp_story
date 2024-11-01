using System.Collections;
using System.Collections.Generic;
using Fungus;
using UnityEngine;
using VLCNP.Core;

namespace VLCNP.Movie
{
    public class SoftBlockBroken : MonoBehaviour
    {
        [SerializeField] GameObject parentObject;
        [SerializeField] Flowchart flowchart;
        [SerializeField] string blockName = "Message";
        [SerializeField] Flag isDoneFlag;
        FlagManager flagManager;

        int initialChildCount = 0;
        bool isDone = false;

        void Awake()
        {
            initialChildCount = parentObject.transform.childCount;
            flagManager = GameObject.FindWithTag("FlagManager").GetComponent<FlagManager>();
        }

        void FixedUpdate()
        {
            // すでにisDoneFlagが立っていたらオブジェクトを破棄して終了
            if (flagManager.GetFlag(isDoneFlag))
            {
                Destroy(gameObject);
                return;
            }
            if (isDone) return;
            // Activeな子オブジェクトの数を数える
            int activeChildCount = 0;
            foreach (Transform child in parentObject.transform)
            {
                if (child.gameObject.activeSelf) activeChildCount++;
            }
            // Activeな子オブジェクトの数が初期値から3つ減ったらBlockを実行
            if (activeChildCount <= initialChildCount - 3)
            {
                flowchart.ExecuteBlock(blockName);
                isDone = true;
                flagManager.SetFlag(isDoneFlag, true);
            }
        }
    }
}
