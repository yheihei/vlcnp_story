using Fungus;
using UnityEngine;

namespace VLCNP.Control
{
    [CommandInfo("Control",
                 "Increment Health Level",
                 "Finds the scene Party and increments the party health level.")]
    [AddComponentMenu("")]
    public class PartyIncrementHealthLevelCommand : Command
    {
        public override void OnEnter()
        {
            PartyCongroller controller = PartyCongroller.FindInScene();
            if (controller == null)
            {
                Debug.LogWarning("[PartyIncrementHealthLevelCommand] No PartyCongroller was found in the scene.");
                Continue();
                return;
            }

            controller.IncrementHealthLevel();
            Continue();
        }

        public override string GetSummary()
        {
            return "IncrementHealthLevel()";
        }

        public override Color GetButtonColor()
        {
            return new Color32(170, 215, 190, 255);
        }
    }
}
