using System;
using EyeCanDo.Art;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace EyeCanDo.UI
{
    /// <summary>
    /// A chunky slider from 0 to 1 in steps of a tenth: a track that fills from the left and a
    /// big round knob. Tap anywhere on it or drag; the whole strip is the touch target.
    /// </summary>
    public sealed class VolumeSlider : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        public const float KnobSize = 84f;
        private const float Steps = 10f;

        private RectTransform track;
        private RectTransform fill;
        private RectTransform knob;
        private float value;

        /// <summary>Raised while the value changes.</summary>
        public event Action<float> Changed;

        /// <summary>Raised when the finger lifts, to play a sample at the new volume.</summary>
        public event Action Released;

        public float Value
        {
            get => value;
            set
            {
                this.value = Mathf.Round(Mathf.Clamp01(value) * Steps) / Steps;
                Layout();
            }
        }

        public static VolumeSlider Create(Transform parent, float width, float value)
        {
            var theme = UiTheme.Instance;
            var root = Ui.Rect(parent, "VolumeSlider");
            root.sizeDelta = new Vector2(width, KnobSize + 20f);
            var hit = root.gameObject.AddComponent<Image>();
            hit.color = Color.clear;
            hit.raycastTarget = true;
            var slider = root.gameObject.AddComponent<VolumeSlider>();

            var outline = Ui.Rounded(root, "Track", theme.ink, 20f);
            slider.track = outline.rectTransform;
            Ui.Place(slider.track, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(width - KnobSize, 40f));
            var groove = Ui.Rounded(slider.track, "Groove", UiTheme.Hex("#E3E8EF"), 14f);
            Ui.Stretch(groove.rectTransform, theme.outlineWidth);
            var bar = Ui.Rounded(groove.rectTransform, "Fill", theme.accent, 14f);
            slider.fill = bar.rectTransform;
            slider.fill.anchorMin = Vector2.zero;
            slider.fill.anchorMax = new Vector2(0f, 1f);
            slider.fill.pivot = new Vector2(0f, 0.5f);
            slider.fill.offsetMin = Vector2.zero;
            slider.fill.offsetMax = Vector2.zero;

            var knobFace = Ui.Disc(root, "Knob", theme.paper);
            slider.knob = (RectTransform)knobFace.parent;
            slider.knob.sizeDelta = new Vector2(KnobSize, KnobSize);
            var dot = Ui.Image(knobFace, "Dot", Shapes.Circle, theme.accent);
            Ui.Stretch(dot.rectTransform, KnobSize * 0.28f);

            root.gameObject.AddComponent<TouchTargetScaler>();
            slider.Value = value;
            return slider;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            SetFrom(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            SetFrom(eventData);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            Released?.Invoke();
        }

        /// <summary>Sets the value as a tap at that point would (also the PlayMode test hook).</summary>
        public void Slide(float to)
        {
            var before = value;
            Value = to;
            if (!Mathf.Approximately(before, value))
            {
                Changed?.Invoke(value);
            }
        }

        private void SetFrom(PointerEventData eventData)
        {
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(track, eventData.position, eventData.pressEventCamera, out var local))
            {
                Slide(Mathf.InverseLerp(track.rect.xMin, track.rect.xMax, local.x));
            }
        }

        private void Layout()
        {
            if (track == null)
            {
                return;
            }

            var inner = track.rect.width - UiTheme.Instance.outlineWidth * 2f;
            fill.sizeDelta = new Vector2(Mathf.Max(0f, inner * value), 0f);
            fill.gameObject.SetActive(value > 0f);
            knob.anchoredPosition = new Vector2(Mathf.Lerp(-track.sizeDelta.x / 2f, track.sizeDelta.x / 2f, value), 0f);
        }
    }
}
