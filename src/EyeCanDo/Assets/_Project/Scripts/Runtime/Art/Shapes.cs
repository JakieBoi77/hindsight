using System;
using System.Collections.Generic;
using UnityEngine;

namespace EyeCanDo.Art
{
    /// <summary>
    /// Simple shapes drawn in code: rounded panels, circles, rings, glows and confetti. Characters,
    /// props and icons are SVG art (see ArtLibrary). Shapes are anti-aliased signed-distance
    /// drawings, generated once and cached. They are white so an Image can tint them.
    /// </summary>
    public static class Shapes
    {
        public const int SliceRadius = 64;

        private static readonly Dictionary<string, Sprite> Cache = new Dictionary<string, Sprite>();

        /// <summary>A 9-sliced rounded rectangle. Set Image.pixelsPerUnitMultiplier to choose the radius.</summary>
        public static Sprite RoundedRect => Get("rounded", () =>
            Draw(SliceRadius * 2 + 4, SliceRadius * 2 + 4, (x, y, w, h) => RoundedBox(x, y, w, h, SliceRadius), new Vector4(SliceRadius, SliceRadius, SliceRadius, SliceRadius)));

        public static Sprite Circle => Get("circle", () => Draw(512, 512, (x, y, w, h) => Length(x - w / 2f, y - h / 2f) - 254f));

        public static Sprite Ring => Get("ring", () => Draw(512, 512, (x, y, w, h) => Math.Abs(Length(x - w / 2f, y - h / 2f) - 226f) - 28f));

        /// <summary>A soft round glow that fades to nothing at the edge, for light and glare.</summary>
        public static Sprite Glow => Get("glow", () => Draw(256, 256, (x, y, w, h) =>
        {
            var t = Mathf.Clamp01(Length(x - w / 2f, y - h / 2f) / 127f);
            // Draw() maps distance d to alpha 0.5 - d; this gives a smooth falloff instead of an edge.
            return 0.5f - (1f - t) * (1f - t);
        }));

        /// <summary>A teardrop pointing up, like a drop about to fall.</summary>
        public static Sprite Drop => Get("drop", () => Draw(192, 256, (x, y, w, h) =>
        {
            var cx = w / 2f;
            var circle = Length(x - cx, y - 92f) - 86f;
            // Cone from the circle up to the tip.
            var tipY = 250f;
            var t = Mathf.Clamp01((y - 92f) / (tipY - 92f));
            var halfWidth = Mathf.Lerp(80f, 2f, t);
            var cone = y < 92f ? 999f : Math.Max(Math.Abs(x - cx) - halfWidth, y - tipY);
            return Math.Min(circle, cone);
        }));

        /// <summary>A triangle pointing down, used for speech bubble tails.</summary>
        public static Sprite Tail => Get("tail", () => Draw(128, 96, (x, y, w, h) =>
        {
            var t = y / h;
            var half = Mathf.Lerp(0f, 64f, t);
            return Math.Abs(x - w / 2f) - half;
        }));

        public static Sprite Ellipse => Circle;

        /// <summary>A five-point star.</summary>
        public static Sprite Star => Get("star", () => Draw(256, 256, (x, y, w, h) => StarDistance(x - w / 2f, y - h / 2f - 6f, 118f, 52f)));

        /// <summary>A house, for the Home button.</summary>
        public static Sprite House => Get("house", () => Draw(256, 256, (x, y, w, h) =>
        {
            var body = Box(x - 128f, y - 86f, 72f, 64f);
            var t = Mathf.Clamp01((y - 136f) / 100f);
            var roofHalf = Mathf.Lerp(116f, 0f, t);
            var roof = y < 136f || y > 236f ? 999f : Math.Abs(x - 128f) - roofHalf;
            var door = Box(x - 128f, y - 52f, 22f, 32f);
            return Math.Max(Math.Min(body, roof), -door);
        }));

        public static Sprite Snowflake => Get("snowflake", () => Draw(256, 256, (x, y, w, h) =>
        {
            var px = x - 128f;
            var py = y - 128f;
            var d = 999f;
            for (var i = 0; i < 3; i++)
            {
                var a = i * Mathf.PI / 3f;
                var rx = px * Mathf.Cos(a) + py * Mathf.Sin(a);
                var ry = -px * Mathf.Sin(a) + py * Mathf.Cos(a);
                d = Math.Min(d, Box(rx, ry, 110f, 14f));
                // Small branches near each end.
                foreach (var end in new[] { -70f, 70f })
                {
                    var bx = rx - end;
                    d = Math.Min(d, Math.Max(Math.Abs(Math.Abs(bx) - Math.Abs(ry) * 0.6f) - 10f, Math.Abs(ry) - 34f));
                }
            }

            return d;
        }));

