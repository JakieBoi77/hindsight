using System.Collections;
using EyeCanDo.App;
using EyeCanDo.Art;
using EyeCanDo.Content;
using EyeCanDo.Core.Animation;
using EyeCanDo.Core.Games;
using EyeCanDo.Core.Progress;
using EyeCanDo.UI;
using UnityEngine;
using UnityEngine.UI;

namespace EyeCanDo.Map
{
    /// <summary>
    /// The Special Drops panel: the story (with its sticker) and the two games (with their stars).
    /// Games are never locked; until the story is done they carry a gentle "Story first?" tag.
    /// </summary>
    public sealed class StopPanel : MonoBehaviour
    {
        private Stage stage;
        private CaptionBank captions;
        private RectTransform root;
        private RectTransform panel;
        private Image dim;

        public ChunkyButton StoryCard { get; private set; }

        public ChunkyButton DropCatchCard { get; private set; }

        public ChunkyButton BlurrySpyCard { get; private set; }

        public ChunkyButton CloseButton { get; private set; }

        public bool IsOpen => root.gameObject.activeSelf;

        public static StopPanel Create(Stage stage, CaptionBank captions)
        {
            var rect = Ui.Rect(stage.Canvas.transform, "StopPanel");
            Ui.Stretch(rect);
            rect.SetSiblingIndex(stage.Overlay.GetSiblingIndex());
            var panel = rect.gameObject.AddComponent<StopPanel>();
            panel.stage = stage;
            panel.captions = captions;
            panel.root = rect;
            panel.Build();
            rect.gameObject.SetActive(false);
            return panel;
        }

        private void Build()
        {
            var theme = UiTheme.Instance;
            dim = Ui.Image(root, "Dim", null, new Color(theme.ink.r, theme.ink.g, theme.ink.b, 0.45f), true);
            Ui.Stretch(dim.rectTransform);

            var face = Ui.Panel(root, "Panel", theme.background, 48f);
            panel = (RectTransform)face.parent;
            Ui.Place(panel, new Vector2(0.5f, 0.5f), new Vector2(0f, -70f), new Vector2(1560f, 720f));

            var titleIcon = Ui.Art(face, "TitleIcon", "Stops/drops", new Vector2(90f, 90f));
            Ui.Place(titleIcon.rectTransform, new Vector2(0.5f, 1f), new Vector2(-250f, -62f), new Vector2(90f, 90f));
            var title = Ui.Text(face, "Special Drops", 70f, theme.ink);
            Ui.Place(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(60f, -62f), new Vector2(560f, 100f));

            CloseButton = ChunkyButton.CreateRound(face, "Close", ArtLibrary.Get("Icons/close"), theme.paper, 110f);
            Ui.Place((RectTransform)CloseButton.transform, new Vector2(1f, 1f), new Vector2(-20f, -20f), new Vector2(110f, 110f));
            CloseButton.Clicked += Close;

            var progress = GameServices.Progress;
            StoryCard = Card(face, new Vector2(-490f, -60f), theme.accent, "Story", "Watch and practise",
                card =>
                {
                    Ui.Art(card, "Doctor", "Characters/doctor_head", new Vector2(170f, 170f)).rectTransform.anchoredPosition = new Vector2(-50f, 96f);
                    Ui.Art(card, "DoctorEyes", "Characters/doctor_eyes_happy", new Vector2(170f, 170f)).rectTransform.anchoredPosition = new Vector2(-50f, 96f);
                    Ui.Art(card, "DoctorGlasses", "Characters/doctor_glasses", new Vector2(170f, 170f)).rectTransform.anchoredPosition = new Vector2(-50f, 96f);
                    Ui.Art(card, "DoctorMouth", "Characters/doctor_mouth_grin", new Vector2(170f, 170f)).rectTransform.anchoredPosition = new Vector2(-50f, 96f);
                    var mia = Characters.AvatarHead.Portrait(card, Core.Avatar.AvatarLook.Mia, 150f);
                    ((RectTransform)mia.transform).anchoredPosition = new Vector2(88f, 84f);
                    var sticker = Ui.Art(card, "Sticker", "Props/sticker_brave_eyes", new Vector2(84f, 84f));
                    sticker.rectTransform.anchoredPosition = new Vector2(0f, -205f);
                    sticker.color = progress.eyeDropsSticker ? Color.white : new Color(0.5f, 0.55f, 0.65f, 0.35f);
                });
            StoryCard.Clicked += () => GameServices.GoTo(SceneNames.EyeDropsStory);

            DropCatchCard = Card(face, new Vector2(0f, -60f), theme.success, "Drop Catch", "Catch the drops!",
                card =>
                {
                    Ui.Art(card, "Bottle", "Props/bottle_buddy", new Vector2(120f, 180f)).rectTransform.anchoredPosition = new Vector2(-40f, 110f);
                    Ui.Art(card, "Drop", "Props/drop", new Vector2(60f, 80f)).rectTransform.anchoredPosition = new Vector2(70f, 40f);
                    Ui.Art(card, "Gold", "Props/drop_gold", new Vector2(50f, 66f)).rectTransform.anchoredPosition = new Vector2(100f, 130f);
                    StarsFor(card, progress.dropCatch);
                });
            DropCatchCard.Clicked += () => StartGame(SceneNames.DropCatch, DropCatchCard);

            BlurrySpyCard = Card(face, new Vector2(490f, -60f), UiTheme.Hex("#B28CF0"), "Blurry Spy", "Find it in the blur!",
                card =>
                {
                    Ui.Art(card, "Eye", "Icons/blurry", new Vector2(210f, 210f)).rectTransform.anchoredPosition = new Vector2(0f, 96f);
                    StarsFor(card, progress.blurrySpy);
                });
            BlurrySpyCard.Clicked += () => StartGame(SceneNames.BlurrySpy, BlurrySpyCard);

            if (!progress.GamesUnlocked)
            {
                Lock(DropCatchCard);
                Lock(BlurrySpyCard);
            }
        }

