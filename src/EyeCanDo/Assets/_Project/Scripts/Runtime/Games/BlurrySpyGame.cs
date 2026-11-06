using System;
using System.Collections;
using System.Collections.Generic;
using EyeCanDo.App;
using EyeCanDo.Art;
using EyeCanDo.Characters;
using EyeCanDo.Core.Animation;
using EyeCanDo.Core.Games;
using EyeCanDo.Core.Interaction;
using EyeCanDo.UI;
using UnityEngine;
using UnityEngine.UI;

namespace EyeCanDo.Games
{
    /// <summary>
    /// Blurry Spy. After eye drops, things can look blurry and bright for a while. The playroom
    /// starts blurry and full of glare; a card shows a thing to find, and the child taps it in the
    /// room. Every find clears the blur a little (it goes away). Wrong taps only wiggle and cost
    /// nothing, but three stars need a perfect round: no wrong taps, and quick finds.
    /// </summary>
    public sealed class BlurrySpyGame : GameScene
    {
        public const string BlurMaterialPath = "SpyBlur";

        private static readonly string[] ItemArt =
        {
            "duck", "ball", "kite", "teddy", "apple", "car", "flower", "fish",
            "book", "rocket", "boat", "cat", "butterfly", "balloon", "shoe", "cupcake", "umbrella", "drum",
        };

        private static readonly Vector2[] Slots =
        {
            new Vector2(60f, 240f), new Vector2(300f, 250f), new Vector2(540f, 230f),
            new Vector2(-850f, -40f), new Vector2(80f, 40f), new Vector2(320f, 30f), new Vector2(560f, 50f),
            new Vector2(-640f, -190f), new Vector2(-390f, -210f), new Vector2(-140f, -140f), new Vector2(150f, -190f), new Vector2(420f, -200f),
            new Vector2(-820f, -430f), new Vector2(-520f, -440f), new Vector2(-210f, -420f), new Vector2(110f, -450f), new Vector2(420f, -430f),
            new Vector2(-610f, 40f),
        };

        private const float BlurScale = 1f;

        private Material blur;
        private RectTransform itemsLayer;
        private Image veil;
        private Image windowGlare;
        private RectTransform card;
        private Image cardPicture;
        private CharacterRig kid;
        private readonly List<SpyItem> items = new List<SpyItem>();
        private readonly List<Image> blurred = new List<Image>();
        private BlurrySpyRound round;
        private readonly IdleHint idleHint = new IdleHint();
        private bool saidClearing;
        private float notThisCooldown;

        public BlurrySpyRound Round => round;

        public IReadOnlyList<SpyItem> Items => items;

        protected override GameId Game => GameId.BlurrySpy;

        protected override string SceneName => SceneNames.BlurrySpy;

        protected override string Title => "Blurry Spy";

        public sealed class SpyItem
        {
            public int Index;
            public string Art;
            public RectTransform Rect;
            public Image Image;
            public PressArea Press;
            public Vector2 Home;
            public float Phase;
            public bool Found;
        }

