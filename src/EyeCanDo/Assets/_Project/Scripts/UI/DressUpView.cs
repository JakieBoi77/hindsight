using System;
using System.Collections.Generic;
using Hindsight.EyeCanDo.Core;
using Hindsight.EyeCanDo.Core.Avatar;
using Hindsight.EyeCanDo.SceneObjects;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Hindsight.EyeCanDo.UI
{
    /// <summary>
    /// Dress Up (adapted from the 2D PoC): tabs for each part of the kid, a grid of colours or
    /// items, and the kid in the room trying each one on. Free and owned items are worn at once;
    /// a shop item is tried on and kept with Buy. Leaving puts back anything not bought.
    /// </summary>
    public sealed class DressUpView : MonoBehaviour
    {
        [SerializeField] Button[] tabs = new Button[0];
        [Tooltip("Slot shown by each tab, same order as Tabs.")]
        [SerializeField] Slot[] tabSlots = new Slot[0];
        [SerializeField] RectTransform grid;
        [SerializeField] Button itemTemplate;
        [SerializeField] Button buyButton;
        [SerializeField] TMP_Text buyPrice;
        [SerializeField] TMP_Text note;
        [SerializeField] Button backButton;
        [SerializeField] CoinCounter purse;
        [SerializeField] KidActor kid;
        [Tooltip("The face atlases: eye, mouth and brow cards show their drawing.")]
        [SerializeField] Texture eyesAtlas;
        [SerializeField] Texture mouthAtlas;
        [SerializeField] Texture browsAtlas;
        [SerializeField] Color tabOn = new(1f, 0.78f, 0.27f);
        [SerializeField] Color tabOff = Color.white;

        readonly List<Button> spawned = new();
        AvatarLook preview;
        Slot slot;
        AvatarItem pending;

        public event Action Closed;

        void Awake()
        {
            itemTemplate.gameObject.SetActive(false);
            for (var i = 0; i < tabs.Length; i++)
            {
                var index = i;
                tabs[i].onClick.AddListener(() => ShowTab(index));
            }
            buyButton.onClick.AddListener(Buy);
            backButton.onClick.AddListener(Close);
        }

        void OnEnable()
        {
            preview = GameServices.Progress.look.Copy();
            pending = null;
            ShowTab(0);
        }

        void ShowTab(int index)
        {
            slot = tabSlots[Mathf.Clamp(index, 0, tabSlots.Length - 1)];
            for (var i = 0; i < tabs.Length; i++) tabs[i].transform.Find("Face").GetComponent<Image>().color = i == index ? tabOn : tabOff;
            Rebuild();
            var scroll = grid.GetComponentInParent<ScrollRect>();
            if (scroll != null) scroll.verticalNormalizedPosition = 1f;
        }

        void Rebuild()
        {
            foreach (var b in spawned) Destroy(b.gameObject);
            spawned.Clear();
            var progress = GameServices.Progress;
            foreach (var item in AvatarCatalog.For(slot))
            {
                var card = Instantiate(itemTemplate, grid);
                card.gameObject.SetActive(true);
                card.name = "Item_" + item.Id;
                var swatch = card.transform.Find("Face/Swatch").GetComponent<Image>();
                var label = card.transform.Find("Face/Label").GetComponent<TMP_Text>();
                var picture = card.transform.Find("Face/Picture").GetComponent<RawImage>();
                var isFace = AvatarCatalog.IsFace(item.Slot) && item.Id != AvatarCatalog.None;
                swatch.gameObject.SetActive(item.IsColor);
                label.gameObject.SetActive(!item.IsColor && !isFace);
                picture.gameObject.SetActive(isFace);
                if (isFace) ShowFacePicture(picture, item);
                if (item.IsColor && ColorUtility.TryParseHtmlString(item.Id, out var c)) swatch.color = c;
                label.text = item.Label;
                var owned = progress.Owns(item);
                var price = card.transform.Find("Price").gameObject;
                price.SetActive(!owned);
                price.GetComponentInChildren<TMP_Text>().text = item.Price.ToString();
                card.transform.Find("Selected").gameObject.SetActive(preview.Get(slot) == item.Id);
                var picked = item;
                card.onClick.AddListener(() => TryOn(picked));
                spawned.Add(card);
            }
            UpdateBuy();
        }

        /// <summary>Eyes, mouths and face accessories are shown as their drawing from the face atlases.</summary>
        void ShowFacePicture(RawImage picture, AvatarItem item)
        {
            var rt = picture.rectTransform;
            switch (item.Slot)
            {
                case Slot.Eyes:
                    picture.texture = eyesAtlas;
                    picture.uvRect = FaceArt.Eyes(FaceArt.EyeIndex(item.Id), false);
                    rt.anchorMin = new Vector2(0.04f, 0.27f); // eye cells are twice as wide as tall
                    rt.anchorMax = new Vector2(0.96f, 0.73f);
                    break;
                case Slot.Mouth:
                    picture.texture = mouthAtlas;
                    picture.uvRect = FaceArt.Mouth(FaceArt.MouthIndex(item.Id), 0);
                    rt.anchorMin = new Vector2(0.1f, 0.1f);
                    rt.anchorMax = new Vector2(0.9f, 0.9f);
                    break;
                default:
                    picture.texture = browsAtlas;
                    picture.uvRect = FaceArt.Brows(FaceArt.BrowIndex(item.Id), Mood.Neutral);
                    rt.anchorMin = new Vector2(0.02f, 0.34f); // brow cells are three times as wide as tall
                    rt.anchorMax = new Vector2(0.98f, 0.66f);
                    break;
            }
            rt.offsetMin = rt.offsetMax = Vector2.zero;
        }

        void TryOn(AvatarItem item)
        {
            preview.Set(item.Slot, item.Id);
            kid.Apply(preview);
            var progress = GameServices.Progress;
            if (progress.Owns(item))
            {
                pending = null;
                progress.Equip(item.Slot, item.Id);
                GameServices.Save();
            }
            else
            {
                pending = item;
            }
            Rebuild();
        }

        void UpdateBuy()
        {
            var showBuy = pending != null;
            buyButton.gameObject.SetActive(showBuy);
            note.text = "";
            if (!showBuy) return;
            buyPrice.text = $"Buy  {pending.Price}";
            var canAfford = GameServices.Progress.CanAfford(pending);
            buyButton.interactable = canAfford;
            if (!canAfford) note.text = $"{pending.Price - GameServices.Progress.coins} more coins to buy";
        }

        void Buy()
        {
            var progress = GameServices.Progress;
            if (pending == null || !progress.TryBuy(pending)) return;
            progress.Equip(pending.Slot, pending.Id);
            pending = null;
            Sfx.Play(SfxId.Purchase);
            GameServices.Save();
            if (purse != null) purse.Refresh();
            Rebuild();
        }

        void Close()
        {
            // Anything tried on but not bought comes off again.
            kid.Apply(GameServices.Progress.look);
            Closed?.Invoke();
        }

#if UNITY_EDITOR
        public void EditorInit(Button[] tabButtons, Slot[] slots, RectTransform gridRoot, Button template, Button buy, TMP_Text price,
            TMP_Text noteText, Button back, CoinCounter coins, KidActor actor, Texture eyes, Texture mouths, Texture brows)
        {
            browsAtlas = brows;
            eyesAtlas = eyes;
            mouthAtlas = mouths;
            tabs = tabButtons;
            tabSlots = slots;
            grid = gridRoot;
            itemTemplate = template;
            buyButton = buy;
            buyPrice = price;
            note = noteText;
            backButton = back;
            purse = coins;
            kid = actor;
        }
#endif
    }
}
