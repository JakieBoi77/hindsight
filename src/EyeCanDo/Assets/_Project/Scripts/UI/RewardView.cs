using System.Collections;
using Hindsight.EyeCanDo.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Hindsight.EyeCanDo.UI
{
    /// <summary>
    /// End-of-level card: the sticker pops up, the coins it earned fly up into the purse, then Home and
    /// Play again.
    /// </summary>
    public sealed class RewardView : MonoBehaviour
    {
        [SerializeField] GameObject panel;
        [SerializeField] RectTransform sticker;
        [SerializeField] Button stickerButton;
        [SerializeField] TMP_Text titleLabel;
        [SerializeField] GameObject coinChip;
        [SerializeField] TMP_Text coinLabel;
        [SerializeField] GameObject endButtons;
        [SerializeField] Button homeButton;
        [SerializeField] Button replayButton;
        [SerializeField] CoinCounter purse;
        [SerializeField] ParticleSystem confetti;
        [Tooltip("A coin picture, copied for each coin that flies to the purse.")]
        [SerializeField] RectTransform flyingCoin;
        [SerializeField] RectTransform purseTarget;

        void Awake()
        {
            if (stickerButton != null) stickerButton.interactable = false;
            if (flyingCoin != null) flyingCoin.gameObject.SetActive(false);
            homeButton.onClick.AddListener(GameFlow.ReturnToLevelSelect);
            replayButton.onClick.AddListener(GameFlow.ReplayLevel);
            panel.SetActive(false);
        }

        /// <summary>Pops the sticker in and lets it sit for a moment.</summary>
        public IEnumerator ShowSticker(string title)
        {
            panel.SetActive(true);
            titleLabel.text = title;
            coinChip.SetActive(false);
            endButtons.SetActive(false);
            sticker.gameObject.SetActive(true);
            if (purse != null)
            {
                // Keep showing the coins from before the level until they fly in.
                purse.gameObject.SetActive(true);
                purse.Hold();
            }
            if (confetti != null && !GameServices.Settings.reduceMotion) confetti.Play();
            yield return Pop(sticker, 0.6f);
            Sfx.Play(SfxId.Sparkle);
            for (var t = 0f; t < 1.2f; t += Time.deltaTime) yield return null;
        }

        /// <summary>Shows "+N", then the coins fly from it up into the purse, which counts up as each lands.</summary>
        public IEnumerator ShowCoins(int coins)
        {
            coinChip.SetActive(coins > 0);
            coinLabel.text = "+" + coins;
            var total = GameServices.Progress.coins;
            if (coins > 0)
            {
                Sfx.Play(SfxId.Coin);
                yield return Pop((RectTransform)coinChip.transform, 0.4f);
                if (purse != null && flyingCoin != null)
                {
                    var from = purse.Shown;
                    var count = Mathf.Clamp(coins / 4, 4, 10);
                    var landed = 0;
                    for (var i = 0; i < count; i++)
                    {
                        StartCoroutine(Fly(() =>
                        {
                            landed++;
                            purse.Bump(Mathf.RoundToInt(Mathf.Lerp(from, total, (float)landed / count)));
                            if (landed % 2 == 1) Sfx.Play(SfxId.Coin);
                        }));
                        for (var t = 0f; t < 0.09f; t += Time.deltaTime) yield return null;
                    }
                    while (landed < count) yield return null;
                }
            }
            for (var t = 0f; t < 0.4f; t += Time.deltaTime) yield return null;
            if (purse != null) purse.Release();
        }

        /// <summary>One coin: from the "+N" chip in an arc up to the purse, shrinking as it goes.</summary>
        IEnumerator Fly(System.Action landed)
        {
            var coin = Instantiate(flyingCoin, flyingCoin.parent);
            coin.gameObject.SetActive(true);
            var start = coinChip.transform.position + (Vector3)(Random.insideUnitCircle * 40f);
            var end = (purseTarget != null ? purseTarget : purse.transform).position;
            var bend = start + (end - start) * 0.5f + new Vector3(-Mathf.Abs(end.x - start.x) * 0.25f, 120f, 0f);
            const float seconds = 0.65f;
            for (var t = 0f; t < seconds; t += Time.deltaTime)
            {
                var k = Mathf.SmoothStep(0f, 1f, t / seconds);
                coin.position = Vector3.Lerp(Vector3.Lerp(start, bend, k), Vector3.Lerp(bend, end, k), k);
                coin.localScale = Vector3.one * Mathf.Lerp(1.2f, 0.7f, k);
                yield return null;
            }
            Destroy(coin.gameObject);
            landed();
        }

        /// <summary>Shows Home and Play again. Stays up.</summary>
        public IEnumerator ShowEndButtons()
        {
            endButtons.SetActive(true);
            yield return Pop((RectTransform)endButtons.transform, 0.4f);
        }

        static IEnumerator Pop(RectTransform target, float seconds)
        {
            for (var t = 0f; t < seconds; t += Time.deltaTime)
            {
                var k = t / seconds;
                target.localScale = Vector3.one * (k * (1f + 0.25f * Mathf.Sin(k * Mathf.PI) * (1f - k)));
                yield return null;
            }
            target.localScale = Vector3.one;
        }

#if UNITY_EDITOR
        public void EditorInit(GameObject panelObject, RectTransform stickerRect, Button stickerTap, TMP_Text title, GameObject chip,
            TMP_Text chipLabel, GameObject endGroup, Button home, Button replay, CoinCounter coins, ParticleSystem particles,
            RectTransform coinPicture, RectTransform purseIcon)
        {
            panel = panelObject;
            sticker = stickerRect;
            stickerButton = stickerTap;
            titleLabel = title;
            coinChip = chip;
            coinLabel = chipLabel;
            endButtons = endGroup;
            homeButton = home;
            replayButton = replay;
            purse = coins;
            confetti = particles;
            flyingCoin = coinPicture;
            purseTarget = purseIcon;
        }
#endif
    }
}
