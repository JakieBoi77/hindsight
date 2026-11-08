using System.Collections.Generic;
using System.IO;
using Hindsight.EyeCanDo.Core.Avatar;
using Hindsight.EyeCanDo.SceneObjects;
using UnityEditor;
using UnityEngine;
using static Hindsight.EyeCanDo.EditorTools.BuilderUtil;

namespace Hindsight.EyeCanDo.EditorTools
{
    /// <summary>
    /// Paints the scouts' face atlases (layout in <see cref="FaceArt"/>): every eye style open and
    /// shut, every mouth in four shapes from closed to wide open, and every brow style in every mood. Shapes are
    /// signed distance functions in each cell's own coordinates (y up from -1 to 1; x spans the cell's
    /// aspect), so edges are anti-aliased at any size. All drawings are original.
    /// </summary>
    internal static class FaceArtPainter
    {
        public static string AtlasPath(FaceArt.Layer layer) => $"{MaterialsDir}/Face{layer}.png";

        static readonly Color Ink = Hex("#231F33");
        static readonly Color White = Color.white;
        static readonly Color Cream = Hex("#F7EFD8"); // eye whites
        static readonly Color Pupil = Hex("#14184A");
        static readonly Color MouthInside = Hex("#5B1830");
        static readonly Color Tongue = Hex("#FF7D9C");
        static readonly Color Tear = Hex("#62C8FF");
        static readonly Color Hair = Hex("#4A2E22");
        static readonly Color Blush = new(1f, 0.45f, 0.55f, 0.55f);
        static readonly Color ScarRed = Hex("#B5463F");
        static readonly Color Gold = Hex("#D9A520");
        static readonly Color Lipstick = Hex("#E8466A");

        public delegate float Sdf(float x, float y);

        readonly struct Layer
        {
            public readonly Sdf Shape;
            public readonly Color Colour;
            public readonly Sdf Clip;

            public Layer(Sdf shape, Color colour, Sdf clip = null)
            {
                Shape = shape;
                Colour = colour;
                Clip = clip;
            }
        }

