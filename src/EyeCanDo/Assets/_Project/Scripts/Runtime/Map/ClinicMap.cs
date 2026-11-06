using System.Collections;
using System.Collections.Generic;
using EyeCanDo.App;
using EyeCanDo.Art;
using EyeCanDo.Characters;
using EyeCanDo.Content;
using EyeCanDo.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EyeCanDo.Map
{
    /// <summary>
    /// The clinic map: the stops of an eye visit along a winding path, in visit order. Special
    /// Drops is open; the other stops are greyed out with a "Soon" ribbon and only wiggle when
    /// tapped. Nothing is locked behind anything else (FR_StartLevel).
    /// </summary>
    public sealed class ClinicMap : MonoBehaviour
    {
        private static readonly Vector2[] StopPositions =
        {
            new Vector2(-760f, 150f), new Vector2(-380f, 120f), new Vector2(0f, 150f), new Vector2(380f, 120f), new Vector2(760f, 150f),
            new Vector2(570f, -220f), new Vector2(190f, -250f), new Vector2(-190f, -220f), new Vector2(-570f, -250f),
        };

        private const float StopSize = 190f;

        [SerializeField] private StopCatalog catalog;
        [SerializeField] private CaptionBank captions;

        private Stage stage;
        private readonly Dictionary<string, RectTransform> stops = new Dictionary<string, RectTransform>();
        private StopPanel panel;

        public StopPanel Panel => panel;

        public Stage Stage => stage;

        public CoinCounter Coins { get; private set; }

        public ChunkyButton DressUpButton { get; private set; }

        private void Awake()
        {
            if (catalog == null)
            {
                catalog = DefaultContent.CreateCatalog();
            }

            if (captions == null)
            {
                captions = DefaultContent.CreateCaptionBank();
            }

            stage = Stage.Create("ClinicMap", true);
            stage.Chrome.Restart.gameObject.SetActive(false);
            stage.Chrome.SkipAllowed = false;
            stage.Chrome.HomePressed += () => GameServices.GoTo(SceneNames.Saves);
            stage.Captions.YouLook = GameServices.Progress.look;
            stage.BuildOutdoors(-60f);
            BuildPath();
            BuildStops();
            BuildAvatar();
            Coins = CoinCounter.Create(stage.Overlay, GameServices.Progress.coins);
            var theme = UiTheme.Instance;
            DressUpButton = ChunkyButton.Create(stage.Overlay, "DressUp", "Dress Up", ArtLibrary.Get("Icons/shirt"), theme.paper, new Vector2(320f, 110f), 55f);
            Ui.Place((RectTransform)DressUpButton.transform, new Vector2(1f, 0f), new Vector2(-26f - 160f, 26f + 62f), new Vector2(320f, 110f));
            DressUpButton.Clicked += () => GameServices.GoToDressUp(SceneNames.ClinicMap);
            panel = StopPanel.Create(stage, captions);
        }

        private IEnumerator Start()
        {
            yield return null;
            if (GameServices.OpenStopOnMap == StopCatalog.EyeDropsId)
            {
                GameServices.OpenStopOnMap = null;
                OpenStop(StopCatalog.EyeDropsId);
                yield break;
            }

            stage.Captions.Show(captions.mapWelcome);
            if (stops.TryGetValue(StopCatalog.EyeDropsId, out var drops) && GameServices.Progress.eyeDropsStory == Core.Progress.StopStatus.NotStarted)
            {
                stage.Ghost.ShowTap(drops);
            }
        }

        private void BuildPath()
        {
            var layer = stage.Layer("Path");
            var sand = UiTheme.Hex("#F6E3B4");
            for (var i = 0; i < StopPositions.Length - 1; i++)
            {
                var from = StopPositions[i];
                var to = StopPositions[i + 1];
                var steps = Mathf.CeilToInt(Vector2.Distance(from, to) / 46f);
                for (var s = 0; s <= steps; s++)
                {
                    var dot = Ui.Image(layer, "PathDot", Shapes.Circle, sand);
                    Ui.Place(dot.rectTransform, new Vector2(0.5f, 0.5f), Vector2.Lerp(from, to, s / (float)steps), new Vector2(64f, 64f));
                }
            }
        }

        private void BuildStops()
        {
            var layer = stage.Layer("Stops");
            for (var i = 0; i < catalog.stops.Count && i < StopPositions.Length; i++)
            {
                var info = catalog.stops[i];
                stops[info.id] = BuildStop(layer, info, i, StopPositions[i]);
            }
        }

        private RectTransform BuildStop(RectTransform layer, StopInfo info, int index, Vector2 position)
        {
            var theme = UiTheme.Instance;
            var size = info.playable ? StopSize * 1.2f : StopSize;
            var face = Ui.Disc(layer, "Stop_" + info.id, info.playable ? theme.primary : UiTheme.Hex("#E3E8EF"));
            var root = (RectTransform)face.parent;
            Ui.Place(root, new Vector2(0.5f, 0.5f), position, new Vector2(size, size));

            var icon = Ui.Image(face, "Icon", ArtLibrary.Get(info.icon), info.playable ? Color.white : new Color(1f, 1f, 1f, 0.45f));
            icon.preserveAspect = true;
            Ui.Stretch(icon.rectTransform, size * 0.18f);

            var number = Ui.Disc(root, "Number", theme.paper);
            Ui.Place((RectTransform)number.parent, new Vector2(0f, 1f), new Vector2(18f, -18f), new Vector2(62f, 62f));
            var numberText = Ui.Text(number, (index + 1).ToString(), 36f, theme.ink);
            Ui.Stretch(numberText.rectTransform);

            var label = Ui.Panel(root, "Label", info.playable ? theme.paper : UiTheme.Hex("#F3F5F8"), 26f);
            var labelRoot = (RectTransform)label.parent;
            Ui.Place(labelRoot, new Vector2(0.5f, 0f), new Vector2(0f, -40f), new Vector2(info.playable ? 330f : 290f, 66f), new Vector2(0.5f, 1f));
            var text = Ui.Text(label, info.title, info.playable ? 40f : 32f, info.playable ? theme.ink : UiTheme.Darker(theme.muted, 0.25f));
            Ui.Stretch(text.rectTransform, 6f);

            var press = PressArea.AddTo(root.gameObject);
            if (info.playable)
            {
                root.gameObject.AddComponent<Bob>().Height = 10f;
                var glow = Ui.Image(layer, "Glow", Shapes.Glow, new Color(1f, 0.93f, 0.55f, 0.9f));
                Ui.Place(glow.rectTransform, new Vector2(0.5f, 0.5f), position, Vector2.one * size * 1.8f);
                glow.transform.SetSiblingIndex(root.GetSiblingIndex());
                var done = GameServices.Progress.eyeDropsSticker;
                if (done)
                {
                    var badge = Ui.Disc(root, "Done", theme.success);
                    Ui.Place((RectTransform)badge.parent, new Vector2(1f, 1f), new Vector2(-14f, -14f), new Vector2(70f, 70f));
                    var check = Ui.Image(badge, "Check", ArtLibrary.Get("Icons/check"), Color.white);
                    check.preserveAspect = true;
                    Ui.Stretch(check.rectTransform, 12f);
                }

                press.Tapped += () => OpenStop(info.id);
            }
            else
            {
                var ribbon = Ui.Panel(root, "Soon", theme.primary, 18f);
                var ribbonRoot = (RectTransform)ribbon.parent;
                Ui.Place(ribbonRoot, new Vector2(0.5f, 0f), new Vector2(0f, 26f), new Vector2(150f, 50f));
                ribbonRoot.localRotation = Quaternion.Euler(0f, 0f, 8f);
                var soon = Ui.Text(ribbon, "Soon", 30f, theme.ink);
                Ui.Stretch(soon.rectTransform, 2f);
                press.Tapped += () => ComingSoon(root);
            }

            return root;
        }

        private void BuildAvatar()
        {
            var layer = stage.Layer("Avatar");
            // Your character waits between Special Drops and the Dress Up button.
            var kid = CharacterRig.CreateKid(layer, GameServices.Progress.look, "wave", 0.36f);
            kid.Root.anchoredPosition = new Vector2(860f, -400f);
            kid.SetMouth(Mouth.Grin);
            var doctor = CharacterRig.CreateDoctor(layer, "wave", 0.46f);
            doctor.Root.anchoredPosition = new Vector2(-860f, -540f);
        }

        private void ComingSoon(RectTransform stop)
        {
            Sfx.Play(Sound.Wiggle);
            StartCoroutine(Tween.Wiggle(stop));
            stage.Captions.Show(captions.comingSoon);
        }

        /// <summary>Opens a stop's panel (also the PlayMode test hook).</summary>
        public void OpenStop(string id)
        {
            if (id != StopCatalog.EyeDropsId)
            {
                if (stops.TryGetValue(id, out var stop))
                {
                    ComingSoon(stop);
                }

                return;
            }

            stage.Ghost.Stop();
            panel.Open();
        }

        public RectTransform StopRect(string id)
        {
            return stops.TryGetValue(id, out var rect) ? rect : null;
        }
    }
}
