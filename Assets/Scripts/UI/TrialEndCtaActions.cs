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
        private string xUrl = "https://x.com/yhei_hei";

        [SerializeField]
        private int titleSceneBuildIndex;

        private bool isReturningToTitle;

        public void OpenWishlist()
        {
            OpenPage(storePageUrl);
        }

        public void OpenX()
        {
            OpenPage(xUrl);
        }

        private void OpenPage(string url)
        {
#if !UNITY_WEBGL
            if (TryOpenSteamOverlay(url))
            {
                return;
            }
#endif

            Debug.Log($"{LogPrefix} Opening page in the default browser. url={url}");
            Application.OpenURL(url);
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
        private bool TryOpenSteamOverlay(string url)
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

                SteamFriends.ActivateGameOverlayToWebPage(url);
                Debug.Log($"{LogPrefix} Opened page in Steam overlay. url={url}");
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
