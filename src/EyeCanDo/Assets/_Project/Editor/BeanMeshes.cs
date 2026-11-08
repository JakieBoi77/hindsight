using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static Hindsight.EyeCanDo.EditorTools.BuilderUtil;

namespace Hindsight.EyeCanDo.EditorTools
{
    /// <summary>
    /// Mesh kit for the scout characters. The torso and the big round head are surfaces of revolution
    /// whose side profiles are superellipses (Shape). Clothes and hats are shells offset from those same
    /// profiles, so they hug the body; face patches follow the head's curve. Sizes are in kid units (the
    /// doctor's visual is scaled up), facing +Z. Generated meshes are saved as assets (prefabs can't keep
    /// in-memory meshes); a rebuild overwrites them in place so their GUIDs stay put.
    /// </summary>
    internal static class BeanMeshes
    {
        public const string Dir = Root + "/Models/Bean";

        /// <summary>A rounded body part: a surface of revolution around +Y whose side profile is a superellipse.</summary>
        internal sealed class Shape
        {
            public readonly string Name;
            readonly float radius, halfHeight, centerY, exponent, egg;

            /// <param name="egg">How much wider the bottom is than the top (0 = symmetric).</param>
            public Shape(string name, float radius, float halfHeight, float centerY, float exponent, float egg)
            {
                Name = name;
                this.radius = radius;
                this.halfHeight = halfHeight;
                this.centerY = centerY;
                this.exponent = exponent;
                this.egg = egg;
            }

            public float Top => centerY + halfHeight;
            public float Bottom => centerY - halfHeight;
            public float CenterY => centerY;

            /// <summary>Profile point (radius, y) at parameter t (-pi/2 at the bottom pole, pi/2 at the top).</summary>
            public Vector2 Profile(float t)
            {
                float s = Mathf.Sin(t), c = Mathf.Cos(t);
                var y = centerY + halfHeight * Mathf.Sign(s) * Mathf.Pow(Mathf.Abs(s), 2f / exponent);
                var eta = (y - centerY) / halfHeight;
                var r = radius * Mathf.Pow(Mathf.Abs(c), 2f / exponent) * (1f - egg * eta);
                return new Vector2(r, y);
            }

            /// <summary>Outward normal of the profile (radius, y) at t.</summary>
            public Vector2 ProfileNormal(float t)
            {
                const float e = 1e-3f;
                if (t <= -Mathf.PI / 2f + e) return Vector2.down;
                if (t >= Mathf.PI / 2f - e) return Vector2.up;
                var d = Profile(t + e) - Profile(t - e);
                return new Vector2(d.y, -d.x).normalized;
            }

            /// <summary>The profile parameter at height y (clamped to the shape).</summary>
            public float TAt(float y)
            {
                float lo = -Mathf.PI / 2f, hi = Mathf.PI / 2f;
                for (var i = 0; i < 40; i++)
                {
                    var mid = (lo + hi) * 0.5f;
                    if (Profile(mid).y < y) lo = mid;
                    else hi = mid;
                }
                return (lo + hi) * 0.5f;
            }

            public float RadiusAt(float y) => Profile(TAt(y)).x;

            /// <summary>Outward surface normal at height y, angle theta (0 = front, +Z; pi/2 = +X).</summary>
            public Vector3 Normal(float y, float theta)
            {
                var n = ProfileNormal(TAt(y));
                return new Vector3(n.x * Mathf.Sin(theta), n.y, n.x * Mathf.Cos(theta)).normalized;
            }

            /// <summary>A point on (or offset off) the surface at height y, angle theta.</summary>
            public Vector3 Surface(float y, float theta, float offset = 0f)
            {
                var t = TAt(y);
                var p = Profile(t);
                var n = ProfileNormal(t);
                var r = p.x + n.x * offset;
                return new Vector3(r * Mathf.Sin(theta), p.y + n.y * offset, r * Mathf.Cos(theta));
            }

