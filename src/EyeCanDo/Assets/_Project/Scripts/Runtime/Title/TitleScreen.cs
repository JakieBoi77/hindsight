using System.Collections;
using EyeCanDo.App;
using EyeCanDo.Art;
using EyeCanDo.Characters;
using EyeCanDo.Content;
using EyeCanDo.UI;
using TMPro;
using UnityEngine;

namespace EyeCanDo.Title
{
    /// <summary>
    /// The title screen: the game's name, Dr. Iris and the character from the save played last,
    /// a big Play button (which leads to the save files) and the Settings.
    /// </summary>
    public sealed class TitleScreen : MonoBehaviour
    {
        private const float FeetY = -500f;

        [SerializeField] private CaptionBank captions;

        private Stage stage;
        private CharacterRig doctor;
        private CharacterRig you;

        public ChunkyButton PlayButton { get; private set; }

        public ChunkyButton SettingsButton { get; private set; }

        private void Awake()
        {
            if (captions == null)
            {
                captions = DefaultContent.CreateCaptionBank();
            }

            stage = Stage.Create("Title", false);
            stage.Captions.YouLook = GameServices.Progress.look;
            stage.BuildOutdoors(-260f);
            Build();
        }

        private void Build()
        {
            var theme = UiTheme.Instance;
            var view = stage.Layer("TitleView");

            var logo = Ui.Rect(view, "Logo");
            Ui.Place(logo, new Vector2(0.5f, 0.5f), new Vector2(0f, 270f), new Vector2(1300f, 220f));
            var eye = Ui.Art(logo, "LogoEye", "Icons/eye", new Vector2(200f, 200f));
            eye.rectTransform.anchoredPosition = new Vector2(-520f, 6f);
            eye.gameObject.AddComponent<Bob>().Height = 8f;
            var title = Ui.Text(logo, "Eye Can Do!", 168f, theme.ink);
            title.rectTransform.anchorMin = Vector2.zero;
            title.rectTransform.anchorMax = Vector2.one;
            title.rectTransform.offsetMin = new Vector2(250f, 0f);
            title.rectTransform.offsetMax = new Vector2(100f, 0f);
            title.textWrappingMode = TextWrappingModes.NoWrap;
            title.alignment = TextAlignmentOptions.MidlineLeft;
            title.enableAutoSizing = false;
            title.outlineWidth = 0.18f;
            title.outlineColor = Color.white;
            var tagline = Ui.Text(view, "Practise for your eye doctor visit!", 54f, theme.ink);
            Ui.Place(tagline.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, 135f), new Vector2(1200f, 80f));

            doctor = CharacterRig.CreateDoctor(view, "idle", 0.66f);
            doctor.Root.anchoredPosition = new Vector2(-640f, FeetY);
            StartCoroutine(WaveSometimes());

            you = CharacterRig.CreateKid(view, GameServices.Progress.look, "stand", 0.66f);
            you.Root.anchoredPosition = new Vector2(640f, FeetY);
            you.SetMouth(Mouth.Grin);

            PlayButton = ChunkyButton.Create(view, "Play", "Play!", ArtLibrary.Get("Icons/play"), theme.primary, new Vector2(460f, 170f), 85f);
            Ui.Place((RectTransform)PlayButton.transform, new Vector2(0.5f, 0.5f), new Vector2(0f, -100f), new Vector2(460f, 170f));
            PlayButton.Label.fontSize = 72f;
            PlayButton.gameObject.AddComponent<Bob>().Height = 8f;
            PlayButton.Clicked += () => GameServices.GoTo(SceneNames.Saves);

            SettingsButton = ChunkyButton.Create(view, "Settings", "Settings", ArtLibrary.Get("Icons/gear"), theme.paper, new Vector2(400f, 130f), 65f);
            Ui.Place((RectTransform)SettingsButton.transform, new Vector2(0.5f, 0.5f), new Vector2(0f, -300f), new Vector2(400f, 130f));
            SettingsButton.Clicked += () => Menus.SettingsPanel.Open();
        }

        private IEnumerator Start()
        {
            yield return null;
            if (GameServices.JustReset)
            {
                GameServices.JustReset = false;
                stage.Captions.Show(captions.titleReset);
            }

            if (!GameServices.Saves.IsUsed(GameServices.CurrentSave))
            {
                yield return Tween.Wait(0.6f);
                stage.Ghost.ShowTap((RectTransform)PlayButton.transform);
            }
        }

        private IEnumerator WaveSometimes()
        {
            while (true)
            {
                yield return Tween.Wait(0.4f);
                doctor.SetPose("wave");
                yield return Tween.Wait(1.6f);
                doctor.SetPose("idle");
                yield return Tween.Wait(Random.Range(4f, 7f));
            }
        }
    }
}