        /// <summary>Paints (or repaints) one atlas and returns the imported texture.</summary>
        public static Texture2D Texture(FaceArt.Layer layer)
        {
            const int size = FaceArt.Size;
            var pixels = new Color[size * size];
            for (var i = 0; i < pixels.Length; i++) pixels[i] = new Color(Ink.r, Ink.g, Ink.b, 0f);
            var aspect = FaceArt.Aspect(layer);
            switch (layer)
            {
                case FaceArt.Layer.Eyes:
                    for (var s = 0; s < FaceArt.EyeStyles.Length; s++)
                    {
                        Paint(pixels, size, FaceArt.Eyes(s, false), aspect, EyeLayers(FaceArt.EyeStyles[s], false));
                        Paint(pixels, size, FaceArt.Eyes(s, true), aspect, EyeLayers(FaceArt.EyeStyles[s], true));
                    }
                    Paint(pixels, size, FaceArt.Cheeks, aspect, CheekLayers());
                    break;
                case FaceArt.Layer.Mouth:
                    for (var s = 0; s < FaceArt.MouthStyles.Length; s++)
                    for (var f = 0; f < FaceArt.MouthFrames; f++)
                        Paint(pixels, size, FaceArt.Mouth(s, f), aspect, MouthLayers(FaceArt.MouthStyles[s], f));
                    break;
                default:
                    for (var s = 0; s < FaceArt.BrowStyles.Length; s++)
                    for (var m = 0; m < FaceArt.MoodCount; m++)
                        Paint(pixels, size, FaceArt.Brows(s, (Mood)m), aspect, BrowLayers(FaceArt.BrowStyles[s], (Mood)m));
                    break;
            }

            var path = AtlasPath(layer);
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.SetPixels(pixels);
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = true;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        /// <summary>Draws layers in order into one cell, blending each over what is there.</summary>
        static void Paint(Color[] pixels, int size, Rect cell, float aspect, List<Layer> layers)
        {
            int x0 = Mathf.RoundToInt(cell.x * size), y0 = Mathf.RoundToInt(cell.y * size);
            int w = Mathf.RoundToInt(cell.width * size), h = Mathf.RoundToInt(cell.height * size);
            var pixel = 2f / h;
            for (var py = 2; py < h - 2; py++)
            for (var px = 2; px < w - 2; px++)
            {
                var x = ((px + 0.5f) / w * 2f - 1f) * aspect;
                var y = (py + 0.5f) / h * 2f - 1f;
                float r = 0, g = 0, b = 0, a = 0; // premultiplied
                foreach (var layer in layers)
                {
                    var cover = Cover(layer.Shape(x, y), pixel);
                    if (cover <= 0f) continue;
                    if (layer.Clip != null) cover *= Cover(layer.Clip(x, y), pixel);
                    if (cover <= 0f) continue;
                    var k = cover * layer.Colour.a;
                    r = layer.Colour.r * k + r * (1f - k);
                    g = layer.Colour.g * k + g * (1f - k);
                    b = layer.Colour.b * k + b * (1f - k);
                    a = k + a * (1f - k);
                }
                if (a <= 0f) continue;
                pixels[(y0 + py) * size + x0 + px] = new Color(r / a, g / a, b / a, a);
            }
        }

        static float Cover(float distance, float pixel) => Mathf.Clamp01(0.5f - distance / pixel);

        // ------------------------------------------------------------------ eyes (cell x -2..2; eyes at x = ±0.95)

        static List<Layer> EyeLayers(string style, bool shut)
        {
            var L = new List<Layer>();
            foreach (var side in new[] { -1f, 1f })
            {
                // side -1: the eye on the left of the picture. "Inner" is toward the nose (x = 0).
                var cx = side * FaceArt.EyeCellX;
                if (shut) ShutEye(L, style, cx, side);
                else OpenEye(L, style, cx, side);
            }
            return L;
        }

        static void OpenEye(List<Layer> L, string style, float cx, float side)
        {
            switch (style)
            {
                case "basic":
                    Eye(L, cx, side, 0f, 0.42f, 0.56f);
                    break;
                case "lashes":
                    Eye(L, cx, side, -0.02f, 0.4f, 0.52f);
                    Lashes(L, cx, side, -0.02f, 0.46f, 0.58f, false);
                    break;
                case "half":
                    L.Add(new Layer(Ellipse(cx, -0.06f, 0.3f, 0.34f), Ink, (x, y) => y - 0.06f));
                    L.Add(new Layer(Stroke(Seg(cx - 0.36f, 0.08f, cx + 0.36f, 0.08f), 0.1f), Ink));
                    L.Add(new Layer(Circle(cx - 0.1f, -0.1f, 0.06f), White));
                    break;
                case "shine":
                    L.Add(new Layer(Ellipse(cx, 0f, 0.32f, 0.42f), Ink));
                    L.Add(new Layer(Circle(cx - 0.1f, 0.15f, 0.13f), White));
                    L.Add(new Layer(Circle(cx + 0.11f, -0.17f, 0.065f), White));
                    break;
                case "squint":
                    // "> <": chevrons pointing in toward the nose.
                    L.Add(new Layer(Stroke(Poly(cx + side * 0.24f, 0.24f, cx - side * 0.2f, 0f, cx + side * 0.24f, -0.24f), 0.13f), Ink));
                    break;
                case "angry":
                    // Top cut on a slant, low at the inner corner (brows are face accessories).
                    L.Add(new Layer(Ellipse(cx, -0.06f, 0.27f, 0.33f), Ink, (x, y) => y - (0.13f + 0.45f * (x - cx) * side)));
                    L.Add(new Layer(Circle(cx + side * 0.06f, -0.12f, 0.06f), White));
                    break;
                case "eyeliner":
                    Eye(L, cx, side, -0.02f, 0.38f, 0.48f);
                    L.Add(new Layer(Stroke(Bez(cx - 0.3f, 0.12f, cx, 0.5f, cx + 0.3f, 0.12f), 0.08f), Ink));
                    L.Add(new Layer(Stroke(Bez(cx + side * 0.28f, 0.14f, cx + side * 0.42f, 0.24f, cx + side * 0.52f, 0.38f), 0.08f), Ink));
                    break;
                case "sad":
                    L.Add(new Layer(Ellipse(cx, -0.06f, 0.27f, 0.33f), Ink, (x, y) => y - (0.13f - 0.45f * (x - cx) * side)));
                    L.Add(new Layer(Circle(cx - side * 0.04f, -0.14f, 0.07f), White));
                    break;
                case "cry":
                    L.Add(new Layer(Ellipse(cx, -0.04f, 0.27f, 0.33f), Ink, (x, y) => y - (0.15f - 0.45f * (x - cx) * side)));
                    Tears(L, cx, side);
                    break;
                case "tired":
                    L.Add(new Layer(Ellipse(cx, -0.05f, 0.27f, 0.32f), Ink, (x, y) => y - 0.1f));
                    L.Add(new Layer(Stroke(Seg(cx - 0.33f, 0.12f, cx + 0.33f, 0.12f), 0.09f), Ink));
                    Bags(L, cx);
                    break;
                case "almond":
                {
                    var lens = Lens(cx, 0f, 0.46f, 0.25f);
                    L.Add(new Layer(lens, Cream));
                    L.Add(new Layer(Circle(cx + side * 0.04f, -0.01f, 0.2f), Ink, lens));
                    L.Add(new Layer(Circle(cx - 0.03f, 0.07f, 0.06f), White));
                    break;
                }
                case "big":
                    L.Add(new Layer(Circle(cx, 0f, 0.46f), Cream));
                    L.Add(new Layer(Circle(cx - side * 0.06f, -0.04f, 0.16f), Pupil));
                    break;
                case "small":
                    L.Add(new Layer(Circle(cx, 0f, 0.3f), Cream));
                    L.Add(new Layer(Circle(cx, -0.02f, 0.13f), Pupil));
                    break;
                case "concern":
                {
                    // Worried: wide whites with a small pupil glancing up and away, the inner corner lifted.
                    var white = Ellipse(cx, -0.04f, 0.38f, 0.46f);
                    Sdf lid = (x, y) => y - (0.3f + 0.25f * (x - cx) * side);
                    L.Add(new Layer(white, Cream, lid));
                    L.Add(new Layer(Circle(cx + side * 0.1f, 0.06f, 0.16f), Pupil, Max(white, lid)));
                    break;
                }
                case "anime":
                    L.Add(new Layer(Ellipse(cx, 0f, 0.38f, 0.54f), Ink));
                    L.Add(new Layer(Ellipse(cx, -0.1f, 0.29f, 0.38f), Hex("#5B47A8")));
                    L.Add(new Layer(Ellipse(cx, -0.2f, 0.22f, 0.22f), Hex("#8C7BE0")));
                    L.Add(new Layer(Circle(cx - 0.13f, 0.2f, 0.15f), White));
                    L.Add(new Layer(Circle(cx + 0.14f, -0.22f, 0.07f), White));
                    L.Add(new Layer(Sparkle(cx + 0.12f, 0.22f, 0.12f), White));
                    break;
                case "real":
                {
                    var lens = Lens(cx, 0f, 0.48f, 0.27f);
                    L.Add(new Layer(lens, Hex("#FBF7F2")));
                    L.Add(new Layer(Circle(cx, 0f, 0.22f), Hex("#6E9C47"), lens));
                    L.Add(new Layer(Circle(cx, 0f, 0.15f), Hex("#4C7A2E"), lens));
                    L.Add(new Layer(Circle(cx, 0f, 0.09f), Ink, lens));
                    L.Add(new Layer(Circle(cx - 0.07f, 0.07f, 0.045f), White, lens));
                    L.Add(new Layer(Stroke(Bez(cx - 0.5f, 0.0f, cx, 0.58f, cx + 0.5f, 0.0f), 0.07f), Ink));
                    L.Add(new Layer(Stroke(Bez(cx - 0.42f, -0.06f, cx, -0.36f, cx + 0.42f, -0.06f), 0.03f), new Color(0.14f, 0.12f, 0.2f, 0.5f)));
                    break;
                }
                case "fierce":
                {
                    Sdf cut = (x, y) => y - (0.12f + 0.4f * (x - cx) * side);
                    var lens = Lens(cx, -0.04f, 0.42f, 0.3f);
                    L.Add(new Layer(lens, Hex("#FFF3D6"), cut));
                    L.Add(new Layer(Circle(cx + side * 0.02f, -0.05f, 0.2f), Hex("#F0B429"), Max(lens, cut)));
                    L.Add(new Layer(Ellipse(cx + side * 0.02f, -0.05f, 0.05f, 0.17f), Ink, Max(lens, cut)));
                    break;
                }
                default: // "inverted": dark eye, white pupil
                    L.Add(new Layer(Ellipse(cx, 0f, 0.3f, 0.4f), Ink));
                    L.Add(new Layer(Circle(cx, -0.03f, 0.12f), White));
                    L.Add(new Layer(Ring(cx, -0.03f, 0.22f, 0.025f), new Color(1f, 1f, 1f, 0.6f)));
                    break;
            }
        }

        /// <summary>A closed eye: a soft curve, keeping whatever the style adds (lashes, brows, tears, wings).</summary>
        static void ShutEye(List<Layer> L, string style, float cx, float side)
        {
            var curve = Stroke(Bez(cx - 0.4f, 0.03f, cx, -0.3f, cx + 0.4f, 0.03f), 0.13f);
            switch (style)
            {
                case "squint":
                    L.Add(new Layer(Stroke(Poly(cx + side * 0.24f, 0.1f, cx - side * 0.2f, 0f, cx + side * 0.24f, -0.1f), 0.12f), Ink));
                    return;
                case "angry":
                case "fierce":
                    L.Add(new Layer(Stroke(Seg(cx + side * 0.3f, 0.02f, cx - side * 0.3f, -0.08f), 0.12f), Ink));
                    return;
                case "sad":
                case "concern":
                    L.Add(new Layer(curve, Ink));
                    return;
                case "cry":
                    L.Add(new Layer(curve, Ink));
                    Tears(L, cx, side);
                    return;
                case "tired":
                    L.Add(new Layer(curve, Ink));
                    Bags(L, cx);
                    return;
                case "lashes":
                    L.Add(new Layer(curve, Ink));
                    Lashes(L, cx, side, -0.08f, 0.38f, 0.2f, true);
                    return;
                case "eyeliner":
                    L.Add(new Layer(curve, Ink));
                    L.Add(new Layer(Stroke(Bez(cx + side * 0.28f, 0.02f, cx + side * 0.42f, 0.12f, cx + side * 0.52f, 0.26f), 0.08f), Ink));
                    return;
                case "real":
                case "almond":
                case "big":
                case "anime":
                    L.Add(new Layer(Stroke(Bez(cx - 0.42f, 0.03f, cx, -0.3f, cx + 0.42f, 0.03f), 0.12f), Ink));
                    return;
                case "small":
                    L.Add(new Layer(Stroke(Seg(cx - 0.13f, 0f, cx + 0.13f, 0f), 0.08f), Ink));
                    return;
                default:
                    L.Add(new Layer(curve, Ink));
                    return;
            }
        }

        /// <summary>
        /// A flat cartoon eye: a borderless cream white with a big solid navy pupil glancing a little
        /// toward the nose.
        /// </summary>
        static void Eye(List<Layer> L, float cx, float side, float cy, float rx, float ry)
        {
            var white = Ellipse(cx, cy, rx, ry);
            L.Add(new Layer(white, Cream));
            var px = cx - side * rx * 0.1f;
            var py = cy - ry * 0.08f;
            L.Add(new Layer(Ellipse(px, py, rx * 0.6f, ry * 0.64f), Pupil, white));
        }

        /// <summary>Two soft blush ovals, wide apart (they sit under the outer half of each eye).</summary>
        static List<Layer> CheekLayers()
        {
            var L = new List<Layer>();
            var blush = new Color(1f, 0.38f, 0.42f, 0.42f);
            foreach (var side in new[] { -1f, 1f })
            {
                L.Add(new Layer(Ellipse(side * 1.3f, 0f, 0.42f, 0.3f), blush));
                L.Add(new Layer(Ellipse(side * 1.3f, 0f, 0.28f, 0.19f), new Color(1f, 0.38f, 0.42f, 0.25f)));
            }
            return L;
        }

        static void Lashes(List<Layer> L, float cx, float side, float cy, float rx, float ry, bool shut)
        {
            for (var i = 0; i < 3; i++)
            {
                var angle = Mathf.Deg2Rad * (shut ? -15f - i * 22f : 30f + i * 24f);
                var dx = Mathf.Cos(angle) * side;
                var dy = Mathf.Sin(angle);
                var bx = cx + dx * rx * 0.95f;
                var by = cy + dy * ry * 0.95f;
                L.Add(new Layer(Stroke(Seg(bx, by, bx + dx * 0.2f, by + dy * 0.2f), 0.075f), Ink));
            }
        }

        static void Tears(List<Layer> L, float cx, float side)
        {
            L.Add(new Layer(RoundBox(cx + side * 0.04f, -0.52f, 0.08f, 0.3f, 0.08f), Tear));
            L.Add(new Layer(Circle(cx + side * 0.04f, -0.8f, 0.13f), Tear));
        }

        static void Bags(List<Layer> L, float cx) =>
            L.Add(new Layer(Stroke(Bez(cx - 0.26f, -0.42f, cx, -0.62f, cx + 0.26f, -0.42f), 0.06f), new Color(0.42f, 0.33f, 0.5f, 0.85f)));

        // ------------------------------------------------------------------ mouths (cell -1..1)

        static List<Layer> MouthLayers(string style, int frame)
        {
            var L = new List<Layer>();
            if (frame == 0) ClosedMouth(L, style);
            else OpenMouth(L, style, frame / 3f, frame);
            return L;
        }

        static void ClosedMouth(List<Layer> L, string style)
        {
            var smile = Stroke(Bez(-0.42f, 0.12f, 0f, -0.32f, 0.42f, 0.12f), 0.11f);
            switch (style)
            {
                case "smiley":
                    L.Add(new Layer(smile, Ink));
                    break;
                case "bean":
                    Bean(L, 0.36f, 0.2f);
                    break;
                case "cat":
                    Omega(L, 0.04f);
                    break;
                case "cheek":
                    L.Add(new Layer(Stroke(Bez(-0.36f, 0.06f, 0.02f, -0.3f, 0.36f, 0.16f), 0.11f), Ink));
                    L.Add(new Layer(Stroke(Bez(0.3f, 0.26f, 0.42f, 0.18f, 0.4f, 0.05f), 0.08f), Ink));
                    break;
                case "drool":
                    L.Add(new Layer(smile, Ink));
                    Drool(L, 0.3f, 0.02f);
                    break;
                case "lips":
                    Lips(L, 0.32f, 0f);
                    break;
                case "frown":
                    L.Add(new Layer(Stroke(Bez(-0.38f, -0.18f, 0f, 0.26f, 0.38f, -0.18f), 0.11f), Ink));
                    break;
                case "kissy":
                    L.Add(new Layer(Stroke(Bez(-0.02f, 0.2f, 0.2f, 0.18f, 0.02f, 0.02f), 0.09f), Ink));
                    L.Add(new Layer(Stroke(Bez(0.02f, 0.02f, 0.2f, -0.14f, -0.02f, -0.18f), 0.09f), Ink));
                    break;
                case "nonplussed":
                    L.Add(new Layer(Stroke(Seg(-0.3f, 0.02f, 0.3f, -0.03f), 0.1f), Ink));
                    break;
                case "o":
                    OpenOval(L, 0f, -0.02f, 0.14f, 0.17f, tongue: false);
                    break;
                case "squiggle":
                    L.Add(new Layer(Stroke(Wave(-0.42f, 0.42f, 0f, 0.08f, 2.5f), 0.09f), Ink));
                    break;
                case "triangle":
                    Triangle(L, 0.3f, 0.28f);
                    break;
                case "smirk":
                    L.Add(new Layer(Stroke(Bez(-0.36f, -0.02f, 0.12f, -0.08f, 0.38f, 0.18f), 0.11f), Ink));
                    break;
                case "cringe":
                    Grimace(L, 0.44f, 0.13f);
                    break;
                case "teeth":
                    DShape(L, 0.48f, 0.4f, teethTop: true, teethBottom: true, gap: 0.02f);
                    break;
                case "vamp":
                    L.Add(new Layer(Fang(-0.17f, -0.08f, 0.07f, 0.16f), White));
                    L.Add(new Layer(Fang(0.17f, -0.08f, 0.07f, 0.16f), White));
                    L.Add(new Layer(Stroke(Bez(-0.44f, 0.1f, 0f, -0.24f, 0.44f, 0.1f), 0.11f), Ink));
                    break;
                case "chomp":
                    OpenOval(L, 0f, -0.04f, 0.4f, 0.14f, tongue: false);
                    Zigzag(L, 0f, -0.04f, 0.4f, 0.14f, top: true, bottom: true, count: 4);
                    break;
                case "wacky":
                    L.Add(new Layer(Circle(-0.16f, -0.16f, 0.18f), Ink, (x, y) => y + 0.06f));
                    L.Add(new Layer(Circle(-0.16f, -0.16f, 0.13f), Tongue, (x, y) => y + 0.06f));
                    L.Add(new Layer(Stroke(Wave(-0.44f, 0.44f, 0.02f, 0.07f, 1.5f), 0.1f), Ink));
                    break;
                default: // "toothy": a wide grin full of little pointy teeth
                    OpenOval(L, 0f, -0.02f, 0.48f, 0.16f, tongue: false);
                    Zigzag(L, 0f, -0.02f, 0.48f, 0.16f, top: true, bottom: true, count: 7);
                    break;
            }
        }

        /// <summary>Talking shapes: each style opens its own way, wider with <paramref name="k"/> (1/3 to 1).</summary>
        static void OpenMouth(List<Layer> L, string style, float k, int frame)
        {
            float ry = Mathf.Lerp(0.12f, 0.42f, k), rx = Mathf.Lerp(0.34f, 0.44f, k);
            switch (style)
            {
                case "bean":
                    Bean(L, Mathf.Lerp(0.38f, 0.44f, k), Mathf.Lerp(0.26f, 0.48f, k));
                    break;
                case "cat":
                    OpenOval(L, 0f, -0.2f, rx * 0.6f, ry * 0.75f);
                    Omega(L, 0.12f);
                    break;
                case "cheek":
                    OpenOval(L, 0.04f, -0.04f, rx * 0.9f, ry);
                    L.Add(new Layer(Stroke(Bez(0.38f, 0.28f, 0.52f, 0.16f, 0.48f, 0.02f), 0.08f), Ink));
                    break;
                case "drool":
                    OpenOval(L, 0f, -0.02f, rx, ry);
                    Drool(L, rx * 0.8f, -0.02f - ry * 0.3f);
                    break;
                case "lips":
                    Lips(L, rx * 0.85f, ry);
                    break;
                case "frown":
                    // Open downward: flat along the bottom, round on top.
                    var frownIn = Max(Ellipse(0f, -0.2f, rx, ry * 1.4f), (x, y) => -0.2f - y);
                    L.Add(new Layer(Grow(frownIn, 0.065f), Ink));
                    L.Add(new Layer(frownIn, MouthInside));
                    break;
                case "kissy":
                case "o":
                    OpenOval(L, 0f, -0.02f, Mathf.Lerp(0.15f, 0.24f, k), Mathf.Lerp(0.18f, 0.34f, k), tongue: style == "o" && frame > 1);
                    break;
                case "nonplussed":
                    OpenOval(L, 0f, -0.02f, rx * 0.8f, ry * 0.45f, tongue: false);
                    break;
                case "squiggle":
                    OpenOval(L, 0f, -0.04f, rx * 0.7f, ry * 0.7f);
                    break;
                case "triangle":
                    Triangle(L, Mathf.Lerp(0.32f, 0.42f, k), Mathf.Lerp(0.34f, 0.62f, k));
                    break;
                case "smirk":
                    OpenOval(L, 0.1f, -0.02f, rx * 0.75f, ry * 0.8f);
                    break;
                case "cringe":
                    Grimace(L, Mathf.Lerp(0.44f, 0.5f, k), Mathf.Lerp(0.18f, 0.36f, k));
                    break;
                case "teeth":
                    DShape(L, 0.5f, Mathf.Lerp(0.44f, 0.62f, k), teethTop: true, teethBottom: true, gap: Mathf.Lerp(0.08f, 0.26f, k));
                    break;
                case "vamp":
                    OpenOval(L, 0f, -0.02f, rx, ry);
                    var vampIn = Ellipse(0f, -0.02f, rx, ry);
                    L.Add(new Layer(Fang(-0.15f, -0.02f + ry - 0.02f, 0.06f, 0.14f), White, vampIn));
                    L.Add(new Layer(Fang(0.15f, -0.02f + ry - 0.02f, 0.06f, 0.14f), White, vampIn));
                    break;
                case "chomp":
                    OpenOval(L, 0f, -0.04f, Mathf.Lerp(0.42f, 0.5f, k), Mathf.Lerp(0.22f, 0.5f, k), tongue: true);
                    Zigzag(L, 0f, -0.04f, Mathf.Lerp(0.42f, 0.5f, k), Mathf.Lerp(0.22f, 0.5f, k), top: true, bottom: true, count: 4);
                    break;
                case "wacky":
                    OpenOval(L, 0f, 0f, rx, ry * 0.8f, tongue: false);
                    var tongueY = -ry * 0.8f;
                    L.Add(new Layer(Circle(-0.16f, tongueY - 0.06f, 0.2f), Ink, (x, y) => y - tongueY + 0.04f));
                    L.Add(new Layer(Circle(-0.16f, tongueY - 0.06f, 0.15f), Tongue));
                    break;
                case "toothy":
                    OpenOval(L, 0f, -0.02f, Mathf.Lerp(0.48f, 0.52f, k), Mathf.Lerp(0.22f, 0.46f, k));
                    Zigzag(L, 0f, -0.02f, Mathf.Lerp(0.48f, 0.52f, k), Mathf.Lerp(0.22f, 0.46f, k), top: true, bottom: true, count: 7);
                    break;
                default: // "smiley"
                    OpenOval(L, 0f, -0.02f, rx, ry, teeth: frame > 1);
                    break;
            }
        }

        static void OpenOval(List<Layer> L, float cx, float cy, float rx, float ry, bool tongue = true, bool teeth = false)
        {
            var inside = Ellipse(cx, cy, rx, ry);
            L.Add(new Layer(Ellipse(cx, cy, rx + 0.065f, ry + 0.065f), Ink));
            L.Add(new Layer(inside, MouthInside));
            if (tongue) L.Add(new Layer(Circle(cx, cy - ry + rx * 0.3f, rx * 0.6f), Tongue, inside));
            if (teeth) L.Add(new Layer(RoundBox(cx, cy + ry - 0.05f, rx * 0.7f, 0.07f, 0.02f), White, inside));
        }

        /// <summary>A D-shaped open grin, flat on top, with rows of teeth and a dark gap between them.</summary>
        static void DShape(List<Layer> L, float rx, float depth, bool teethTop, bool teethBottom, float gap)
        {
            const float top = 0.22f;
            Sdf D(float grow) => (x, y) => Mathf.Max(Ellipse(0f, top, rx + grow, depth + grow)(x, y), y - top - grow);
            var inside = D(0f);
            L.Add(new Layer(D(0.065f), Ink));
            L.Add(new Layer(inside, MouthInside));
            var mid = top - depth * 0.45f;
            if (teethTop) L.Add(new Layer((x, y) => mid + gap * 0.5f - y, White, inside));
            if (teethBottom) L.Add(new Layer((x, y) => y - (mid - gap * 0.5f), White, inside));
            for (var i = -2; i <= 2; i++)
                L.Add(new Layer(Stroke(Seg(i * rx * 0.3f, top, i * rx * 0.3f, top - depth), 0.025f), new Color(0.6f, 0.55f, 0.65f), (x, y) =>
                    Mathf.Max(inside(x, y), Mathf.Abs(y - mid) - gap * 0.5f < 0f ? 1f : -1f)));
        }

        /// <summary>A bean-shaped open mouth: round, dipping in at the top.</summary>
        static void Bean(List<Layer> L, float rx, float ry)
        {
            Sdf bean = (x, y) => Mathf.Max(Ellipse(0f, 0f, rx, ry)(x, y), -Circle(0.08f, ry + rx * 0.62f, rx * 0.75f)(x, y));
            L.Add(new Layer(Grow(bean, 0.065f), Ink));
            L.Add(new Layer(bean, MouthInside));
            L.Add(new Layer(Circle(0.05f, -ry + rx * 0.3f, rx * 0.55f), Tongue, bean));
        }

        static void Omega(List<Layer> L, float lift)
        {
            L.Add(new Layer(Stroke(Bez(-0.4f, 0.06f + lift, -0.2f, -0.2f + lift, 0f, 0.04f + lift), 0.1f), Ink));
            L.Add(new Layer(Stroke(Bez(0f, 0.04f + lift, 0.2f, -0.2f + lift, 0.4f, 0.06f + lift), 0.1f), Ink));
        }

        static void Drool(List<Layer> L, float x, float y)
        {
            L.Add(new Layer(RoundBox(x, y - 0.18f, 0.045f, 0.16f, 0.045f), Tear));
            L.Add(new Layer(Circle(x, y - 0.36f, 0.08f), Tear));
        }

        static void Lips(List<Layer> L, float rx, float open)
        {
            var dark = Hex("#B32F4E");
            if (open > 0f)
            {
                var inside = Ellipse(0f, -0.04f, rx * 0.8f, open * 0.8f);
                L.Add(new Layer(Ellipse(0f, -0.04f, rx + 0.08f, open * 0.8f + 0.13f), Lipstick));
                L.Add(new Layer(inside, MouthInside));
                L.Add(new Layer(Circle(0f, -0.04f - open * 0.8f + rx * 0.3f, rx * 0.5f), Tongue, inside));
                return;
            }
            L.Add(new Layer(Ellipse(0f, -0.1f, rx, 0.14f), Lipstick));
            L.Add(new Layer(Circle(-rx * 0.4f, 0.04f, rx * 0.55f), Lipstick, (x, y) => -y - 0.02f));
            L.Add(new Layer(Circle(rx * 0.4f, 0.04f, rx * 0.55f), Lipstick, (x, y) => -y - 0.02f));
            L.Add(new Layer(Stroke(Bez(-rx, -0.02f, 0f, -0.08f, rx, -0.02f), 0.04f), dark));
        }

        static void Triangle(List<Layer> L, float hw, float h)
        {
            var tri = Poly3(-hw, 0.16f, hw, 0.16f, 0f, 0.16f - h);
            L.Add(new Layer(Grow(tri, 0.06f), Ink));
            L.Add(new Layer(tri, MouthInside));
            L.Add(new Layer(Circle(0f, 0.16f - h * 0.75f, hw * 0.4f), Tongue, tri));
        }

        /// <summary>A clenched grimace: a rounded box of teeth split by a line, opening into a gap.</summary>
        static void Grimace(List<Layer> L, float hw, float hh)
        {
            var box = RoundBox(0f, 0f, hw, hh, Mathf.Min(hh, 0.1f));
            L.Add(new Layer(Grow(box, 0.06f), Ink));
            L.Add(new Layer(box, White));
            var gap = Mathf.Max(0f, hh - 0.14f);
            if (gap > 0f) L.Add(new Layer((x, y) => Mathf.Abs(y) - gap, MouthInside, box));
            else L.Add(new Layer(Stroke(Seg(-hw, 0f, hw, 0f), 0.035f), Ink, box));
            for (var i = -2; i <= 2; i++)
                L.Add(new Layer(Stroke(Seg(i * hw * 0.33f, -hh, i * hw * 0.33f, hh), 0.035f), Ink, (x, y) => Mathf.Max(box(x, y), gap - Mathf.Abs(y))));
        }

        /// <summary>Rows of pointy teeth along the top and bottom of an oval mouth.</summary>
        static void Zigzag(List<Layer> L, float cx, float cy, float rx, float ry, bool top, bool bottom, int count)
        {
            var inside = Ellipse(cx, cy, rx, ry);
            var w = rx * 2f / count;
            var tooth = Mathf.Min(ry * 0.55f, w * 0.9f);
            for (var i = 0; i < count; i++)
            {
                var x = cx - rx + w * (i + 0.5f);
                if (top) L.Add(new Layer(Poly3(x - w * 0.5f, cy + ry, x + w * 0.5f, cy + ry, x, cy + ry - tooth - ry * 0.25f), White, inside));
                if (bottom) L.Add(new Layer(Poly3(x - w * 0.5f, cy - ry, x + w * 0.5f, cy - ry, x, cy - ry + tooth + ry * 0.25f), White, inside));
            }
        }

        static Sdf Fang(float x, float yTop, float hw, float h) => Poly3(x - hw, yTop, x + hw, yTop, x, yTop - h);

        // ------------------------------------------------------------------ brows (cell -3..3 by -1..1; eyes at x = ±EyeXInBrows)

        /// <summary>A brow's shape: half its length, thickness at the inner and outer ends, and how much it arches.</summary>
        readonly struct BrowShape
        {
            public readonly float HalfLength, Inner, Outer, Arch, PeakShift;

            public BrowShape(float halfLength, float inner, float outer, float arch, float peakShift = 0f)
            {
                HalfLength = halfLength;
                Inner = inner;
                Outer = outer;
                Arch = arch;
                PeakShift = peakShift;
            }
        }

        static BrowShape ShapeOf(string style) => style switch
        {
            "thin" => new BrowShape(0.6f, 0.15f, 0.12f, 0.14f),
            "soft" => new BrowShape(0.62f, 0.22f, 0.17f, 0.12f),
            "arched" => new BrowShape(0.6f, 0.15f, 0.11f, 0.32f, 0.18f),
            "straight" => new BrowShape(0.62f, 0.22f, 0.21f, 0f),
            "short" => new BrowShape(0.32f, 0.2f, 0.17f, 0.05f),
            "angled" => new BrowShape(0.62f, 0.17f, 0.13f, 0.26f, 0.22f),
            "tapered" => new BrowShape(0.66f, 0.3f, 0.07f, 0.14f),
            "thick" => new BrowShape(0.64f, 0.38f, 0.32f, 0.08f),
            "bushy" => new BrowShape(0.66f, 0.34f, 0.27f, 0.1f),
            "dot" => new BrowShape(0.09f, 0.34f, 0.34f, 0f),
            _ => new BrowShape(0.6f, 0.22f, 0.17f, 0.08f), // unibrow
        };

        /// <summary>How a mood moves the brows: the whole brow up, the inner and outer ends up, and extra arch.</summary>
        static (float up, float inner, float outer, float arch) MoodOf(Mood mood) => mood switch
        {
            Mood.Happy => (0.2f, -0.02f, 0f, 0.12f),
            Mood.Worried => (0.12f, 0.38f, -0.12f, -0.1f),
            Mood.Angry => (-0.12f, -0.38f, 0.14f, -0.08f),
            Mood.Surprised => (0.5f, 0.05f, 0f, 0.18f),
            Mood.Focused => (-0.1f, -0.14f, 0.02f, -0.06f),
            _ => (0f, 0f, 0f, 0f),
        };

        static List<Layer> BrowLayers(string style, Mood mood)
        {
            var L = new List<Layer>();
            var shape = ShapeOf(style);
            var (up, innerUp, outerUp, archUp) = MoodOf(mood);
            // Short brows tilt as much as long ones, not more.
            var tilt = Mathf.Min(1f, shape.HalfLength / 0.6f);
            var arch = Mathf.Max(0f, shape.Arch + archUp * Mathf.Min(1f, shape.HalfLength / 0.3f));
            var inners = new Vector2[2];
            for (var i = 0; i < 2; i++)
            {
                var side = i == 0 ? -1f : 1f;
                var cx = side * FaceArt.EyeXInBrows;
                var y = FaceArt.BrowRestY + up;
                var outer = new Vector2(cx + side * shape.HalfLength, y + outerUp * tilt);
                var inner = new Vector2(cx - side * shape.HalfLength, y + innerUp * tilt);
                inners[i] = inner;
                var mid = (outer + inner) * 0.5f;
                Vector2[] path;
                if (style == "angled")
                {
                    // A sharp peak two thirds of the way out.
                    var peak = Vector2.Lerp(inner, outer, 0.62f) + Vector2.up * arch;
                    path = new[] { inner, peak, outer };
                }
                else
                {
                    var control = mid + new Vector2(side * shape.PeakShift, 2f * arch);
                    path = Bez(inner.x, inner.y, control.x, control.y, outer.x, outer.y);
                }
                L.Add(new Layer(Taper(path, shape.Inner, shape.Outer), Ink));
                if (style == "bushy")
                {
                    // Tufts along the top and a flick at the outer end.
                    for (var k = 1; k <= 5; k++)
                    {
                        var q = path[Mathf.RoundToInt(k / 6f * (path.Length - 1))];
                        var r = Mathf.Lerp(shape.Inner, shape.Outer, k / 6f) * 0.42f;
                        L.Add(new Layer(Circle(q.x + side * 0.03f, q.y + r * 0.55f, r), Ink));
                    }
                    L.Add(new Layer(Taper(new[] { outer, outer + new Vector2(side * 0.12f, 0.1f) }, 0.12f, 0.03f), Ink));
                }
            }
            if (style == "unibrow")
            {
                // The two meet over the nose, dipping a little.
                var a = inners[0];
                var b = inners[1];
                var dip = (a + b) * 0.5f - Vector2.up * 0.05f;
                L.Add(new Layer(Taper(Bez(a.x, a.y, dip.x, dip.y, b.x, b.y), 0.17f, 0.17f), Ink));
            }
            return L;
        }

        // ------------------------------------------------------------------ distance functions (negative inside)

        static Sdf Circle(float cx, float cy, float r) => (x, y) => Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy)) - r;

