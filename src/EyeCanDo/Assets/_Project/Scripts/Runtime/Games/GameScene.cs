using System;
using System.Collections;
using EyeCanDo.App;
using EyeCanDo.Art;
using EyeCanDo.Content;
using EyeCanDo.Core.Animation;
using EyeCanDo.Core.Games;
using EyeCanDo.Core.Progress;
using EyeCanDo.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EyeCanDo.Games
{
    /// <summary>
    /// What every game screen shares: pick a level (all open, the suggested one bobs), a score and
    /// timer bar, and a results card with stars, the score and your best. Results only ever add:
    /// bests never go down and every finished round earns at least one star.
    /// </summary>
    public abstract class GameScene : MonoBehaviour
    {
        [SerializeField] protected CaptionBank captions;

        private RectTransform modal;
        private RectTransform modalPanel;
        private TextMeshProUGUI scoreText;
        private TextMeshProUGUI timerText;
        private Image timerIcon;
        private RectTransform hud;

        protected Stage Stage { get; private set; }

        public int Level { get; private set; }

        public bool IsPlaying { get; private set; }

        /// <summary>The countdown has finished and the round is running.</summary>
        public bool Live { get; private set; }

        /// <summary>The number on screen during the countdown (3, 2, 1, then 0 for "Go!").</summary>
        public int CountingDown { get; private set; } = -1;

        public bool ShowingResults { get; private set; }

        public bool PickingLevel { get; private set; }

        public ChunkyButton[] LevelButtons { get; } = new ChunkyButton[Levels.Max];

        public ChunkyButton AgainButton { get; private set; }

        public ChunkyButton NextButton { get; private set; }

        public ChunkyButton MapButton { get; private set; }

        public int LastStars { get; private set; }

        public int LastCoins { get; private set; }

        public CoinCounter Coins { get; private set; }

        /// <summary>Where menus go: beside the characters, never on top of them.</summary>
        protected virtual Vector2 MenuCentre => new Vector2(60f, -70f);

        /// <summary>The game is locked because the story is not finished yet.</summary>
        public bool ShowingLocked { get; private set; }

        public int LastScore { get; private set; }

        protected abstract GameId Game { get; }

        protected abstract string SceneName { get; }

        protected abstract string Title { get; }

        protected abstract void BuildScene();

        /// <summary>Plays one round, then reports its stars and score.</summary>
        protected abstract IEnumerator PlayRound(int level, System.Action<int, int> finished);

        protected virtual void Awake()
        {
            if (captions == null)
            {
                captions = DefaultContent.CreateCaptionBank();
            }

            Stage = Stage.Create(SceneName, true);
            Stage.Captions.YouLook = GameServices.Progress.look;
            Stage.Chrome.SkipAllowed = false;
            Stage.Chrome.HomePressed += () =>
            {
                GameServices.Save();
                GameServices.GoToMap(StopCatalog.EyeDropsId);
            };
            Stage.Chrome.RestartPressed += () => Replay(Level > 0 ? Level : 0);
            BuildScene();
            BuildHud();
            Coins = CoinCounter.Create(Stage.Overlay, GameServices.Progress.coins);
            modal = Ui.Rect(Stage.Canvas.transform, "Modal");
            Ui.Stretch(modal);
            modal.SetSiblingIndex(Stage.Overlay.GetSiblingIndex());
            modal.gameObject.SetActive(false);
        }

        private IEnumerator Start()
        {
            yield return null;
            var pending = GameServices.PendingLevel;
            GameServices.PendingLevel = 0;
            if (!GameServices.Progress.GamesUnlocked)
            {
                ShowLocked();
            }
            else if (pending >= Levels.Min && pending <= Levels.Max)
            {
                StartLevel(pending);
            }
            else
            {
                ShowLevelPicker();
            }
        }

        private void Replay(int level)
        {
            GameServices.PendingLevel = level;
            GameServices.GoTo(SceneName);
        }

        // HUD ----------------------------------------------------------------------------------

        private void BuildHud()
        {
            hud = Stage.Layer("Hud");
            scoreText = Pill(hud, new Vector2(-700f, 262f), "Props/drop", out _);
            timerText = Pill(hud, new Vector2(700f, 262f), "Icons/clock", out timerIcon);
            hud.gameObject.SetActive(false);
        }

        private static TextMeshProUGUI Pill(RectTransform parent, Vector2 position, string icon, out Image iconImage)
        {
            var theme = UiTheme.Instance;
            var face = Ui.Panel(parent, "Pill", theme.paper, 46f);
            Ui.Place((RectTransform)face.parent, new Vector2(0.5f, 0.5f), position, new Vector2(280f, 100f));
            iconImage = Ui.Art(face, "Icon", icon, new Vector2(70f, 70f));
            Ui.Place(iconImage.rectTransform, new Vector2(0f, 0.5f), new Vector2(56f, 0f), new Vector2(70f, 70f));
            var text = Ui.Text(face, "0", 60f, theme.ink);
            Ui.Place(text.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(40f, 0f), new Vector2(170f, 90f));
            text.enableAutoSizing = false;
            return text;
        }

        protected void ShowHud(string timerIconArt, bool showScore = true)
        {
            hud.gameObject.SetActive(true);
            scoreText.transform.parent.parent.gameObject.SetActive(showScore);
            timerIcon.sprite = ArtLibrary.Get(timerIconArt);
        }

        protected void SetScore(int score, bool bump = true)
        {
            scoreText.text = score.ToString();
            if (bump)
            {
                StartCoroutine(Tween.Bump(scoreText.transform.parent, 0.12f, 0.2f));
            }
        }

        protected void SetTimer(string text)
        {
            timerText.text = text;
        }

        // Level picker -------------------------------------------------------------------------

        public void ShowLevelPicker()
        {
            PickingLevel = true;
            var theme = UiTheme.Instance;
            var face = OpenModal(new Vector2(1100f, 620f));
            var title = Ui.Text(face, Title, 66f, theme.ink);
            Ui.Place(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -60f), new Vector2(900f, 96f));
            var record = GameServices.Progress.For(Game);
            for (var level = Levels.Min; level <= Levels.Max; level++)
            {
                var chosen = level;
                var color = level == 1 ? theme.success : level == 2 ? theme.primary : UiTheme.Hex("#FF8E72");
                var button = ChunkyButton.Create(face, "Level" + level, null, null, color, new Vector2(310f, 360f), 44f);
                Ui.Place((RectTransform)button.transform, new Vector2(0.5f, 0.5f), new Vector2((level - 2) * 340f, -60f), new Vector2(310f, 360f));
                var buttonFace = (RectTransform)button.transform.Find("Face");
                var number = Ui.Text(buttonFace, level.ToString(), 140f, theme.ink);
                Ui.Place(number.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, 70f), new Vector2(260f, 160f));
                number.enableAutoSizing = false;
                for (var d = 0; d < level; d++)
                {
                    var drop = Ui.Art(buttonFace, "Difficulty", "Props/drop", new Vector2(40f, 54f));
                    drop.rectTransform.anchoredPosition = new Vector2((d - (level - 1) / 2f) * 46f, -30f);
                }

                var stars = StarRow.Create(buttonFace, 70f);
                Ui.Place((RectTransform)stars.transform, new Vector2(0.5f, 0.5f), new Vector2(0f, -110f), ((RectTransform)stars.transform).sizeDelta);
                stars.Set(record.StarsAt(level));
                if (level == record.SuggestedLevel)
                {
                    button.gameObject.AddComponent<Bob>().Height = 12f;
                }

                button.Clicked += () => StartLevel(chosen);
                LevelButtons[level - 1] = button;
            }

            Stage.Captions.Show(captions.pickLevel);
        }

        /// <summary>The games open once the story is finished; until then, say so and offer the map.</summary>
        private void ShowLocked()
        {
            ShowingLocked = true;
            var theme = UiTheme.Instance;
            var face = OpenModal(new Vector2(900f, 560f));
            var padlock = Ui.Art(face, "Padlock", "Icons/lock", new Vector2(190f, 190f));
            padlock.rectTransform.anchoredPosition = new Vector2(0f, 90f);
            MapButton = ResultButton(face, "Map", "Icons/home", theme.primary, new Vector2(0f, -160f));
            MapButton.Clicked += () => GameServices.GoToMap(StopCatalog.EyeDropsId);
            Sfx.Play(Sound.Locked);
            Stage.Captions.Show(captions.gamesLocked);
        }

        /// <summary>Starts a round at a level (also the PlayMode test hook).</summary>
        public void StartLevel(int level)
        {
            if (IsPlaying || ShowingLocked)
            {
                return;
            }

            PickingLevel = false;
            CloseModal();
            Level = Levels.Clamp(level);
            StartCoroutine(Run());
        }

        private IEnumerator Run()
        {
            IsPlaying = true;
            var stars = 1;
            var score = 0;
            yield return PlayRound(Level, (s, p) =>
            {
                stars = s;
                score = p;
            });
            IsPlaying = false;
            Live = false;
            Stage.Ghost.Stop();
            yield return ShowResults(stars, score);
        }

        // Get ready ----------------------------------------------------------------------------

        /// <summary>
        /// The start of every round: one instruction that a tap continues at once, then 3, 2, 1, Go!
        /// <paramref name="during"/> runs every frame of the countdown, so the scene can get ready.
        /// </summary>
        protected IEnumerator GetReady(CaptionLine instruction, CaptionLine whileCounting, Action<float> during = null)
        {
            yield return Stage.Captions.Instruct(instruction);
            Stage.Captions.Show(whileCounting);
            var layer = Stage.Layer("Countdown");
            var theme = UiTheme.Instance;
            for (var count = 3; count >= 0; count--)
            {
                CountingDown = count;
                var go = count == 0;
                var disc = Ui.Disc(layer, "Count", go ? theme.success : theme.paper);
                var root = (RectTransform)disc.parent;
                Ui.Place(root, new Vector2(0.5f, 0.5f), new Vector2(MenuCentre.x, 70f), new Vector2(go ? 300f : 240f, go ? 300f : 240f));
                var label = Ui.Text(disc, go ? "Go!" : count.ToString(), go ? 110f : 150f, theme.ink);
                Ui.Stretch(label.rectTransform);
                label.enableAutoSizing = false;
                Sfx.Play(go ? Sound.Go : Sound.Count);
                StartCoroutine(Tween.Pop(root, 0.25f));
                var seconds = go ? 0.45f : 0.6f;
                for (var t = 0f; t < seconds; t += Time.deltaTime)
                {
                    during?.Invoke(Time.deltaTime);
                    yield return null;
                }

                if (go)
                {
                    StartCoroutine(FadeOut(root));
                }
                else
                {
                    Destroy(root.gameObject);
                }
            }

            CountingDown = -1;
            Live = true;
        }

        private static IEnumerator FadeOut(RectTransform rect)
        {
            var group = rect.gameObject.AddComponent<CanvasGroup>();
            yield return Tween.Run(0.25f, Ease.OutCubic, t =>
            {
                group.alpha = 1f - t;
                rect.localScale = Vector3.one * (1f + 0.3f * t);
            });
            Destroy(rect.gameObject);
        }

        // Results ------------------------------------------------------------------------------

        private IEnumerator ShowResults(int stars, int score)
        {
            LastStars = stars;
            LastScore = score;
            var previousBest = GameServices.Progress.For(Game).ScoreAt(Level);
            var round = GameServices.Progress.FinishRound(Game, Level, stars, score);
            var result = round.Record;
            LastCoins = round.Coins;
            GameServices.Save();

            var theme = UiTheme.Instance;
            Sfx.Play(Sound.RoundEnd);
            var face = OpenModal(new Vector2(1100f, 700f));
            ShowingResults = true;
            var title = Ui.Text(face, $"Level {Level}", 56f, theme.ink);
            Ui.Place(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -50f), new Vector2(700f, 76f));
            var row = StarRow.Create(face, 140f, 24f);
            Ui.Place((RectTransform)row.transform, new Vector2(0.5f, 0.5f), new Vector2(0f, 150f), ((RectTransform)row.transform).sizeDelta);
            row.Set(0);

            var scoreLine = Ui.Rect(face, "Score");
            Ui.Place(scoreLine, new Vector2(0.5f, 0.5f), new Vector2(0f, 20f), new Vector2(700f, 90f));
            var icon = Ui.Art(scoreLine, "Icon", "Props/drop", new Vector2(64f, 84f));
            icon.rectTransform.anchoredPosition = new Vector2(-110f, 0f);
            var points = Ui.Text(scoreLine, score.ToString(), 80f, theme.ink);
            Ui.Place(points.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(30f, 0f), new Vector2(240f, 90f));
            var best = Ui.Text(face, result.NewBestScore && previousBest > 0 ? "New best!" : $"Best: {Mathf.Max(score, previousBest)}", 40f,
                result.NewBestScore && previousBest > 0 ? UiTheme.Darker(theme.success, 0.25f) : UiTheme.Darker(theme.muted, 0.35f));
            Ui.Place(best.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, -46f), new Vector2(600f, 56f));
            var coinLine = Ui.Rect(face, "Coins");
            Ui.Place(coinLine, new Vector2(0.5f, 0.5f), new Vector2(0f, -116f), new Vector2(400f, 80f));
            var coinIcon = Ui.Art(coinLine, "Coin", "Icons/coin", new Vector2(70f, 70f));
            coinIcon.rectTransform.anchoredPosition = new Vector2(-70f, 0f);
            var coinText = Ui.Text(coinLine, "+" + round.Coins, 60f, UiTheme.Darker(theme.primary, 0.3f));
            Ui.Place(coinText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(40f, 0f), new Vector2(180f, 76f));
            coinText.enableAutoSizing = false;

            AgainButton = ResultButton(face, "Again", "Icons/restart", theme.paper, new Vector2(-330f, -262f));
            AgainButton.Clicked += () => Replay(Level);
            MapButton = ResultButton(face, "Map", "Icons/home", theme.paper, Level < Levels.Max ? new Vector2(0f, -262f) : new Vector2(330f, -262f));
            MapButton.Clicked += () => GameServices.GoToMap(StopCatalog.EyeDropsId);
            if (Level < Levels.Max)
            {
                NextButton = ResultButton(face, $"Level {Level + 1}", "Icons/arrow_right", theme.primary, new Vector2(330f, -262f));
                NextButton.Clicked += () => Replay(Level + 1);
                if (stars >= 2)
                {
                    NextButton.gameObject.AddComponent<Bob>().Height = 8f;
                }
            }

            // First the stars, one chime each; then the celebration; then the coins land.
            yield return Tween.Wait(0.5f);
            yield return row.Reveal(stars);
            StartCoroutine(Confetti.Burst(face, new Vector2(0f, 150f), 14 + stars * 6, 520f));
            Sfx.Play(result.NewBestScore && previousBest > 0 ? Sound.Reward : Sound.Cheer);
            Stage.Captions.Show(result.NewBestScore && previousBest > 0 ? captions.newBest : captions.greatGame);
            yield return Tween.Wait(1.1f);
            yield return Coins.Earn(coinIcon.rectTransform.position, round.Coins, Stage.Overlay);
        }

        private static ChunkyButton ResultButton(RectTransform parent, string label, string icon, Color color, Vector2 position)
        {
            var button = ChunkyButton.Create(parent, label, label, ArtLibrary.Get(icon), color, new Vector2(300f, 130f), 50f);
            Ui.Place((RectTransform)button.transform, new Vector2(0.5f, 0.5f), position, new Vector2(300f, 130f));
            button.Label.fontSize = 44f;
            return button;
        }

        // Modal --------------------------------------------------------------------------------

        private RectTransform OpenModal(Vector2 size)
        {
            CloseModal();
            var theme = UiTheme.Instance;
            modal.gameObject.SetActive(true);
            var dim = Ui.Image(modal, "Dim", null, new Color(theme.ink.r, theme.ink.g, theme.ink.b, 0.4f), true);
            Ui.Stretch(dim.rectTransform);
            var face = Ui.Panel(modal, "Panel", theme.background, 48f);
            modalPanel = (RectTransform)face.parent;
            Ui.Place(modalPanel, new Vector2(0.5f, 0.5f), MenuCentre, size);
            StartCoroutine(Tween.Pop(modalPanel, 0.35f));
            Sfx.Play(Sound.Whoosh);
            return face;
        }

        private void CloseModal()
        {
            for (var i = modal.childCount - 1; i >= 0; i--)
            {
                Destroy(modal.GetChild(i).gameObject);
            }

            modal.gameObject.SetActive(false);
            ShowingResults = false;
        }

        /// <summary>Floating "+1" style text that rises and fades, in world coordinates.</summary>
        protected void FloatText(RectTransform layer, string text, Vector2 position, Color color, float size = 64f)
        {
            var label = Ui.Text(layer, text, size, color);
            label.enableAutoSizing = false;
            label.outlineWidth = 0.2f;
            label.outlineColor = UiTheme.Instance.ink;
            Ui.Place(label.rectTransform, new Vector2(0.5f, 0.5f), position, new Vector2(320f, 100f));
            StartCoroutine(FloatAway(label, position));
        }

        private static IEnumerator FloatAway(TextMeshProUGUI label, Vector2 from)
        {
            yield return Tween.Run(0.9f, Ease.OutCubic, t =>
            {
                if (label == null)
                {
                    return;
                }

                label.rectTransform.anchoredPosition = from + Vector2.up * (110f * t);
                label.alpha = 1f - t * t;
            });
            if (label != null)
            {
                Destroy(label.gameObject);
            }
        }
    }
}
