using System;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Wallfall.EditorTools
{
    /// <summary>Repeatable WebGL configuration and build entry points for local and batch use.</summary>
    public static class WallfallWebBuild
    {
        const string ScenePath = "Assets/Scenes/SampleScene.unity";
        const string DevelopmentOutput = "Builds/WebGL-Development";
        const string ReleaseOutput = "Builds/WebGL";

        [MenuItem("WALLFALL/Web/Configure WebGL Player")]
        public static void Configure()
        {
            EnsureBuildScene();

            PlayerSettings.defaultWebScreenWidth = 1280;
            PlayerSettings.defaultWebScreenHeight = 720;
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
            PlayerSettings.WebGL.decompressionFallback = false;
            PlayerSettings.WebGL.threadsSupport = false;
            PlayerSettings.WebGL.template = "APPLICATION:Default";

            AssetDatabase.SaveAssets();
            Debug.Log("WALLFALL WEB CONFIG: 1280x720, Default template, Gzip, fallback off, threads off.");
        }

        [MenuItem("WALLFALL/Web/Build Development")]
        public static void BuildDevelopment()
        {
            Build(DevelopmentOutput, BuildOptions.Development);
        }

        [MenuItem("WALLFALL/Web/Build Release")]
        public static void BuildRelease()
        {
            Build(ReleaseOutput, BuildOptions.None);
        }

        /// <summary>CLI entry point. Exits nonzero on any preflight or build failure.</summary>
        public static void BuildDevelopmentBatch()
        {
            RunBatch(() => BuildDevelopment());
        }

        /// <summary>CLI entry point. Exits nonzero on any preflight or build failure.</summary>
        public static void BuildReleaseBatch()
        {
            RunBatch(() => BuildRelease());
        }

        static void Build(string output, BuildOptions options)
        {
            WallfallWebResourceSetup.Prepare();
            Configure();

            if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.WebGL, BuildTarget.WebGL))
                throw new InvalidOperationException("WebGL Build Support is not installed for this Unity Editor.");

            if (!EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WebGL, BuildTarget.WebGL))
                throw new InvalidOperationException("Unity could not switch the active build target to WebGL.");

            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = output,
                target = BuildTarget.WebGL,
                targetGroup = BuildTargetGroup.WebGL,
                options = options,
            });

            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException(
                    $"WebGL build failed: {report.summary.result}, {report.summary.totalErrors} errors, " +
                    $"{report.summary.totalWarnings} warnings.");

            Debug.Log(
                $"WALLFALL WEB BUILD: {report.summary.result} at {output} " +
                $"({report.summary.totalSize} bytes, {report.summary.totalTime}).");
        }

        static void EnsureBuildScene()
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) == null)
                throw new InvalidOperationException($"Missing build scene: {ScenePath}");

            var bootstrapGuid = AssetDatabase.AssetPathToGUID("Assets/Scripts/GameBootstrap.cs");
            var sceneText = System.IO.File.ReadAllText(ScenePath);
            if (string.IsNullOrEmpty(bootstrapGuid) || !sceneText.Contains("guid: " + bootstrapGuid))
                throw new InvalidOperationException($"{ScenePath} does not contain GameBootstrap.");

            var current = EditorBuildSettings.scenes;
            if (current.Length != 1 || current[0].path != ScenePath || !current[0].enabled)
                EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        }

        static void RunBatch(Action action)
        {
            try
            {
                action();
                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                Debug.LogError("WALLFALL WEB BUILD FAILED: " + exception);
                EditorApplication.Exit(1);
            }
        }
    }
}
