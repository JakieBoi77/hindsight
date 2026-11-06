using System.Collections;
using EyeCanDo.App;
using EyeCanDo.Art;
using EyeCanDo.Characters;
using EyeCanDo.Content;
using EyeCanDo.Core.Games;
using EyeCanDo.Core.Progress;
using EyeCanDo.UI;
using TMPro;
using UnityEngine;

namespace EyeCanDo.Menus
{
    /// <summary>
    /// "Who's playing?": one card per save file, between Play and the clinic map. A used save
    /// shows its character, coins, stars and whether the story is done; tapping it carries on
    /// from there. An empty save starts a new game. Each used save has a Delete button that asks
    /// first and says what will be lost.
    /// </summary>
    public sealed class SaveSelectScreen : MonoBehaviour
    {
        public static readonly Vector2 CardSize = new Vector2(500f, 660f);
        private const float CardGap = 560f;
        private const float CardY = -20f;
        private const float KidScale = 0.37f;

        [SerializeField] private CaptionBank captions;

        private Stage stage;
        private RectTransform layer;

        public Stage Stage => stage;

        /// <summary>The card for each slot (also the PlayMode test hook).</summary>
        public ChunkyButton[] Cards { get; } = new ChunkyButton[SaveSlots.Count];

        /// <summary>The Delete button under each used save; null under an empty one.</summary>
        public ChunkyButton[] DeleteButtons { get; } = new ChunkyButton[SaveSlots.Count];

        private void Awake()
        {
            if (captions == null)
            {
                captions = DefaultContent.CreateCaptionBank();
            }

            stage = Stage.Create("Saves", true);
            stage.Chrome.Restart.gameObject.SetActive(false);
            stage.Chrome.SkipAllowed = false;
            stage.Chrome.HomePressed += () => GameServices.GoTo(SceneNames.Title);
            stage.BuildOutdoors(-260f);
            Build();
        }

        private IEnumerator Start()
        {
            yield return null;
            stage.Captions.Show(captions.savesHello);
            if (!AnyUsed())
            {
                yield return Tween.Wait(0.6f);
                stage.Ghost.ShowTap((RectTransform)Cards[0].transform);
            }
        }

        private static bool AnyUsed()
        {
            for (var slot = 0; slot < SaveSlots.Count; slot++)
            {
                if (GameServices.Saves.IsUsed(slot))
                {
                    return true;
                }
            }

            return false;
        }

        private void Build()
        {
            if (layer != null)
            {
                Destroy(layer.gameObject);
            }

            layer = stage.Layer("Saves");
            for (var slot = 0; slot < SaveSlots.Count; slot++)
            {
                var position = new Vector2((slot - (SaveSlots.Count - 1) / 2f) * CardGap, CardY);
                var progress = GameServices.Saves.Peek(slot);
                Cards[slot] = progress != null ? UsedCard(slot, progress, position) : EmptyCard(slot, position);
                var chosen = slot;
                Cards[slot].Clicked += () => GameServices.PlaySave(chosen);
                DeleteButtons[slot] = progress != null ? DeleteButton(slot, progress, position) : null;
            }
        }

