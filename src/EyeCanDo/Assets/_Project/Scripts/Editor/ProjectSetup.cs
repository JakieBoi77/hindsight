using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace EyeCanDo.EditorTools
{
    /// <summary>Player and graphics settings, kept in code so they are reviewable and reproducible.</summary>
    public static class ProjectSetup
    {
        public const string CompanyName = "Team Hindsight";
        public const string ProductName = "EyeCanDo";
        public const string BundleId = "ca.mcmaster.hindsight.eyecando";
        private const string PipelinePath = "Assets/_Project/Settings/Rendering/UniversalRP.asset";

        public static void Configure()
        {
            PlayerSettings.companyName = CompanyName;
            PlayerSettings.productName = ProductName;
            PlayerSettings.bundleVersion = "0.1.0";
            foreach (var target in new[] { NamedBuildTarget.Android, NamedBuildTarget.iOS, NamedBuildTarget.Standalone })
            {
                PlayerSettings.SetApplicationIdentifier(target, BundleId);
            }

            // Landscape on phones and tablets.
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true;
            PlayerSettings.allowedAutorotateToLandscapeRight = true;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;

            // Store requirement: 64-bit IL2CPP on Android.
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;

            PlayerSettings.defaultScreenWidth = 1600;
            PlayerSettings.defaultScreenHeight = 900;
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            PlayerSettings.resizableWindow = true;
            PlayerSettings.SplashScreen.showUnityLogo = false;
            PlayerSettings.runInBackground = false;

            // No network use (C_NoNetworkCommunications): no analytics or crash uploads.
            PlayerSettings.enableCrashReportAPI = false;

            var pipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(PipelinePath);
            if (pipeline != null)
            {
                GraphicsSettings.defaultRenderPipeline = pipeline;
            }
            else
            {
                Debug.LogError($"[EyeCanDo] URP asset missing at {PipelinePath}.");
            }
        }
    }
}
