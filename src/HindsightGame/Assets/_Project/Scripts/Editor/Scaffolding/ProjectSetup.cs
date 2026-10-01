using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Hindsight.Editor.Scaffolding
{
    /// <summary>
    /// One-time project configuration (player settings, render pipeline, input, TextMeshPro).
    /// Kept in code so the settings are reviewable and reproducible. Safe to re-run.
    /// Run from the menu or headless:
    ///   Unity -batchmode -quit -projectPath . -executeMethod Hindsight.Editor.Scaffolding.ProjectSetup.Run
    /// </summary>
    public static class ProjectSetup
    {
        public const string CompanyName = "Team Hindsight";
        public const string ProductName = "Ophthalmology Game";
        public const string BundleId = "ca.mcmaster.hindsight.ophthalmologygame";

        private const string RenderingRoot = "Assets/_Project/Settings/Rendering/";
        private const string TmpEssentialsPackage = "Package Resources/TMP Essential Resources.unitypackage";
        private const int InputSystemOnly = 1;

        [MenuItem("Hindsight/Setup/1. Configure Project Settings", priority = 1)]
        public static void Run()
        {
            ConfigurePlayer();
            ConfigureInputHandling();
            ConfigureRenderPipeline();
            ImportTextMeshProEssentials();

            AssetDatabase.ImportAsset("Assets/_Project/Art", ImportAssetOptions.ImportRecursive | ImportAssetOptions.ForceUpdate);
            AssetDatabase.ImportAsset("Assets/ThirdParty", ImportAssetOptions.ImportRecursive | ImportAssetOptions.ForceUpdate);
            AssetDatabase.SaveAssets();
            Debug.Log("[Hindsight] Project settings configured.");
        }

        private static void ConfigurePlayer()
        {
            PlayerSettings.companyName = CompanyName;
            PlayerSettings.productName = ProductName;
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, BundleId);
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.iOS, BundleId);
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Standalone, BundleId);
            PlayerSettings.bundleVersion = "0.1.0";

            // Landscape only: the layouts assume a wide screen on both phones and tablets.
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true;
            PlayerSettings.allowedAutorotateToLandscapeRight = true;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;

            // Store requirements: 64-bit IL2CPP on Android.
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;

            PlayerSettings.defaultScreenWidth = 1600;
            PlayerSettings.defaultScreenHeight = 900;
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            PlayerSettings.resizableWindow = true;
            PlayerSettings.SplashScreen.showUnityLogo = false;
        }

        private static void ConfigureInputHandling()
        {
            var playerSettings = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset");
            if (playerSettings.Length == 0)
            {
                Debug.LogError("[Hindsight] Could not load ProjectSettings.asset to set the input handler.");
                return;
            }

            var serialized = new SerializedObject(playerSettings[0]);
            var handler = serialized.FindProperty("activeInputHandler");
            if (handler == null)
            {
                Debug.LogError("[Hindsight] 'activeInputHandler' setting not found; set Active Input Handling manually.");
                return;
            }

            handler.intValue = InputSystemOnly;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ConfigureRenderPipeline()
        {
            // URP 2D assets are copied from Unity's bundled 2D template (Assets/_Project/Settings/Rendering).
            var pipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(RenderingRoot + "UniversalRP.asset");
            var globalSettings = AssetDatabase.LoadAssetAtPath<RenderPipelineGlobalSettings>(RenderingRoot + "UniversalRenderPipelineGlobalSettings.asset");
            if (pipeline == null || globalSettings == null)
            {
                Debug.LogError($"[Hindsight] URP assets missing under {RenderingRoot}.");
                return;
            }

            GraphicsSettings.defaultRenderPipeline = pipeline;
            var currentQuality = QualitySettings.GetQualityLevel();
            for (var level = 0; level < QualitySettings.names.Length; level++)
            {
                QualitySettings.SetQualityLevel(level, false);
                QualitySettings.renderPipeline = pipeline;
            }

            QualitySettings.SetQualityLevel(currentQuality, false);
            EditorGraphicsSettings.SetRenderPipelineGlobalSettingsAsset(typeof(UniversalRenderPipeline), globalSettings);
        }

        private static void ImportTextMeshProEssentials()
        {
            if (File.Exists("Assets/TextMesh Pro/Resources/TMP Settings.asset"))
            {
                return;
            }

            // Package import needs a physical path; packages live in Library/PackageCache.
            var ugui = UnityEditor.PackageManager.PackageInfo.FindForAssetPath("Packages/com.unity.ugui");
            var packagePath = Path.Combine(ugui.resolvedPath, TmpEssentialsPackage);
            if (Application.isBatchMode)
            {
                // Package import is queued on the editor loop, which never runs in batch mode.
                Debug.LogError($"[Hindsight] TMP Essential Resources missing. Re-run Unity with: -importPackage \"{packagePath}\"");
                return;
            }

            UnityEditor.AssetPackage.Package.Import(packagePath, false);
        }
    }
}