        static Sdf Ring(float cx, float cy, float r, float thickness) => (x, y) => Mathf.Abs(Circle(cx, cy, r)(x, y)) - thickness * 0.5f;

        /// <summary>An approximate ellipse distance: exact on the axes, good enough for soft cartoon edges.</summary>
        static Sdf Ellipse(float cx, float cy, float rx, float ry) => (x, y) =>
        {
            float dx = (x - cx) / rx, dy = (y - cy) / ry;
            var k = Mathf.Sqrt(dx * dx + dy * dy);
            if (k < 1e-5f) return -Mathf.Min(rx, ry);
            var grad = Mathf.Sqrt(dx * dx / (rx * rx) + dy * dy / (ry * ry));
            return (k - 1f) * k / Mathf.Max(grad, 1e-5f);
        };

        /// <summary>An almond/lens shape (two circle arcs) of half-width hw and half-height hh.</summary>
        static Sdf Lens(float cx, float cy, float hw, float hh)
        {
            var r = (hw * hw + hh * hh) / (2f * hh);
            var off = r - hh;
            return Max(Circle(cx, cy - off, r), Circle(cx, cy + off, r));
        }

        /// <summary>A pointed leaf from (ax, ay) to (bx, by) of half-width w.</summary>
        static Sdf Leaf(float ax, float ay, float bx, float by, float w)
        {
            var len = Vector2.Distance(new Vector2(ax, ay), new Vector2(bx, by));
            var mid = new Vector2((ax + bx) * 0.5f, (ay + by) * 0.5f);
            var dir = new Vector2(bx - ax, by - ay) / len;
            var lens = Lens(0f, 0f, len * 0.5f, w);
            return (x, y) =>
            {
                var p = new Vector2(x, y) - mid;
                return lens(Vector2.Dot(p, dir), Vector2.Dot(p, new Vector2(-dir.y, dir.x)));
            };
        }

