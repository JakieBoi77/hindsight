using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Hindsight.Tests.PlayMode
{
    /// <summary>
    /// Captures 1920x1080 screenshots even in batch mode (no Game view): overlay canvases are
    /// switched to camera space and the main camera renders into a RenderTexture.
    /// </summary>
    internal sealed class ScreenCaptureRig : IDisposable
    {
        private const int Width = 1920;
        private const int Height = 1080;

        private readonly string outputFolder;
        private readonly RenderTexture target;

        public ScreenCaptureRig(string outputFolder)
        {
            this.outputFolder = outputFolder;
            Directory.CreateDirectory(outputFolder);
            target = new RenderTexture(Width, Height, 24, RenderTextureFormat.ARGB32);
            SceneManager.sceneLoaded += OnSceneLoaded;
            Attach();
        }

        public IEnumerator Capture(string name)
        {
            Attach();

            // Let CanvasScaler and layout settle for the render-texture size. (WaitForEndOfFrame never
            // fires in batch mode, so plain frame yields are used.)
            yield return null;
            yield return null;

            var camera = Camera.main;
            camera.Render();
            var previous = RenderTexture.active;
            RenderTexture.active = target;
            var texture = new Texture2D(Width, Height, TextureFormat.RGB24, false);
            texture.ReadPixels(new Rect(0, 0, Width, Height), 0, 0);
            texture.Apply();
            RenderTexture.active = previous;

            File.WriteAllBytes(Path.Combine(outputFolder, name + ".png"), texture.EncodeToPNG());
            Object.Destroy(texture);
        }

        public void Dispose()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            var camera = Camera.main;
            if (camera != null)
            {
                camera.targetTexture = null;
            }

            foreach (var canvas in Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include))
            {
                if (canvas.isRootCanvas && canvas.renderMode == RenderMode.ScreenSpaceCamera)
                {
                    canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                }
            }

            target.Release();
            Object.Destroy(target);
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Attach();
        }

        private void Attach()
        {
            var camera = Camera.main;
            if (camera == null)
            {
                return;
            }

            camera.targetTexture = target;
            foreach (var canvas in Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include))
            {
                if (!canvas.isRootCanvas)
                {
                    continue;
                }

                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = camera;
                canvas.planeDistance = 1f;
            }
        }
    }
}
