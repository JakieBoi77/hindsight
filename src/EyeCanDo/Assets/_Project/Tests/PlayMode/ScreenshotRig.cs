using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace EyeCanDo.Tests.PlayMode
{
    /// <summary>
    /// Saves 1920x1080 screenshots, also in batch mode where there is no Game view: root canvases
    /// are moved to camera space and the main camera renders into a texture.
    /// </summary>
    internal sealed class ScreenshotRig : IDisposable
    {
        private readonly string folder;
        private readonly RenderTexture target;
        private readonly int width;
        private readonly int height;

        public ScreenshotRig(string folder, int width = 1920, int height = 1080)
        {
            this.folder = folder;
            this.width = width;
            this.height = height;
            Directory.CreateDirectory(folder);
            target = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        public IEnumerator Capture(string name)
        {
            Attach();
            yield return null;
            yield return null;
            var camera = Camera.main;
            if (camera == null)
            {
                yield break;
            }

            camera.Render();
            var previous = RenderTexture.active;
            RenderTexture.active = target;
            var texture = new Texture2D(width, height, TextureFormat.RGB24, false);
            texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            texture.Apply();
            RenderTexture.active = previous;
            File.WriteAllBytes(Path.Combine(folder, name + ".png"), texture.EncodeToPNG());
            Object.Destroy(texture);
        }

        public void Dispose()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            if (Camera.main != null)
            {
                Camera.main.targetTexture = null;
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
                canvas.planeDistance = 1f + (1000 - canvas.sortingOrder) * 0.001f;
            }
        }
    }
}
