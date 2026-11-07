using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using static Hindsight.Ophthalmology.EditorTools.BuilderUtil;
using static Hindsight.Ophthalmology.EditorTools.CeilingArt;

namespace Hindsight.Ophthalmology.EditorTools
{
    /// <summary>
    /// Paints the wall chart Dr. Iris uses to explain the drops, in three picture panels with arrows
    /// between them and no words: (1) a drop goes into an eye with a small pupil, (2) the pupil is now
    /// big, (3) a light shines through the big pupil and shows the inside of the eye. Same ink-outlined
    /// style as the ceiling picture. <see cref="PanelRect"/> gives each panel's place in the texture.
    /// </summary>
    internal static class ChartArt
    {
        const string TexturePath = MaterialsDir + "/DropsChart.png";
        public const int Width = 2048;
        public const int Height = 640;
        const int PanelSize = 560;
        const int Margin = 40;
        const int Gap = 144;

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

            for (var i = 0; i < 3; i++)
            {
                var r = PanelRect(i);
                var c = new Vector2(r.x + r.width / 2f, r.y + r.height / 2f);
                Layer(p => RoundBox(p, c, r.width / 2f - 6f, r.height / 2f - 6f, 40f), Card, 8f);
                // A number badge in the corner, so the order reads without words.
                var badge = c + new Vector2(-r.width / 2f + 62f, r.height / 2f - 62f);
                Layer(p => Circle(p, badge, 36f), UiKit.Hex(new[] { "#4A90E2", "#FF9F43", "#5CBF73" }[i]), 6f);
                for (var dots = 0; dots <= i; dots++)
                {
                    var off = new Vector2((dots - i * 0.5f) * 22f, 0f);
                    Layer(p => Circle(p, badge + off, 8f), Color.white, 0f);
                }
            }

            // Arrows in the gaps between the panels.
            for (var i = 0; i < 2; i++)
            {
                var x0 = PanelRect(i).xMax + 22f;
                var x1 = PanelRect(i + 1).x - 22f;
                var y = Height / 2f;
                Layer(p => Mathf.Min(Segment(p, new Vector2(x0, y), new Vector2(x1 - 6f, y)),
                    Mathf.Min(Segment(p, new Vector2(x1 - 44f, y + 40f), new Vector2(x1, y)), Segment(p, new Vector2(x1 - 44f, y - 40f), new Vector2(x1, y)))) - 12f,
                    UiKit.Hex("#FFC845"), 6f);
            }

            // Panel 1: a drop falls from the bottle into an eye with a small pupil.
            var c1 = Centre(0) + new Vector2(0, -70);
            Eye(Layer, c1, 22f, 0.85f);
            var tip = c1 + new Vector2(70, 250);
            Layer(p => Mathf.Min(RoundBox(p, tip + new Vector2(55, 75), 48, 70, 18), Segment(p, tip + new Vector2(30, 10), tip + new Vector2(55, 25)) - 18), UiKit.Hex("#E3F4FF"));
            Layer(p => RoundBox(p, tip + new Vector2(55, 150), 30, 14, 6), UiKit.Hex("#FF6B6B"));
            Layer(p => Drop(p, c1 + new Vector2(40, 140), 26f), UiKit.Hex("#4AB3F5"));

            // Panel 2: the same eye, the pupil big now, with "growing" marks around it.
            var c2 = Centre(1) + new Vector2(0, -20);
            Eye(Layer, c2, 66f, 0.85f);
            for (var k = 0; k < 4; k++)
            {
                var a = Mathf.PI / 4f + k * Mathf.PI / 2f;
                var dir = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
                Layer(p => Segment(p, c2 + dir * 150f, c2 + dir * 200f) - 9f, UiKit.Hex("#FF9F43"), 5f);
            }

            // Panel 3: a light shines through the big pupil and shows the inside: an orange back of the eye with blood vessels.
            var c3 = Centre(2) + new Vector2(-30, -40);
            var lamp = c3 + new Vector2(190, 200);
            Layer(p => Mathf.Max(Triangle(p, lamp, c3 + new Vector2(-30, 55), c3 + new Vector2(40, -45)), -1f), new Color(1f, 0.93f, 0.55f), 0f);
            Eye(Layer, c3, 70f, 0.85f, inside: true);
            Layer(p => RoundBox(p, lamp + new Vector2(30, 30), 26, 60, 12), UiKit.Hex("#3D4A6B"));
            Layer(p => Circle(p, lamp, 22f), UiKit.Hex("#FFE27A"), 5f);

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
            layer(Almond, Color.white, 8f);
            var iris = 88f * scale;
            layer(p => Mathf.Max(Circle(p, c, iris), Almond(p)), UiKit.Hex("#4A90E2"), 5f);
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
            var circle = Circle(p, c, r);
            var cone = Mathf.Max(Mathf.Abs(p.x - c.x) - r * (1f - (p.y - c.y) / (r * 2.2f)), Mathf.Max(c.y - p.y, p.y - c.y - r * 2.2f));
            return Mathf.Min(circle, cone);
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