        static Sdf RoundBox(float cx, float cy, float hx, float hy, float r) => (x, y) =>
        {
            float qx = Mathf.Abs(x - cx) - hx + r, qy = Mathf.Abs(y - cy) - hy + r;
            return Mathf.Sqrt(Mathf.Max(qx, 0f) * Mathf.Max(qx, 0f) + Mathf.Max(qy, 0f) * Mathf.Max(qy, 0f)) + Mathf.Min(Mathf.Max(qx, qy), 0f) - r;
        };

        /// <summary>A triangle (points in any order).</summary>
        static Sdf Poly3(float ax, float ay, float bx, float by, float cx, float cy)
        {
            var p = new[] { new Vector2(ax, ay), new Vector2(bx, by), new Vector2(cx, cy) };
            return (x, y) =>
            {
                var q = new Vector2(x, y);
                var d = float.MaxValue;
                var inside = true;
                var sign = 0f;
                for (var i = 0; i < 3; i++)
                {
                    var a = p[i];
                    var e = p[(i + 1) % 3] - a;
                    var t = Mathf.Clamp01(Vector2.Dot(q - a, e) / e.sqrMagnitude);
                    d = Mathf.Min(d, (q - (a + e * t)).sqrMagnitude);
                    var cross = e.x * (q.y - a.y) - e.y * (q.x - a.x);
                    if (i == 0) sign = Mathf.Sign(cross);
                    else if (Mathf.Sign(cross) != sign) inside = false;
                }
                return Mathf.Sqrt(d) * (inside ? -1f : 1f);
            };
        }

