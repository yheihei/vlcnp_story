using System.Linq;
using System.Reflection;
using Fungus;
using UnityEditor;
using UnityEngine;
using VLCNP.Control;
using VLCNP.Core;

namespace VLCNP.Editor
{
    /**
     * FlowchartStopAllGuardのプレイモード検証用デバッグメニュー。
     * プレイモード中にunicli exec Menu.Executeから叩き、結果はConsole.GetLogで読む。
     */
    public static class FlowchartGuardVerifyMenu
    {
        [MenuItem("Tools/GuardVerify/Report State")]
        public static void ReportState()
        {
            FieldInfo field = typeof(FlowchartStopAllGuard).GetField(
                "activeGuards", BindingFlags.NonPublic | BindingFlags.Static);
            int active = (int)field.GetValue(null);
            string players = string.Join(
                ", ",
                Object.FindObjectsOfType<PlayerController>(true)
                    .Select(p => $"{p.name}:IsStopped={p.IsStopped}:active={p.gameObject.activeInHierarchy}"));
            string executing = string.Join(
                ", ",
                Object.FindObjectsOfType<Flowchart>(true)
                    .Where(f => f.HasExecutingBlocks())
                    .Select(f => f.transform.root.name + "/" + f.name));
            bool menuOpen = MenuDialog.ActiveMenuDialog != null
                && MenuDialog.ActiveMenuDialog.gameObject.activeSelf;
            Debug.Log($"[GuardVerify] activeGuards={active} menuOpen={menuOpen} players=[{players}] executing=[{executing}]");
        }

        [MenuItem("Tools/GuardVerify/Advance Dialog")]
        public static void AdvanceDialog()
        {
            DialogInput input = Object.FindObjectsOfType<DialogInput>(false)
                .FirstOrDefault(d => d.gameObject.activeInHierarchy);
            if (input == null)
            {
                Debug.Log("[GuardVerify] no active DialogInput");
                return;
            }
            input.SetNextLineFlag();
            Debug.Log("[GuardVerify] advanced dialog");
        }

        [MenuItem("Tools/GuardVerify/Click Menu Option 1")]
        public static void ClickMenuOption1() => ClickMenuOption(0);

        [MenuItem("Tools/GuardVerify/Click Menu Option 2")]
        public static void ClickMenuOption2() => ClickMenuOption(1);

        [MenuItem("Tools/GuardVerify/Click Menu Option 5")]
        public static void ClickMenuOption5() => ClickMenuOption(4);

        [MenuItem("Tools/GuardVerify/Click Last Menu Option")]
        public static void ClickLastMenuOption()
        {
            MenuDialog dialog = MenuDialog.ActiveMenuDialog;
            if (dialog == null || !dialog.gameObject.activeSelf)
            {
                Debug.Log("[GuardVerify] no active MenuDialog");
                return;
            }
            int visible = dialog.CachedButtons.Count(b => b.gameObject.activeSelf);
            ClickMenuOption(visible - 1);
        }

        [MenuItem("Tools/GuardVerify/Set Flag VLNarukamiJoined")]
        public static void SetNarukamiJoined()
        {
            GameObject.FindWithTag("FlagManager").GetComponent<FlagManager>()
                .SetFlag(Flag.VLNarukamiJoined, true);
            Debug.Log("[GuardVerify] set VLNarukamiJoined");
        }

        static void ClickMenuOption(int index)
        {
            MenuDialog dialog = MenuDialog.ActiveMenuDialog;
            if (dialog == null || !dialog.gameObject.activeSelf)
            {
                Debug.Log("[GuardVerify] no active MenuDialog");
                return;
            }
            var buttons = dialog.CachedButtons.Where(b => b.gameObject.activeSelf).ToArray();
            if (index >= buttons.Length)
            {
                Debug.Log($"[GuardVerify] option index {index} out of range (visible={buttons.Length})");
                return;
            }
            Debug.Log($"[GuardVerify] clicking option {index}: {buttons[index].GetComponentInChildren<TMPro.TMP_Text>()?.text}");
            buttons[index].onClick.Invoke();
        }

        [MenuItem("Tools/GuardVerify/Execute Yumekawakitune (flag20)")]
        public static void ExecuteYumekawakitune() => SetFlagAndExecute(Flag.IkehayaBlockChainChated, "Yumekawakitune");

        [MenuItem("Tools/GuardVerify/Execute Deguchi AfterOrochi (flag26)")]
        public static void ExecuteDeguchi() => SetFlagAndExecute(Flag.IkehayaBlockChainChated, "DeguchiGameEvent");

        [MenuItem("Tools/GuardVerify/Execute ToKaze3Boss (flag48)")]
        public static void ExecuteToKaze3Boss() => SetFlagAndExecute(Flag.VLKamaitachi1Defeated2, "GameEventTransitionToKaze3Boss");

        [MenuItem("Tools/GuardVerify/Execute CheckPointGameEvent")]
        public static void ExecuteCheckPoint() => SetFlagAndExecute(Flag.None, "CheckPointGameEvent");

        [MenuItem("Tools/GuardVerify/Execute Tuti GameEventTransition_2 (flag26)")]
        public static void ExecuteTutiTransition2() => SetFlagAndExecute(Flag.VLOrochiJoined, "GameEventTransition_2");

        static void SetFlagAndExecute(Flag flag, string gameObjectName)
        {
            if (flag != Flag.None)
            {
                GameObject.FindWithTag("FlagManager").GetComponent<FlagManager>().SetFlag(flag, true);
            }
            GameObject target = GameObject.Find(gameObjectName);
            if (target == null)
            {
                Debug.Log($"[GuardVerify] {gameObjectName} not found");
                return;
            }
            GameEvent gameEvent = target.GetComponent<GameEvent>();
            if (gameEvent == null)
            {
                Debug.Log($"[GuardVerify] {gameObjectName} has no GameEvent");
                return;
            }
            Debug.Log($"[GuardVerify] executing {gameObjectName}");
            gameEvent.Execute();
        }
    }
}
