using System;
using Hindsight.Tweening;
using Hindsight.UI;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Hindsight.Editor.Scaffolding
{
    /// <summary>Small helpers for building uGUI hierarchies in code with consistent styling.</summary>
    internal static class UiBuilder
    {
        public const int UiLayer = 5;

        public static TMP_FontAsset Font { get; set; }

        public static class Palette
        {
            public static readonly Color Ink = Hex("#2E3A59");
            public static readonly Color White = Hex("#FFFFFF");
            public static readonly Color Green = Hex("#4CB867");
            public static readonly Color GreenDark = Hex("#3A9552");
            public static readonly Color Blue = Hex("#4A90E2");
            public static readonly Color BlueDark = Hex("#2F6DB5");
            public static readonly Color Coral = Hex("#FF6B6B");
            public static readonly Color CoralDark = Hex("#D94F4F");
            public static readonly Color Yellow = Hex("#FFC845");
            public static readonly Color Grey = Hex("#8E99AE");
            public static readonly Color GreyDark = Hex("#56627A");
            public static readonly Color Sky = Hex("#8FD3FF");
        }

        public enum IdleKind
        {
            Bob = 0,
            Pulse = 1,
            Sway = 2,
        }

        public static Color Hex(string hex)
        {
            if (!ColorUtility.TryParseHtmlString(hex, out var color))
            {
                throw new ArgumentException($"Invalid colour '{hex}'.", nameof(hex));
            }

            return color;
        }

        public static RectTransform Rect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform)) { layer = UiLayer };
            if (parent != null)
            {
                go.transform.SetParent(parent, false);
            }

            return (RectTransform)go.transform;
        }

        /// <summary>Fills the parent, inset by the given margins.</summary>
        public static RectTransform Stretch(this RectTransform rect, float left = 0f, float bottom = 0f, float right = 0f, float top = 0f)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = new Vector2(left, bottom);
            rect.offsetMax = new Vector2(-right, -top);
            return rect;
        }

        /// <summary>Anchors to a point of the parent (0..1) with a fixed size.</summary>
        public static RectTransform Place(this RectTransform rect, Vector2 anchor, Vector2 position, Vector2 size, Vector2? pivot = null)
        {
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = pivot ?? new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            return rect;
        }

        /// <summary>Centre-anchored placement, the common case inside step prefabs.</summary>
        public static RectTransform Center(this RectTransform rect, float x, float y, float width, float height)
        {
            return rect.Place(new Vector2(0.5f, 0.5f), new Vector2(x, y), new Vector2(width, height));
        }

        public static RectTransform Region(this RectTransform rect, Vector2 anchorMin, Vector2 anchorMax)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return rect;
        }

        public static Image Image(this RectTransform rect, Sprite sprite, Color? color = null, bool raycast = false, bool preserveAspect = true)
        {
            var image = rect.gameObject.AddComponent<Image>();
            image.sprite = sprite;
            image.color = color ?? Color.white;
            image.raycastTarget = raycast;
            image.preserveAspect = preserveAspect && sprite != null;
            return image;
        }

        /// <summary>Rounded 9-sliced panel tinted with <paramref name="color"/>.</summary>
        public static Image Panel(this RectTransform rect, Color color, bool raycast = false)
        {
            var image = rect.gameObject.AddComponent<Image>();
            image.sprite = ArtLibrary.PanelSprite;
            image.type = UnityEngine.UI.Image.Type.Sliced;
            image.pixelsPerUnitMultiplier = 1f;
            image.color = color;
            image.raycastTarget = raycast;
            return image;
        }

        public static TextMeshProUGUI Text(this RectTransform rect, string text, float size, Color color, TextAlignmentOptions alignment = TextAlignmentOptions.Center, float minAutoSize = 0f)
        {
            var label = rect.gameObject.AddComponent<TextMeshProUGUI>();
            label.font = Font;
            label.text = text;
            label.fontSize = size;
            label.color = color;
            label.alignment = alignment;

            // The variable font's default instance is a light weight; bold reads better for children.
            label.fontStyle = FontStyles.Bold;
            label.raycastTarget = false;
            label.textWrappingMode = TextWrappingModes.Normal;
            if (minAutoSize > 0f)
            {
                label.enableAutoSizing = true;
                label.fontSizeMin = minAutoSize;
                label.fontSizeMax = size;
            }

            return label;
        }

        public static CanvasGroup Group(this RectTransform rect, float alpha = 1f)
        {
            var group = rect.gameObject.AddComponent<CanvasGroup>();
            group.alpha = alpha;
            return group;
        }

        public static IdleMotion Idle(this RectTransform rect, IdleKind motion, float amplitude, float cyclesPerSecond, bool enabled = true)
        {
            var idle = rect.gameObject.AddComponent<IdleMotion>();
            SerializedWriter.For(idle)
                .Enum("kind", (int)motion)
                .Float("amplitude", amplitude)
                .Float("cyclesPerSecond", cyclesPerSecond)
                .Apply();
            idle.enabled = enabled;
            return idle;
        }

        public static Button MakeButton(this RectTransform rect, Graphic target, bool feedback = true)
        {
            var button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = target;
            var colors = button.colors;
            colors.highlightedColor = Color.white;
            colors.selectedColor = Color.white;
            colors.pressedColor = new Color(0.9f, 0.9f, 0.9f);
            colors.disabledColor = new Color(0.75f, 0.75f, 0.75f, 0.8f);
            button.colors = colors;

            // Children cannot "select" with a keyboard; avoid odd lingering highlight states.
            button.navigation = new Navigation { mode = Navigation.Mode.None };
            if (feedback)
            {
                rect.gameObject.AddComponent<ButtonFeedback>();
            }

            return button;
        }

        /// <summary>Chunky "3D" rounded button: a darker base with a raised face and a label.</summary>
        public static Button ChunkyButton(Transform parent, string name, string label, Color face, Color depth, Vector2 size, float fontSize = 64f, Sprite icon = null)
        {
            var root = Rect(name, parent);
            root.sizeDelta = size;
            root.Panel(depth, raycast: true);

            var faceRect = Rect("Face", root).Stretch(0f, 12f, 0f, 0f);
            var faceImage = faceRect.Panel(face);

            if (icon != null)
            {
                var iconRect = Rect("Icon", faceRect).Place(new Vector2(0f, 0.5f), new Vector2(size.y * 0.5f + 10f, 0f), Vector2.one * (size.y * 0.55f));
                iconRect.Image(icon);
            }

            if (!string.IsNullOrEmpty(label))
            {
                var leftInset = icon != null ? size.y * 0.8f : 16f;
                Rect("Label", faceRect).Stretch(leftInset, 6f, 16f, 6f).Text(label, fontSize, Palette.White, TextAlignmentOptions.Center, fontSize * 0.6f);
            }

            return root.MakeButton(faceImage);
        }

        /// <summary>Round icon button (home, continue, back).</summary>
        public static Button RoundButton(Transform parent, string name, Sprite icon, Color face, float diameter, float iconScale = 0.55f)
        {
            var root = Rect(name, parent);
            root.sizeDelta = Vector2.one * diameter;
            var faceImage = root.Image(ArtLibrary.Circle, face, raycast: true, preserveAspect: false);
            Rect("Icon", root).Center(0f, 0f, diameter * iconScale, diameter * iconScale).Image(icon);
            return root.MakeButton(faceImage);
        }

        public static GameObject InstantiatePrefab(GameObject prefab, Transform parent)
        {
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            instance.transform.SetParent(parent, false);
            return instance;
        }

        public static T SaveAsPrefab<T>(GameObject root, string path)
            where T : Component
        {
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, path, out var success);
            UnityEngine.Object.DestroyImmediate(root);
            if (!success || prefab == null)
            {
                throw new InvalidOperationException($"Failed to save prefab at {path}.");
            }

            return prefab.GetComponent<T>();
        }
    }
}