        /// <summary>A four-pointed sparkle.</summary>
        static Sdf Sparkle(float cx, float cy, float r) => (x, y) =>
        {
            // A superellipse with exponent 1/2: concave sides, four sharp points at distance r.
            float dx = Mathf.Abs(x - cx), dy = Mathf.Abs(y - cy);
            var s = Mathf.Sqrt(dx) + Mathf.Sqrt(dy);
            return (s * s - r) * 0.5f;
        };

        static Sdf Grow(Sdf s, float by) => (x, y) => s(x, y) - by;

        static Sdf Max(Sdf a, Sdf b) => (x, y) => Mathf.Max(a(x, y), b(x, y));

        static Vector2[] Seg(float ax, float ay, float bx, float by) => new[] { new Vector2(ax, ay), new Vector2(bx, by) };

        static Vector2[] Poly(params float[] xy)
        {
            var pts = new Vector2[xy.Length / 2];
            for (var i = 0; i < pts.Length; i++) pts[i] = new Vector2(xy[i * 2], xy[i * 2 + 1]);
            return pts;
        }

        static Vector2[] Bez(float ax, float ay, float cx, float cy, float bx, float by)
        {
            const int n = 24;
            var pts = new Vector2[n + 1];
            for (var i = 0; i <= n; i++)
            {
                var t = i / (float)n;
                var u = 1f - t;
                pts[i] = new Vector2(u * u * ax + 2f * u * t * cx + t * t * bx, u * u * ay + 2f * u * t * cy + t * t * by);
            }
            return pts;
        }

