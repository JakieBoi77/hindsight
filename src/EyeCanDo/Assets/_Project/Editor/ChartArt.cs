using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using static Hindsight.Ophthalmology.EditorTools.BuilderUtil;
using static Hindsight.Ophthalmology.EditorTools.CeilingArt;

namespace Hindsight.Ophthalmology.EditorTools
{
    /// <summary>
    /// Paints the wall chart Dr. Iris uses to explain the drops: two big picture panels with an arrow
    /// between them and no words. (1) A drop falls from the bottle into an eye with a small pupil;
    /// (2) the same eye, its pupil now big. Same ink-outlined style as the ceiling picture.
    /// <see cref="PanelRect"/> gives each panel's place in the texture.
    /// </summary>
    internal static class ChartArt
    {
        const string TexturePath = MaterialsDir + "/DropsChart.png";
        public const int Width = 2048;
        public const int Height = 1024;
        public const int PanelCount = 2;
        const int PanelSize = 860;
        const int Margin = 82;
        const int Gap = 2048 - 2 * 82 - 2 * 860; // 164: room for the arrow

        static readonly Color Board = UiKit.Hex("#FFF6E0");
        static readonly Color Card = Color.white;
        static readonly Color Ink = UiKit.Hex("#2E3A59");

        /// <summary>Panel <paramref name="i"/> in texture pixels (x, y from the bottom-left, size).</summary>
        public static RectInt PanelRect(int i) => new(Margin + i * (PanelSize + Gap), Margin, PanelSize, PanelSize);

