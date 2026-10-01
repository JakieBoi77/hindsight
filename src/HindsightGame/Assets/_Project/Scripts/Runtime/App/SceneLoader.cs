using System;
using Hindsight.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Hindsight.App
{
    /// <summary>Loads scenes behind a full-screen fade so children never see a half-built screen.</summary>
    public sealed class SceneLoader
    {
        private readonly ScreenFader fader;

        public SceneLoader(ScreenFader fader)
        {
            this.fader = fader;
        }

        public bool IsLoading { get; private set; }

        public async Awaitable LoadAsync(string sceneName)
        {
            if (IsLoading)
            {
                // Double taps on navigation buttons are common with young children; ignore repeats.
                return;
            }

            IsLoading = true;
            try
            {
                if (fader != null)
                {
                    await fader.FadeOutAsync();
                }

                var operation = SceneManager.LoadSceneAsync(sceneName);
                if (operation == null)
                {
                    throw new InvalidOperationException($"Scene '{sceneName}' could not be loaded. Is it in Build Settings?");
                }

                await Awaitable.FromAsyncOperation(operation);

                if (fader != null)
                {
                    await fader.FadeInAsync();
                }
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
