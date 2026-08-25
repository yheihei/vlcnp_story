using System.Collections;
using Fungus;
using UnityEngine;

namespace VLCNP.Control
{
    /**
     * 同じGameObjectのFlowchartのブロックが実行を始めたらStoppableController.StopAllで操作を止め、
     * 全ブロック(メニューの選択待ち・選択先ブロックを含む)が終わったらStartAllで解除するガード。
     * 止めたいFlowchartに付けて使う(opt-in)。ブロック毎にStopAll/StartAllコマンドを積む必要はない。
     * シーンにStoppableControllerが無い場合(オープニング等)は何もしない。
     */
    [RequireComponent(typeof(Flowchart))]
    public class FlowchartStopAllGuard : MonoBehaviour
    {
        Flowchart flowchart;
        Coroutine watching;

        void Awake()
        {
            flowchart = GetComponent<Flowchart>();
        }

        void OnEnable()
        {
            BlockSignals.OnBlockStart += HandleBlockStart;
        }

        void OnDisable()
        {
            BlockSignals.OnBlockStart -= HandleBlockStart;
            // 実行監視中に無効化・破棄された場合、止めっぱなしにしない
            if (watching != null)
            {
                watching = null;
                StoppableController.FindInScene()?.StartAll();
            }
        }

        void HandleBlockStart(Block block)
        {
            if (watching != null) return;
            if (block == null || block.GetFlowchart() != flowchart) return;
            StoppableController controller = StoppableController.FindInScene();
            if (controller == null) return;
            controller.StopAll();
            watching = StartCoroutine(WatchUntilFinished(controller));
        }

        IEnumerator WatchUntilFinished(StoppableController controller)
        {
            while (true)
            {
                yield return new WaitUntil(() => flowchart.HasExecutingBlocks() == false);
                // メニューが開いていたら、閉じたあとに選択先ブロックの実行を待ち直す
                MenuDialog menuDialog = MenuDialog.ActiveMenuDialog;
                if (menuDialog != null && menuDialog.gameObject.activeSelf)
                {
                    yield return new WaitUntil(() => menuDialog.gameObject.activeSelf == false);
                    yield return null;
                    continue;
                }
                break;
            }
            watching = null;
            controller.StartAll();
        }
    }
}
