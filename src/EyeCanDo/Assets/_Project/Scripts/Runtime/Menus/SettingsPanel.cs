using System;
using System.Collections;
using System.Collections.Generic;
using EyeCanDo.App;
using EyeCanDo.Art;
using EyeCanDo.Core.Animation;
using EyeCanDo.Core.Progress;
using EyeCanDo.UI;
using TMPro;
using UnityEngine;

namespace EyeCanDo.Menus
{
    /// <summary>
    /// The settings, opened from the title screen: sound volume, text size and speed,
    /// auto-continue and reduce motion (FR_Preferences). Each change is saved and applied at once,
    /// while the panel is still open. Resetting the whole game sits at the bottom, apart from the
    /// rest, and asks twice before it deletes anything (FR_ResetJourney, FR_ClearAllData).
    /// </summary>
    public sealed class SettingsPanel : MonoBehaviour
    {
        public const int SortingOrder = 600;

        private const float RowTop = -180f;
        private const float RowStep = 124f;
        private const float RowWidth = 1380f;
        private const float OptionWidth = 214f;

        private readonly Dictionary<string, ChunkyButton[]> options = new Dictionary<string, ChunkyButton[]>();
        private RectTransform panel;
        private ChunkyButton mute;
        private float volumeBeforeMute = 1f;

        public static SettingsPanel Current { get; private set; }

        public ChunkyButton CloseButton { get; private set; }

        public ChunkyButton ResetButton { get; private set; }

        public VolumeSlider Volume { get; private set; }

        /// <summary>The choice buttons of each row, by row name ("Text size", "Text speed", ...).</summary>
        public IReadOnlyDictionary<string, ChunkyButton[]> Options => options;

        public static SettingsPanel Open()
        {
            if (Current != null)
            {
                return Current;
            }

            Ui.EnsureEventSystem();
            var canvas = Ui.CreateCanvas("Settings", SortingOrder);
            var settings = canvas.gameObject.AddComponent<SettingsPanel>();
            Current = settings;
            settings.Build((RectTransform)canvas.transform);
            Sfx.Play(Sound.Whoosh);
            settings.StartCoroutine(Tween.Pop(settings.panel, 0.3f));
            return settings;
        }

        private void Build(RectTransform root)
        {
            var theme = UiTheme.Instance;
            var current = GameServices.Settings;
            var dim = Ui.Image(root, "Dim", null, new Color(theme.ink.r, theme.ink.g, theme.ink.b, 0.45f), true);
            Ui.Stretch(dim.rectTransform);

            var face = Ui.Panel(root, "Panel", theme.background, 48f);
            panel = (RectTransform)face.parent;
            Ui.Place(panel, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1500f, 1000f));

            var gear = Ui.Art(face, "Gear", "Icons/gear", new Vector2(96f, 96f));
            Ui.Place(gear.rectTransform, new Vector2(0.5f, 1f), new Vector2(-170f, -74f), new Vector2(96f, 96f));
            var title = Ui.Text(face, "Settings", 72f, theme.ink);
            Ui.Place(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(60f, -74f), new Vector2(420f, 100f));

            CloseButton = ChunkyButton.CreateRound(face, "Close", ArtLibrary.Get("Icons/close"), theme.paper, 110f);
            Ui.Place((RectTransform)CloseButton.transform, new Vector2(1f, 1f), new Vector2(-30f, -30f), new Vector2(110f, 110f), new Vector2(1f, 1f));
            CloseButton.Clicked += Close;

            var sound = Row(face, 0, "Icons/sound_on", "Sound", "Sound effects volume");
            mute = ChunkyButton.CreateRound(sound, "Mute", ArtLibrary.Get(current.SoundOn ? "Icons/sound_on" : "Icons/sound_off"), theme.paper, 100f);
            Ui.Place((RectTransform)mute.transform, new Vector2(1f, 0.5f), new Vector2(-650f, 0f), new Vector2(100f, 100f));
            mute.Clicked += ToggleMute;
            Volume = VolumeSlider.Create(sound, 580f, current.soundVolume);
            Ui.Place((RectTransform)Volume.transform, new Vector2(1f, 0.5f), new Vector2(-290f, 0f), ((RectTransform)Volume.transform).sizeDelta);
            Volume.Changed += SetVolume;
            Volume.Released += () => Sfx.Play(Sound.Correct);

            Choice(Row(face, 1, "Icons/text_size", "Text size", "Size of the words"), "Text size",
                new[] { "Normal", "Big" }, (int)current.textSize, i => GameServices.Settings.textSize = (TextSize)i);
            Choice(Row(face, 2, "Icons/clock", "Text speed", "How fast the words appear"), "Text speed",
                new[] { "Slow", "Normal", "Fast" }, (int)current.textSpeed, i => GameServices.Settings.textSpeed = (TextSpeed)i);
            Choice(Row(face, 3, "Icons/play", "Auto-continue", "Words move on without a tap"), "Auto-continue",
                new[] { "Off", "On" }, current.autoContinue ? 1 : 0, i => GameServices.Settings.autoContinue = i == 1);
            Choice(Row(face, 4, "Icons/wiggle", "Reduce motion", "Less bouncing and confetti"), "Reduce motion",
                new[] { "Off", "On" }, current.reduceMotion ? 1 : 0, i => GameServices.Settings.reduceMotion = i == 1);

