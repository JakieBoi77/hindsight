using System.Collections;
using EyeCanDo.App;
using EyeCanDo.Art;
using EyeCanDo.Core.Games;
using UnityEngine;
using UnityEngine.UI;

namespace EyeCanDo.UI
{
    /// <summary>
    /// Up to three stars. Earned stars are filled yellow; missing ones are pale with an outline,
    /// so the difference never depends on colour alone (NFR_Colour).
    /// </summary>
    public sealed class StarRow : MonoBehaviour
    {
        private readonly Image[] stars = new Image[StarRules.MaxStars];

        public static StarRow Create(Transform parent, float starSize, float spacing = 12f)
        {
            var rect = Ui.Rect(parent, "Stars");
            rect.sizeDelta = new Vector2(starSize * StarRules.MaxStars + spacing * (StarRules.MaxStars - 1), starSize);
            var row = rect.gameObject.AddComponent<StarRow>();
            for (var i = 0; i < StarRules.MaxStars; i++)
            {
                var star = Ui.Image(rect, "Star" + i, ArtLibrary.Get("Icons/star_empty"), Color.white);
                star.preserveAspect = true;
                Ui.Place(star.rectTransform, new Vector2(0f, 0.5f), new Vector2(starSize / 2f + i * (starSize + spacing), 0f), new Vector2(starSize, starSize));
                row.stars[i] = star;
            }

            return row;
        }

        public void Set(int count)
        {
            for (var i = 0; i < stars.Length; i++)
            {
                SetStar(i, i < count);
            }
        }

        /// <summary>Fills stars one by one with a pop and a chime.</summary>
        public IEnumerator Reveal(int count)
        {
            Set(0);
            for (var i = 0; i < count && i < stars.Length; i++)
            {
                yield return Tween.Wait(0.25f);
                SetStar(i, true);
                Sfx.Play(Sound.Star, Sfx.ScaleNote(i * 2));
                yield return Tween.Pop(stars[i].transform, 0.4f, 1.2f);
                yield return Tween.Scale(stars[i].transform, Vector3.one, 0.15f);
            }
        }

        private void SetStar(int index, bool earned)
        {
            stars[index].sprite = ArtLibrary.Get(earned ? "Icons/star" : "Icons/star_empty");
        }
    }
}
