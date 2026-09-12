using UnityEngine;
using UnityEngine.UI;

namespace VLCNP.UI
{
    /** 会話の選択状態に合わせて文字色と矢印を切り替えるボタン。 */
    public class DialogueChoiceButton : Button
    {
        [SerializeField] private Text label;
        [SerializeField] private Text selectionMarker;

        private static readonly Color LightText = new Color32(230, 227, 214, 255);
        private static readonly Color DarkText = new Color32(25, 30, 37, 255);
        private static readonly Color DisabledText = new Color32(143, 147, 150, 255);

        protected override void DoStateTransition(SelectionState state, bool instant)
        {
            base.DoStateTransition(state, instant);
            ApplyTextState(state == SelectionState.Selected || state == SelectionState.Pressed,
                state == SelectionState.Disabled);
        }

        protected override void InstantClearState()
        {
            base.InstantClearState();
            ApplyTextState(false, false);
        }

        private void ApplyTextState(bool selected, bool disabled)
        {
            if (label != null)
                label.color = disabled ? DisabledText : selected ? DarkText : LightText;

            if (selectionMarker != null)
            {
                selectionMarker.color = DarkText;
                selectionMarker.enabled = selected && !disabled;
            }
        }
    }
}
