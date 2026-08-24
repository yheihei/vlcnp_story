using Fungus;
using UnityEngine;

namespace VLCNP.Core
{
    /** 指定フラグが立っているときだけ選択肢を表示するMenuコマンド */
    [CommandInfo("Narrative",
                 "Flag Menu",
                 "指定フラグが立っているときだけ選択肢を表示するMenu")]
    [AddComponentMenu("")]
    public class FlagMenuCommand : Menu
    {
        [Tooltip("この選択肢を表示する条件フラグ。Noneなら常に表示")]
        [SerializeField]
        protected Flag requiredFlag = Flag.None;

        public override void OnEnter()
        {
            if (requiredFlag != Flag.None)
            {
                FlagManager manager = FlagManager.FindInScene();
                if (manager == null || !manager.GetFlag(requiredFlag))
                {
                    Continue();
                    return;
                }
            }
            base.OnEnter();
        }

        public override string GetSummary()
        {
            return $"[{requiredFlag}] {base.GetSummary()}";
        }
    }
}
