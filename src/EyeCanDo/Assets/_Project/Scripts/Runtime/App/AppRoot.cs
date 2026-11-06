using System.Collections;
using EyeCanDo.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EyeCanDo.App
{
    /// <summary>
    /// Lives for the whole session: fades between scenes and pauses everything when the device
    /// interrupts the app, such as a call or the app going to the background (FR_Suspend).
    /// All game timers and tweens use scaled time, so pausing time pauses the game.
    /// </summary>
    public sealed class AppRoot : MonoBehaviour
    {
        private const float FadeSeconds = 0.25f;

        private CanvasGroup fader;
        private bool loading;

        public bool IsLoading => loading;

        public bool IsSuspended { get; private set; }

        private void Awake()
        {
            var canvas = Ui.CreateCanvas("Fader", 1000, transform);
            var cover = Ui.Image(canvas.transform, "Cover", null, UiTheme.Instance.background);
            Ui.Stretch(cover.rectTransform);
            fader = canvas.gameObject.AddComponent<CanvasGroup>();
            fader.alpha = 0f;
            fader.blocksRaycasts = false;
            Application.targetFrameRate = 60;
        }

        public void LoadScene(string sceneName)
        {
            if (!loading)
            {
                StartCoroutine(LoadRoutine(sceneName));
            }
        }

        private IEnumerator LoadRoutine(string sceneName)
        {
            loading = true;
            fader.blocksRaycasts = true;
            yield return FadeTo(1f);
            var operation = SceneManager.LoadSceneAsync(sceneName);
            while (operation != null && !operation.isDone)
            {
                yield return null;
            }

            yield return null;
            yield return FadeTo(0f);
            fader.blocksRaycasts = false;
            loading = false;
            GameServices.NavigationGate.Open();
        }

        private IEnumerator FadeTo(float target)
        {
            var start = fader.alpha;
            var time = 0f;
            while (time < FadeSeconds)
            {
                time += Time.unscaledDeltaTime;
                fader.alpha = Mathf.Lerp(start, target, time / FadeSeconds);
                yield return null;
            }

            fader.alpha = target;
        }

        private void OnApplicationPause(bool paused)
        {
            SetSuspended(paused);
        }

        private void OnApplicationFocus(bool focused)
        {
            // In the editor, clicking another window should not freeze the game.
            if (!Application.isEditor)
            {
                SetSuspended(!focused);
            }
        }

        private void SetSuspended(bool suspended)
        {
            IsSuspended = suspended;
            Time.timeScale = suspended ? 0f : 1f;
            AudioListener.pause = suspended;
            if (suspended)
            {
                GameServices.Save();
            }
        }
    }
}