        private ChunkyButton Card(int slot, Vector2 position, Color band, out RectTransform face)
        {
            var theme = UiTheme.Instance;
            var card = ChunkyButton.Create(layer, "Save_" + (slot + 1), null, null, theme.paper, CardSize, 40f);
            Ui.Place((RectTransform)card.transform, new Vector2(0.5f, 0.5f), position, CardSize);
            face = (RectTransform)card.transform.Find("Face");
            var top = Ui.Rounded(face, "Band", band, 34f);
            Ui.Place(top.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -10f), new Vector2(CardSize.x - 32f, 360f), new Vector2(0.5f, 1f));
            var name = Ui.Text(face, $"Save {slot + 1}", 52f, theme.ink);
            Ui.Place(name.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, -100f), new Vector2(CardSize.x - 40f, 70f));
            return card;
        }

        private static Color BandColour(int slot)
        {
            var theme = UiTheme.Instance;
            switch (slot)
            {
                case 0: return theme.accent;
                case 1: return theme.success;
                default: return UiTheme.Hex("#B28CF0");
            }
        }

        private ChunkyButton UsedCard(int slot, PlayerProgress progress, Vector2 position)
        {
            var card = Card(slot, position, BandColour(slot), out var face);
            var kid = CharacterRig.CreateKid(face, progress.look, "stand", KidScale);
            kid.Root.anchoredPosition = new Vector2(0f, -36f);
            kid.SetMouth(Mouth.Grin);

            var stars = progress.dropCatch.TotalStars + progress.blurrySpy.TotalStars;
            Stat(face, new Vector2(-112f, -178f), 220f, "Icons/coin", progress.coins.ToString(), true);
            Stat(face, new Vector2(112f, -178f), 220f, "Icons/star", $"{stars}/{2 * Levels.Max * StarRules.MaxStars}", true);
            var storyDone = progress.eyeDropsSticker;
            Stat(face, new Vector2(0f, -256f), 320f, "Props/sticker_brave_eyes", storyDone ? "Story done!" : "Story to do", storyDone);
            return card;
        }

        private ChunkyButton EmptyCard(int slot, Vector2 position)
        {
            var theme = UiTheme.Instance;
            var card = Card(slot, position, UiTheme.Hex("#E3E8EF"), out var face);
            var plus = Ui.Disc(face, "New", theme.primary);
            Ui.Place((RectTransform)plus.parent, new Vector2(0.5f, 1f), new Vector2(0f, -190f), new Vector2(180f, 180f));
            var icon = Ui.Image(plus, "Plus", ArtLibrary.Get("Icons/plus"), Color.white);
            icon.preserveAspect = true;
            Ui.Stretch(icon.rectTransform, 40f);
            var title = Ui.Text(face, "New game", 46f, theme.ink);
            Ui.Place(title.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, -180f), new Vector2(CardSize.x - 40f, 64f));
            var hint = Ui.Text(face, "Tap to start", 32f, UiTheme.Darker(theme.muted, 0.35f), TextAlignmentOptions.Center, false);
            Ui.Place(hint.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, -248f), new Vector2(CardSize.x - 40f, 50f));
            return card;
        }

        /// <summary>A picture and a number. Things not earned yet are greyed and say so in words too.</summary>
        private static void Stat(RectTransform face, Vector2 position, float width, string icon, string text, bool earned)
        {
            var theme = UiTheme.Instance;
            var row = Ui.Rect(face, "Stat");
            Ui.Place(row, new Vector2(0.5f, 0.5f), position, new Vector2(width, 70f));
            var picture = Ui.Art(row, "Icon", icon, new Vector2(62f, 62f));
            Ui.Place(picture.rectTransform, new Vector2(0f, 0.5f), new Vector2(34f, 0f), new Vector2(62f, 62f));
            picture.color = earned ? Color.white : new Color(0.5f, 0.55f, 0.65f, 0.4f);
            var label = Ui.Text(row, text, 40f, earned ? theme.ink : UiTheme.Darker(theme.muted, 0.35f), TextAlignmentOptions.MidlineLeft);
            Ui.Place(label.rectTransform, new Vector2(0f, 0.5f), new Vector2(76f, 0f), new Vector2(width - 76f, 64f), new Vector2(0f, 0.5f));
            label.textWrappingMode = TextWrappingModes.NoWrap;
        }

        private ChunkyButton DeleteButton(int slot, PlayerProgress progress, Vector2 cardPosition)
        {
            var theme = UiTheme.Instance;
            var size = new Vector2(300f, 100f);
            var button = ChunkyButton.Create(layer, "Delete_" + (slot + 1), "Delete", ArtLibrary.Get("Icons/trash"), theme.paper, size, 50f);
            Ui.Place((RectTransform)button.transform, new Vector2(0.5f, 0.5f), cardPosition + new Vector2(0f, -CardSize.y / 2f - 84f), size);
            button.Label.fontSize = 40f;
            button.Clicked += () => AskDelete(slot, progress);
            return button;
        }

        /// <summary>Asks before deleting, and says exactly what goes: this save's character and earnings.</summary>
        public void AskDelete(int slot, PlayerProgress progress)
        {
            stage.Ghost.Stop();
            ConfirmDialog.Show(
                $"Delete Save {slot + 1}?",
                $"This player's character, {progress.coins} coins, stars and sticker will be deleted. The other saves stay.",
                "Icons/trash", "Delete", "Keep it",
                () =>
                {
                    GameServices.DeleteSave(slot);
                    Sfx.Play(Sound.Wipe);
                    Build();
                    stage.Captions.Show(captions.saveDeleted);
                });
        }
    }
}
