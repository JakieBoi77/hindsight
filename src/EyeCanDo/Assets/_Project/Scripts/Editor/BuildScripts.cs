using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace EyeCanDo.EditorTools
{
    /// <summary>Device and desktop builds. Output goes to Builds/ (git-ignored).</summary>
    public static class BuildScripts
    {
        [MenuItem("EyeCanDo/Build/Windows (smoke test)", priority = 30)]
        public static void BuildWindows()
        {
            Build(BuildTarget.StandaloneWindows64, "Builds/Windows/EyeCanDo.exe");
        }

        [MenuItem("EyeCanDo/Build/Android APK", priority = 31)]
        public static void BuildAndroid()
        {
            Build(BuildTarget.Android, "Builds/Android/EyeCanDo.apk");
        }

        [MenuItem("EyeCanDo/Build/iOS Xcode Project", priority = 32)]
        public static void BuildIos()
        {
            Build(BuildTarget.iOS, "Builds/iOS");
        }

        private static void Build(BuildTarget target, string path)
        {
            var scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = path,
                target = target,
                options = BuildOptions.None,
            });

            if (report.summary.result != BuildResult.Succeeded)
            {
                Debug.LogError($"[EyeCanDo] Build failed: {report.summary.result}");
                if (Application.isBatchMode)
                {
                    EditorApplication.Exit(1);
                }

                return;
            }

            Debug.Log($"[EyeCanDo] Built {target} to {path}");
        }
    }
}
