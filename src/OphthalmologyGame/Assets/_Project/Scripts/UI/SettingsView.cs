using Hindsight.Ophthalmology.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Hindsight.Ophthalmology.UI
{
    /// <summary>
    /// Settings for the grown-up (adapted from the 2D PoC): sound and voice volume, captions,
    /// big text, auto-continue and reduce motion. Each change is saved and applied at once and is
    /// shared by every save. "Reset game" asks twice before deleting every save and setting.
    /// Opened from the title (and from the level HUD, where it pauses the game).
    /// </summary>
    public sealed class SettingsView : MonoBehaviour
    {
        [SerializeField] GameObject panel;
        [SerializeField] Slider soundSlider;
        [SerializeField] Slider voiceSlider;
        [SerializeField] OnOffButton captions;
        [SerializeField] OnOffButton bigText;
        [SerializeField] OnOffButton autoContinue;
        [SerializeField] OnOffButton reduceMotion;
        [SerializeField] Button resetButton;
        [SerializeField] Button closeButton;
        [SerializeField] ConfirmDialog confirm;

        bool pausedGame;

        public bool IsOpen => panel.activeSelf;

        void Awake()
        {
            soundSlider.onValueChanged.AddListener(v =>
            {
                GameServices.Settings.soundVolume = v;
                GameServices.SaveSettings();
            });
            voiceSlider.onValueChanged.AddListener(v =>
            {
                GameServices.Settings.voiceVolume = v;
                GameServices.SaveSettings();
            });
            captions.Changed += v => Apply(() => GameServices.Settings.captionsOn = v);
            bigText.Changed += v => Apply(() => GameServices.Settings.bigText = v);
            autoContinue.Changed += v => Apply(() => GameServices.Settings.autoContinue = v);
            reduceMotion.Changed += v => Apply(() => GameServices.Settings.reduceMotion = v);
            resetButton.onClick.AddListener(AskReset);
            closeButton.onClick.AddListener(Close);
            panel.SetActive(false);
        }

        static void Apply(System.Action change)
        {
            change();
            GameServices.SaveSettings();
        }

        public void Open(bool pause = false)
        {
            var s = GameServices.Settings;
            soundSlider.SetValueWithoutNotify(s.soundVolume);
            voiceSlider.SetValueWithoutNotify(s.voiceVolume);
            captions.Set(s.captionsOn);
            bigText.Set(s.bigText);
            autoContinue.Set(s.autoContinue);
            reduceMotion.Set(s.reduceMotion);
            panel.SetActive(true);
            panel.transform.SetAsLastSibling();

            pausedGame = pause;
            if (pause)
            {
                Time.timeScale = 0f;
                AudioListener.pause = true;
            }
        }

        public void Close()
        {
            if (confirm != null) confirm.Close();
            panel.SetActive(false);
            if (!pausedGame) return;
            pausedGame = false;
            Time.timeScale = 1f;
            AudioListener.pause = false;
        }

        /// <summary>The first question. Saying yes leads to a second, plainer one.</summary>
        void AskReset() => confirm.Ask("Reset the whole game?",
            "This deletes every save: all coins, stickers and clothes. Settings go back to normal.",
            "Yes, reset", "Keep it", AskResetAgain);

        void AskResetAgain() => confirm.Ask("Are you really sure?",
            "Everything will be deleted. This cannot be undone.",
            "Delete everything", "Keep it", ResetEverything);

        void ResetEverything()
        {
            GameServices.DeleteEverything();
            GameServices.JustReset = true;
            Close();
            GameFlow.ReturnToTitle(); // reload the menu so nothing shows the deleted player
        }

#if UNITY_EDITOR
        public void EditorInit(GameObject panelObject, Slider sound, Slider voice, OnOffButton cc, OnOffButton big, OnOffButton auto,
            OnOffButton calm, Button reset, Button close, ConfirmDialog dialog)
        {
            panel = panelObject;
            soundSlider = sound;
            voiceSlider = voice;
            captions = cc;
            bigText = big;
            autoContinue = auto;
            reduceMotion = calm;
            resetButton = reset;
            closeButton = close;
            confirm = dialog;
        }
#endif
    }
}
