using System;
using EyeCanDo.Art;
using EyeCanDo.Core.Interaction;
using UnityEngine;
using UnityEngine.UI;

namespace EyeCanDo.UI
{
    /// <summary>
    /// The controls on every story and challenge screen, always in the same place (NFR_Layout):
    /// Home (one touch back to the map, NFR_Exit), Restart (FR_RestartLevel) and a hold-to-skip
    /// button (FR_NoPenalty) that is hidden while a sensation is being described.
    /// </summary>
    public sealed class LevelChrome : MonoBehaviour
    {
        public const float ButtonSize = 120f;
        public const float Margin = 26f;

        private readonly SkipHold skipHold = new SkipHold();
        private RectTransform skipRoot;
        private Image skipFill;

        public event Action HomePressed;

        public event Action RestartPressed;

        public event Action SkipTriggered;

        public ChunkyButton Home { get; private set; }

        public ChunkyButton Restart { get; private set; }

        public bool SkipAllowed
        {
            get => skipHold.Enabled;
            set
            {
                skipHold.Enabled = value;
                if (!value)
                {
                    skipHold.Release();
                }

                skipRoot.gameObject.SetActive(value);
            }
        }

        public static LevelChrome Create(Transform overlay)
        {
            var theme = UiTheme.Instance;
            var bar = Ui.Rect(overlay, "LevelChrome");
            Ui.Stretch(bar);
            var chrome = bar.gameObject.AddComponent<LevelChrome>();

            chrome.Home = ChunkyButton.CreateRound(bar, "Home", ArtLibrary.Get("Icons/home"), theme.paper, ButtonSize);
            Ui.Place((RectTransform)chrome.Home.transform, new Vector2(0f, 1f), new Vector2(Margin + ButtonSize / 2f, -Margin - ButtonSize / 2f), new Vector2(ButtonSize, ButtonSize));
            chrome.Home.Clicked += () => chrome.HomePressed?.Invoke();

            chrome.Restart = ChunkyButton.CreateRound(bar, "Restart", ArtLibrary.Get("Icons/restart"), theme.paper, ButtonSize);
            Ui.Place((RectTransform)chrome.Restart.transform, new Vector2(0f, 1f), new Vector2(Margin * 2f + ButtonSize * 1.5f, -Margin - ButtonSize / 2f), new Vector2(ButtonSize, ButtonSize));
            chrome.Restart.Clicked += () => chrome.RestartPressed?.Invoke();

            chrome.skipRoot = Ui.Rect(bar, "Skip");
            Ui.Place(chrome.skipRoot, new Vector2(1f, 1f), new Vector2(-Margin - ButtonSize / 2f, -Margin - ButtonSize / 2f), new Vector2(ButtonSize, ButtonSize));
            var skipButton = ChunkyButton.CreateRound(chrome.skipRoot, "SkipButton", ArtLibrary.Get("Icons/skip"), theme.paper, ButtonSize);
            Ui.Place((RectTransform)skipButton.transform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(ButtonSize, ButtonSize));
            var fill = Ui.Image(chrome.skipRoot, "HoldRing", Shapes.Ring, theme.accent);
            Ui.Stretch(fill.rectTransform, -14f);
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Radial360;
            fill.fillOrigin = (int)Image.Origin360.Top;
            fill.fillAmount = 0f;
            chrome.skipFill = fill;
            var skipPress = skipButton.gameObject.AddComponent<SkipPressRelay>();
            skipPress.Owner = chrome;

            return chrome;
        }

        /// <summary>Test hook and keyboard-free shortcut: acts as if skip was held long enough.</summary>
        public void TriggerSkip()
        {
            if (SkipAllowed)
            {
                SkipTriggered?.Invoke();
            }
        }

        internal void PressSkip()
        {
            skipHold.Press();
        }

        internal void ReleaseSkip()
        {
            skipHold.Release();
        }

        private void Update()
        {
            if (skipHold.Tick(Time.unscaledDeltaTime))
            {
                SkipTriggered?.Invoke();
            }

            if (skipFill != null)
            {
                skipFill.fillAmount = skipHold.Progress;
            }
        }
    }
}