        /// <summary>A sine wave from x0 to x1 around height y.</summary>
        static Vector2[] Wave(float x0, float x1, float y, float amplitude, float waves)
        {
            const int n = 40;
            var pts = new Vector2[n + 1];
            for (var i = 0; i <= n; i++)
            {
                var t = i / (float)n;
                pts[i] = new Vector2(Mathf.Lerp(x0, x1, t), y + amplitude * Mathf.Sin(t * waves * Mathf.PI * 2f));
            }
            return pts;
        }

        static Sdf Stroke(Vector2[] pts, float thickness) => (x, y) =>
        {
            var p = new Vector2(x, y);
            var best = float.MaxValue;
            for (var i = 0; i < pts.Length - 1; i++)
            {
                var a = pts[i];
                var ab = pts[i + 1] - a;
                var t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / Mathf.Max(ab.sqrMagnitude, 1e-8f));
                best = Mathf.Min(best, (p - (a + ab * t)).sqrMagnitude);
            }
            return Mathf.Sqrt(best) - thickness * 0.5f;
        };

        /// <summary>A stroke along a path whose thickness goes from <paramref name="start"/> to <paramref name="end"/>, with round ends.</summary>
        static Sdf Taper(Vector2[] pts, float start, float end)
        {
            var lengths = new float[pts.Length];
            for (var i = 1; i < pts.Length; i++) lengths[i] = lengths[i - 1] + Vector2.Distance(pts[i - 1], pts[i]);
            var total = Mathf.Max(lengths[pts.Length - 1], 1e-6f);
            return (x, y) =>
            {
                var p = new Vector2(x, y);
                var best = float.MaxValue;
                for (var i = 0; i < pts.Length - 1; i++)
                {
                    var a = pts[i];
                    var ab = pts[i + 1] - a;
                    var t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / Mathf.Max(ab.sqrMagnitude, 1e-8f));
                    var along = (lengths[i] + (lengths[i + 1] - lengths[i]) * t) / total;
                    var d = (p - (a + ab * t)).magnitude - Mathf.Lerp(start, end, along) * 0.5f;
                    best = Mathf.Min(best, d);
                }
                return best;
            };
        }

        static Color Hex(string hex) => ColorUtility.TryParseHtmlString(hex, out var c) ? c : Color.magenta;
    }
}
