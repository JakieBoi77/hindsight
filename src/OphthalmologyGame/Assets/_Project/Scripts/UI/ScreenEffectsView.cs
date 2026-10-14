using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Hindsight.Ophthalmology.UI
{
    /// <summary>
    /// Full-screen overlays that sell the first-person moments: eyelids closing (blink),
    /// blurry vision after the drops, a cold blue flash when the drop lands, and fades.
    /// </summary>
    public sealed class ScreenEffectsView : MonoBehaviour
    {
        [SerializeField] RectTransform upperLid;
        [SerializeField] RectTransform lowerLid;
        [SerializeField] Image blur;
        [SerializeField] Image coldFlash;
        [SerializeField] RectTransform snowflake;
        [SerializeField] Image fader;

        void Awake()
        {
            SetLids(0f);
            SetAlpha(blur, 0f);
            SetAlpha(coldFlash, 0f);
            SetAlpha(fader, 0f);
            if (snowflake != null) snowflake.gameObject.SetActive(false);
        }

        public IEnumerator Blink(float closeSeconds = 0.09f, float openSeconds = 0.14f)
        {
            yield return Animate(closeSeconds, SetLids);
            yield return Animate(openSeconds, k => SetLids(1f - k));
        }

        /// <summary>Sets how blurry things look, 0 = clear, 1 = very blurry.</summary>
        public void SetBlur(float amount) => SetAlpha(blur, Mathf.Clamp01(amount) * 0.45f);

        public IEnumerator ColdSplash(float seconds = 1.4f)
        {
            if (snowflake != null) snowflake.gameObject.SetActive(true);
            yield return Animate(seconds, k =>
            {
                SetAlpha(coldFlash, 0.55f * (1f - k));
                if (snowflake == null) return;
                // Pop in (0-20%), hold (20-60%), shrink away (60-100%), turning slowly.
                var scale = k < 0.2f ? Mathf.SmoothStep(0f, 1.1f, k / 0.2f)
                    : k < 0.6f ? 1.1f
                    : Mathf.Lerp(1.1f, 0f, Mathf.SmoothStep(0f, 1f, (k - 0.6f) / 0.4f));
                snowflake.localScale = Vector3.one * scale;
                snowflake.localRotation = Quaternion.Euler(0f, 0f, 40f * k);
            });
            if (snowflake != null) snowflake.gameObject.SetActive(false);
        }

        public IEnumerator FadeOut(float seconds = 0.35f) => Animate(seconds, k => SetAlpha(fader, k));
        public IEnumerator FadeIn(float seconds = 0.35f) => Animate(seconds, k => SetAlpha(fader, 1f - k));

        void SetLids(float closed)
        {
            // Each lid covers half the screen when fully closed.
            if (upperLid != null) upperLid.anchorMin = new Vector2(0f, 1f - 0.5f * closed);
            if (lowerLid != null) lowerLid.anchorMax = new Vector2(1f, 0.5f * closed);
        }

        static void SetAlpha(Image image, float a)
        {
            if (image == null) return;
            var c = image.color;
            c.a = a;
            image.color = c;
            image.raycastTarget = false;
        }

        static IEnumerator Animate(float seconds, System.Action<float> apply)
        {
            for (var t = 0f; t < seconds; t += Time.deltaTime)
            {
                apply(t / seconds);
                yield return null;
            }
            apply(1f);
        }

#if UNITY_EDITOR
        public void EditorInit(RectTransform upper, RectTransform lower, Image blurImage, Image cold, Image fade, RectTransform snow)
        {
            snowflake = snow;
            upperLid = upper;
            lowerLid = lower;
            blur = blurImage;
            coldFlash = cold;
            fader = fade;
        }
#endif
    }
}
