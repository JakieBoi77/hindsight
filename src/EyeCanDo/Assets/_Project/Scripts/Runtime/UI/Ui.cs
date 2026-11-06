using EyeCanDo.Art;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace EyeCanDo.UI
{
    /// <summary>
    /// Small helpers to build styled uGUI from code, so every screen uses the same shapes and
    /// sizes. Layout uses a 1920x1080 reference area that is always fully on screen: wider phones
    /// get extra room at the sides and 4:3 tablets get extra room above and below
    /// (CanvasScaler Expand), so nothing important is ever cut off.
    /// </summary>
    public static class Ui
    {
        public static readonly Vector2 ReferenceResolution = new Vector2(1920f, 1080f);

        public static UiTheme Theme => UiTheme.Instance;

        public static Canvas CreateCanvas(string name, int sortingOrder, Transform parent = null)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            if (parent != null)
            {
                go.transform.SetParent(parent, false);
            }

            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = ReferenceResolution;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            scaler.referencePixelsPerUnit = 100f;
            return canvas;
        }

        public static void EnsureEventSystem()
        {
            if (Object.FindAnyObjectByType<EventSystem>() != null)
            {
                return;
            }

            var go = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            go.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
        }

        public static RectTransform Rect(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        public static RectTransform Stretch(RectTransform rect, float inset = 0f)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = new Vector2(inset, inset);
            rect.offsetMax = new Vector2(-inset, -inset);
            return rect;
        }

        /// <summary>Anchors the rect at one point of its parent (0..1) and sets position and size.</summary>
        public static RectTransform Place(RectTransform rect, Vector2 anchor, Vector2 position, Vector2 size, Vector2? pivot = null)
        {
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = pivot ?? new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            return rect;
        }

        public static Image Image(Transform parent, string name, Sprite sprite, Color color, bool raycast = false)
        {
            var rect = Rect(parent, name);
            var image = rect.gameObject.AddComponent<Image>();
            image.sprite = sprite;
            image.color = color;
            image.raycastTarget = raycast;
            image.preserveAspect = sprite != null && sprite.border == Vector4.zero;
            return image;
        }

        /// <summary>A rounded rectangle with the given corner radius.</summary>
        public static Image Rounded(Transform parent, string name, Color color, float radius, bool raycast = false)
        {
            var image = Image(parent, name, Shapes.RoundedRect, color, raycast);
            image.type = UnityEngine.UI.Image.Type.Sliced;
            image.preserveAspect = false;
            image.pixelsPerUnitMultiplier = Shapes.SliceRadius / Mathf.Max(radius, 1f);
            return image;
        }

        /// <summary>
        /// A paper panel in the house style: ink outline, soft drop shadow and rounded corners.
        /// Returns the inner face, which children should be added to.
        /// </summary>
        public static RectTransform Panel(Transform parent, string name, Color face, float radius = -1f)
        {
            var theme = Theme;
            radius = radius < 0f ? theme.cornerRadius : radius;
            var root = Rect(parent, name);
            var shadow = Rounded(root, "Shadow", new Color(theme.ink.r, theme.ink.g, theme.ink.b, 0.18f), radius);
            Stretch(shadow.rectTransform);
            shadow.rectTransform.anchoredPosition = new Vector2(0f, -theme.shadowOffset);
            var outline = Rounded(root, "Outline", theme.ink, radius);
            Stretch(outline.rectTransform);
            var inner = Rounded(root, "Face", face, Mathf.Max(radius - theme.outlineWidth, 1f), true);
            Stretch(inner.rectTransform, theme.outlineWidth);
            return inner.rectTransform;
        }

        /// <summary>
        /// A round panel in the house style, drawn from circles so it stays crisp at any size.
        /// Returns the inner face; its parent is the root to place.
        /// </summary>
        public static RectTransform Disc(Transform parent, string name, Color face)
        {
            var theme = Theme;
            var root = Rect(parent, name);
            var shadow = Image(root, "Shadow", Shapes.Circle, new Color(theme.ink.r, theme.ink.g, theme.ink.b, 0.18f));
            Stretch(shadow.rectTransform);
            shadow.rectTransform.anchoredPosition = new Vector2(0f, -theme.shadowOffset);
            var outline = Image(root, "Outline", Shapes.Circle, theme.ink);
            Stretch(outline.rectTransform);
            var inner = Image(root, "Face", Shapes.Circle, face, true);
            Stretch(inner.rectTransform, theme.outlineWidth);
            return inner.rectTransform;
        }

        public static TextMeshProUGUI Text(Transform parent, string text, float size, Color color, TextAlignmentOptions align = TextAlignmentOptions.Center, bool bold = true)
        {
            var rect = Rect(parent, "Text");
            var label = rect.gameObject.AddComponent<TextMeshProUGUI>();
            if (Theme.font != null)
            {
                label.font = Theme.font;
            }

            label.text = text;
            label.fontSize = size;
            label.color = color;
            label.alignment = align;
            label.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
            label.textWrappingMode = TextWrappingModes.Normal;
            label.raycastTarget = false;
            label.enableAutoSizing = true;
            label.fontSizeMax = size;
            label.fontSizeMin = size * 0.6f;
            return label;
        }

        public static Image Icon(Transform parent, Sprite sprite, Color color, Vector2 size)
        {
            var image = Image(parent, "Icon", sprite, color);
            image.preserveAspect = true;
            image.rectTransform.sizeDelta = size;
            return image;
        }

        public static Sprite PictogramFor(Content.CaptionIcon icon)
        {
            switch (icon)
            {
                case Content.CaptionIcon.Cold: return ArtLibrary.Get("Icons/snowflake");
                case Content.CaptionIcon.Clock: return ArtLibrary.Get("Icons/clock");
                case Content.CaptionIcon.Blurry: return ArtLibrary.Get("Icons/blurry");
                case Content.CaptionIcon.Light: return ArtLibrary.Get("Icons/sun");
                case Content.CaptionIcon.Heart: return ArtLibrary.Get("Icons/heart");
                case Content.CaptionIcon.Star: return ArtLibrary.Get("Icons/star");
                case Content.CaptionIcon.Speak: return ArtLibrary.Get("Icons/speak");
                case Content.CaptionIcon.Drop: return ArtLibrary.Get("Icons/drop");
                case Content.CaptionIcon.Eye: return ArtLibrary.Get("Icons/eye_big_pupil");
                case Content.CaptionIcon.Sunglasses: return ArtLibrary.Get("Icons/sunglasses");
                case Content.CaptionIcon.Coin: return ArtLibrary.Get("Icons/coin");
                case Content.CaptionIcon.Lock: return ArtLibrary.Get("Icons/lock");
                default: return null;
            }
        }

        /// <summary>An art sprite at its own aspect ratio, untinted.</summary>
        public static Image Art(Transform parent, string name, string sprite, Vector2 size)
        {
            var image = Image(parent, name, ArtLibrary.Get(sprite), Color.white);
            image.preserveAspect = true;
            image.rectTransform.sizeDelta = size;
            return image;
        }
    }
}
