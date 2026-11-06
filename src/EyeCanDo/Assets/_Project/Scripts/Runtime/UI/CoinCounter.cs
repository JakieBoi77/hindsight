using System.Collections;
using EyeCanDo.App;
using EyeCanDo.Art;
using EyeCanDo.Core.Animation;
using TMPro;
using UnityEngine;

namespace EyeCanDo.UI
{
    /// <summary>
    /// The coin purse shown in the top-right corner: a coin and the number of coins. New coins
    /// fly into it one by one and the number counts up, with one jingle of coins as they land.
    /// </summary>
    public sealed class CoinCounter : MonoBehaviour
    {
        public const float Width = 250f;
        public const float Height = 100f;

        private TextMeshProUGUI label;
        private RectTransform coin;
        private int shown;
        private Coroutine pulse;

        public int Shown => shown;

        /// <summary>Puts the purse in the top-right of the overlay, left of the Skip button's spot.</summary>
        public static CoinCounter Create(RectTransform overlay, int coins, float rightMargin = 26f)
        {
            var theme = UiTheme.Instance;
            var face = Ui.Panel(overlay, "Coins", theme.paper, 50f);
            var root = (RectTransform)face.parent;
            Ui.Place(root, new Vector2(1f, 1f), new Vector2(-rightMargin - Width / 2f, -26f - Height / 2f), new Vector2(Width, Height));
            var counter = root.gameObject.AddComponent<CoinCounter>();
            var icon = Ui.Art(face, "Coin", "Icons/coin", new Vector2(76f, 76f));
            Ui.Place(icon.rectTransform, new Vector2(0f, 0.5f), new Vector2(52f, 0f), new Vector2(76f, 76f));
            counter.coin = icon.rectTransform;
            counter.label = Ui.Text(face, coins.ToString(), 56f, theme.ink);
            Ui.Place(counter.label.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(38f, 0f), new Vector2(150f, 84f));
            counter.label.enableAutoSizing = false;
            counter.shown = coins;
            return counter;
        }

        public void Set(int coins)
        {
            shown = coins;
            label.text = coins.ToString();
        }

        /// <summary>Coins fly from a world position into the purse, and the number counts up.</summary>
        public IEnumerator Earn(Vector3 fromWorld, int amount, RectTransform layer)
        {
            if (amount <= 0)
            {
                yield break;
            }

            var flights = Mathf.Clamp(amount / 3, 3, 12);
            var perFlight = amount / (float)flights;
            var target = coin.position;
            var earned = 0f;
            var start = shown;
            var landed = false;
            for (var i = 0; i < flights; i++)
            {
                StartCoroutine(Fly(layer, fromWorld, target, i * 0.07f, () =>
                {
                    earned += perFlight;
                    Set(start + Mathf.Min(amount, Mathf.RoundToInt(earned)));
                    if (!landed)
                    {
                        landed = true;
                        Sfx.Play(Sound.Coin);
                    }

                    Pulse();
                }));
            }

            yield return Tween.Wait(flights * 0.07f + 0.7f);
            Set(start + amount);
        }

        /// <summary>A little bump that always settles back to normal size, even when bumps overlap.</summary>
        private void Pulse()
        {
            if (pulse != null)
            {
                StopCoroutine(pulse);
            }

            pulse = StartCoroutine(Tween.Run(0.15f, Ease.Linear, t => transform.localScale = Vector3.one * (1f + Mathf.Sin(t * Mathf.PI) * 0.08f)));
        }

        private IEnumerator Fly(RectTransform layer, Vector3 from, Vector3 to, float delay, System.Action arrived)
        {
            yield return Tween.Wait(delay);
            var image = Ui.Art(layer, "FlyingCoin", "Icons/coin", new Vector2(70f, 70f));
            var rect = image.rectTransform;
            var jitter = new Vector3(Random.Range(-60f, 60f), Random.Range(-30f, 60f), 0f) * layer.lossyScale.x;
            yield return Tween.Run(0.6f, Ease.InOutSine, t =>
            {
                var p = Vector3.Lerp(from + jitter, to, t);
                p += Vector3.up * Mathf.Sin(t * Mathf.PI) * 160f * layer.lossyScale.x;
                rect.position = p;
                rect.localScale = Vector3.one * Mathf.Lerp(1.1f, 0.7f, t);
            });
            Destroy(image.gameObject);
            arrived();
        }
    }
}