        protected override void BuildScene()
        {
            var source = Resources.Load<Material>(BlurMaterialPath);
            blur = source != null ? new Material(source) : null;

            var theme = UiTheme.Instance;
            Stage.Fill(UiTheme.Hex("#FFF1DC"));
            var floor = Stage.Fill(UiTheme.Hex("#F7D7A8"), -330f);
            floor.name = "Floor";
            var decor = Stage.Layer("Room");
            Blurred(Stage.Prop(decor, "Room/window", new Vector2(-360f, 150f), new Vector2(420f, 340f)));
            Blurred(Stage.Prop(decor, "Spy/shelf", new Vector2(430f, 70f), new Vector2(380f, 434f)));
            Blurred(Stage.Prop(decor, "Spy/sofa", new Vector2(-300f, -250f), new Vector2(620f, 300f)));
            Blurred(Stage.Prop(decor, "Room/rug", new Vector2(150f, -470f), new Vector2(760f, 170f)));
            Blurred(Stage.Prop(decor, "Room/plant", new Vector2(-880f, -280f), new Vector2(140f, 216f)));

            itemsLayer = Stage.Layer("Items");

            var glare = Stage.Layer("Glare");
            var glareGroup = glare.gameObject.AddComponent<CanvasGroup>();
            glareGroup.blocksRaycasts = false;
            windowGlare = Ui.Image(glare, "WindowGlare", Shapes.Glow, new Color(1f, 0.98f, 0.86f, 1f));
            Ui.Place(windowGlare.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(-360f, 150f), new Vector2(1400f, 1200f));
            veil = Ui.Image(glare, "Veil", null, new Color(1f, 1f, 0.96f, 0.3f));
            Ui.Stretch(veil.rectTransform);
            veil.rectTransform.offsetMin = new Vector2(-400f, -400f);
            veil.rectTransform.offsetMax = new Vector2(400f, 400f);

            var people = Stage.Layer("Kid");
            kid = CharacterRig.CreateKid(people, GameServices.Progress.look, "stand", 0.46f);
            kid.Root.anchoredPosition = new Vector2(820f, -540f);
            kid.SetEyes(Eyes.Squeeze);

            // The "find this" card, top left of the play area.
            var cardFace = Ui.Panel(Stage.Layer("Card"), "FindCard", theme.paper, 36f);
            card = (RectTransform)cardFace.parent;
            Ui.Place(card, new Vector2(0.5f, 0.5f), new Vector2(-760f, 210f), new Vector2(230f, 230f));
            var magnifier = Ui.Disc(cardFace, "Badge", theme.accent);
            Ui.Place((RectTransform)magnifier.parent, new Vector2(0f, 1f), new Vector2(16f, -16f), new Vector2(76f, 76f));
            var eye = Ui.Art(magnifier, "Eye", "Icons/eye", new Vector2(56f, 56f));
            eye.rectTransform.anchoredPosition = Vector2.zero;
            cardPicture = Ui.Art(cardFace, "Picture", "Spy/duck", new Vector2(170f, 170f));
            cardPicture.rectTransform.anchoredPosition = new Vector2(0f, -6f);
            card.gameObject.SetActive(false);
        }

        private void Blurred(Image image)
        {
            if (blur != null)
            {
                image.material = blur;
            }

            blurred.Add(image);
        }

        protected override IEnumerator PlayRound(int level, Action<int, int> finished)
        {
            var settings = BlurrySpySettings.ForLevel(level);
            round = new BlurrySpyRound(settings, Environment.TickCount);
            PlaceItems(settings);
            ApplyLook();
            ShowHud("Icons/eye", false);
            SetScore(0, false);
            SetTimer($"0/{settings.Targets}");

            yield return GetReady(captions.spyIntro, captions.spyHowTo);
            card.gameObject.SetActive(true);
            StartCoroutine(Tween.Pop(card, 0.3f));
            ShowTarget();
            idleHint.Restart();

            while (!round.Done)
            {
                var dt = Time.deltaTime;
                round.Tick(dt);
                notThisCooldown -= dt;
                if (settings.ItemsDrift)
                {
                    Drift(dt);
                }

                if (idleHint.Tick(dt))
                {
                    HintTarget();
                }

                yield return null;
            }

            card.gameObject.SetActive(false);
            kid.SetEyes(Eyes.Happy);
            kid.SetMouth(Mouth.Grin);
            StartCoroutine(kid.Bounce(40f, false));
            yield return Stage.Captions.Say(captions.allFound, false);
            finished(round.Stars, round.Score);
        }

        private void PlaceItems(BlurrySpySettings settings)
        {
            foreach (var item in items)
            {
                Destroy(item.Rect.gameObject);
            }

            items.Clear();
            var art = new List<string>(ItemArt);
            var slots = new List<Vector2>(Slots);
            var random = new System.Random(Environment.TickCount);
            Shuffle(art, random);
            Shuffle(slots, random);
            for (var i = 0; i < settings.Items; i++)
            {
                var image = Ui.Art(itemsLayer, "Item_" + art[i], "Spy/" + art[i], new Vector2(settings.ItemSize, settings.ItemSize));
                image.raycastTarget = true;
                image.rectTransform.anchoredPosition = slots[i];
                image.rectTransform.localRotation = Quaternion.Euler(0f, 0f, (float)(random.NextDouble() * 24.0 - 12.0));
                Blurred(image);
                var item = new SpyItem
                {
                    Index = i,
                    Art = art[i],
                    Rect = image.rectTransform,
                    Image = image,
                    Home = slots[i],
                    Phase = (float)random.NextDouble() * 6f,
                };
                item.Press = image.gameObject.AddComponent<PressArea>();
                var captured = item;
                item.Press.Tapped += () => Tap(captured);
                items.Add(item);
            }
        }

