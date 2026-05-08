using System.Collections;
using Fungus;
using UnityEngine;
using VLCNP.Core;

namespace VLCNP.SceneManagement
{
    public class VLMitamaJoinedAutoEvent : MonoBehaviour
    {
        [SerializeField]
        string leeleeLine = "これで VLミタマも仲間やな";

        [SerializeField]
        string speakerName = "VLリーリー";

        IEnumerator Start()
        {
            yield return null;

            SayDialog sayDialog = SayDialog.GetSayDialog();
            if (sayDialog == null)
            {
                SetJoinedFlag();
                yield break;
            }

            bool completed = false;
            sayDialog.SetActive(true);
            sayDialog.SetCharacterName(speakerName, Color.white);
            sayDialog.SetCharacterImage(null);
            sayDialog.Say(
                leeleeLine,
                true,
                true,
                true,
                true,
                false,
                null,
                () => completed = true
            );

            while (!completed)
            {
                yield return null;
            }

            SetJoinedFlag();
        }

        void SetJoinedFlag()
        {
            FlagManager flagManager = FindObjectOfType<FlagManager>();
            flagManager?.SetFlag(Flag.VLMitamaJoined, true);
        }
    }
}