            // Kept apart at the bottom so it is never pressed by mistake while changing the rest.
            var divider = Ui.Rounded(face, "Divider", theme.muted, 4f);
            Ui.Place(divider.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, RowY(4) - 80f), new Vector2(RowWidth, 6f));
            var reset = Row(face, 5, "Icons/warning", "Reset game", "Deletes every save file and setting", 36f);
            ResetButton = ChunkyButton.Create(reset, "Reset", "Reset", ArtLibrary.Get("Icons/trash"), theme.danger, new Vector2(320f, 104f), 50f);
            Ui.Place((RectTransform)ResetButton.transform, new Vector2(1f, 0.5f), new Vector2(-160f, 0f), new Vector2(320f, 104f));
            ResetButton.Clicked += AskReset;
        }

        /// <summary>A settings row: picture and name on the left, the control on the right.</summary>
        private static RectTransform Row(RectTransform face, int index, string icon, string name, string note, float extraGap = 0f)
        {
            var theme = UiTheme.Instance;
            var row = Ui.Rect(face, "Row_" + name);
            Ui.Place(row, new Vector2(0.5f, 1f), new Vector2(0f, RowY(index) - extraGap), new Vector2(RowWidth, 112f));
            var picture = Ui.Art(row, "Icon", icon, new Vector2(80f, 80f));
            Ui.Place(picture.rectTransform, new Vector2(0f, 0.5f), new Vector2(46f, 0f), new Vector2(80f, 80f));
            var label = Ui.Text(row, name, 46f, theme.ink, TextAlignmentOptions.MidlineLeft);
            Ui.Place(label.rectTransform, new Vector2(0f, 0.5f), new Vector2(110f, 18f), new Vector2(520f, 60f), new Vector2(0f, 0.5f));
            var sub = Ui.Text(row, note, 28f, UiTheme.Darker(theme.muted, 0.4f), TextAlignmentOptions.MidlineLeft, false);
            Ui.Place(sub.rectTransform, new Vector2(0f, 0.5f), new Vector2(110f, -30f), new Vector2(560f, 40f), new Vector2(0f, 0.5f));
            return row;
        }

        private static float RowY(int index)
        {
            return RowTop - 12f - RowStep * index;
        }

        /// <summary>
        /// A row of choice buttons. The chosen one is yellow and carries a tick, so the choice
        /// does not depend on colour (NFR_Colour).
        /// </summary>
        private void Choice(RectTransform row, string name, string[] labels, int selected, Action<int> apply)
        {
            var theme = UiTheme.Instance;
            var buttons = new ChunkyButton[labels.Length];
            var size = new Vector2(OptionWidth, 96f);
            for (var i = 0; i < labels.Length; i++)
            {
                var index = i;
                var button = ChunkyButton.Create(row, "Option_" + labels[i], labels[i], ArtLibrary.Get("Icons/check"), theme.paper, size, 40f);
                var x = -(labels.Length - 1 - i) * (OptionWidth + 16f) - OptionWidth / 2f - 10f;
                Ui.Place((RectTransform)button.transform, new Vector2(1f, 0.5f), new Vector2(x, 0f), size);
                button.Label.fontSize = 36f;
                button.IconImage.GetComponent<UnityEngine.UI.LayoutElement>().preferredWidth = 40f;
                button.IconImage.GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight = 40f;
                button.Clicked += () =>
                {
                    apply(index);
                    Mark(buttons, index);
                    GameServices.SaveSettings();
                };
                buttons[i] = button;
            }

            Mark(buttons, selected);
            options[name] = buttons;
        }

        private static void Mark(ChunkyButton[] buttons, int selected)
        {
            var theme = UiTheme.Instance;
            for (var i = 0; i < buttons.Length; i++)
            {
                buttons[i].SetColor(i == selected ? theme.primary : theme.paper);
                buttons[i].IconImage.gameObject.SetActive(i == selected);
            }
        }

        private void SetVolume(float volume)
        {
            GameServices.Settings.soundVolume = volume;
            if (volume > 0f)
            {
                volumeBeforeMute = volume;
            }

            mute.IconImage.sprite = ArtLibrary.Get(volume > 0f ? "Icons/sound_on" : "Icons/sound_off");
            GameServices.SaveSettings();
        }

        private void ToggleMute()
        {
            Volume.Slide(GameServices.Settings.SoundOn ? 0f : volumeBeforeMute);
            Sfx.Play(Sound.Correct);
        }

        /// <summary>The first question. Saying yes leads to a second, plainer one.</summary>
        private void AskReset()
        {
            ConfirmDialog.Show(
                "Reset the whole game?",
                $"This deletes all {SaveSlots.Count} save files and all settings. The game will be like new.",
                "Icons/warning", "Yes, reset", "No", AskResetAgain);
        }

        private void AskResetAgain()
        {
            ConfirmDialog.Show(
                "Are you really sure?",
                "This will delete everything: every character, coin, sticker, star and setting. It cannot be undone.",
                "Icons/trash", "Delete everything", "No, keep it", ResetEverything);
        }

        private void ResetEverything()
        {
            GameServices.DeleteEverything();
            GameServices.JustReset = true;
            Close();
            GameServices.GoTo(SceneNames.Title);
        }

        public void Close()
        {
            if (Current == this)
            {
                Current = null;
            }

            StartCoroutine(CloseRoutine());
        }

        private IEnumerator CloseRoutine()
        {
            GetComponent<UnityEngine.UI.GraphicRaycaster>().enabled = false;
            yield return Tween.Scale(panel, Vector3.zero, 0.2f, Ease.InCubic);
            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            if (Current == this)
            {
                Current = null;
            }
        }
    }
}
