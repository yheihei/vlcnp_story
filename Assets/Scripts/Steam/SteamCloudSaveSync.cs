using System;
using UnityEngine;

#if !UNITY_WEBGL
using Steamworks;
#endif

namespace VLCNP.Steam
{
    public static class SteamCloudSaveSync
    {
        public const string CloudFilePattern = "*.json";
        private const string LogPrefix = "[SteamCloudSaveSync]";

        public static IDisposable BeginFileWriteBatch(string path)
        {
#if !UNITY_WEBGL
            if (!SteamBootstrap.IsInitialized)
            {
                return NullScope.Instance;
            }

            try
            {
                bool started = SteamRemoteStorage.BeginFileWriteBatch();
                if (!started)
                {
                    Debug.LogWarning($"{LogPrefix} Steam file write batch was already in progress. path={path}");
                    return NullScope.Instance;
                }

                Debug.Log($"{LogPrefix} Begin file write batch. path={path}");
                return new FileWriteBatchScope(path);
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"{LogPrefix} BeginFileWriteBatch failed. path={path} error={exception.Message}");
            }
#endif

            return NullScope.Instance;
        }

        public static void LogStatus(string saveDirectoryPath)
        {
#if !UNITY_WEBGL
            if (!SteamBootstrap.IsInitialized)
            {
                return;
            }

            try
            {
                bool accountEnabled = SteamRemoteStorage.IsCloudEnabledForAccount();
                bool appEnabled = SteamRemoteStorage.IsCloudEnabledForApp();
                bool quotaAvailable = SteamRemoteStorage.GetQuota(out ulong totalBytes, out ulong availableBytes);

                if (quotaAvailable)
                {
                    Debug.Log($"{LogPrefix} Cloud status accountEnabled={accountEnabled} appEnabled={appEnabled} quotaTotalBytes={totalBytes} quotaAvailableBytes={availableBytes} saveDirectory={saveDirectoryPath} pattern={CloudFilePattern}");
                }
                else
                {
                    Debug.LogWarning($"{LogPrefix} Cloud quota unavailable. accountEnabled={accountEnabled} appEnabled={appEnabled} saveDirectory={saveDirectoryPath} pattern={CloudFilePattern}");
                }
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"{LogPrefix} Cloud status check failed. error={exception.Message}");
            }
#endif
        }

        private sealed class FileWriteBatchScope : IDisposable
        {
            private readonly string path;
            private bool disposed;

            public FileWriteBatchScope(string path)
            {
                this.path = path;
            }

            public void Dispose()
            {
                if (disposed)
                {
                    return;
                }

                disposed = true;

#if !UNITY_WEBGL
                try
                {
                    bool ended = SteamRemoteStorage.EndFileWriteBatch();
                    if (ended)
                    {
                        Debug.Log($"{LogPrefix} End file write batch. path={path}");
                    }
                    else
                    {
                        Debug.LogWarning($"{LogPrefix} EndFileWriteBatch returned false. path={path}");
                    }
                }
                catch (Exception exception)
                {
                    Debug.LogWarning($"{LogPrefix} EndFileWriteBatch failed. path={path} error={exception.Message}");
                }
#endif
            }
        }

        private sealed class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new NullScope();

            private NullScope()
            {
            }

            public void Dispose()
            {
            }
        }
    }
}
