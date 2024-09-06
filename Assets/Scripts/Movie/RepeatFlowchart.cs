using System.Collections;
using System.Collections.Generic;
using Fungus;
using UnityEngine;

namespace VLCNP.Movie
{
    public class RepeatFlowchart : MonoBehaviour
    {
        public Flowchart flowchart;
        public string blockName;

        private void FixedUpdate()
        {
            // 今flowchartが実行中の場合何もしない
            if (flowchart.HasExecutingBlocks())
            {
                return;
            }
            // flowchartが終わっていたら指定のブロックを実行
            flowchart.ExecuteBlock(blockName);
        }
    }
}