        private static ChunkyButton Card(RectTransform parent, Vector2 position, Color color, string title, string subtitle, System.Action<RectTransform> fill)
        {
            var theme = UiTheme.Instance;
            var size = new Vector2(440f, 520f);
            var button = ChunkyButton.Create(parent, "Card_" + title, null, null, theme.paper, size, 40f);
            Ui.Place((RectTransform)button.transform, new Vector2(0.5f, 0.5f), position, size);
            var face = (RectTransform)button.transform.Find("Face");
            var band = Ui.Rounded(face, "Band", color, 34f);
            Ui.Place(band.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -10f), new Vector2(size.x - 32f, 290f), new Vector2(0.5f, 1f));
            var content = Ui.Rect(face, "Content");
            Ui.Stretch(content);
            fill(content);
            var label = Ui.Text(face, title, 56f, theme.ink);
            Ui.Place(label.rectTransform, new Vector2(0.5f, 0f), new Vector2(0f, 172f), new Vector2(size.x - 30f, 66f));
            var sub = Ui.Text(face, subtitle, 30f, UiTheme.Darker(theme.muted, 0.35f), TMPro.TextAlignmentOptions.Center, false);
            Ui.Place(sub.rectTransform, new Vector2(0.5f, 0f), new Vector2(0f, 126f), new Vector2(size.x - 30f, 40f));
            return button;
        }

        private static void StarsFor(RectTransform card, GameRecord record)
        {
            var theme = UiTheme.Instance;
            var row = Ui.Rect(card, "StarTotal");
            Ui.Place(row, new Vector2(0.5f, 0.5f), new Vector2(0f, -205f), new Vector2(240f, 64f));
            var star = Ui.Art(row, "Star", "Icons/star", new Vector2(64f, 64f));
            star.rectTransform.anchoredPosition = new Vector2(-70f, 0f);
            var text = Ui.Text(row, $"{record.TotalStars} / {Levels.Max * StarRules.MaxStars}", 42f, theme.ink);
            Ui.Place(text.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(40f, 0f), new Vector2(160f, 64f));
        }

        /// <summary>Greys a game card out with a padlock until the story is finished.</summary>
        private static void Lock(ChunkyButton card)
        {
            var theme = UiTheme.Instance;
            var cover = Ui.Rounded(card.transform, "Locked", new Color(theme.background.r, theme.background.g, theme.background.b, 0.7f), 40f);
            Ui.Stretch(cover.rectTransform, 6f);
            cover.rectTransform.offsetMin = new Vector2(6f, -6f);
            var padlock = Ui.Art(cover.rectTransform, "Padlock", "Icons/lock", new Vector2(150f, 150f));
            padlock.rectTransform.anchoredPosition = new Vector2(0f, 70f);
            var text = Ui.Text(cover.rectTransform, "Finish the story first", 34f, theme.ink);
            Ui.Place(text.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, -60f), new Vector2(380f, 100f));
        }

        private void StartGame(string scene, ChunkyButton card)
        {
            if (!GameServices.Progress.GamesUnlocked)
            {
                Sfx.Play(Sound.Locked);
                StartCoroutine(Tween.Wiggle(card.transform));
                stage.Captions.Show(captions.gamesLocked);
                stage.Ghost.ShowTap((RectTransform)StoryCard.transform);
                return;
            }

            GameServices.PendingLevel = 0;
            GameServices.GoTo(scene);
        }

        public void Open()
        {
            if (IsOpen)
            {
                return;
            }

            root.gameObject.SetActive(true);
            Sfx.Play(Sound.Whoosh);
            StartCoroutine(Tween.Pop(panel, 0.35f));
            stage.Captions.Show(GameServices.Progress.eyeDropsStory == StopStatus.Completed ? captions.pickActivity : captions.storyFirst);
            if (GameServices.Progress.eyeDropsStory == StopStatus.NotStarted)
            {
                StartCoroutine(HintStory());
            }
        }

        private IEnumerator HintStory()
        {
            yield return Tween.Wait(0.5f);
            stage.Ghost.ShowTap((RectTransform)StoryCard.transform);
        }

        public void Close()
        {
            stage.Ghost.Stop();
            StartCoroutine(CloseRoutine());
        }

        private IEnumerator CloseRoutine()
        {
            yield return Tween.Scale(panel, Vector3.zero, 0.2f, Ease.InCubic);
            panel.localScale = Vector3.one;
            root.gameObject.SetActive(false);
            stage.Captions.Hide();
        }
    }
}
