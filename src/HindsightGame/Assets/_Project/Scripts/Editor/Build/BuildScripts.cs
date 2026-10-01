using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Hindsight.Editor.Build
{
    /// <summary>
    /// Headless build entry points for local use and CI (e.g. GameCI), for example:
    ///   Unity -batchmode -quit -projectPath . -executeMethod Hindsight.Editor.Build.BuildScripts.BuildAndroid
    /// Output goes to Builds/&lt;platform&gt;/ (git-ignored).
    /// </summary>
    public static class BuildScripts
    {
        private const string OutputRoot = "Builds";

        [MenuItem("Hindsight/Build/Android (APK)", priority = 40)]
        public static void BuildAndroid()
        {
            EditorUserBuildSettings.buildAppBundle = false;
            Build(BuildTarget.Android, Path.Combine(OutputRoot, "Android", "OphthalmologyGame.apk"));
        }

        [MenuItem("Hindsight/Build/iOS (Xcode project)", priority = 41)]
        public static void BuildIos()
        {
            Build(BuildTarget.iOS, Path.Combine(OutputRoot, "iOS"));
        }

        [MenuItem("Hindsight/Build/Windows", priority = 42)]
        public static void BuildWindows()
        {
            Build(BuildTarget.StandaloneWindows64, Path.Combine(OutputRoot, "Windows", "OphthalmologyGame.exe"));
        }

        private static void Build(BuildTarget target, string outputPath)
        {
            var scenes = EditorBuildSettings.scenes.Where(scene => scene.enabled).Select(scene => scene.path).ToArray();
            if (scenes.Length == 0)
            {
                throw new InvalidOperationException("No scenes are enabled in Build Settings.");
            }

            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = outputPath,
                target = target,
                targetGroup = BuildPipeline.GetBuildTargetGroup(target),
                options = BuildOptions.None,
            };

            var report = BuildPipeline.BuildPlayer(options);
            var summary = report.summary;

            // summary.totalSize counts uncompressed content; the file size is what families download.
            var sizeText = File.Exists(outputPath) ? $"{new FileInfo(outputPath).Length / (1024f * 1024f):0.0} MB on disk" : "size n/a";
            Debug.Log($"[Hindsight] Build {target}: {summary.result}, {sizeText}, {summary.totalErrors} error(s) -> {outputPath}");

            // Non-zero exit code so CI fails when the build fails.
            if (summary.result != BuildResult.Succeeded && Application.isBatchMode)
            {
                EditorApplication.Exit(1);
            }
        }
    }
}