            public Mesh Mesh() => Shell($"{Name}_Body", Bottom - 1f, Top + 1f, 0f, rings: 56, segments: 56);

            /// <summary>
            /// A shell over the shape between heights y0 and y1 (beyond the ends it closes over the pole),
            /// offset off the surface plus flare more toward the bottom (from flareFrom down). An angle
            /// range makes an open piece, which should be double-sided.
            /// </summary>
            public Mesh Shell(string name, float y0, float y1, float offset, float flare = 0f, float flareFrom = 0f,
                float a0 = 0f, float a1 = Mathf.PI * 2f, bool doubleSided = false, int rings = 32, int segments = 48)
            {
                var t0 = y0 <= Bottom ? -Mathf.PI / 2f : TAt(y0);
                var t1 = y1 >= Top ? Mathf.PI / 2f : TAt(y1);
                var pts = new List<Vector2>();
                var nrm = new List<Vector2>();
                for (var i = 0; i <= rings; i++)
                {
                    var t = Mathf.Lerp(t0, t1, i / (float)rings);
                    var p = Profile(t);
                    var n = ProfileNormal(t);
                    pts.Add(p + n * (offset + FlareAt(p.y, flare, flareFrom)));
                    nrm.Add(n);
                }
                return Lathe(name, pts, nrm, segments, a0, a1, doubleSided);
            }

            /// <summary>Extra offset of a flared hem at height y.</summary>
            public float FlareAt(float y, float flare, float flareFrom)
            {
                if (flare <= 0f || y >= flareFrom) return 0f;
                return flare * Mathf.SmoothStep(0f, 1f, (flareFrom - y) / (flareFrom - Bottom));
            }

            /// <summary>
            /// A curved patch hugging the front for a face feature: width of arc by height, centred at
            /// height y. UVs run 0..1 left to right as seen from the front.
            /// </summary>
            public Mesh Patch(string name, float yCentre, float width, float height, float offset, int cols = 18, int rows = 12)
            {
                var mesh = new Mesh { name = name };
                var verts = new List<Vector3>();
                var norms = new List<Vector3>();
                var uvs = new List<Vector2>();
                var halfAngle = width * 0.5f / RadiusAt(yCentre);
                for (var i = 0; i <= rows; i++)
                for (var j = 0; j <= cols; j++)
                {
                    float u = j / (float)cols, v = i / (float)rows;
                    var y = yCentre + (v - 0.5f) * height;
                    var theta = halfAngle - u * 2f * halfAngle; // seen from the front, +X is on the left
                    verts.Add(Surface(y, theta, offset));
                    norms.Add(Normal(y, theta));
                    uvs.Add(new Vector2(u, v));
                }
                var tris = new List<int>();
                for (var i = 0; i < rows; i++)
                for (var j = 0; j < cols; j++)
                {
                    int a = i * (cols + 1) + j, b = a + 1, d = a + cols + 1, c = d + 1;
                    tris.AddRange(new[] { a, d, c, a, c, b });
                }
                mesh.SetVertices(verts);
                mesh.SetNormals(norms);
                mesh.SetUVs(0, uvs);
                mesh.SetTriangles(tris, 0);
                mesh.RecalculateBounds();
                return Save(mesh);
            }
        }

        /// <summary>The torso (from the hips): a chest broadest at the shoulders, narrowing to the waist.</summary>
        public static readonly Shape Torso = new("Torso", 0.14f, 0.19f, 0.15f, 2.8f, -0.1f);

        /// <summary>The big rounded head (from the neck joint), a little taller than wide with soft, fuller sides.</summary>
        public static readonly Shape Head = new("Head", 0.165f, 0.18f, 0.17f, 2.6f, 0f);