        public static Sprite Clock => Get("clock", () => Draw(256, 256, (x, y, w, h) =>
        {
            var px = x - 128f;
            var py = y - 128f;
            var ring = Math.Abs(Length(px, py) - 104f) - 16f;
            var hourHand = Box(px, py - 34f, 12f, 46f);
            var minuteHand = Box(px - 30f, py, 42f, 12f);
            return Math.Min(ring, Math.Min(hourHand, minuteHand));
        }));

        public static Sprite Sun => Get("sun", () => Draw(256, 256, (x, y, w, h) =>
        {
            var px = x - 128f;
            var py = y - 128f;
            var core = Length(px, py) - 58f;
            var angle = Mathf.Atan2(py, px);
            var sector = Mathf.Repeat(angle + Mathf.PI / 8f, Mathf.PI / 4f) - Mathf.PI / 8f;
            var r = Length(px, py);
            var rx = r * Mathf.Cos(sector);
            var ry = r * Mathf.Sin(sector);
            var ray = Box(rx - 100f, ry, 24f, 11f);
            return Math.Min(core, ray);
        }));

        public static Sprite Heart => Get("heart", () => Draw(256, 256, (x, y, w, h) =>
        {
            var px = (x - 128f) / 110f;
            var py = (y - 118f) / 110f;
            // Classic implicit heart curve, scaled to roughly pixel distances.
            var a = px * px + py * py - 0.6f;
            var v = a * a * a - px * px * py * py * py;
            return v * 140f;
        }));

        public static Sprite Speech => Get("speech", () => Draw(256, 256, (x, y, w, h) =>
        {
            var bubble = RoundedBox(x - 18f, y - 40f, 220f, 168f, 54f) ;
            var t = Mathf.Clamp01((60f - y) / 50f);
            var tail = y > 60f || y < 10f ? 999f : Math.Abs(x - 70f + t * 30f) - Mathf.Lerp(26f, 2f, t);
            return Math.Min(bubble, tail);
        }));

        private static Sprite Get(string key, Func<Sprite> create)
        {
            if (!Cache.TryGetValue(key, out var sprite) || sprite == null)
            {
                sprite = create();
                sprite.name = key;
                Cache[key] = sprite;
            }

            return sprite;
        }

        private delegate float DistanceFunction(float x, float y, float width, float height);

        private static Sprite Draw(int width, int height, DistanceFunction distance, Vector4 border = default)
        {
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
                hideFlags = HideFlags.DontSave,
            };
            var pixels = new Color32[width * height];
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var d = distance(x + 0.5f, y + 0.5f, width, height);
                    var alpha = Mathf.Clamp01(0.5f - d);
                    pixels[y * width + x] = new Color32(255, 255, 255, (byte)(alpha * 255f));
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, border);
        }

        private static float Length(float x, float y)
        {
            return Mathf.Sqrt(x * x + y * y);
        }

        private static float Box(float x, float y, float halfWidth, float halfHeight)
        {
            var dx = Math.Abs(x) - halfWidth;
            var dy = Math.Abs(y) - halfHeight;
            var outside = Length(Math.Max(dx, 0f), Math.Max(dy, 0f));
            var inside = Math.Min(Math.Max(dx, dy), 0f);
            return outside + inside;
        }

        private static float RoundedBox(float x, float y, float w, float h, float radius)
        {
            var px = x - w / 2f;
            var py = y - h / 2f;
            return Box(px, py, w / 2f - radius - 2f, h / 2f - radius - 2f) - radius;
        }

        private static float StarDistance(float x, float y, float outer, float inner)
        {
            // Polygon distance to a 5-point star, computed against its 10 edges.
            var points = new Vector2[10];
            for (var i = 0; i < 10; i++)
            {
                var a = Mathf.PI / 2f + i * Mathf.PI / 5f;
                var r = i % 2 == 0 ? outer : inner;
                points[i] = new Vector2(Mathf.Cos(a) * r, Mathf.Sin(a) * r);
            }

            var p = new Vector2(x, y);
            var d = float.MaxValue;
            var inside = false;
            for (int i = 0, j = 9; i < 10; j = i++)
            {
                var a = points[i];
                var b = points[j];
                var e = b - a;
                var wv = p - a;
                var proj = Mathf.Clamp01(Vector2.Dot(wv, e) / Vector2.Dot(e, e));
                d = Math.Min(d, (wv - e * proj).magnitude);
                if ((a.y > y) != (b.y > y) && x < (b.x - a.x) * (y - a.y) / (b.y - a.y) + a.x)
                {
                    inside = !inside;
                }
            }

            return inside ? -d : d;
        }
    }
}
