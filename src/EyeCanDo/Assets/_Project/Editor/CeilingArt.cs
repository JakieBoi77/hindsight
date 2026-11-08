using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using static Hindsight.EyeCanDo.EditorTools.BuilderUtil;

namespace Hindsight.EyeCanDo.EditorTools
{
    /// <summary>
    /// Paints the ceiling picture the child looks at while the drop goes in: a smiley sun with rays, a
    /// rainbow and two clouds, in the UI's ink-outlined style. Drawn from signed distances with
    /// anti-aliased edges onto the ceiling's own colour, so it reads as painted on the ceiling.
    /// </summary>
    internal static class CeilingArt
    {
        const string TexturePath = MaterialsDir + "/CeilingSun.png";
        const string MaterialPath = MaterialsDir + "/CeilingSun.mat";
        const int Size = 1024;

        static readonly Color Ceiling = new(0.97f, 0.97f, 0.97f);
        static readonly Color Ink = UiKit.Hex("#2E3A59");

        public static Material Material()
        {
            var tex = Paint();
            var mat = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            if (mat == null)
            {
                mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                AssetDatabase.CreateAsset(mat, MaterialPath);
            }
            mat.SetTexture("_BaseMap", tex);
            mat.SetColor("_BaseColor", Color.white);
            mat.SetFloat("_Smoothness", 0.1f);
            // A little glow so it stays cheerful in the ceiling's shade.
            mat.EnableKeyword("_EMISSION");
            mat.SetTexture("_EmissionMap", tex);
            mat.SetColor("_EmissionColor", Color.white * 0.35f);
            EditorUtility.SetDirty(mat);
            return mat;
        }

        static Texture2D Paint()
        {
            var px = new Color[Size * Size];
            for (var i = 0; i < px.Length; i++) px[i] = Ceiling;

            void Layer(Func<Vector2, float> sdf, Color fill, float outline = 7f)
            {
                for (var y = 0; y < Size; y++)
                for (var x = 0; x < Size; x++)
                {
                    var d = sdf(new Vector2(x + 0.5f, y + 0.5f));
                    if (d > outline + 1f) continue;
                    var i = y * Size + x;
                    var edge = Mathf.Clamp01(outline + 0.5f - d); // ink ring + inside
                    var inner = Mathf.Clamp01(0.5f - d);          // fill inside
                    px[i] = Color.Lerp(px[i], Ink, edge);
                    px[i] = Color.Lerp(px[i], fill, inner);
                }
            }

            // Rainbow: bands of an annulus, top half only.
            var rc = new Vector2(512, 250);
            var bands = new[] { "#FF6B6B", "#FF9F43", "#FFD34D", "#5CBF73", "#4A90E2", "#9B6BE0" };
            const float outer = 470f, width = 30f;
            Layer(p => Mathf.Max(Mathf.Abs((p - rc).magnitude - (outer - width * 3)) - width * 3, rc.y - p.y), Color.white);
            for (var i = 0; i < bands.Length; i++)
            {
                var r = outer - width * (i + 0.5f);
                Layer(p => Mathf.Max(Mathf.Abs((p - rc).magnitude - r) - width * 0.5f, rc.y - p.y), UiKit.Hex(bands[i]), 0f);
            }

            // Clouds at the rainbow's feet.
            foreach (var cx in new[] { 155f, 869f })
            {
                var c = new Vector2(cx, 250);
                Layer(p => Mathf.Min(Mathf.Min(Circle(p, c + new Vector2(-70, -10), 70), Circle(p, c + new Vector2(70, -10), 70)),
                    Mathf.Min(Circle(p, c + new Vector2(0, 30), 90), Box(p, c + new Vector2(0, -45), 140, 45))), UiKit.Hex("#E3F4FF"));
            }

            // Sun: rays, disc, face.
            var sc = new Vector2(512, 600);
            for (var i = 0; i < 12; i++)
            {
                var a = i * Mathf.PI / 6f;
                var dir = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
                Layer(p => Segment(p, sc + dir * 200, sc + dir * 285) - 24, UiKit.Hex("#FF9F43"));
            }
            Layer(p => Circle(p, sc, 175), UiKit.Hex("#FFC845"), 9f);
            Layer(p => Circle(p, sc + new Vector2(-62, 30), 22), Ink, 0f);
            Layer(p => Circle(p, sc + new Vector2(62, 30), 22), Ink, 0f);
            Layer(p => Circle(p, sc + new Vector2(-55, 40), 7), Color.white, 0f);
            Layer(p => Circle(p, sc + new Vector2(69, 40), 7), Color.white, 0f);
            Layer(p => Circle(p, sc + new Vector2(-105, -25), 28), UiKit.Hex("#FF9BC2"), 0f);
            Layer(p => Circle(p, sc + new Vector2(105, -25), 28), UiKit.Hex("#FF9BC2"), 0f);
            // Smile: the lower half of a ring.
            Layer(p => Mathf.Max(Mathf.Abs((p - (sc + new Vector2(0, -10))).magnitude - 70) - 9, p.y - (sc.y - 10)), Ink, 0f);

            var tex = new Texture2D(Size, Size, TextureFormat.RGBA32, true);
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

        internal static float Circle(Vector2 p, Vector2 c, float r) => (p - c).magnitude - r;

        internal static float Box(Vector2 p, Vector2 c, float hw, float hh)
        {
            var q = new Vector2(Mathf.Abs(p.x - c.x) - hw, Mathf.Abs(p.y - c.y) - hh);
            return Vector2.Max(q, Vector2.zero).magnitude + Mathf.Min(Mathf.Max(q.x, q.y), 0f);
        }

        internal static float Segment(Vector2 p, Vector2 a, Vector2 b)
        {
            var pa = p - a;
            var ba = b - a;
            var h = Mathf.Clamp01(Vector2.Dot(pa, ba) / Vector2.Dot(ba, ba));
            return (pa - ba * h).magnitude;
        }
    }
}
