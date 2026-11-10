using System.Collections;
using System.IO;
using Hindsight.EyeCanDo.UI;
using UnityEngine;

namespace Hindsight.EyeCanDo.DevTools
{
    /// <summary>
    /// When the player is launched with "-splashrecord &lt;dir&gt;" it plays the studio splash at a fixed
    /// 60 frames a second and saves every frame to &lt;dir&gt;/f_0000.png, then quits. Joined with the
    /// splash's sound (ffmpeg), that is a video for reviewing it. Does nothing without the flag.
    /// </summary>
    public sealed class SplashRecorder : MonoBehaviour
    {
        const int FramesPerSecond = 60;
        string outDir;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            var args = System.Environment.GetCommandLineArgs();
            var i = System.Array.IndexOf(args, "-splashrecord");
            if (i < 0) return;
            var go = new GameObject("SplashRecorder");
            DontDestroyOnLoad(go);
            go.AddComponent<SplashRecorder>().outDir = i + 1 < args.Length ? args[i + 1] : "splash";
        }

        IEnumerator Start()
        {
            var splash = FindAnyObjectByType<StudioSplash>();
            if (splash == null)
            {
                Debug.LogError("SPLASHRECORD no StudioSplash in the first scene");
                Application.Quit(1);
                yield break;
            }
            Time.captureFramerate = FramesPerSecond;
            Directory.CreateDirectory(outDir);
            splash.Preview();
            for (var frame = 0; !splash.Finished; frame++)
            {
                yield return new WaitForEndOfFrame();
                var shot = ScreenCapture.CaptureScreenshotAsTexture();
                File.WriteAllBytes(Path.Combine(outDir, $"f_{frame:0000}.png"), shot.EncodeToPNG());
                Destroy(shot);
            }
            Debug.Log("SPLASHRECORD done");
            Application.Quit(0);
        }
    }
}
