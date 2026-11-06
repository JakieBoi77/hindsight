using System.Collections;
using EyeCanDo.Art;
using UnityEngine;
using UnityEngine.UI;

namespace EyeCanDo.UI
{
    /// <summary>
    /// A gentle burst of soft shapes for celebrations. Pieces drift and fade slowly: nothing
    /// flashes (NFR_Flashing) and nothing is sudden. Each piece animates itself, so it goes away
    /// cleanly if its panel or scene closes first. Reduce motion keeps a few pieces close by.
    /// </summary>
    public static class Confetti
    {
        public static IEnumerator Burst(RectTransform parent, Vector2 origin, int pieces = 18, float spread = 420f)
        {
            var theme = UiTheme.Instance;
            if (App.GameServices.Settings.reduceMotion)
            {
                pieces = Mathf.Max(4, pieces / 3);
                spread *= 0.5f;
            }

            var colors = new[] { theme.primary, theme.accent, theme.success, theme.danger, theme.drop };
            for (var i = 0; i < pieces; i++)
            {
                var sprite = i % 3 == 0 ? Shapes.Star : Shapes.Circle;
                var image = Ui.Image(parent, "Confetti", sprite, colors[i % colors.Length]);
                var size = Random.Range(22f, 44f);
                Ui.Place(image.rectTransform, new Vector2(0.5f, 0.5f), origin, new Vector2(size, size));
                var angle = Random.Range(0f, Mathf.PI * 2f);
                var target = origin + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle) * 0.7f + 0.3f) * Random.Range(spread * 0.4f, spread);
                image.gameObject.AddComponent<Piece>().Begin(origin, target);
            }

            yield return Tween.Wait(Piece.Seconds);
        }

        private sealed class Piece : MonoBehaviour
        {
            public const float Seconds = 1.6f;

            private Image image;
            private RectTransform rect;
            private Vector2 from;
            private Vector2 to;
            private Color color;
            private float spin;
            private float time;

            public void Begin(Vector2 start, Vector2 end)
            {
                image = GetComponent<Image>();
                rect = image.rectTransform;
                from = start;
                to = end;
                color = image.color;
                spin = Random.Range(-90f, 90f);
            }

            private void Update()
            {
                time += Time.deltaTime;
                var t = Mathf.Clamp01(time / Seconds);
                var eased = Core.Animation.Ease.OutCubic(t);
                rect.anchoredPosition = Vector2.Lerp(from, to, eased) + Vector2.down * (eased * eased * 60f);
                rect.localRotation = Quaternion.Euler(0f, 0f, spin * eased);
                image.color = new Color(color.r, color.g, color.b, 1f - eased * eased);
                if (t >= 1f)
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}