        public static Texture2D Texture()
        {
            var px = new Color[Width * Height];
            for (var i = 0; i < px.Length; i++) px[i] = Board;

            void Layer(Func<Vector2, float> sdf, Color fill, float outline = 7f)
            {
                for (var y = 0; y < Height; y++)
                for (var x = 0; x < Width; x++)
                {
                    var d = sdf(new Vector2(x + 0.5f, y + 0.5f));
                    if (d > outline + 1f) continue;
                    var i = y * Width + x;
                    px[i] = Color.Lerp(px[i], Ink, Mathf.Clamp01(outline + 0.5f - d));
                    px[i] = Color.Lerp(px[i], fill, Mathf.Clamp01(0.5f - d));
                }
            }

            for (var i = 0; i < PanelCount; i++)
            {
                var r = PanelRect(i);
                var c = new Vector2(r.x + r.width / 2f, r.y + r.height / 2f);
                Layer(p => RoundBox(p, c, r.width / 2f - 8f, r.height / 2f - 8f, 56f), Card, 11f);
                // A number badge in the corner, so the order reads without words.
                var badge = c + new Vector2(-r.width / 2f + 90f, r.height / 2f - 90f);
                Layer(p => Circle(p, badge, 54f), UiKit.Hex(new[] { "#4A90E2", "#FF9F43" }[i]), 8f);
                for (var dots = 0; dots <= i; dots++)
                {
                    var off = new Vector2((dots - i * 0.5f) * 34f, 0f);
                    Layer(p => Circle(p, badge + off, 12f), Color.white, 0f);
                }
            }

            // A fat arrow from the first panel to the second.
            {
                var x0 = PanelRect(0).xMax + 30f;
                var x1 = PanelRect(1).x - 30f;
                var y = Height / 2f;
                Layer(p => Mathf.Min(Segment(p, new Vector2(x0, y), new Vector2(x1 - 10f, y)),
                    Mathf.Min(Segment(p, new Vector2(x1 - 44f, y + 44f), new Vector2(x1, y)), Segment(p, new Vector2(x1 - 44f, y - 44f), new Vector2(x1, y)))) - 15f,
                    UiKit.Hex("#FFC845"), 8f);
            }

            // Panel 1: the bottle, tip down, lets a big drop fall into an eye with a small pupil.
            var c1 = Centre(0);
            var eye1 = c1 + new Vector2(0, -150);
            Eye(Layer, eye1, 26f, 1.45f);
            var bottle = c1 + new Vector2(0, 225);
            Layer(p => RoundBox(p, bottle + new Vector2(0, 52), 74, 80, 26), UiKit.Hex("#E3F4FF"), 10f);       // body
            Layer(p => RoundBox(p, bottle + new Vector2(0, 52), 74, 26, 4), UiKit.Hex("#FF6B6B"), 0f);          // label
            Layer(p => RoundBox(p, bottle + new Vector2(0, 150), 46, 22, 8), UiKit.Hex("#FF6B6B"), 10f);       // cap
            Layer(p => Triangle(p, bottle + new Vector2(-30, -30), bottle + new Vector2(30, -30), bottle + new Vector2(0, -78)), UiKit.Hex("#E3F4FF"), 10f); // nozzle
            Layer(p => Drop(p, c1 + new Vector2(0, 50), 36f), UiKit.Hex("#4AB3F5"), 10f);
            // Little motion lines beside the falling drop.
            foreach (var side in new[] { -1f, 1f })
                Layer(p => Segment(p, c1 + new Vector2(side * 70, 125), c1 + new Vector2(side * 70, 70)) - 7f, UiKit.Hex("#4AB3F5"), 0f);

            // Panel 2: the same eye, its pupil big now, with "growing" marks around it.
            var c2 = Centre(1) + new Vector2(0, -30);
            Eye(Layer, c2, 74f, 1.45f);
            for (var k = 0; k < 4; k++)
            {
                var a = Mathf.PI / 4f + k * Mathf.PI / 2f;
                var dir = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
                Layer(p => Segment(p, c2 + dir * 270f, c2 + dir * 345f) - 14f, UiKit.Hex("#FF9F43"), 7f);
            }

            var tex = new Texture2D(Width, Height, TextureFormat.RGBA32, true);
            tex.SetPixels(px);
            File.WriteAllBytes(TexturePath, tex.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(TexturePath, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(TexturePath);
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.mipmapEnabled = true;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath);
        }

        static Vector2 Centre(int i)
        {
            var r = PanelRect(i);
            return new Vector2(r.x + r.width / 2f, r.y + r.height / 2f);
        }

        /// <summary>An almond-shaped eye: white, blue iris, black pupil, a shine. With <paramref name="inside"/> the pupil shows the back of the eye.</summary>
        static void Eye(Action<Func<Vector2, float>, Color, float> layer, Vector2 c, float pupil, float scale, bool inside = false)
        {
            // Two overlapping circles make the almond: half-width 200, half-height 110 (times scale).
            float hw = 200f * scale, hh = 110f * scale;
            var big = (hw * hw / hh + hh) / 2f;
            var k = big - hh;
            float Almond(Vector2 p) => Mathf.Max(Circle(p, c + new Vector2(0, -k), big), Circle(p, c + new Vector2(0, k), big));
            layer(Almond, Color.white, 11f);
            var iris = 88f * scale;
            layer(p => Mathf.Max(Circle(p, c, iris), Almond(p)), UiKit.Hex("#4A90E2"), 7f);
            var pr = pupil * scale;
            if (inside)
            {
                layer(p => Circle(p, c, pr), UiKit.Hex("#E8743B"), 4f);
                // Blood vessels from the optic disc, and the disc itself.
                var disc = c + new Vector2(-pr * 0.3f, pr * 0.1f);
                foreach (var a in new[] { 0.5f, 1.2f, 2.3f, 3.6f, 4.6f, 5.6f })
                {
                    var end = disc + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * pr * 0.95f;
                    layer(p => Mathf.Max(Segment(p, disc, end) - 3.5f, Circle(p, c, pr - 4f)), UiKit.Hex("#B3261E"), 0f);
                }
                layer(p => Circle(p, disc, pr * 0.2f), UiKit.Hex("#FFD9A0"), 0f);
            }
            else
            {
                layer(p => Circle(p, c, pr), UiKit.Hex("#141414"), 0f);
            }
            layer(p => Circle(p, c + new Vector2(iris * 0.35f, iris * 0.38f), 14f * scale), Color.white, 0f);
        }

        static float RoundBox(Vector2 p, Vector2 c, float hw, float hh, float r) => Box(p, c, hw - r, hh - r) - r;

        /// <summary>A water drop: a circle with a point on top.</summary>
        static float Drop(Vector2 p, Vector2 c, float r)
        {
            // The point's sides touch the circle exactly, so the outline runs smoothly into it.
            var apex = c + new Vector2(0f, r * 2.2f);
            var k = r / (r * 2.2f);
            var side = Mathf.Sqrt(1f - k * k);
            var cone = Triangle(p, apex, c + new Vector2(-r * side, r * k), c + new Vector2(r * side, r * k));
            return Mathf.Min(Circle(p, c, r), cone);
        }

        /// <summary>Signed distance to a triangle: the distance to its nearest edge, negative inside.</summary>
        static float Triangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
        {
            var d = Mathf.Min(Segment(p, a, b), Mathf.Min(Segment(p, b, c), Segment(p, c, a)));
            float Side(Vector2 u, Vector2 v) => (v.x - u.x) * (p.y - u.y) - (v.y - u.y) * (p.x - u.x);
            float s1 = Side(a, b), s2 = Side(b, c), s3 = Side(c, a);
            var inside = (s1 >= 0 && s2 >= 0 && s3 >= 0) || (s1 <= 0 && s2 <= 0 && s3 <= 0);
            return inside ? -d : d;
        }
    }
}
