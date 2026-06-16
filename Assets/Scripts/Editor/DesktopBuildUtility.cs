using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace VLCNP.Editor
{
    public static class DesktopBuildUtility
    {
        private const string BuildPathArgName = "-vlcnpBuildPath";
        private const string SteamAppIdArgName = "-vlcnpSteamAppId";
        private const string BuildPathEnvName = "VLCNP_BUILD_PATH";
        private const string SteamAppIdEnvName = "VLCNP_STEAM_APP_ID";
        private const string DefaultWindowsBuildPath = "Builds/Windows/VlcnpStory.exe";
        private const string DefaultMacBuildPath = "Builds/Mac/VlcnpStory.app";
        private const string DefaultSteamDemoWindowsBuildPath = "Builds/SteamDemo/Windows/VlcnpStory.exe";
        private const string DefaultSteamDemoMacBuildPath = "Builds/SteamDemo/Mac/VlcnpStory.app";

        [MenuItem("Tools/VLCNP/Build/Windows Development Build", false, 2100)]
        public static void BuildWindowsDevelopment()
        {
            BuildPlayer(BuildTarget.StandaloneWindows64, DefaultWindowsBuildPath, BuildOptions.Development | BuildOptions.AllowDebugging);
        }

        [MenuItem("Tools/VLCNP/Build/macOS Development Build", false, 2101)]
        public static void BuildMacDevelopment()
        {
            BuildPlayer(BuildTarget.StandaloneOSX, DefaultMacBuildPath, BuildOptions.Development | BuildOptions.AllowDebugging);
        }

        [MenuItem("Tools/VLCNP/Build/Steam Demo Windows Release Build", false, 2110)]
        public static void BuildSteamDemoWindowsRelease()
        {
            BuildPlayer(BuildTarget.StandaloneWindows64, DefaultSteamDemoWindowsBuildPath, BuildOptions.None);
            WriteSteamAppIdIfConfigured(DefaultSteamDemoWindowsBuildPath);
        }

        [MenuItem("Tools/VLCNP/Build/Steam Demo macOS Release Build", false, 2111)]
        public static void BuildSteamDemoMacRelease()
        {
            BuildPlayer(BuildTarget.StandaloneOSX, DefaultSteamDemoMacBuildPath, BuildOptions.None);
            WriteSteamAppIdIfConfigured(DefaultSteamDemoMacBuildPath);
        }

        public static void BuildWindowsDevelopmentFromCommandLine()
        {
            BuildPlayer(BuildTarget.StandaloneWindows64, ResolveBuildPath(DefaultWindowsBuildPath), BuildOptions.Development | BuildOptions.AllowDebugging);
        }

        public static void BuildMacDevelopmentFromCommandLine()
        {
            BuildPlayer(BuildTarget.StandaloneOSX, ResolveBuildPath(DefaultMacBuildPath), BuildOptions.Development | BuildOptions.AllowDebugging);
        }

        public static void BuildSteamDemoWindowsReleaseFromCommandLine()
        {
            string buildPath = ResolveBuildPath(DefaultSteamDemoWindowsBuildPath);
            BuildPlayer(BuildTarget.StandaloneWindows64, buildPath, BuildOptions.None);
            WriteSteamAppIdIfConfigured(buildPath);
        }

        public static void BuildSteamDemoMacReleaseFromCommandLine()
        {
            string buildPath = ResolveBuildPath(DefaultSteamDemoMacBuildPath);
            BuildPlayer(BuildTarget.StandaloneOSX, buildPath, BuildOptions.None);
            WriteSteamAppIdIfConfigured(buildPath);
        }

        private static void BuildPlayer(BuildTarget target, string locationPathName, BuildOptions buildOptions)
        {
            var targetGroup = BuildPipeline.GetBuildTargetGroup(target);
            if (!BuildPipeline.IsBuildTargetSupported(targetGroup, target))
            {
                throw new BuildFailedException(
                    $"Build target is not supported in this Unity installation: {target}. Install the matching Unity build support module."
                );
            }

            string[] scenes = GetEnabledScenes();
            string absoluteLocationPath = Path.GetFullPath(locationPathName);
            string outputDirectory = Path.GetDirectoryName(absoluteLocationPath);
            if (!string.IsNullOrEmpty(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            if (EditorUserBuildSettings.activeBuildTarget != target)
            {
                if (!EditorUserBuildSettings.SwitchActiveBuildTarget(targetGroup, target))
                {
                    throw new BuildFailedException($"Failed to switch active build target to {target}.");
                }
            }

            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = absoluteLocationPath,
                target = target,
                targetGroup = targetGroup,
                options = buildOptions
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            BuildSummary summary = report.summary;
            if (summary.result != BuildResult.Succeeded || summary.totalErrors > 0)
            {
                throw new BuildFailedException(
                    $"Build failed for {target}: {summary.result}, errors={summary.totalErrors}, warnings={summary.totalWarnings}"
                );
            }

            ValidateBuildOutput(target, absoluteLocationPath);

            Debug.Log(
                $"Build succeeded for {target}: {absoluteLocationPath}, size={summary.totalSize} bytes, time={summary.totalTime}"
            );
        }

        private static void ValidateBuildOutput(BuildTarget target, string absoluteLocationPath)
        {
            switch (target)
            {
                case BuildTarget.StandaloneWindows64:
                    ValidateWindowsBuildOutput(absoluteLocationPath);
                    break;
                case BuildTarget.StandaloneOSX:
                    ValidateMacBuildOutput(absoluteLocationPath);
                    break;
                default:
                    ValidateGenericBuildOutput(absoluteLocationPath);
                    break;
            }
        }

        private static void ValidateWindowsBuildOutput(string absoluteLocationPath)
        {
            if (!File.Exists(absoluteLocationPath))
            {
                throw new BuildFailedException($"Windows build executable was not created: {absoluteLocationPath}");
            }

            string buildDirectory = Path.GetDirectoryName(absoluteLocationPath);
            string dataDirectory = Path.Combine(
                buildDirectory,
                $"{Path.GetFileNameWithoutExtension(absoluteLocationPath)}_Data"
            );
            if (!Directory.Exists(dataDirectory))
            {
                throw new BuildFailedException($"Windows build data directory was not created: {dataDirectory}");
            }

            string unityPlayerPath = Path.Combine(buildDirectory, "UnityPlayer.dll");
            if (!File.Exists(unityPlayerPath))
            {
                throw new BuildFailedException($"Windows build UnityPlayer.dll was not created: {unityPlayerPath}");
            }
        }

        private static void ValidateMacBuildOutput(string absoluteLocationPath)
        {
            if (!Directory.Exists(absoluteLocationPath))
            {
                throw new BuildFailedException($"macOS app bundle was not created: {absoluteLocationPath}");
            }

            string infoPlistPath = Path.Combine(absoluteLocationPath, "Contents", "Info.plist");
            if (!File.Exists(infoPlistPath))
            {
                throw new BuildFailedException($"macOS app bundle is missing Info.plist: {infoPlistPath}");
            }
        }

        private static void ValidateGenericBuildOutput(string absoluteLocationPath)
        {
            bool expectsFile = !string.IsNullOrEmpty(Path.GetExtension(absoluteLocationPath));
            if (expectsFile && !File.Exists(absoluteLocationPath))
            {
                throw new BuildFailedException($"Build file was not created: {absoluteLocationPath}");
            }

            if (!expectsFile && !Directory.Exists(absoluteLocationPath))
            {
                throw new BuildFailedException($"Build directory was not created: {absoluteLocationPath}");
            }
        }

        private static string[] GetEnabledScenes()
        {
            string[] scenes = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path)
                .ToArray();

            if (scenes.Length == 0)
            {
                throw new BuildFailedException("No enabled scenes found in EditorBuildSettings.");
            }

            string missingScene = scenes.FirstOrDefault(scene => !File.Exists(scene));
            if (!string.IsNullOrEmpty(missingScene))
            {
                throw new BuildFailedException($"Enabled build scene does not exist: {missingScene}");
            }

            return scenes;
        }

        private static string ResolveBuildPath(string defaultPath)
        {
            string pathFromArgs = GetArgumentValue(BuildPathArgName);
            if (!string.IsNullOrEmpty(pathFromArgs))
            {
                return pathFromArgs;
            }

            string pathFromEnv = Environment.GetEnvironmentVariable(BuildPathEnvName);
            if (!string.IsNullOrEmpty(pathFromEnv))
            {
                return pathFromEnv;
            }

            return defaultPath;
        }

        private static void WriteSteamAppIdIfConfigured(string buildPath)
        {
            string appId = ResolveSteamAppId();
            if (string.IsNullOrEmpty(appId))
            {
                Debug.Log("Steam App ID was not configured. Skipping steam_appid.txt generation.");
                return;
            }

            foreach (string appIdPath in GetSteamAppIdPaths(buildPath))
            {
                File.WriteAllText(appIdPath, appId.Trim());
                Debug.Log($"Wrote steam_appid.txt for local Steam testing: {appIdPath}");
            }
        }

        private static string ResolveSteamAppId()
        {
            string appIdFromArgs = GetArgumentValue(SteamAppIdArgName);
            if (!string.IsNullOrEmpty(appIdFromArgs))
            {
                return appIdFromArgs;
            }

            string appIdFromEnv = Environment.GetEnvironmentVariable(SteamAppIdEnvName);
            if (!string.IsNullOrEmpty(appIdFromEnv))
            {
                return appIdFromEnv;
            }

            return string.Empty;
        }

        private static string[] GetSteamAppIdPaths(string buildPath)
        {
            string fullPath = Path.GetFullPath(buildPath);
            if (Path.GetExtension(fullPath).Equals(".app", StringComparison.OrdinalIgnoreCase))
            {
                string appParentDirectory = Path.GetDirectoryName(fullPath);
                string appExecutableDirectory = Path.Combine(fullPath, "Contents", "MacOS");
                Directory.CreateDirectory(appExecutableDirectory);
                return new[]
                {
                    Path.Combine(appParentDirectory, "steam_appid.txt"),
                    Path.Combine(appExecutableDirectory, "steam_appid.txt")
                };
            }

            string buildDirectory = File.Exists(fullPath) || !string.IsNullOrEmpty(Path.GetExtension(fullPath))
                ? Path.GetDirectoryName(fullPath)
                : fullPath;

            return new[] { Path.Combine(buildDirectory, "steam_appid.txt") };
        }

        private static string GetArgumentValue(string argumentName)
        {
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i] == argumentName)
                {
                    return args[i + 1];
                }
            }

            return string.Empty;
        }
    }
}