        private static void Shuffle<T>(IList<T> list, System.Random random)
        {
            for (var i = list.Count - 1; i > 0; i--)
            {
                var j = random.Next(i + 1);
                var swap = list[i];
                list[i] = list[j];
                list[j] = swap;
            }
        }

        private void Drift(float dt)
        {
            foreach (var item in items)
            {
                if (item.Found)
                {
                    continue;
                }

                item.Phase += dt * 0.6f;
                item.Rect.anchoredPosition = item.Home + new Vector2(Mathf.Sin(item.Phase) * 40f, Mathf.Cos(item.Phase * 0.8f) * 24f);
            }
        }

        /// <summary>Taps an item (also the PlayMode test hook).</summary>
        public void Tap(SpyItem item)
        {
            if (round == null || !Live || round.Done || item.Found)
            {
                return;
            }

            idleHint.Reset();
            var result = round.Tap(item.Index);
            if (result == SpyTapResult.Found)
            {
                StartCoroutine(Found(item));
            }
            else if (result == SpyTapResult.NotThisOne)
            {
                Sfx.Play(Sound.Wiggle);
                StartCoroutine(Tween.Wiggle(item.Rect, 12f));
                if (notThisCooldown <= 0f)
                {
                    notThisCooldown = 6f;
                    Stage.Captions.Show(captions.spyNotThis);
                }
            }
        }

        public SpyItem CurrentTargetItem => round == null || round.Done ? null : items[round.CurrentTarget];

        private IEnumerator Found(SpyItem item)
        {
            item.Found = true;
            item.Press.Interactable = false;
            // Each find is the next note up a scale; the last one ends the tune a step higher still.
            Sfx.Play(Sound.Correct, Sfx.ScaleNote(round.Found - 1));
            if (round.Done)
            {
                StartCoroutine(Finale(round.Found + 1));
            }

            SetScore(round.Score);
            SetTimer($"{round.Found}/{round.Settings.Targets}");
            item.Image.material = null;
            FloatText(itemsLayer, "+10", item.Rect.anchoredPosition + new Vector2(0f, 80f), UiTheme.Instance.success);
            StartCoroutine(Confetti.Burst(itemsLayer, item.Rect.anchoredPosition, 10, 220f));
            ApplyLook();
            if (!saidClearing && round.Cleared >= 0.5f && !round.Done)
            {
                saidClearing = true;
                kid.SetEyes(Eyes.Open);
                Stage.Captions.Show(captions.spyClearing);
            }

            var from = item.Rect.anchoredPosition;
            var to = card.anchoredPosition;
            yield return Tween.Bump(item.Rect, 0.3f, 0.25f);
            yield return Tween.Run(0.45f, Ease.InOutSine, t =>
            {
                item.Rect.anchoredPosition = Vector2.Lerp(from, to, t) + Vector2.up * (Mathf.Sin(t * Mathf.PI) * 120f);
                item.Rect.localScale = Vector3.one * Mathf.Lerp(1f, 0.4f, t);
            });
            item.Rect.gameObject.SetActive(false);
            if (!round.Done)
            {
                ShowTarget();
            }
        }

        private static IEnumerator Finale(int note)
        {
            yield return Tween.Wait(0.18f);
            Sfx.Play(Sound.Correct, Sfx.ScaleNote(note));
            Sfx.Play(Sound.Sparkle, Sfx.ScaleNote(note));
        }

        private void ShowTarget()
        {
            var target = CurrentTargetItem;
            if (target == null)
            {
                return;
            }

            cardPicture.sprite = ArtLibrary.Get("Spy/" + target.Art);
            Sfx.Play(Sound.Pop);
            StartCoroutine(Tween.Pop(cardPicture.transform, 0.35f));
        }

        private void HintTarget()
        {
            var target = CurrentTargetItem;
            if (target != null)
            {
                StartCoroutine(Tween.Bump(target.Rect, 0.25f, 0.5f));
                StartCoroutine(Tween.Bump(card, 0.1f, 0.4f));
            }
        }

        private void ApplyLook()
        {
            if (blur != null)
            {
                var pixelScale = Stage.Canvas.scaleFactor;
                blur.SetFloat("_BlurPixels", round.Blur * BlurScale * pixelScale);
            }

            var glare = round.Glare;
            veil.color = new Color(1f, 1f, 0.96f, 0.34f * glare);
            windowGlare.color = new Color(1f, 0.98f, 0.86f, Mathf.Clamp01(0.95f * glare));
        }
    }
}
