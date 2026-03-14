using Fungus;
using UnityEngine;

namespace VLCNP.Control
{
    public enum StoppableControllerCommandAction
    {
        StopAll,
        StartAll
    }

    [CommandInfo("Control",
                 "Stoppable Controller",
                 "Finds the scene StoppableController and invokes StopAll or StartAll.")]
    [AddComponentMenu("")]
    public class StoppableControllerCommand : Command
    {
        [Tooltip("Action to invoke on the scene's StoppableController.")]
        [SerializeField] protected StoppableControllerCommandAction action = StoppableControllerCommandAction.StopAll;

        public override void OnEnter()
        {
            StoppableController controller = StoppableController.FindInScene();
            if (controller == null)
            {
                Debug.LogWarning("[StoppableControllerCommand] No StoppableController was found in the scene.");
                Continue();
                return;
            }

            switch (action)
            {
                case StoppableControllerCommandAction.StartAll:
                    controller.StartAll();
                    break;
                default:
                    controller.StopAll();
                    break;
            }

            Continue();
        }

        public override string GetSummary()
        {
            return action.ToString();
        }

        public override Color GetButtonColor()
        {
            return new Color32(170, 215, 190, 255);
        }
    }
}
