using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EyeCanDo.App;
using EyeCanDo.Art;
using EyeCanDo.Characters;
using EyeCanDo.Content;
using EyeCanDo.Core.Animation;
using EyeCanDo.Core.Avatar;
using EyeCanDo.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EyeCanDo.DressUp
{
    /// <summary>
    /// Make your own character, and the little shop. Faces, hair and colours are free to change
    /// any time; shirt pictures, hats, glasses and things to hold cost coins earned in the story
    /// and games. Tapping a shop item tries it on; the Buy button buys it. Leaving without
    /// buying puts the old item back.
    /// </summary>
    public sealed class DressUpScreen : MonoBehaviour
    {
        private const float TileSize = 104f;
        private const float TileGap = 14f;
        private static readonly Vector2 PreviewFeet = new Vector2(-650f, -420f);
        private const float PreviewScale = 0.8f;
        private static readonly Vector2 PanelCentre = new Vector2(300f, -105f);
        private static readonly Vector2 PanelSize = new Vector2(1240f, 830f);

        public sealed class Tab
        {
            public string Name;
            public string Icon;
            public (string Label, Slot Slot)[] Sections;
        }

        public static readonly Tab[] Tabs =
        {
            new Tab { Name = "Skin", Icon = "Icons/palette", Sections = new[] { ("Skin", Slot.Skin), ("Ears", Slot.Ears) } },
            new Tab { Name = "Eyes", Icon = "Icons/eye", Sections = new[] { ("Eyes", Slot.EyeStyle), ("Eye colour", Slot.EyeColor), ("Eyebrows", Slot.Brows) } },
            new Tab { Name = "Face", Icon = "Icons/smile", Sections = new[] { ("Nose", Slot.Nose), ("Mouth", Slot.Mouth) } },
            new Tab { Name = "Hair", Icon = "Icons/hair", Sections = new[] { ("Hair", Slot.HairStyle), ("Hair colour", Slot.HairColor) } },
            new Tab { Name = "Clothes", Icon = "Icons/shirt", Sections = new[] { ("Shirt", Slot.ShirtColor), ("Shirt picture", Slot.Emblem), ("Shorts", Slot.ShortsColor), ("Shoes", Slot.ShoesColor) } },
            new Tab { Name = "Hats", Icon = "Icons/cap", Sections = new[] { ("Hats", Slot.Hat) } },
            new Tab { Name = "Glasses", Icon = "Icons/glasses", Sections = new[] { ("Glasses", Slot.Glasses) } },
            new Tab { Name = "Hold", Icon = "Icons/hold", Sections = new[] { ("Things to hold", Slot.Held) } },
        };

        [SerializeField] private CaptionBank captions;

        private Stage stage;
        private CharacterRig preview;
        private CoinCounter coins;
        private RectTransform panelFace;
        private RectTransform content;
        private ScrollRect scroll;
        private readonly List<ChunkyButton> tabButtons = new List<ChunkyButton>();
        private readonly Dictionary<AvatarItem, RectTransform> tiles = new Dictionary<AvatarItem, RectTransform>();
        private RectTransform buyBar;
        private ChunkyButton buyButton;
        private AvatarLook draft;
        private AvatarItem pending;
        private int tabIndex;

        public ChunkyButton BuyButton => buyButton;

        public AvatarItem Pending => pending;

        public int TabIndex => tabIndex;

        public CharacterRig Preview => preview;

        private Core.Progress.PlayerProgress Progress => GameServices.Progress;

        private void Awake()
        {
            if (captions == null)
            {
                captions = DefaultContent.CreateCaptionBank();
            }

            stage = Stage.Create("DressUp", true);
            stage.Chrome.Restart.gameObject.SetActive(false);
            stage.Chrome.SkipAllowed = false;
            stage.Chrome.HomePressed += Leave;
            stage.Captions.YouLook = Progress.look;
            stage.BuildOutdoors(-300f);
            draft = Progress.look.Copy();

            var people = stage.Layer("Preview");
            var spot = Ui.Image(people, "Spot", Shapes.Circle, new Color(0f, 0f, 0f, 0.12f));
            Ui.Place(spot.rectTransform, new Vector2(0.5f, 0.5f), PreviewFeet + new Vector2(0f, 40f * PreviewScale), new Vector2(340f, 70f));
            preview = CharacterRig.CreateKid(people, draft, "stand", PreviewScale);
            preview.Root.anchoredPosition = PreviewFeet;
            preview.SetMouth(Mouth.Grin);

            coins = CoinCounter.Create(stage.Overlay, Progress.coins);
            BuildPanel();
            BuildBuyBar();
            ShowTab(0);
        }

        private IEnumerator Start()
        {
            yield return null;
            stage.Captions.Show(captions.dressUpHello);
        }

        private void BuildPanel()
        {
            var theme = UiTheme.Instance;
            panelFace = Ui.Panel(stage.Layer("Panel"), "Wardrobe", theme.background, 48f);
            Ui.Place((RectTransform)panelFace.parent, new Vector2(0.5f, 0.5f), PanelCentre, PanelSize);
            for (var i = 0; i < Tabs.Length; i++)
            {
                var index = i;
                var tab = ChunkyButton.Create(panelFace, "Tab_" + Tabs[i].Name, null, ArtLibrary.Get(Tabs[i].Icon), theme.paper, new Vector2(132f, 112f), 30f);
                Ui.Place((RectTransform)tab.transform, new Vector2(0f, 1f), new Vector2(40f + 66f + i * 146f, -30f - 56f), new Vector2(132f, 112f));
                tab.Clicked += () => ShowTab(index);
                tabButtons.Add(tab);
            }

            // The tab's choices scroll up and down when they do not all fit.
            var viewport = Ui.Rect(panelFace, "Viewport");
            viewport.anchorMin = Vector2.zero;
            viewport.anchorMax = Vector2.one;
            viewport.offsetMin = new Vector2(40f, 24f);
            viewport.offsetMax = new Vector2(-40f, -166f);
            var hit = viewport.gameObject.AddComponent<Image>();
            hit.color = Color.clear;
            viewport.gameObject.AddComponent<RectMask2D>();
            content = Ui.Rect(viewport, "Content");
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.pivot = new Vector2(0.5f, 1f);
            content.offsetMin = Vector2.zero;
            content.offsetMax = Vector2.zero;
            scroll = viewport.gameObject.AddComponent<ScrollRect>();
            scroll.viewport = viewport;
            scroll.content = content;
            scroll.horizontal = false;
            scroll.movementType = ScrollRect.MovementType.Elastic;
            scroll.scrollSensitivity = 40f;
        }

        private void BuildBuyBar()
        {
            var theme = UiTheme.Instance;
            buyBar = Ui.Rect(stage.Layer("Buy"), "BuyBar");
            Ui.Place(buyBar, new Vector2(0.5f, 0.5f), new Vector2(PreviewFeet.x, -480f), new Vector2(400f, 100f));
            buyButton = ChunkyButton.Create(buyBar, "Buy", "Buy 00", ArtLibrary.Get("Icons/coin"), theme.success, new Vector2(380f, 96f), 48f);
            Ui.Place((RectTransform)buyButton.transform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(380f, 96f));
            buyButton.Clicked += Buy;
            buyBar.gameObject.SetActive(false);
        }

        /// <summary>Opens a tab (also the PlayMode test hook).</summary>
        public void ShowTab(int index)
        {
            tabIndex = Mathf.Clamp(index, 0, Tabs.Length - 1);
            var theme = UiTheme.Instance;
            for (var i = 0; i < tabButtons.Count; i++)
            {
                tabButtons[i].SetColor(i == tabIndex ? theme.primary : theme.paper);
            }

            for (var i = content.childCount - 1; i >= 0; i--)
            {
                Destroy(content.GetChild(i).gameObject);
            }

            tiles.Clear();
            var y = 0f;
            foreach (var (label, slot) in Tabs[tabIndex].Sections)
            {
                var title = Ui.Text(content, label, 38f, theme.ink, TextAlignmentOptions.MidlineLeft);
                Ui.Place(title.rectTransform, new Vector2(0f, 1f), new Vector2(0f, -y), new Vector2(600f, 48f), new Vector2(0f, 1f));
                y += 54f;
                var items = AvatarCatalog.For(slot).ToList();
                var perRow = Mathf.FloorToInt((PanelSize.x - 80f + TileGap) / (TileSize + TileGap));
                for (var i = 0; i < items.Count; i++)
                {
                    var tile = Tile(items[i]);
                    var row = i / perRow;
                    var column = i % perRow;
                    Ui.Place(tile, new Vector2(0f, 1f), new Vector2(column * (TileSize + TileGap), -y - row * (TileSize + TileGap)), new Vector2(TileSize, TileSize), new Vector2(0f, 1f));
                    tiles[items[i]] = tile;
                }

                y += Mathf.CeilToInt(items.Count / (float)perRow) * (TileSize + TileGap) + 12f;
            }

            content.sizeDelta = new Vector2(0f, y);
            content.anchoredPosition = Vector2.zero;
            RefreshTiles();
        }

        private RectTransform Tile(AvatarItem item)
        {
            var theme = UiTheme.Instance;
            var button = ChunkyButton.Create(content, "Tile_" + item.Key, null, null, theme.paper, new Vector2(TileSize, TileSize), 24f);
            var rect = (RectTransform)button.transform;
            var face = (RectTransform)rect.Find("Face");
            var mask = Ui.Image(face, "Mask", Shapes.RoundedRect, Color.white);
            mask.type = Image.Type.Sliced;
            mask.pixelsPerUnitMultiplier = Shapes.SliceRadius / 18f;
            Ui.Stretch(mask.rectTransform, 4f);
            mask.gameObject.AddComponent<Mask>().showMaskGraphic = false;
            FillTile(mask.rectTransform, item);

            if (!item.IsFree)
            {
                var badge = Ui.Panel(rect, "Price", theme.primary, 18f);
                var badgeRoot = (RectTransform)badge.parent;
                Ui.Place(badgeRoot, new Vector2(0.5f, 0f), new Vector2(0f, 4f), new Vector2(92f, 38f));
                var coin = Ui.Art(badge, "Coin", "Icons/coin", new Vector2(28f, 28f));
                Ui.Place(coin.rectTransform, new Vector2(0f, 0.5f), new Vector2(20f, 0f), new Vector2(28f, 28f));
                var price = Ui.Text(badge, item.Price.ToString(), 26f, theme.ink);
                Ui.Place(price.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(14f, 0f), new Vector2(56f, 34f));
                price.enableAutoSizing = false;
            }

            var check = Ui.Disc(rect, "Selected", theme.success);
            Ui.Place((RectTransform)check.parent, new Vector2(1f, 1f), new Vector2(-4f, -4f), new Vector2(40f, 40f));
            var tick = Ui.Art(check, "Tick", "Icons/check", new Vector2(26f, 26f));
            tick.color = Color.white;
            button.Clicked += () => Choose(item);
            return rect;
        }

        private void FillTile(RectTransform area, AvatarItem item)
        {
            if (item.IsColor)
            {
                var swatch = Ui.Disc(area, "Swatch", UiTheme.Hex(item.Id));
                Ui.Place((RectTransform)swatch.parent, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(70f, 70f));
                return;
            }

            if (item.Id == AvatarCatalog.None)
            {
                var none = Ui.Art(area, "None", "Icons/close", new Vector2(54f, 54f));
                none.color = new Color(1f, 1f, 1f, 0.35f);
                return;
            }

            switch (item.Slot)
            {
                case Slot.Emblem:
                    Ui.Art(area, "Emblem", "Avatar/emblem_" + item.Id, new Vector2(80f, 80f));
                    return;
                case Slot.Held:
                    Ui.Art(area, "Held", "Avatar/held_" + item.Id, new Vector2(66f, 96f));
                    return;
            }

            // Face, hair, hats and glasses: a small head wearing just this choice.
            var look = draft.Copy();
            look.Set(item.Slot, item.Id);
            if (item.Slot != Slot.Hat)
            {
                look.hat = AvatarCatalog.None;
            }

            var zoomed = item.Slot == Slot.EyeStyle || item.Slot == Slot.Brows || item.Slot == Slot.Nose || item.Slot == Slot.Mouth || item.Slot == Slot.Glasses;
            var height = zoomed ? 230f : item.Slot == Slot.Hat ? 124f : 132f;
            var head = AvatarHead.Portrait(area, look, height);
            var rect = (RectTransform)head.transform;
            // Centre on the eyes when zoomed in, otherwise on the whole head.
            rect.anchoredPosition = zoomed ? new Vector2(0f, height * (AvatarHead.EyeCentres[0].y / AvatarHead.Canvas.y - 0.5f) + (item.Slot == Slot.Mouth || item.Slot == Slot.Nose ? -18f : 0f))
                : new Vector2(0f, item.Slot == Slot.Hat ? 2f : -12f);
        }

        /// <summary>Picks an item (also the PlayMode test hook). Owned items are worn; shop items are tried on.</summary>
        public void Choose(AvatarItem item)
        {
            if (item == null)
            {
                return;
            }

            if (pending != null && pending.Slot != item.Slot)
            {
                draft.Set(pending.Slot, Progress.look.Get(pending.Slot));
            }

            draft.Set(item.Slot, item.Id);
            if (Progress.Owns(item))
            {
                pending = null;
                Progress.Equip(item.Slot, item.Id);
                GameServices.Save();
            }
            else
            {
                pending = item;
                stage.Captions.Show(Progress.CanAfford(item) ? captions.dressUpTryOn : captions.dressUpNeedCoins);
            }

            Sfx.Play(Sound.Pop);
            preview.SetLook(draft);
            StartCoroutine(preview.Bounce(18f));
            RefreshTiles();
            RefreshBuyBar();
            if (item.IsColor)
            {
                // Colour changes show on the head tiles too, so rebuild them.
                ShowTab(tabIndex);
            }
        }

        /// <summary>Buys the item being tried on (also the PlayMode test hook).</summary>
        public void Buy()
        {
            if (pending == null)
            {
                return;
            }

            if (!Progress.TryBuy(pending))
            {
                Sfx.Play(Sound.Locked);
                stage.Captions.Show(captions.dressUpNeedCoins);
                StartCoroutine(Tween.Wiggle(buyBar));
                return;
            }

            Progress.Equip(pending.Slot, pending.Id);
            GameServices.Save();
            coins.Set(Progress.coins);
            Sfx.Play(Sound.Purchase);
            StartCoroutine(Confetti.Burst((RectTransform)preview.Root.parent, PreviewFeet + new Vector2(0f, 360f), 22, 380f));
            StartCoroutine(preview.Wave(1.2f));
            stage.Captions.Show(captions.dressUpBought);
            pending = null;
            ShowTab(tabIndex);
            RefreshBuyBar();
        }

        private void RefreshBuyBar()
        {
            buyBar.gameObject.SetActive(pending != null);
            if (pending == null)
            {
                return;
            }

            var theme = UiTheme.Instance;
            var affordable = Progress.CanAfford(pending);
            buyButton.Label.text = affordable ? $"Buy {pending.Price}" : $"Need {pending.Price}";
            buyButton.SetColor(affordable ? theme.success : theme.muted);
            StartCoroutine(Tween.Pop(buyBar, 0.3f));
        }

        private void RefreshTiles()
        {
            foreach (var pair in tiles)
            {
                var selected = draft.Get(pair.Key.Slot) == pair.Key.Id;
                pair.Value.Find("Selected").gameObject.SetActive(selected);
                var price = pair.Value.Find("Price");
                if (price != null)
                {
                    price.gameObject.SetActive(!Progress.Owns(pair.Key));
                }
            }
        }

        public RectTransform TileFor(AvatarItem item)
        {
            return tiles.TryGetValue(item, out var tile) ? tile : null;
        }

        private void Leave()
        {
            GameServices.GoTo(GameServices.DressUpReturn ?? SceneNames.ClinicMap);
        }
    }
}
