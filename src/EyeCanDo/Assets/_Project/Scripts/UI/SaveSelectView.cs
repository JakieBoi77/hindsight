using System;
using Hindsight.Ophthalmology.Core;
using Hindsight.Ophthalmology.Core.Progress;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Hindsight.Ophthalmology.UI
{
    /// <summary>
    /// "Who's playing?": one card per save file (adapted from the 2D PoC). A used save shows its
    /// kid, coins and a sticker tick; an empty one says "New game". Delete asks first and names
    /// the coins that will go; "Keep it" changes nothing.
    /// </summary>
    public sealed class SaveSelectView : MonoBehaviour
    {
        [SerializeField] Button[] cards = new Button[SaveSlots.Count];
        [SerializeField] TMP_Text[] titles = new TMP_Text[SaveSlots.Count];
        [SerializeField] GameObject[] coinRows = new GameObject[SaveSlots.Count];
        [SerializeField] TMP_Text[] coinLabels = new TMP_Text[SaveSlots.Count];
        [SerializeField] GameObject[] stickers = new GameObject[SaveSlots.Count];
        [SerializeField] GameObject[] newBadges = new GameObject[SaveSlots.Count];
        [SerializeField] Button[] deleteButtons = new Button[SaveSlots.Count];
        [SerializeField] PortraitStudio studio;
        [SerializeField] ConfirmDialog confirm;

        /// <summary>Raised with the slot the child tapped.</summary>
        public event Action<int> Picked;

        public Button[] Cards => cards;

        void Awake()
        {
            for (var i = 0; i < SaveSlots.Count; i++)
            {
                var slot = i;
                cards[i].onClick.AddListener(() => Picked?.Invoke(slot));
                deleteButtons[i].onClick.AddListener(() => AskDelete(slot));
            }
        }

        void OnEnable() => Refresh();

        public void Refresh()
        {
            if (GameServices.Saves == null) return;
            for (var i = 0; i < SaveSlots.Count; i++)
            {
                var progress = GameServices.Saves.Peek(i);
                var used = progress != null;
                titles[i].text = used ? $"Save {i + 1}" : "New game";
                coinRows[i].SetActive(used);
                if (used) coinLabels[i].text = progress.coins.ToString();
                stickers[i].SetActive(used && progress.stickers.Count > 0);
                newBadges[i].SetActive(!used);
                deleteButtons[i].gameObject.SetActive(used);
                if (studio != null) studio.Show(i, used ? progress.look : null);
            }
        }

        void AskDelete(int slot)
        {
            var progress = GameServices.Saves.Peek(slot);
            if (progress == null) return;
            var coins = progress.coins == 1 ? "1 coin" : $"{progress.coins} coins";
            confirm.Ask($"Delete Save {slot + 1}?", $"Its kid, {coins}, stickers and clothes will be gone.", "Delete", "Keep it", () =>
            {
                GameServices.DeleteSave(slot);
                Refresh();
            });
        }

#if UNITY_EDITOR
        public void EditorInit(Button[] cardButtons, TMP_Text[] cardTitles, GameObject[] coinRow, TMP_Text[] coinText, GameObject[] stickerTicks,
            GameObject[] newGame, Button[] delete, PortraitStudio portraits, ConfirmDialog dialog)
        {
            cards = cardButtons;
            titles = cardTitles;
            coinRows = coinRow;
            coinLabels = coinText;
            stickers = stickerTicks;
            newBadges = newGame;
            deleteButtons = delete;
            studio = portraits;
            confirm = dialog;
        }
#endif
    }
}
