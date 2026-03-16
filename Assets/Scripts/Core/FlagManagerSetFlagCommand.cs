using Fungus;
using UnityEngine;

namespace VLCNP.Core
{
    [CommandInfo("Control",
                 "Set Flag",
                 "Finds the scene FlagManager and sets the specified Flag value.")]
    [AddComponentMenu("")]
    public class FlagManagerSetFlagCommand : Command
    {
        [Tooltip("Flag to update on the scene's FlagManager.")]
        [SerializeField] protected Flag flag = Flag.None;

        [Tooltip("Value to assign to the selected Flag.")]
        [SerializeField] protected bool value = true;

        public override void OnEnter()
        {
            FlagManager manager = FlagManager.FindInScene();
            if (manager == null)
            {
                Debug.LogWarning("[FlagManagerSetFlagCommand] No FlagManager was found in the scene.");
                Continue();
                return;
            }

            manager.SetFlag(flag, value);
            Continue();
        }

        public override string GetSummary()
        {
            return flag + " = " + value;
        }

        public override Color GetButtonColor()
        {
            return new Color32(170, 215, 190, 255);
        }
    }
}
