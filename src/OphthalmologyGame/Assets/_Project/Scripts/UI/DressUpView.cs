using System;
using System.Collections.Generic;
using Hindsight.Ophthalmology.Core;
using Hindsight.Ophthalmology.Core.Avatar;
using Hindsight.Ophthalmology.SceneObjects;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Hindsight.Ophthalmology.UI
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
                swatch.gameObject.SetActive(item.IsColor);
                label.gameObject.SetActive(!item.IsColor);
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
            TMP_Text noteText, Button back, CoinCounter coins, KidActor actor)
        {
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
