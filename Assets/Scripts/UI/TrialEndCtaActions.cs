using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using VLCNP.Steam;

#if !UNITY_WEBGL
using Steamworks;
#endif

namespace VLCNP.UI
{
    /**
     * 体験版終了 CTA からストアページとタイトル画面へ遷移する。
     */
    public sealed class TrialEndCtaActions : MonoBehaviour
    {
        private const string LogPrefix = "[TrialEndCTA]";

        [SerializeField]
        private string storePageUrl = "https://store.steampowered.com/app/4829520/VLCNP_Story/";

        [SerializeField]
        private int titleSceneBuildIndex;

        private bool isReturningToTitle;

        public void OpenWishlist()
        {
#if !UNITY_WEBGL
            if (TryOpenSteamOverlay())
            {
                return;
            }
#endif

            Debug.Log($"{LogPrefix} Opening store page in the default browser. url={storePageUrl}");
            Application.OpenURL(storePageUrl);
        }

        public void BackToTitle()
        {
            if (isReturningToTitle)
            {
                return;
            }

            isReturningToTitle = true;
            Debug.Log($"{LogPrefix} Returning to title. sceneBuildIndex={titleSceneBuildIndex}");
            SceneManager.LoadSceneAsync(titleSceneBuildIndex);
        }

#if !UNITY_WEBGL
        private bool TryOpenSteamOverlay()
        {
            if (!SteamBootstrap.IsInitialized)
            {
                return false;
            }

            try
            {
                if (!SteamUtils.IsOverlayEnabled())
                {
                    Debug.LogWarning($"{LogPrefix} Steam overlay is unavailable; using browser fallback.");
                    return false;
                }

                SteamFriends.ActivateGameOverlayToWebPage(storePageUrl);
                Debug.Log($"{LogPrefix} Opened store page in Steam overlay. url={storePageUrl}");
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"{LogPrefix} Steam overlay failed; using browser fallback. error={exception.Message}");
                return false;
            }
        }
#endif
    }
}
