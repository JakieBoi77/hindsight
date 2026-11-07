using System;
using System.IO;
using Hindsight.Ophthalmology.UI;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static Hindsight.Ophthalmology.EditorTools.BuilderUtil;

namespace Hindsight.Ophthalmology.EditorTools
{
    /// <summary>
    /// The kid-friendly UI style (borrowed from the 2D prototype): Fredoka font, navy "ink" text,
    /// chunky two-tone buttons, round icon buttons and outlined cards. Shape sprites are generated
    /// here at high resolution with anti-aliased edges so they stay sharp on tablet screens.
    /// </summary>
    internal static class UiKit
    {
        const string GeneratedDir = Root + "/Art/UI";
        const string IconDir = Root + "/Art/Icons";
        public const string FredokaPath = Root + "/Fonts/Fredoka SDF.asset";

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
            public static readonly Color YellowDark = Hex("#E0A526");
            public static readonly Color Purple = Hex("#9B6BE0");
            public static readonly Color PurpleDark = Hex("#7449B8");
            public static readonly Color Teal = Hex("#2BB3A3");
            public static readonly Color Grey = Hex("#8E99AE");
            public static readonly Color Sky = Hex("#E1F1FF");
            public static readonly Color TileEdge = Hex("#CDE8FF");
        }

        public static Color Hex(string hex) => ColorUtility.TryParseHtmlString(hex, out var c) ? c : Color.magenta;

        // ------------------------------------------------------------ sprites

        public static Sprite Rounded { get; private set; }
        public static Sprite Circle { get; private set; }
        public static Sprite Ring { get; private set; }
        public static Sprite Play { get; private set; }
        public static Sprite Check { get; private set; }
        public static TMP_FontAsset Fredoka { get; private set; }

        public static Sprite Icon(string name)
        {
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{IconDir}/{name}.svg");
            if (sprite == null) Debug.LogError($"UiKit: missing icon {name}.svg");
            return sprite;
        }