        /// <summary>A capsule hanging from the origin down to -<paramref name="length"/> (limbs, sleeves).</summary>
        public static Mesh Capsule(float radius, float length)
        {
            var pts = new List<Vector2>();
            var nrm = new List<Vector2>();
            const int cap = 8;
            for (var i = 0; i <= cap; i++)
            {
                var a = Mathf.Lerp(-Mathf.PI / 2f, 0f, i / (float)cap);
                pts.Add(new Vector2(Mathf.Cos(a) * radius, -length + Mathf.Sin(a) * radius));
                nrm.Add(new Vector2(Mathf.Cos(a), Mathf.Sin(a)));
            }
            for (var i = 0; i <= cap; i++)
            {
                var a = Mathf.Lerp(0f, Mathf.PI / 2f, i / (float)cap);
                pts.Add(new Vector2(Mathf.Cos(a) * radius, Mathf.Sin(a) * radius));
                nrm.Add(new Vector2(Mathf.Cos(a), Mathf.Sin(a)));
            }
            return Lathe($"Capsule_{radius * 1000f:0}_{length * 1000f:0}", pts, nrm, 24);
        }

        /// <summary>A cone standing on the origin, pointing up +Y, with a flat base.</summary>
        public static Mesh Cone(float baseRadius, float height)
        {
            var slant = new Vector2(height, baseRadius).normalized;
            var pts = new List<Vector2> { Vector2.zero, new(baseRadius, 0f), new(baseRadius, 0f), new(0f, height) };
            var nrm = new List<Vector2> { Vector2.down, Vector2.down, slant, slant };
            return Lathe($"Cone_{baseRadius * 1000f:0}_{height * 1000f:0}", pts, nrm, 32);
        }

        /// <summary>A ring lying flat (around +Y): radius to the tube's centre, and the tube's radius.</summary>
        public static Mesh Torus(float ringRadius, float tubeRadius)
        {
            var pts = new List<Vector2>();
            var nrm = new List<Vector2>();
            const int sides = 16;
            for (var i = 0; i <= sides; i++)
            {
                var a = -Mathf.PI / 2f + i * Mathf.PI * 2f / sides;
                var n = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
                pts.Add(new Vector2(ringRadius, 0f) + n * tubeRadius);
                nrm.Add(n);
            }
            return Lathe($"Torus_{ringRadius * 1000f:0}_{tubeRadius * 1000f:0}", pts, nrm, 48);
        }

        /// <summary>A short open band (a crown's ring), double-sided.</summary>
        public static Mesh Band(float bottomRadius, float topRadius, float height)
        {
            var slant = new Vector2(height, bottomRadius - topRadius).normalized;
            var pts = new List<Vector2> { new(bottomRadius, 0f), new(topRadius, height) };
            var nrm = new List<Vector2> { slant, slant };
            return Lathe($"Band_{bottomRadius * 1000f:0}_{topRadius * 1000f:0}_{height * 1000f:0}", pts, nrm, 48, doubleSided: true);
        }

