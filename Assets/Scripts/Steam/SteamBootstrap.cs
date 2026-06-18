using UnityEngine;

#if !UNITY_WEBGL
using Steamworks;
#endif

namespace VLCNP.Steam
{
    public sealed class SteamBootstrap : MonoBehaviour
    {
        private static SteamBootstrap instance;

#if !UNITY_WEBGL
        private bool initialized;
#endif

        public static bool IsInitialized
        {
            get
            {
#if !UNITY_WEBGL
                return instance != null && instance.initialized;
#else
                return false;
#endif
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Create()
        {
            if (Application.isEditor || instance != null)
            {
                return;
            }

            var gameObject = new GameObject(nameof(SteamBootstrap));
            instance = gameObject.AddComponent<SteamBootstrap>();
            DontDestroyOnLoad(gameObject);
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            Debug.Log($"[SteamBootstrap] persistentDataPath={Application.persistentDataPath}");

#if !UNITY_WEBGL
            if (Application.isEditor)
            {
                return;
            }

            if (!SteamAPI.Init())
            {
                Debug.LogWarning("[SteamBootstrap] SteamAPI.Init failed. Continuing without Steam integration.");
                return;
            }

            initialized = true;
            Debug.Log($"[SteamBootstrap] Steam initialized. AppID={SteamUtils.GetAppID()}");
            SteamCloudSaveSync.LogStatus(Application.persistentDataPath);
#endif
        }

        private void Update()
        {
#if !UNITY_WEBGL
            if (initialized)
            {
                SteamAPI.RunCallbacks();
            }
#endif
        }

        private void OnApplicationQuit()
        {
#if !UNITY_WEBGL
            if (!initialized)
            {
                return;
            }

            SteamAPI.Shutdown();
            initialized = false;
            Debug.Log("[SteamBootstrap] Steam shutdown.");
#endif
        }
    }
}