        /// <summary>Generates (or loads) the shape sprites and the font. Call once before building UI.</summary>
        public static void Prepare()
        {
            EnsureFolder(GeneratedDir);
            Rounded = MakeSprite("rounded", 256, (x, y) => RoundedBoxSdf(x, y, 128, 128, 126, 126, 96), border: 110);
            Circle = MakeSprite("circle", 512, (x, y) => Vector2.Distance(new Vector2(x, y), new Vector2(256, 256)) - 252);
            Ring = MakeSprite("ring", 512, (x, y) => Mathf.Abs(Vector2.Distance(new Vector2(x, y), new Vector2(256, 256)) - 228) - 24);
            Play = MakeSprite("play", 256, (x, y) => TriangleSdf(new Vector2(x, y), new Vector2(78, 40), new Vector2(78, 216), new Vector2(218, 128)) - 14);
            Check = MakeSprite("check", 256, (x, y) =>
                Mathf.Min(SegmentSdf(new Vector2(x, y), new Vector2(56, 132), new Vector2(108, 78)),
                          SegmentSdf(new Vector2(x, y), new Vector2(108, 78), new Vector2(204, 182))) - 22);
            Fredoka = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FredokaPath);
            if (Fredoka == null) Debug.LogError($"UiKit: missing font at {FredokaPath}");
        }

        static Sprite MakeSprite(string name, int size, Func<float, float, float> sdf, int border = 0)
        {
            var path = $"{GeneratedDir}/{name}.png";
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color32[size * size];
            for (var y = 0; y < size; y++)
            for (var x = 0; x < size; x++)
            {
                // Signed distance in pixels (negative inside) -> 1px anti-aliased edge.
                var d = sdf(x + 0.5f, y + 0.5f);
                var a = (byte)Mathf.RoundToInt(Mathf.Clamp01(0.5f - d) * 255f);
                pixels[y * size + x] = new Color32(255, 255, 255, a);
            }
            tex.SetPixels32(pixels);
            File.WriteAllBytes(path, tex.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(tex);

            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.filterMode = FilterMode.Bilinear;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.spriteBorder = new Vector4(border, border, border, border);
            importer.spritePixelsPerUnit = 100;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        static float RoundedBoxSdf(float x, float y, float cx, float cy, float hw, float hh, float r)
        {
            var q = new Vector2(Mathf.Abs(x - cx) - hw + r, Mathf.Abs(y - cy) - hh + r);
            return Vector2.Max(q, Vector2.zero).magnitude + Mathf.Min(Mathf.Max(q.x, q.y), 0f) - r;
        }

        static float SegmentSdf(Vector2 p, Vector2 a, Vector2 b)
        {
            var pa = p - a;
            var ba = b - a;
            var h = Mathf.Clamp01(Vector2.Dot(pa, ba) / Vector2.Dot(ba, ba));
            return (pa - ba * h).magnitude;
        }

        static float TriangleSdf(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
        {
            var d = Mathf.Min(SegmentSdf(p, a, b), Mathf.Min(SegmentSdf(p, b, c), SegmentSdf(p, c, a)));
            var inside = Side(p, a, b) == Side(p, b, c) && Side(p, b, c) == Side(p, c, a);
            return inside ? -d : d;
        }

        static bool Side(Vector2 p, Vector2 a, Vector2 b) => (b.x - a.x) * (p.y - a.y) - (b.y - a.y) * (p.x - a.x) > 0f;

        // ------------------------------------------------------------ widgets

        /// <summary>Rounded panel. <paramref name="radius"/> is the corner radius in canvas units.</summary>
        public static Image Panel(RectTransform rt, Color color, float radius = 48f, bool raycast = false)
        {
            var img = rt.gameObject.AddComponent<Image>();
            img.sprite = Rounded;
            img.type = Image.Type.Sliced;
            img.pixelsPerUnitMultiplier = 96f / Mathf.Max(1f, radius); // the sprite's corner arc is 96px
            img.color = color;
            img.raycastTarget = raycast;
            return img;
        }

        /// <summary>Card with an ink outline: an ink panel with an inset fill panel. Returns the fill.</summary>
        public static RectTransform OutlinedCard(RectTransform rt, Color fill, float radius = 48f, float outline = 6f, Color? edge = null)
        {
            Panel(rt, edge ?? Palette.Ink, radius, raycast: true);
            var inner = Rect("Fill", rt, Vector2.zero, Vector2.one, new Vector2(outline, outline), new Vector2(-outline, -outline));
            Panel(inner, fill, Mathf.Max(4f, radius - outline));
            return inner;
        }

        public static Image Picture(RectTransform rt, Sprite sprite, Color? color = null)
        {
            var img = rt.gameObject.AddComponent<Image>();
            img.sprite = sprite;
            img.preserveAspect = true;
            img.color = color ?? Color.white;
            img.raycastTarget = false;
            return img;
        }

        public static TextMeshProUGUI Label(RectTransform parent, string name, string text, float size, Color color,
            TextAlignmentOptions align = TextAlignmentOptions.Center, float autoMin = 0f)
        {
            var rt = Stretch(name, parent);
            var tmp = rt.gameObject.AddComponent<TextMeshProUGUI>();
            tmp.font = Fredoka;
            tmp.fontStyle = FontStyles.Bold; // the variable font's default instance is light
            tmp.text = text;
            tmp.fontSize = size;
            tmp.color = color;
            tmp.alignment = align;
            tmp.textWrappingMode = TextWrappingModes.Normal;
            tmp.raycastTarget = false;
            if (autoMin > 0f)
            {
                tmp.enableAutoSizing = true;
                tmp.fontSizeMin = autoMin;
                tmp.fontSizeMax = size;
            }
            return tmp;
        }

        /// <summary>Thick outline for headline text on top of the 3D scene.</summary>
        public static void TextOutline(TMP_Text tmp, float width = 0.22f)
        {
            tmp.outlineWidth = width;
            tmp.outlineColor = Palette.Ink;
        }

        static Button MakeButton(GameObject go, Graphic target)
        {
            var button = go.AddComponent<Button>();
            button.targetGraphic = target;
            var colors = button.colors;
            colors.highlightedColor = Color.white;
            colors.selectedColor = Color.white;
            colors.pressedColor = new Color(0.92f, 0.92f, 0.92f);
            colors.disabledColor = new Color(0.8f, 0.8f, 0.8f, 0.85f);
            button.colors = colors;
            button.navigation = new Navigation { mode = Navigation.Mode.None };
            go.AddComponent<ButtonFeedback>();
            return button;
        }

        /// <summary>Chunky two-tone button: a darker base with a raised face, label and optional icon.</summary>
        public static Button Chunky(RectTransform parent, string name, string label, Color face, Color depth,
            Vector2 anchor, Vector2 size, Vector2 offset = default, Sprite icon = null, float fontSize = 60f, float radius = 40f)
        {
            var root = Rect(name, parent, anchor, anchor);
            root.sizeDelta = size;
            root.anchoredPosition = offset;
            Panel(root, depth, radius, raycast: true);
            var faceRt = Rect("Face", root, Vector2.zero, Vector2.one, new Vector2(0, 14), Vector2.zero);
            var faceImg = Panel(faceRt, face, radius);

            var textLeft = 24f;
            if (icon != null)
            {
                var iconSize = size.y * 0.52f;
                var iconRt = Rect("Icon", faceRt, new Vector2(0, 0.5f), new Vector2(0, 0.5f));
                iconRt.sizeDelta = Vector2.one * iconSize;
                iconRt.anchoredPosition = new Vector2(size.y * 0.5f, 0);
                Picture(iconRt, icon);
                textLeft = size.y * 0.85f;
            }
            if (!string.IsNullOrEmpty(label))
            {
                var tmp = Label(faceRt, "Label", label, fontSize, Palette.White, TextAlignmentOptions.Center, fontSize * 0.6f);
                tmp.rectTransform.offsetMin = new Vector2(textLeft, 6);
                tmp.rectTransform.offsetMax = new Vector2(-24, -6);
            }
            return MakeButton(root.gameObject, faceImg);
        }

        /// <summary>Round icon button with a darker rim.</summary>
        public static Button Round(RectTransform parent, string name, Sprite icon, Color face, Color rim,
            Vector2 anchor, float diameter, Vector2 offset = default, float iconScale = 0.5f)
        {
            var root = Rect(name, parent, anchor, anchor);
            root.sizeDelta = Vector2.one * diameter;
            root.anchoredPosition = offset;
            var rimImg = root.gameObject.AddComponent<Image>();
            rimImg.sprite = Circle;
            rimImg.color = rim;
            var faceRt = Rect("Face", root, Vector2.zero, Vector2.one, new Vector2(8, 16), new Vector2(-8, -4));
            var faceImg = faceRt.gameObject.AddComponent<Image>();
            faceImg.sprite = Circle;
            faceImg.color = face;
            faceImg.raycastTarget = false;
            if (icon != null)
            {
                var iconRt = Rect("Icon", faceRt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
                iconRt.sizeDelta = Vector2.one * diameter * iconScale;
                Picture(iconRt, icon);
            }
            return MakeButton(root.gameObject, rimImg);
        }

        /// <summary>Wraps <paramref name="child"/> in a pulsing/bobbing parent (never on the button itself).</summary>
        public static RectTransform WithIdle(RectTransform child, IdleMotion.Kind kind, float amplitude, float speed)
        {
            var wrap = Rect(child.name + "Wrap", child.parent, child.anchorMin, child.anchorMax);
            wrap.sizeDelta = child.sizeDelta;
            wrap.anchoredPosition = child.anchoredPosition;
            wrap.SetSiblingIndex(child.GetSiblingIndex());
            child.SetParent(wrap, false);
            child.anchorMin = Vector2.zero;
            child.anchorMax = Vector2.one;
            child.offsetMin = Vector2.zero;
            child.offsetMax = Vector2.zero;
            wrap.gameObject.AddComponent<IdleMotion>().EditorInit(kind, amplitude, speed);
            return wrap;
        }
    }
}
