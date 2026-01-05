using Fungus;
using UnityEngine;

namespace VLCNP.Control
{
    /// <summary>
    /// Fungus の SayDialog に対してゲームパッド入力をブリッジする常駐コンポーネント。
    /// </summary>
    public class FungusDialogInputBridge : MonoBehaviour
    {
        const string BridgeObjectName = "[FungusDialogInputBridge]";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            if (FindObjectOfType<FungusDialogInputBridge>() != null)
                return;

            GameObject go = new GameObject(BridgeObjectName);
            DontDestroyOnLoad(go);
            go.hideFlags = HideFlags.HideInHierarchy;
            go.AddComponent<FungusDialogInputBridge>();
        }

        void Update()
        {
            SayDialog sayDialog = SayDialog.ActiveSayDialog;
            if (sayDialog == null || !sayDialog.gameObject.activeInHierarchy)
            {
                return;
            }

            MenuDialog menu = MenuDialog.ActiveMenuDialog;
            if (menu != null && menu.IsActive() && menu.DisplayedOptionsCount > 0)
            {
                return;
            }

            DialogInput dialogInput = sayDialog.GetComponent<DialogInput>();
            if (dialogInput == null)
            {
                return;
            }

            if (PlayerInputAdapter.WasMenuSubmitPressed())
            {
                dialogInput.SetNextLineFlag();
            }
        }
    }
}
