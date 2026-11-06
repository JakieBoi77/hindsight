using System;
using EyeCanDo.Art;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace EyeCanDo.UI
{
    /// <summary>
    /// The house button: a rounded face on a darker base, with an ink outline, an optional icon
    /// and label. The face sinks when pressed. Sized to at least 15 mm (NFR_TouchTarget).
    /// </summary>
    public sealed class ChunkyButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler, IPointerExitHandler
    {
        private const float Depth = 12f;

        private RectTransform face;
        private Image faceImage;
        private Image baseImage;
        private Color faceColor;
        private bool pressed;

        public event Action Clicked;

        public bool Interactable { get; private set; } = true;

        public TextMeshProUGUI Label { get; private set; }

        public Image IconImage { get; private set; }

        public static ChunkyButton Create(Transform parent, string name, string label, Sprite icon, Color color, Vector2 size, float radius = -1f)
        {
            var theme = UiTheme.Instance;
            radius = radius < 0f ? theme.cornerRadius : radius;

            var root = Ui.Rect(parent, name);
            root.sizeDelta = size;
            var button = root.gameObject.AddComponent<ChunkyButton>();
            button.faceColor = color;

            var hit = root.gameObject.AddComponent<Image>();
            hit.color = Color.clear;
            hit.raycastTarget = true;

            var outline = Ui.Rounded(root, "Outline", theme.ink, radius);
            Ui.Stretch(outline.rectTransform);
            outline.rectTransform.offsetMin = new Vector2(0f, -Depth);

            button.baseImage = Ui.Rounded(root, "Base", UiTheme.Darker(color, 0.25f), radius - theme.outlineWidth);
            Ui.Stretch(button.baseImage.rectTransform, theme.outlineWidth);
            button.baseImage.rectTransform.offsetMin = new Vector2(theme.outlineWidth, theme.outlineWidth - Depth);

            button.faceImage = Ui.Rounded(root, "Face", color, radius - theme.outlineWidth);
            button.face = button.faceImage.rectTransform;
            Ui.Stretch(button.face, theme.outlineWidth);

            var content = Ui.Rect(button.face, "Content");
            Ui.Stretch(content, 10f);
            var layout = content.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.spacing = 18f;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            layout.childControlWidth = true;
            layout.childControlHeight = true;

            if (icon != null)
            {
                button.IconImage = Ui.Image(content, "Icon", icon, Color.white);
                button.IconImage.preserveAspect = true;
                var iconSize = Mathf.Min(size.y * 0.58f, 110f);
                var element = button.IconImage.gameObject.AddComponent<LayoutElement>();
                element.preferredWidth = iconSize;
                element.preferredHeight = iconSize;
            }

            if (!string.IsNullOrEmpty(label))
            {
                button.Label = Ui.Text(content, label, theme.buttonSize, theme.ink);
                button.Label.enableAutoSizing = false;
                button.Label.textWrappingMode = TextWrappingModes.NoWrap;
                var element = button.Label.gameObject.AddComponent<LayoutElement>();
                element.flexibleWidth = 0f;
            }

            root.gameObject.AddComponent<TouchTargetScaler>();
            return button;
        }

        /// <summary>A round icon-only button, used for Home, Restart and Skip.</summary>
        public static ChunkyButton CreateRound(Transform parent, string name, Sprite icon, Color color, float diameter)
        {
            return Create(parent, name, null, icon, color, new Vector2(diameter, diameter), diameter / 2f);
        }

        public void SetInteractable(bool value)
        {
            Interactable = value;
            if (faceImage != null)
            {
                faceImage.color = value ? faceColor : Color.Lerp(faceColor, UiTheme.Instance.muted, 0.7f);
                baseImage.color = UiTheme.Darker(faceImage.color, 0.25f);
            }
        }

        public void SetColor(Color color)
        {
            faceColor = color;
            SetInteractable(Interactable);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!Interactable)
            {
                return;
            }

            pressed = true;
            face.anchoredPosition = new Vector2(0f, -Depth * 0.7f);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            Unpress();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            Unpress();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Click();
        }

        /// <summary>Activates the button as a tap would. Also used by the PlayMode auto-player.</summary>
        public void Click()
        {
            if (Interactable)
            {
                App.Sfx.Play(App.Sound.Tap);
                Clicked?.Invoke();
            }
        }

        private void Unpress()
        {
            if (!pressed)
            {
                return;
            }

            pressed = false;
            face.anchoredPosition = Vector2.zero;
        }
    }
}