        /// <summary>A five-pointed star facing +Z (and -Z), <paramref name="depth"/> thick.</summary>
        public static Mesh Star(float outer, float inner, float depth)
        {
            var mesh = new Mesh { name = $"Star_{outer * 1000f:0}_{inner * 1000f:0}_{depth * 1000f:0}" };
            var verts = new List<Vector3>();
            var norms = new List<Vector3>();
            var tris = new List<int>();
            var rim = new Vector2[10];
            for (var i = 0; i < 10; i++)
            {
                var a = Mathf.PI / 2f + i * Mathf.PI / 5f;
                var r = i % 2 == 0 ? outer : inner;
                rim[i] = new Vector2(Mathf.Cos(a) * r, Mathf.Sin(a) * r);
            }
            // Both faces, each drawn both ways round so it shows whichever way it faces.
            foreach (var side in new[] { 1f, -1f })
            {
                var centre = verts.Count;
                verts.Add(new Vector3(0f, 0f, side * depth * 0.5f));
                norms.Add(new Vector3(0f, 0f, side));
                for (var i = 0; i < 10; i++)
                {
                    verts.Add(new Vector3(rim[i].x, rim[i].y, side * depth * 0.5f));
                    norms.Add(new Vector3(0f, 0f, side));
                }
                for (var i = 0; i < 10; i++)
                {
                    int a = centre + 1 + i, b = centre + 1 + (i + 1) % 10;
                    tris.AddRange(new[] { centre, a, b, centre, b, a });
                }
            }
            for (var i = 0; i < 10; i++)
            {
                var p = rim[i];
                var q = rim[(i + 1) % 10];
                var n = new Vector3(q.y - p.y, p.x - q.x, 0f).normalized;
                if (Vector3.Dot(n, new Vector3(p.x + q.x, p.y + q.y, 0f)) < 0f) n = -n;
                var start = verts.Count;
                verts.Add(new Vector3(p.x, p.y, depth * 0.5f));
                verts.Add(new Vector3(q.x, q.y, depth * 0.5f));
                verts.Add(new Vector3(q.x, q.y, -depth * 0.5f));
                verts.Add(new Vector3(p.x, p.y, -depth * 0.5f));
                for (var k = 0; k < 4; k++) norms.Add(n);
                tris.AddRange(new[] { start, start + 1, start + 2, start, start + 2, start + 3 });
                tris.AddRange(new[] { start, start + 2, start + 1, start, start + 3, start + 2 });
            }
            mesh.SetVertices(verts);
            mesh.SetNormals(norms);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateBounds();
            return Save(mesh);
        }

        /// <summary>
        /// Spins a profile (radius, y) from bottom to top around +Y. θ = 0 is the front (+Z), θ grows
        /// toward +X. Faces point outward; a double-sided mesh also gets the inside.
        /// </summary>
        public static Mesh Lathe(string name, IList<Vector2> profile, IList<Vector2> normals, int segments, float a0 = 0f,
            float a1 = Mathf.PI * 2f, bool doubleSided = false)
        {
            var mesh = new Mesh { name = name };
            var verts = new List<Vector3>();
            var norms = new List<Vector3>();
            var uvs = new List<Vector2>();
            var n = profile.Count;
            for (var i = 0; i < n; i++)
            for (var j = 0; j <= segments; j++)
            {
                var theta = Mathf.Lerp(a0, a1, j / (float)segments);
                float s = Mathf.Sin(theta), c = Mathf.Cos(theta);
                var p = profile[i];
                var q = normals[i];
                verts.Add(new Vector3(p.x * s, p.y, p.x * c));
                norms.Add(new Vector3(q.x * s, q.y, q.x * c).normalized);
                uvs.Add(new Vector2(j / (float)segments, i / (float)(n - 1)));
            }
            var tris = new List<int>();
            for (var i = 0; i < n - 1; i++)
            for (var j = 0; j < segments; j++)
            {
                int a = i * (segments + 1) + j, b = a + 1, c = b + segments + 1, d = a + segments + 1;
                tris.AddRange(new[] { a, b, c, a, c, d });
            }
            if (doubleSided)
            {
                var count = verts.Count;
                for (var k = 0; k < count; k++)
                {
                    verts.Add(verts[k] - norms[k] * 0.002f);
                    norms.Add(-norms[k]);
                    uvs.Add(uvs[k]);
                }
                var front = tris.Count;
                for (var k = 0; k < front; k += 3) tris.AddRange(new[] { tris[k] + count, tris[k + 2] + count, tris[k + 1] + count });
            }
            mesh.SetVertices(verts);
            mesh.SetNormals(norms);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateBounds();
            return Save(mesh);
        }

        /// <summary>Keeps the mesh as an asset under <see cref="Dir"/>, overwriting one of the same name in place.</summary>
        internal static Mesh Save(Mesh mesh)
        {
            EnsureFolder(Dir);
            var path = $"{Dir}/{mesh.name}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (existing == null)
            {
                AssetDatabase.CreateAsset(mesh, path);
                return mesh;
            }
            EditorUtility.CopySerialized(mesh, existing);
            Object.DestroyImmediate(mesh);
            EditorUtility.SetDirty(existing);
            return existing;
        }
    }
}
