using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace BubbleShot.Editor
{
    /// <summary>
    /// Headless CI/CD build scripts for automated Android APK/AAB and Desktop binary generation.
    /// </summary>
    public static class BuildScript
    {
        private static readonly string[] DefaultScenes =
        {
            "Assets/Game/Scenes/Bootstrap.unity",
            "Assets/Game/Scenes/MainMenu.unity",
            "Assets/Game/Scenes/Gameplay.unity"
        };

        private static string[] GetBuildScenes()
        {
            var scenes = EditorBuildSettings.scenes
                .Where(s => s.enabled && File.Exists(s.path))
                .Select(s => s.path)
                .ToArray();

            return scenes.Length > 0 ? scenes : DefaultScenes;
        }

        [MenuItem("BubbleShot/Build/Android APK")]
        public static void BuildAndroid()
        {
            ConfigurePlayerSettings();

            string outputDir = Path.Combine(Directory.GetCurrentDirectory(), "Builds", "Android");
            Directory.CreateDirectory(outputDir);
            string outputPath = Path.Combine(outputDir, "BubbleShot.apk");

            var buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = GetBuildScenes(),
                locationPathName = outputPath,
                target = BuildTarget.Android,
                options = BuildOptions.None
            };

            Debug.Log($"[BuildScript] Starting Android build targeting: {outputPath}");
            var report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            ProcessBuildReport(report, "Android");
        }

        [MenuItem("BubbleShot/Build/Windows Standalone")]
        public static void BuildStandaloneWindows()
        {
            ConfigurePlayerSettings();

            string outputDir = Path.Combine(Directory.GetCurrentDirectory(), "Builds", "Windows");
            Directory.CreateDirectory(outputDir);
            string outputPath = Path.Combine(outputDir, "BubbleShot.exe");

            var buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = GetBuildScenes(),
                locationPathName = outputPath,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            Debug.Log($"[BuildScript] Starting Windows Standalone build targeting: {outputPath}");
            var report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            ProcessBuildReport(report, "Windows Standalone");
        }

        private static void ConfigurePlayerSettings()
        {
            PlayerSettings.companyName = "BorisSokolov";
            PlayerSettings.productName = "BubbleShot";
            PlayerSettings.bundleVersion = "1.0.0";
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.BorisSokolov.BubbleShot");
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64 | AndroidArchitecture.ARMv7;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel24; // Android 7.0 Nougat
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel34; // Android 14
        }

        private static void ProcessBuildReport(BuildReport report, string platform)
        {
            var summary = report.summary;
            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"[BuildScript] {platform} build SUCCEEDED! Total time: {summary.totalTime.TotalSeconds:F1}s, Size: {summary.totalSize} bytes");
                if (Application.isBatchMode) EditorApplication.Exit(0);
            }
            else
            {
                Debug.LogError($"[BuildScript] {platform} build FAILED! Errors: {summary.totalErrors}");
                if (Application.isBatchMode) EditorApplication.Exit(1);
            }
        }
    }
}
