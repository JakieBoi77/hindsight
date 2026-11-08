using System;
using System.Collections.Generic;
using System.Linq;
using Hindsight.EyeCanDo.Core.Avatar;
using Hindsight.EyeCanDo.SceneObjects;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using static Hindsight.EyeCanDo.EditorTools.BuilderUtil;
using static Hindsight.EyeCanDo.EditorTools.BeanMeshes;

namespace Hindsight.EyeCanDo.EditorTools
{
    /// <summary>
    /// Builds a scout character (<see cref="BeanRig"/>) and its wardrobe from generated meshes, in the
    /// spirit of PEAK's scouts: a big round head on a small pear-shaped torso, long arms with mitten
    /// hands, short legs in boots and a drawn face.
    /// Hierarchy: Visual (scaled: the doctor is bigger) / Hips / { Body (torso, fit, Shoulder /
    /// Elbow / Hand, Head (face, hats)), LegL, LegR }. Joints have no rotation, so the rig writes each
    /// pose from scratch. Switchable parts are named "&lt;Slot&gt;_&lt;id&gt;[_&lt;part&gt;]". The skin colour
    /// reaches the skin renderers through property blocks; clothes keep their own colours (as in PEAK).
    /// </summary>
    internal static class BeanBuilder
    {
        public const float HipsY = 0.33f;
        const float NeckY = 0.33f; // on the torso
        const float ShoulderY = 0.285f;
        const float UpperArmBase = 0.115f;
        const float ForearmBase = 0.1f;
        const float ArmRadius = 0.036f;
        const float HandRadius = 0.046f;
        const float LegX = 0.062f;
        const float LegLength = 0.22f;
        const float LegRadius = 0.048f;

        /// <summary>Waist height on the torso (where shorts and trousers start).</summary>
        const float Waist = 0.08f;

        static float C => Head.CenterY; // the head's centre, from the neck joint

        sealed class Ctx
        {
            public Transform Body, Head, ShoulderL, ShoulderR, ElbowL, ElbowR, LegL, LegR;
            public readonly List<Renderer> Skin = new();
            public readonly List<GameObject> Items = new();
            public readonly List<Transform> Spinners = new();
            public Material SkinMat;
            public float UpperArm = UpperArmBase, Forearm = ForearmBase;

            public Transform Item(string name, Transform parent)
            {
                var go = new GameObject(name);
                go.transform.SetParent(parent, false);
                Items.Add(go);
                return go.transform;
            }
        }

        /// <summary>
        /// Builds a scout under <paramref name="root"/> with the given hats and fits, and adds its rig.
        /// <paramref name="armLength"/> stretches the arms (grown-ups reach further).
        /// </summary>
        public static BeanRig Build(GameObject root, float scale, IEnumerable<string> hats, IEnumerable<string> outfits, AvatarLook preview,
            float armLength = 1f)
        {
            var c = new Ctx
            {
                // A touch of glow in the skin's own colour keeps the colours bright and soft in a dim room.
                SkinMat = Lit("BeanSkin", Color.white, 0.12f, Color.black),
                UpperArm = UpperArmBase * armLength,
                Forearm = ForearmBase * armLength,
            };
            var visual = new GameObject("Visual").transform;
            visual.SetParent(root.transform, false);
            visual.localScale = Vector3.one * scale;

            var hips = Node("Hips", visual, new Vector3(0f, HipsY, 0f));
            c.Body = Node("Body", hips, Vector3.zero);
            Part("Torso", c.Body, Torso.Mesh(), c.SkinMat, Vector3.zero, Quaternion.identity, c.Skin);
            var seat = Node("SeatPoint", hips, new Vector3(0f, Torso.Bottom + 0.01f, 0f));

            // ---- Head on the neck: round, with the face drawn on in three layers.
            c.Head = Node("Head", c.Body, new Vector3(0f, NeckY, 0f));
            Part("Skull", c.Head, Head.Mesh(), c.SkinMat, Vector3.zero, Quaternion.identity, c.Skin);
            var eyesPatch = FacePatch(c.Head, FaceArt.Layer.Eyes, FaceArt.EyesY, FaceArt.EyesHeight * FaceArt.EyesAspect, FaceArt.EyesHeight, 0.003f, 0);
            var mouthPatch = FacePatch(c.Head, FaceArt.Layer.Mouth, FaceArt.MouthY, FaceArt.MouthSize, FaceArt.MouthSize, 0.003f, 0);
            var browsPatch = FacePatch(c.Head, FaceArt.Layer.Brows, FaceArt.BrowsY, FaceArt.BrowsHeight * FaceArt.BrowsAspect, FaceArt.BrowsHeight,
                0.0045f, 1);
            var cheeksPatch = FacePatch(c.Head, FaceArt.Layer.Eyes, FaceArt.CheeksY, FaceArt.EyesHeight * FaceArt.EyesAspect, FaceArt.EyesHeight, 0.002f, -1,
                "Cheeks");
            var eyes = Node("Eyes", c.Head, Head.Surface(C + FaceArt.EyesY, 0f, 0.01f));
            var headCenter = Node("HeadCenter", c.Head, new Vector3(0f, C, 0f));

            // ---- Long arms with mitten hands.
            Transform handL = null, handR = null;
            foreach (var side in new[] { -1f, 1f })
            {
                var tag = side < 0 ? "L" : "R";
                var shoulder = Node("Shoulder" + tag, c.Body, new Vector3(side * (Torso.RadiusAt(ShoulderY) + 0.005f), ShoulderY, 0f));
                Part("UpperArm", shoulder, Capsule(ArmRadius, c.UpperArm), c.SkinMat, Vector3.zero, Quaternion.identity, c.Skin);
                var elbow = Node("Elbow" + tag, shoulder, new Vector3(0f, -c.UpperArm, 0f));
                Part("Forearm", elbow, Capsule(ArmRadius * 0.95f, c.Forearm), c.SkinMat, Vector3.zero, Quaternion.identity, c.Skin);
                var hand = Node("Hand" + tag, elbow, new Vector3(0f, -c.Forearm - 0.02f, 0.004f));
                Sphere("Mitten", hand, Vector3.zero, new Vector3(HandRadius * 2f, HandRadius * 2.15f, HandRadius * 1.8f), c.SkinMat, c.Skin);
                if (side < 0) (c.ShoulderL, c.ElbowL, handL) = (shoulder, elbow, hand);
                else (c.ShoulderR, c.ElbowR, handR) = (shoulder, elbow, hand);
            }

            // ---- Short legs in hiking boots; the soles touch the ground.
            var boot = Fixed("Boot", "#6B4429", 0.25f);
            var sole = Fixed("BootSole", "#3A2A20", 0.2f);
            var legs = new Transform[2];
            for (var i = 0; i < 2; i++)
            {
                var side = i == 0 ? -1f : 1f;
                var leg = Node(i == 0 ? "LegL" : "LegR", hips, new Vector3(side * LegX, 0f, 0f));
                Part("Leg", leg, Capsule(LegRadius, LegLength), c.SkinMat, Vector3.zero, Quaternion.identity, c.Skin);
                var footY = -HipsY + 0.045f;
                Sphere("Boot", leg, new Vector3(0f, footY, 0.025f), new Vector3(0.105f, 0.09f, 0.155f), boot);
                Disc("Cuff", leg, new Vector3(0f, footY + 0.04f, 0.005f), 0.11f, 0.036f, boot);
                Sphere("Sole", leg, new Vector3(0f, footY - 0.025f, 0.03f), new Vector3(0.11f, 0.04f, 0.165f), sole);
                legs[i] = leg;
            }
            (c.LegL, c.LegR) = (legs[0], legs[1]);

            // ---- Wardrobe.
            foreach (var id in hats)
            {
                if (!HatBuilders.TryGetValue(id, out var build)) Debug.LogError($"BeanBuilder: no hat '{id}'");
                else build(c, c.Item("Hat_" + id, c.Head));
            }
            foreach (var id in outfits)
            {
                if (!OutfitBuilders.TryGetValue(id, out var build)) Debug.LogError($"BeanBuilder: no fit '{id}'");
                else build(c, id);
            }

            var rig = root.AddComponent<BeanRig>();
            rig.EditorInit(hips, c.Body, c.Head, c.ShoulderL, c.ShoulderR, c.ElbowL, c.ElbowR, legs[0], legs[1], eyes, headCenter, handL, handR, seat,
                eyesPatch, mouthPatch, browsPatch, cheeksPatch, c.Skin.ToArray(), c.Items.ToArray(), c.Spinners.ToArray(), scale);
            rig.ApplyLook(preview); // so the prefab shows a look in the editor (looks are applied again at runtime)
            return rig;
        }

        static Renderer FacePatch(Transform head, FaceArt.Layer layer, float y, float width, float height, float offset, int priority, string name = null)
        {
            name ??= layer.ToString();
            var go = Part("Face" + name, head, Head.Patch("Head_Face" + name, C + y, width, height, offset), FaceMaterial(layer),
                Vector3.zero, Quaternion.identity, null, shadows: false);
            var r = go.GetComponent<Renderer>();
            r.rendererPriority = priority; // accessories (glasses, brows) draw over the eyes
            return r;
        }

        /// <summary>A band hugging the torso all the way round, its middle at height pathY(θ).</summary>
        static Mesh Ribbon(string name, Func<float, float> pathY, float width, float offset)
        {
            const int segs = 72;
            var mesh = new Mesh { name = name };
            var verts = new List<Vector3>();
            var norms = new List<Vector3>();
            var tris = new List<int>();
            for (var j = 0; j <= segs; j++)
            {
                var theta = j * Mathf.PI * 2f / segs;
                var y = pathY(theta);
                foreach (var dy in new[] { -width * 0.5f, width * 0.5f })
                {
                    verts.Add(Torso.Surface(y + dy, theta, offset));
                    norms.Add(Torso.Normal(y + dy, theta));
                }
            }
            for (var j = 0; j < segs; j++)
            {
                int a = j * 2, b = a + 2, cc = a + 3, d = a + 1;
                tris.AddRange(new[] { a, b, cc, a, cc, d });
            }
            mesh.SetVertices(verts);
            mesh.SetNormals(norms);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateBounds();
            return Save(mesh);
        }

        // ------------------------------------------------------------------ helpers

        static Transform Node(string name, Transform parent, Vector3 localPos)
        {
            var t = new GameObject(name).transform;
            t.SetParent(parent, false);
            t.localPosition = localPos;
            return t;
        }

        static GameObject Part(string name, Transform parent, Mesh mesh, Material mat, Vector3 localPos, Quaternion localRot,
            List<Renderer> region, Vector3? localScale = null, bool shadows = true)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localRotation = localRot;
            go.transform.localScale = localScale ?? Vector3.one;
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var r = go.AddComponent<MeshRenderer>();
            r.sharedMaterial = mat;
            if (!shadows) r.shadowCastingMode = ShadowCastingMode.Off;
            region?.Add(r);
            return go;
        }

        /// <summary>A primitive sized in body units (Unity's sphere and cube are 1 across; the cylinder is 2 tall).</summary>
        static GameObject Prim(PrimitiveType type, string name, Transform parent, Vector3 localPos, Vector3 size, Material mat,
            List<Renderer> region = null, Quaternion? rot = null)
        {
            var go = Primitive(type, name, parent, localPos, size, mat);
            go.transform.localRotation = rot ?? Quaternion.identity;
            region?.Add(go.GetComponent<Renderer>());
            return go;
        }

        static GameObject Sphere(string name, Transform parent, Vector3 pos, Vector3 size, Material mat, List<Renderer> region = null,
            Quaternion? rot = null) => Prim(PrimitiveType.Sphere, name, parent, pos, size, mat, region, rot);

        static GameObject Box(string name, Transform parent, Vector3 pos, Vector3 size, Material mat, Quaternion? rot = null) =>
            Prim(PrimitiveType.Cube, name, parent, pos, size, mat, null, rot);

        /// <summary>A flat disc (a squashed cylinder) of the given diameter and thickness.</summary>
        static GameObject Disc(string name, Transform parent, Vector3 pos, float diameter, float thickness, Material mat, Quaternion? rot = null) =>
            Prim(PrimitiveType.Cylinder, name, parent, pos, new Vector3(diameter, thickness * 0.5f, diameter), mat, null, rot);

        static Quaternion Facing(Shape shape, float y, float theta) => Quaternion.LookRotation(shape.Normal(y, theta), Vector3.up);

        static Quaternion UpAlong(Vector3 dir) => Quaternion.FromToRotation(Vector3.up, dir);

        /// <summary>A ring around a shape at height y (a collar, a belt, a hat band).</summary>
        static GameObject Ring(Shape shape, string name, Transform parent, float y, float offset, float tube, Material mat)
        {
            var p = shape.Surface(y, 0f, offset);
            return Part(name, parent, Torus(p.z, tube), mat, new Vector3(0f, p.y, 0f), Quaternion.identity, null);
        }

        static Color Hex(string hex) => ColorUtility.TryParseHtmlString(hex, out var col) ? col : Color.magenta;

        static Material Fixed(string name, string hex, float smoothness = 0.2f, Color? emission = null) =>
            Lit("Bean_" + name, Hex(hex), smoothness, emission);

        static readonly Dictionary<FaceArt.Layer, Texture2D> Atlases = new();

        /// <summary>Transparent lit material showing one face atlas; each patch picks its cell with _BaseMap_ST.</summary>
        static Material FaceMaterial(FaceArt.Layer layer)
        {
            var path = $"{MaterialsDir}/BeanFace{layer}.mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                AssetDatabase.CreateAsset(mat, path);
            }
            // Painted once per editor session (code changes reload it), not once per character.
            if (!Atlases.TryGetValue(layer, out var atlas) || atlas == null) Atlases[layer] = atlas = FaceArtPainter.Texture(layer);
            mat.SetTexture("_BaseMap", atlas);
            mat.SetColor("_BaseColor", Color.white);
            // Drawn features: no shine. (A transparent Lit material would otherwise add highlights
            // over the clear parts of the patch, showing a pale rectangle.)
            mat.SetFloat("_Smoothness", 0f);
            mat.SetFloat("_SpecularHighlights", 0f);
            mat.EnableKeyword("_SPECULARHIGHLIGHTS_OFF");
            mat.SetFloat("_EnvironmentReflections", 0f);
            mat.EnableKeyword("_ENVIRONMENTREFLECTIONS_OFF");
            mat.SetFloat("_BlendModePreserveSpecular", 0f);
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.SetFloat("_Surface", 1f);
            mat.SetFloat("_Blend", 0f);
            mat.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            mat.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            mat.SetFloat("_SrcBlendAlpha", (float)BlendMode.One);
            mat.SetFloat("_DstBlendAlpha", (float)BlendMode.OneMinusSrcAlpha);
            mat.SetFloat("_ZWrite", 0f);
            mat.SetOverrideTag("RenderType", "Transparent");
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.renderQueue = (int)RenderQueue.Transparent;
            mat.SetShaderPassEnabled("ShadowCaster", false);
            EditorUtility.SetDirty(mat);
            return mat;
        }

        // ------------------------------------------------------------------ hats (on the head; y from the neck joint)

        /// <summary>Where hats sit: just above the brows.</summary>
        static float Brow => C + 0.115f;

        static float HeadTop => Head.Top;

        static readonly Dictionary<string, Action<Ctx, Transform>> HatBuilders = new()
        {
            ["cap"] = (c, hat) =>
            {
                var red = Fixed("CapRed", "#E8434F", 0.3f);
                Part("Dome", hat, Head.Shell("Hat_cap", Brow, HeadTop, 0.012f), red, Vector3.zero, Quaternion.identity, null);
                var front = Head.Surface(Brow, 0f, 0.012f);
                Disc("Brim", hat, new Vector3(0f, front.y - 0.004f, front.z + 0.04f), 0.26f, 0.01f, red, Quaternion.Euler(10f, 0f, 0f));
                Sphere("Button", hat, new Vector3(0f, HeadTop + 0.01f, 0f), Vector3.one * 0.03f, red);
            },
            ["beanie"] = (c, hat) =>
            {
                var knit = Fixed("Knit", "#4C8DF6", 0.1f);
                var cuff = Fixed("KnitCuff", "#E8F1FF", 0.1f);
                Part("Dome", hat, Head.Shell("Hat_beanie", Brow - 0.01f, HeadTop, 0.02f), knit, Vector3.zero, Quaternion.identity, null);
                Ring(Head, "Cuff", hat, Brow, 0.024f, 0.026f, cuff);
                Sphere("Pompom", hat, new Vector3(0f, HeadTop + 0.045f, 0f), Vector3.one * 0.09f, cuff);
            },
            ["sprout"] = (c, hat) =>
            {
                var green = Fixed("Leaf", "#3FBF5A", 0.4f);
                Part("Stem", hat, Capsule(0.011f, 0.07f), green, new Vector3(0f, HeadTop - 0.01f, 0f), Quaternion.Euler(180f, 0f, 0f), null);
                Sphere("LeafL", hat, new Vector3(-0.045f, HeadTop + 0.065f, 0f), new Vector3(0.09f, 0.02f, 0.05f), green, rot: Quaternion.Euler(0f, 0f, -30f));
                Sphere("LeafR", hat, new Vector3(0.045f, HeadTop + 0.065f, 0f), new Vector3(0.09f, 0.02f, 0.05f), green, rot: Quaternion.Euler(0f, 0f, 30f));
            },
            ["bandana"] = (c, hat) =>
            {
                var red = Fixed("Bandana", "#D93A3A", 0.2f);
                Part("Wrap", hat, Head.Shell("Hat_bandana", Brow - 0.01f, HeadTop, 0.012f), red, Vector3.zero, Quaternion.identity, null);
                var white = Fixed("BandanaDot", "#FFFFFF", 0.2f);
                for (var i = 0; i < 7; i++)
                {
                    var theta = -1.2f + i * 0.4f;
                    var y = Brow + 0.03f + (i % 2) * 0.035f;
                    Sphere("Dot", hat, Head.Surface(y, theta, 0.013f), new Vector3(0.018f, 0.018f, 0.006f), white, rot: Facing(Head, y, theta));
                }
                var knot = Head.Surface(Brow + 0.01f, Mathf.PI, 0.02f);
                Sphere("Knot", hat, knot, Vector3.one * 0.04f, red);
                Sphere("TailL", hat, knot + new Vector3(-0.03f, -0.05f, -0.02f), new Vector3(0.03f, 0.08f, 0.015f), red, rot: Quaternion.Euler(0f, 0f, -20f));
                Sphere("TailR", hat, knot + new Vector3(0.03f, -0.05f, -0.02f), new Vector3(0.03f, 0.08f, 0.015f), red, rot: Quaternion.Euler(0f, 0f, 20f));
            },
            ["beret"] = (c, hat) =>
            {
                var navy = Fixed("Beret", "#2E3A6B", 0.15f);
                Sphere("Beret", hat, new Vector3(0.02f, HeadTop - 0.025f, -0.01f), new Vector3(0.42f, 0.11f, 0.4f), navy, rot: Quaternion.Euler(-6f, 0f, -12f));
                Sphere("Nub", hat, new Vector3(0.03f, HeadTop + 0.03f, -0.01f), Vector3.one * 0.025f, navy);
            },
            ["headband"] = (c, hat) =>
            {
                var cloth = Fixed("NinjaBand", "#2B2F45", 0.2f);
                var metal = Fixed("NinjaPlate", "#C9D1DC", 0.8f);
                Ring(Head, "Band", hat, Brow, 0.008f, 0.024f, cloth);
                Box("Plate", hat, Head.Surface(Brow, 0f, 0.03f), new Vector3(0.11f, 0.05f, 0.012f), metal, Facing(Head, Brow, 0f));
                var back = Head.Surface(Brow, Mathf.PI, 0.03f);
                Box("TailL", hat, back + new Vector3(-0.03f, -0.07f, -0.02f), new Vector3(0.03f, 0.14f, 0.006f), cloth, Quaternion.Euler(10f, 0f, -18f));
                Box("TailR", hat, back + new Vector3(0.03f, -0.07f, -0.03f), new Vector3(0.03f, 0.14f, 0.006f), cloth, Quaternion.Euler(10f, 0f, 14f));
            },
            ["visor"] = (c, hat) =>
            {
                var green = Fixed("Visor", "#2EC4B6", 0.4f);
                Ring(Head, "Band", hat, Brow, 0.01f, 0.02f, green);
                var front = Head.Surface(Brow, 0f, 0.01f);
                Prim(PrimitiveType.Cylinder, "Brim", hat, new Vector3(0f, front.y, front.z + 0.04f), new Vector3(0.26f, 0.006f, 0.17f), green,
                    rot: Quaternion.Euler(14f, 0f, 0f));
            },
            ["sailor"] = (c, hat) =>
            {
                var white = Fixed("SailorHat", "#FAFBFF", 0.2f);
                var navy = Fixed("SailorBand", "#2E3A6B", 0.2f);
                var y0 = HeadTop - 0.06f;
                var r = Head.RadiusAt(y0) + 0.01f;
                Part("Crown", hat, Band(r, r + 0.03f, 0.075f), white, new Vector3(0f, y0, 0f), Quaternion.identity, null);
                Disc("Top", hat, new Vector3(0f, y0 + 0.075f, 0f), (r + 0.03f) * 2f, 0.012f, white);
                Part("Ribbon", hat, Torus(r + 0.004f, 0.012f), navy, new Vector3(0f, y0 + 0.012f, 0f), Quaternion.identity, null);
            },
            ["medic"] = (c, hat) =>
            {
                var white = Fixed("MedicCap", "#FAFBFF", 0.2f);
                var red = Fixed("MedicCross", "#E8434F", 0.3f);
                Part("Cap", hat, Head.Shell("Hat_medic", Brow, Brow + 0.075f, 0.02f, a0: -1.1f, a1: 1.1f, doubleSided: true, segments: 24), white,
                    Vector3.zero, Quaternion.identity, null);
                var p = Head.Surface(Brow + 0.04f, 0f, 0.024f);
                Box("CrossH", hat, p, new Vector3(0.045f, 0.014f, 0.006f), red, Facing(Head, Brow + 0.04f, 0f));
                Box("CrossV", hat, p, new Vector3(0.014f, 0.045f, 0.006f), red, Facing(Head, Brow + 0.04f, 0f));
            },
            ["straw"] = (c, hat) =>
            {
                var straw = Fixed("Straw", "#E8C878", 0.15f);
                Part("Crown", hat, Head.Shell("Hat_straw", Brow, HeadTop, 0.022f), straw, Vector3.zero, Quaternion.identity, null);
                Disc("Brim", hat, new Vector3(0f, Brow + 0.005f, 0f), 0.68f, 0.01f, straw);
                Ring(Head, "Ribbon", hat, Brow + 0.02f, 0.024f, 0.014f, Fixed("StrawRibbon", "#E8434F", 0.2f));
            },
            ["sunhat"] = (c, hat) =>
            {
                var pink = Fixed("SunHat", "#FF9FC8", 0.15f);
                Part("Crown", hat, Head.Shell("Hat_sunhat", Brow, HeadTop, 0.02f), pink, Vector3.zero, Quaternion.identity, null);
                var r = Head.RadiusAt(Brow) + 0.02f;
                Part("Brim", hat, Band(r, r + 0.17f, -0.05f), pink, new Vector3(0f, Brow + 0.01f, 0f), Quaternion.identity, null);
                var petal = Fixed("Flower", "#FFFFFF", 0.3f);
                const float theta = 0.7f;
                var y = Brow + 0.03f;
                var p = Head.Surface(y, theta, 0.03f);
                var face = Quaternion.LookRotation(Head.Normal(y, theta));
                for (var i = 0; i < 5; i++)
                {
                    var a = i * Mathf.PI * 2f / 5f;
                    Sphere("Petal", hat, p + face * new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0f) * 0.022f, Vector3.one * 0.026f, petal);
                }
                Sphere("Middle", hat, p, Vector3.one * 0.022f, Fixed("FlowerMiddle", "#FFC845", 0.3f));
            },
            ["pith"] = (c, hat) =>
            {
                var khaki = Fixed("Pith", "#D9C08A", 0.2f);
                Part("Dome", hat, Head.Shell("Hat_pith", Brow - 0.02f, HeadTop, 0.045f), khaki, Vector3.zero, Quaternion.identity, null);
                var r = Head.RadiusAt(Brow - 0.02f) + 0.045f;
                Part("Brim", hat, Band(r, r + 0.07f, -0.035f), khaki, new Vector3(0f, Brow - 0.02f, 0f), Quaternion.identity, null);
                Ring(Head, "Band", hat, Brow, 0.05f, 0.012f, Fixed("PithBand", "#8B6A3A", 0.2f));
                Sphere("Vent", hat, new Vector3(0f, HeadTop + 0.04f, 0f), new Vector3(0.04f, 0.025f, 0.04f), khaki);
            },
            ["propeller"] = (c, hat) =>
            {
                var colours = new[] { "#E8434F", "#FFC845", "#4C8DF6", "#3FBF5A" };
                for (var q = 0; q < 4; q++)
                {
                    var a0 = q * Mathf.PI / 2f;
                    Part("Panel" + q, hat, Head.Shell("Hat_propeller_" + q, Brow, HeadTop, 0.014f, a0: a0, a1: a0 + Mathf.PI / 2f, segments: 12),
                        Fixed("Propeller" + q, colours[q], 0.35f), Vector3.zero, Quaternion.identity, null);
                }
                var grey = Fixed("PropellerStem", "#8A93A6", 0.5f);
                Prim(PrimitiveType.Cylinder, "Stem", hat, new Vector3(0f, HeadTop + 0.03f, 0f), new Vector3(0.016f, 0.02f, 0.016f), grey);
                var spin = Node("Spin", hat, new Vector3(0f, HeadTop + 0.05f, 0f));
                Box("BladeA", spin, new Vector3(0.07f, 0f, 0f), new Vector3(0.14f, 0.008f, 0.045f), Fixed("BladeA", "#E8434F", 0.4f), Quaternion.Euler(10f, 0f, 0f));
                Box("BladeB", spin, new Vector3(-0.07f, 0f, 0f), new Vector3(0.14f, 0.008f, 0.045f), Fixed("BladeB", "#4C8DF6", 0.4f), Quaternion.Euler(-10f, 0f, 0f));
                Sphere("Hub", spin, Vector3.zero, Vector3.one * 0.026f, grey);
                c.Spinners.Add(spin);
            },
            ["chef"] = (c, hat) =>
            {
                var white = Fixed("Chef", "#FAFBFF", 0.15f);
                var y0 = Brow + 0.01f;
                var r = Head.RadiusAt(y0) + 0.012f;
                Part("Band", hat, Band(r, r + 0.01f, 0.08f), white, new Vector3(0f, y0, 0f), Quaternion.identity, null);
                Part("Top", hat, Head.Shell("Hat_chef_top", y0, HeadTop, 0.012f), white, Vector3.zero, Quaternion.identity, null);
                foreach (var (x, z, s) in new[] { (0f, 0f, 0.26f), (-0.08f, 0.04f, 0.18f), (0.08f, 0.04f, 0.18f), (0f, -0.08f, 0.18f) })
                    Sphere("Puff", hat, new Vector3(x, y0 + 0.13f, z), new Vector3(s, s * 0.8f, s), white);
            },
            ["mushroom"] = (c, hat) =>
            {
                var red = Fixed("Mushroom", "#E8434F", 0.3f);
                var spot = Fixed("MushroomSpot", "#FFF6E8", 0.3f);
                var y0 = Brow + 0.01f;
                Sphere("Cap", hat, new Vector3(0f, y0 + 0.03f, 0f), new Vector3(0.6f, 0.3f, 0.6f), red);
                // White spots on the cap's surface (an ellipsoid of radii 0.3, 0.15, 0.3).
                var centre = new Vector3(0f, y0 + 0.03f, 0f);
                foreach (var (theta, elevation, s) in new[] { (0f, 0.35f, 0.07f), (1.2f, 0.3f, 0.06f), (-1.3f, 0.35f, 0.065f), (2.5f, 0.3f, 0.07f), (-2.4f, 0.25f, 0.05f), (0.5f, 0.95f, 0.05f) })
                {
                    var dir = new Vector3(Mathf.Sin(theta) * Mathf.Cos(elevation), Mathf.Sin(elevation), Mathf.Cos(theta) * Mathf.Cos(elevation));
                    var p = centre + Vector3.Scale(dir, new Vector3(0.3f, 0.15f, 0.3f));
                    var n = new Vector3(dir.x / 0.3f, dir.y / 0.15f, dir.z / 0.3f).normalized;
                    Sphere("Spot", hat, p, new Vector3(s, s, s * 0.35f), spot, rot: Quaternion.LookRotation(n));
                }
            },
            ["wolfears"] = (c, hat) =>
            {
                var grey = Fixed("WolfFur", "#8A8F9E", 0.15f);
                var pink = Fixed("WolfEar", "#F4A6B8", 0.2f);
                foreach (var side in new[] { -1f, 1f })
                {
                    var theta = side * 0.75f;
                    var y = HeadTop - 0.07f;
                    var p = Head.Surface(y, theta, -0.01f);
                    var up = UpAlong((Head.Normal(y, theta) + Vector3.up * 1.6f).normalized);
                    Part("Ear", hat, Cone(0.055f, 0.12f), grey, p, up, null, new Vector3(1f, 1f, 0.55f));
                    Part("Inside", hat, Cone(0.035f, 0.085f), pink, p + new Vector3(0f, 0f, 0.012f), up, null, new Vector3(1f, 1f, 0.4f));
                }
            },
            ["frog"] = (c, hat) =>
            {
                var green = Fixed("Frog", "#5DBB4C", 0.25f);
                Part("Dome", hat, Head.Shell("Hat_frog", Brow - 0.005f, HeadTop, 0.02f), green, Vector3.zero, Quaternion.identity, null);
                foreach (var side in new[] { -1f, 1f })
                {
                    var p = new Vector3(side * 0.085f, HeadTop + 0.005f, 0.06f);
                    Sphere("EyeBump", hat, p, Vector3.one * 0.09f, green);
                    Sphere("EyeWhite", hat, p + new Vector3(0f, 0.012f, 0.03f), new Vector3(0.06f, 0.06f, 0.035f), Fixed("FrogEye", "#FFFFFF", 0.4f));
                    Sphere("Pupil", hat, p + new Vector3(0f, 0.014f, 0.047f), new Vector3(0.03f, 0.03f, 0.012f), Fixed("FrogPupil", "#231F33", 0.6f));
                }
            },
            ["cowboy"] = (c, hat) =>
            {
                var brown = Fixed("Cowboy", "#8B5A2B", 0.2f);
                var y0 = Brow + 0.005f;
                var r = Head.RadiusAt(y0) + 0.015f;
                Part("Brim", hat, Band(r, r + 0.13f, 0.035f), brown, new Vector3(0f, y0, 0f), Quaternion.identity, null);
                Part("Crown", hat, Band(r, r - 0.015f, 0.15f), brown, new Vector3(0f, y0, 0f), Quaternion.identity, null);
                Sphere("Top", hat, new Vector3(0f, y0 + 0.15f, 0f), new Vector3((r - 0.015f) * 2f, 0.06f, (r - 0.015f) * 1.8f), brown);
                Part("Band", hat, Torus(r + 0.002f, 0.012f), Fixed("CowboyBand", "#3A2A20", 0.2f), new Vector3(0f, y0 + 0.02f, 0f), Quaternion.identity, null);
            },
            ["gnome"] = (c, hat) =>
            {
                var red = Fixed("Gnome", "#D93A3A", 0.2f);
                var y0 = Brow - 0.005f;
                Part("Brim", hat, Head.Shell("Hat_gnome", y0, HeadTop, 0.018f), red, Vector3.zero, Quaternion.identity, null);
                Part("Cone", hat, Cone(Head.RadiusAt(y0 + 0.06f) + 0.02f, 0.36f), red, new Vector3(0f, y0 + 0.06f, -0.01f), Quaternion.Euler(-14f, 0f, 0f), null);
            },
            ["wizard"] = (c, hat) =>
            {
                var navy = Fixed("Wizard", "#3D3F9B", 0.25f);
                var gold = Fixed("WizardStar", "#FFD23F", 0.5f, Hex("#FFB000") * 0.4f);
                var y0 = Brow + 0.005f;
                Disc("Brim", hat, new Vector3(0f, y0, 0f), 0.56f, 0.01f, navy);
                var cone = Part("Cone", hat, Cone(0.19f, 0.4f), navy, new Vector3(0f, y0, 0f), Quaternion.Euler(-10f, 0f, 0f), null).transform;
                var star = Star(0.032f, 0.014f, 0.008f);
                var slope = Mathf.Atan2(0.19f, 0.4f) * Mathf.Rad2Deg;
                foreach (var (x, h, s) in new[] { (-0.04f, 0.1f, 1f), (0.035f, 0.2f, 0.75f), (-0.01f, 0.29f, 0.55f) })
                {
                    var rr = 0.19f * (1f - h / 0.4f);
                    Part("Star", cone, star, gold, new Vector3(x, h, Mathf.Sqrt(Mathf.Max(0f, rr * rr - x * x)) + 0.004f), Quaternion.Euler(-slope, 0f, 0f), null, Vector3.one * s);
                }
            },
            ["jester"] = (c, hat) =>
            {
                var purple = Fixed("JesterA", "#7B4FC9", 0.25f);
                var yellow = Fixed("JesterB", "#FFC845", 0.25f);
                var bell = Fixed("JesterBell", "#FFD23F", 0.8f);
                Part("Cap", hat, Head.Shell("Hat_jester", Brow - 0.01f, HeadTop, 0.018f), purple, Vector3.zero, Quaternion.identity, null);
                foreach (var (angle, mat) in new[] { (-55f, purple), (0f, yellow), (55f, purple) })
                {
                    var rot = Quaternion.Euler(0f, 0f, -angle) * Quaternion.Euler(-12f, 0f, 0f);
                    var basePos = new Vector3(Mathf.Sin(angle * Mathf.Deg2Rad) * 0.08f, HeadTop - 0.04f, 0f);
                    Part("Point", hat, Cone(0.07f, 0.22f), mat, basePos, rot, null);
                    Sphere("Bell", hat, basePos + rot * new Vector3(0f, 0.23f, 0f), Vector3.one * 0.04f, bell);
                }
            },
            ["antlers"] = (c, hat) =>
            {
                var brown = Fixed("Antler", "#B08A5A", 0.2f);
                foreach (var side in new[] { -1f, 1f })
                {
                    var theta = side * 0.95f;
                    var y = HeadTop - 0.08f;
                    var p = Head.Surface(y, theta, -0.01f);
                    var main = UpAlong(new Vector3(side * 0.6f, 1f, 0f));
                    var flip = Quaternion.Euler(180f, 0f, 0f); // capsules hang down from their origin; turn them to grow up
                    Part("Beam", hat, Capsule(0.014f, 0.16f), brown, p, main * flip, null);
                    var tip = p + main * new Vector3(0f, 0.16f, 0f);
                    Part("Tine", hat, Capsule(0.011f, 0.07f), brown, p + main * new Vector3(0f, 0.08f, 0f), UpAlong(new Vector3(-side * 0.2f, 1f, 0.3f)) * flip, null);
                    Part("Tine", hat, Capsule(0.011f, 0.06f), brown, tip, UpAlong(new Vector3(side * 0.9f, 0.6f, 0f)) * flip, null);
                }
            },
            ["laurel"] = (c, hat) =>
            {
                var leaf = Fixed("Laurel", "#7FB044", 0.3f);
                var y0 = Brow - 0.005f;
                var r = Head.RadiusAt(y0) + 0.02f;
                for (var i = 0; i < 26; i++)
                {
                    var theta = 0.45f + i * ((Mathf.PI * 2f - 0.9f) / 25f); // round the sides and back, open at the front
                    var p = new Vector3(Mathf.Sin(theta) * r, y0 + (i % 2) * 0.02f, Mathf.Cos(theta) * r);
                    Sphere("Leaf", hat, p, new Vector3(0.07f, 0.03f, 0.018f), leaf,
                        rot: Quaternion.LookRotation(new Vector3(p.x, 0f, p.z)) * Quaternion.Euler(0f, 0f, i % 2 == 0 ? 35f : -35f));
                }
            },
            ["knight"] = (c, hat) =>
            {
                var steel = Fixed("Steel", "#C9D1DC", 0.75f);
                var plume = Fixed("Plume", "#E8434F", 0.2f);
                Part("Helmet", hat, Head.Shell("Hat_knight", Brow - 0.015f, HeadTop, 0.026f), steel, Vector3.zero, Quaternion.identity, null);
                Ring(Head, "Rim", hat, Brow - 0.012f, 0.028f, 0.014f, Fixed("SteelDark", "#8E99A8", 0.6f));
                for (var i = 0; i < 7; i++)
                {
                    var t = Mathf.Lerp(0.9f, Mathf.PI - 0.9f, i / 6f);
                    var tp = Mathf.PI / 2f - Mathf.Abs(Mathf.PI / 2f - t);
                    var p = Head.Profile(tp);
                    var n = Head.ProfileNormal(tp);
                    var z = (t < Mathf.PI / 2f ? 1f : -1f) * (p.x + n.x * 0.06f);
                    Sphere("Plume", hat, new Vector3(0f, p.y + n.y * 0.06f, z), new Vector3(0.045f, 0.08f, 0.07f), plume);
                }
            },
            ["crown"] = (c, hat) =>
            {
                var gold = Fixed("Crown", "#FFC83D", 0.85f, Hex("#7A5200") * 0.6f);
                var y0 = HeadTop - 0.07f;
                const float height = 0.065f;
                var rb = Head.RadiusAt(y0) + 0.008f;
                Part("Band", hat, Band(rb, rb + 0.012f, height), gold, new Vector3(0f, y0, 0f), Quaternion.identity, null);
                var point = Cone(0.026f, 0.055f);
                for (var i = 0; i < 5; i++)
                {
                    var a = i * Mathf.PI * 2f / 5f;
                    var r = rb + 0.008f;
                    Part("Point", hat, point, gold, new Vector3(Mathf.Sin(a) * r, y0 + height - 0.005f, Mathf.Cos(a) * r), Quaternion.identity, null);
                    Sphere("Tip", hat, new Vector3(Mathf.Sin(a) * r, y0 + height + 0.052f, Mathf.Cos(a) * r), Vector3.one * 0.02f, gold);
                }
                Sphere("Gem", hat, new Vector3(0f, y0 + height * 0.5f, rb + 0.01f), new Vector3(0.03f, 0.03f, 0.014f), Fixed("GemRed", "#E8434F", 0.9f));
            },
            [AvatarLook.DoctorHat] = (c, hat) =>
            {
                var band = Fixed("MirrorBand", "#5C6B82", 0.3f);
                var mirror = Fixed("Mirror", "#DDE6F0", 0.95f);
                Ring(Head, "Band", hat, Brow + 0.005f, 0.006f, 0.011f, band);
                // On the forehead just above the band, facing mostly forward.
                var y = Brow + 0.03f;
                const float theta = 0.3f;
                var facing = (Head.Normal(y, theta) + new Vector3(Mathf.Sin(theta), 0f, Mathf.Cos(theta)) * 1.5f).normalized;
                var disc = Disc("Mirror", hat, Head.Surface(y, theta, 0.012f) + facing * 0.012f, 0.11f, 0.008f, mirror, UpAlong(facing)).transform;
                Prim(PrimitiveType.Cylinder, "Hole", disc, new Vector3(0f, 0.6f, 0f), new Vector3(0.28f, 0.6f, 0.28f), Fixed("MirrorHole", "#2B3245", 0.3f));
            },
        };

        // ------------------------------------------------------------------ fits (on the body)

        static readonly Dictionary<string, Action<Ctx, string>> OutfitBuilders = new()
        {
            // The everyday fit: a sky-blue camp shirt, navy shorts with a belt, and white socks.
            [AvatarCatalog.DefaultOutfit] = (c, id) =>
            {
                CampShirt(c, id);
                Shorts(c, id, Fixed("Navy", "#33406E", 0.15f), Fixed("Belt", "#6B4226", 0.3f));
                Socks(c, id);
            },
            ["sailor"] = (c, id) =>
            {
                var white = Fixed("SailorShirt", "#FAFBFF", 0.15f);
                var navy = Fixed("SailorNavy", "#2E3A6B", 0.2f);
                var body = Shirt(c, id, white, Torso.Top - 0.02f, Sleeve.Short);
                Part("Collar", body, Torso.Shell("Outfit_sailor_collar", Torso.Top - 0.09f, Torso.Top - 0.015f, 0.02f, a0: Mathf.PI - 1.1f, a1: Mathf.PI + 1.1f,
                    doubleSided: true, segments: 20), navy, Vector3.zero, Quaternion.identity, null);
                Part("Tie", body, Cone(0.04f, 0.07f), Fixed("SailorTie", "#E8434F", 0.2f), Torso.Surface(Torso.Top - 0.06f, 0f, 0.02f), Quaternion.Euler(180f, 0f, 0f), null);
                Shorts(c, id, navy, null);
            },
            ["tropical"] = (c, id) =>
            {
                var body = Shirt(c, id, Fixed("Tropical", "#FF9A3C", 0.15f), Torso.Top - 0.02f, Sleeve.Short);
                var flowers = new[] { Fixed("FlowerA", "#FFFFFF", 0.2f), Fixed("FlowerB", "#FF7FBF", 0.2f), Fixed("FlowerC", "#FFE066", 0.2f) };
                for (var i = 0; i < 9; i++)
                {
                    var theta = -1.4f + (i % 5) * 0.7f + (i / 5) * 0.35f;
                    var y = 0.02f + (i % 3) * 0.075f;
                    Sphere("Flower", body, Torso.Surface(y, theta, 0.014f), new Vector3(0.035f, 0.035f, 0.008f), flowers[i % 3], rot: Facing(Torso, y, theta));
                }
                Shorts(c, id, Fixed("TropicalShorts", "#2EC4B6", 0.15f), null);
            },
            ["climber"] = (c, id) =>
            {
                Shirt(c, id, Fixed("Climber", "#FF7A1A", 0.2f), Torso.Top - 0.015f, Sleeve.Long);
                var harness = c.Item($"Outfit_{id}_Harness", c.Body);
                Ring(Torso, "Harness", harness, Waist + 0.01f, 0.024f, 0.016f, Fixed("Harness", "#2B2F45", 0.3f));
                Sphere("Carabiner", harness, Torso.Surface(Waist, 0.5f, 0.04f), new Vector3(0.025f, 0.04f, 0.01f), Fixed("Carabiner", "#C9D1DC", 0.8f));
                Shorts(c, id, Fixed("ClimberPants", "#5C6B82", 0.15f), null, LegCover.Long);
            },
            ["ice"] = (c, id) =>
            {
                var blue = Fixed("IcePuffer", "#7FC4F5", 0.25f);
                var body = Shirt(c, id, blue, Torso.Top - 0.01f, Sleeve.Long, offset: 0.02f, extra: 0.02f);
                foreach (var y in new[] { -0.01f, 0.07f, 0.15f, 0.22f })
                    Ring(Torso, "Puff", body, y, 0.022f, 0.03f, blue);
                Ring(Torso, "Collar", body, Torso.Top - 0.01f, 0.022f, 0.03f, Fixed("IceFur", "#FAFBFF", 0.1f));
                Shorts(c, id, Fixed("IcePants", "#FAFBFF", 0.15f), null, LegCover.Long);
            },
            ["cowboy"] = (c, id) =>
            {
                var body = Shirt(c, id, Fixed("CowboyShirt", "#F4EBDC", 0.15f), Torso.Top - 0.02f, Sleeve.Long);
                Part("Vest", body, Torso.Shell("Outfit_cowboy_vest", Waist - 0.02f, Torso.Top - 0.03f, 0.024f, a0: 0.45f, a1: Mathf.PI * 2f - 0.45f, doubleSided: true),
                    Fixed("CowboyVest", "#8B5A2B", 0.2f), Vector3.zero, Quaternion.identity, null);
                Part("Bandana", body, Cone(0.05f, 0.07f), Fixed("CowboyBandana", "#D93A3A", 0.2f), Torso.Surface(Torso.Top - 0.04f, 0f, 0.02f), Quaternion.Euler(180f, 0f, 0f), null);
                var jeans = Shorts(c, id, Fixed("Jeans", "#4A6A9E", 0.15f), Fixed("Belt", "#6B4226", 0.3f), LegCover.Long);
                Box("Buckle", jeans, Torso.Surface(Waist, 0f, 0.03f), new Vector3(0.05f, 0.035f, 0.01f), Fixed("Buckle", "#FFC83D", 0.8f), Facing(Torso, Waist, 0f));
            },
            ["aviator"] = (c, id) =>
            {
                var body = Shirt(c, id, Fixed("Leather", "#7A4A2A", 0.35f), Torso.Top - 0.02f, Sleeve.Long, offset: 0.016f);
                Ring(Torso, "FurCollar", body, Torso.Top - 0.025f, 0.02f, 0.035f, Fixed("Fur", "#EADFC8", 0.05f));
                Box("Zip", body, Torso.Surface(0.12f, 0f, 0.02f), new Vector3(0.012f, 0.18f, 0.006f), Fixed("Zip", "#C9D1DC", 0.8f), Facing(Torso, 0.12f, 0f));
                Shorts(c, id, Fixed("Khaki", "#C9A66B", 0.15f), Fixed("Belt", "#6B4226", 0.3f), LegCover.Long);
            },
            ["gnome"] = (c, id) =>
            {
                var body = Shirt(c, id, Fixed("GnomeTunic", "#3B6FD9", 0.15f), Torso.Top - 0.02f, Sleeve.Long, flare: 0.05f);
                Ring(Torso, "Belt", body, Waist, 0.026f, 0.02f, Fixed("Belt", "#6B4226", 0.3f));
                Box("Buckle", body, Torso.Surface(Waist, 0f, 0.045f), new Vector3(0.045f, 0.035f, 0.01f), Fixed("Buckle", "#FFC83D", 0.8f), Facing(Torso, Waist, 0f));
                LegSleeves(c, id, Fixed("GnomeTights", "#5A3A28", 0.15f), 1f);
            },
            ["toga"] = (c, id) =>
            {
                var white = Fixed("Toga", "#FAF6EC", 0.15f);
                var body = Shirt(c, id, white, 0.2f, Sleeve.None, flare: 0.04f, collar: false);
                // A drape over one shoulder.
                Part("Drape", body, Ribbon("Outfit_toga_drape", th => 0.14f - 0.11f * Mathf.Sin(th), 0.09f, 0.02f), white, Vector3.zero, Quaternion.identity, null);
                Ring(Torso, "Belt", body, Waist + 0.02f, 0.026f, 0.012f, Fixed("TogaGold", "#FFC83D", 0.8f));
            },
            ["mummy"] = (c, id) =>
            {
                var wrap = Fixed("Bandage", "#EDE3C8", 0.1f);
                var body = Shirt(c, id, wrap, Torso.Top - 0.01f, Sleeve.Long);
                for (var i = 0; i < 6; i++)
                {
                    var ring = Ring(Torso, "Wrap", body, Torso.Bottom + 0.05f + i * 0.045f, 0.014f, 0.01f, wrap).transform;
                    ring.localRotation = Quaternion.Euler(i % 2 == 0 ? 6f : -6f, 0f, i % 2 == 0 ? 5f : -5f);
                }
                LegSleeves(c, id, wrap, 1f);
            },
            ["skele"] = (c, id) =>
            {
                var black = Fixed("Skele", "#2B2B33", 0.2f);
                var bone = Fixed("Bone", "#F4F1EA", 0.2f);
                var body = Shirt(c, id, black, Torso.Top - 0.01f, Sleeve.Long);
                for (var i = 0; i < 4; i++)
                {
                    var y = 0.2f - i * 0.05f;
                    foreach (var side in new[] { -1f, 1f })
                    {
                        var theta = side * 0.42f;
                        Box("Rib", body, Torso.Surface(y, theta, 0.016f), new Vector3(0.08f - i * 0.006f, 0.016f, 0.004f), bone, Facing(Torso, y, theta));
                    }
                }
                Box("Sternum", body, Torso.Surface(0.14f, 0f, 0.016f), new Vector3(0.022f, 0.15f, 0.004f), bone, Facing(Torso, 0.14f, 0f));
                LegSleeves(c, id, black, 1f);
            },
            ["jester"] = (c, id) =>
            {
                var purple = Fixed("JesterA", "#7B4FC9", 0.25f);
                var yellow = Fixed("JesterB", "#FFC845", 0.25f);
                var body = c.Item($"Outfit_{id}_Body", c.Body);
                Part("HalfA", body, Torso.Shell("Outfit_jester_a", Torso.Bottom - 1f, Torso.Top - 0.02f, 0.014f, a0: 0f, a1: Mathf.PI), purple, Vector3.zero, Quaternion.identity, null);
                Part("HalfB", body, Torso.Shell("Outfit_jester_b", Torso.Bottom - 1f, Torso.Top - 0.02f, 0.014f, a0: Mathf.PI, a1: Mathf.PI * 2f), yellow, Vector3.zero, Quaternion.identity, null);
                for (var i = 0; i < 8; i++)
                {
                    var theta = i * Mathf.PI * 2f / 8f;
                    var p = Torso.Surface(Torso.Top - 0.03f, theta, 0.03f);
                    Part("Point", body, Cone(0.03f, 0.06f), i % 2 == 0 ? purple : yellow, p, UpAlong(Torso.Normal(Torso.Top - 0.03f, theta) + Vector3.down * 0.8f), null);
                }
                SleevePair(c, id, yellow, purple, true);
                LegSleevesPair(c, id, purple, yellow);
            },
            ["fairy"] = (c, id) =>
            {
                var body = Shirt(c, id, Fixed("Fairy", "#FF9FC8", 0.25f), Torso.Top - 0.04f, Sleeve.None, flare: 0.08f, collar: false);
                var wing = Fixed("Wing", "#BFE8FF", 0.6f, Hex("#7FD0FF") * 0.15f);
                var back = Torso.Surface(0.17f, Mathf.PI, 0.02f);
                foreach (var side in new[] { -1f, 1f })
                {
                    Sphere("WingTop", body, back + new Vector3(side * 0.11f, 0.06f, -0.03f), new Vector3(0.18f, 0.14f, 0.012f), wing, rot: Quaternion.Euler(0f, side * 20f, side * -25f));
                    Sphere("WingLow", body, back + new Vector3(side * 0.08f, -0.06f, -0.03f), new Vector3(0.12f, 0.09f, 0.012f), wing, rot: Quaternion.Euler(0f, side * 20f, side * 30f));
                }
            },
            ["knight"] = (c, id) =>
            {
                var steel = Fixed("Steel", "#C9D1DC", 0.75f);
                var body = Shirt(c, id, steel, Torso.Top - 0.015f, Sleeve.Long, offset: 0.018f);
                Ring(Torso, "Belt", body, Waist, 0.024f, 0.018f, Fixed("Belt", "#6B4226", 0.3f));
                foreach (var side in new[] { -1f, 1f })
                    Sphere("Pauldron", body, new Vector3(side * (Torso.RadiusAt(ShoulderY) - 0.01f), ShoulderY + 0.025f, 0f), new Vector3(0.1f, 0.07f, 0.1f), steel);
                var red = Fixed("KnightCross", "#E8434F", 0.3f);
                Box("CrossV", body, Torso.Surface(0.15f, 0f, 0.026f), new Vector3(0.03f, 0.11f, 0.008f), red, Facing(Torso, 0.15f, 0f));
                Box("CrossH", body, Torso.Surface(0.17f, 0f, 0.026f), new Vector3(0.09f, 0.03f, 0.008f), red, Facing(Torso, 0.17f, 0f));
                LegSleeves(c, id, steel, 1f);
            },
            ["astronaut"] = (c, id) =>
            {
                var suit = Fixed("Spacesuit", "#F4F6FA", 0.3f);
                var grey = Fixed("SpaceGrey", "#8A93A6", 0.4f);
                var body = Shirt(c, id, suit, Torso.Top - 0.01f, Sleeve.Long, offset: 0.02f, extra: 0.012f, collar: false);
                Ring(Torso, "Collar", body, Torso.Top - 0.01f, 0.024f, 0.028f, grey);
                Box("Panel", body, Torso.Surface(0.13f, 0f, 0.035f), new Vector3(0.12f, 0.08f, 0.025f), grey, Facing(Torso, 0.13f, 0f));
                var lights = new[] { "#E8434F", "#FFC845", "#4C8DF6" };
                for (var i = 0; i < 3; i++)
                    Sphere("Light", body, Torso.Surface(0.13f, (i - 1) * 0.17f, 0.05f), Vector3.one * 0.02f, Fixed("SpaceLight" + i, lights[i], 0.6f, Hex(lights[i]) * 0.3f));
                Box("Pack", body, Torso.Surface(0.13f, Mathf.PI, 0.07f), new Vector3(0.24f, 0.26f, 0.11f), suit);
                LegSleeves(c, id, suit, 1f, extra: 0.012f);
            },
            [AvatarLook.DoctorOutfit] = (c, id) =>
            {
                var scrubs = Fixed("Scrubs", "#4C8DF6", 0.15f);
                var coat = Fixed("Coat", "#FAFBFF", 0.2f);
                var body = Shirt(c, id, scrubs, Torso.Top - 0.02f, Sleeve.None);
                Part("Coat", body, Torso.Shell("Outfit_labcoat_coat", Torso.Bottom - 1f, Torso.Top - 0.01f, 0.026f, flare: 0.03f, flareFrom: 0.08f,
                    a0: 0.36f, a1: Mathf.PI * 2f - 0.36f, doubleSided: true), coat, Vector3.zero, Quaternion.identity, null);
                Box("Pocket", body, Torso.Surface(0.15f, -0.62f, 0.034f), new Vector3(0.06f, 0.055f, 0.012f), Fixed("CoatPocket", "#E3E8F2", 0.2f), Facing(Torso, 0.15f, -0.62f));
                Part("Pen", body, Capsule(0.007f, 0.045f), scrubs, Torso.Surface(0.2f, -0.6f, 0.034f), Quaternion.identity, null);
                var badge = Box("Badge", body, Torso.Surface(0.18f, 0.6f, 0.034f), new Vector3(0.06f, 0.04f, 0.01f), Fixed("Badge", "#4C8DF6", 0.5f), Facing(Torso, 0.18f, 0.6f)).transform;
                var white = Fixed("BadgeCross", "#FFFFFF", 0.1f);
                Box("CrossH", badge, new Vector3(0f, 0f, 0.6f), new Vector3(0.45f, 0.2f, 0.2f), white);
                Box("CrossV", badge, new Vector3(0f, 0f, 0.6f), new Vector3(0.14f, 0.7f, 0.2f), white);
                SleevePair(c, id, coat, coat, true);
                LegSleeves(c, id, scrubs, 1f);
            },
        };

        enum Sleeve
        {
            None,
            Short,
            Long,
        }

        enum LegCover
        {
            Short,
            Long,
        }

        /// <summary>A top: a shell over the torso from the bottom up to <paramref name="top"/>, with a rolled collar and sleeves.</summary>
        static Transform Shirt(Ctx c, string id, Material mat, float top, Sleeve sleeves, float offset = 0.012f, float extra = 0.008f, float flare = 0f,
            bool collar = true)
        {
            var body = c.Item($"Outfit_{id}_Body", c.Body);
            Part("Top", body, Torso.Shell($"Outfit_{id}", Torso.Bottom - 1f, top, offset, flare, Waist), mat, Vector3.zero, Quaternion.identity, null);
            if (collar) Ring(Torso, "Collar", body, top, offset + 0.002f, 0.013f, mat);
            if (sleeves != Sleeve.None) SleevePair(c, id, mat, mat, sleeves == Sleeve.Long, extra);
            return body;
        }

        /// <summary>The everyday shirt: sky blue, short sleeves with turned-up cuffs, a white pointed collar and a chest pocket.</summary>
        static void CampShirt(Ctx c, string id)
        {
            var shirt = Fixed("CampShirt", "#7CC4F2", 0.12f);
            var white = Fixed("CampCollar", "#FAFBFF", 0.12f);
            var body = Shirt(c, id, shirt, Torso.Top - 0.012f, Sleeve.Short, offset: 0.012f, extra: 0.014f, collar: false);
            // Two collar points folded over the chest, either side of the neck.
            var neck = Torso.Top - 0.02f;
            var flap = CollarFlap();
            foreach (var side in new[] { -1f, 1f })
            {
                var theta = side * 0.3f;
                var y = neck - 0.006f;
                var p = Torso.Surface(y, theta, 0.019f);
                // Points down and a little outward, lying flat on the shirt.
                Part("CollarPoint", body, flap, white, p, Facing(Torso, y, theta) * Quaternion.Euler(0f, 0f, side * 22f), null);
            }
            Ring(Torso, "CollarBand", body, neck, 0.016f, 0.012f, white);
            // A pocket with a flap on the left of the chest, and a seam of buttons down the front.
            const float pocketTheta = 0.5f, pocketY = 0.23f;
            Box("Pocket", body, Torso.Surface(pocketY, pocketTheta, 0.02f), new Vector3(0.06f, 0.055f, 0.01f), Fixed("CampPocket", "#5FA7D8", 0.12f),
                Facing(Torso, pocketY, pocketTheta));
            foreach (var y in new[] { 0.12f, 0.19f, 0.26f })
                Sphere("Button", body, Torso.Surface(y, 0f, 0.018f), new Vector3(0.016f, 0.016f, 0.008f), white, rot: Facing(Torso, y, 0f));
            // Turned-up cuffs on the sleeves.
            foreach (var (shoulder, tag) in new[] { (c.ShoulderL, "L"), (c.ShoulderR, "R") })
            {
                var cuff = c.Item($"Outfit_{id}_Cuff{tag}", shoulder);
                Part("Cuff", cuff, Torus(ArmRadius + 0.016f, 0.011f), shirt, new Vector3(0f, -c.UpperArm * 0.6f + 0.012f, 0f), Quaternion.identity, null);
            }
        }

        /// <summary>A flat triangular collar point (pointing down -Y from its top edge), facing +Z, both sides drawn.</summary>
        static Mesh CollarFlap()
        {
            const float w = 0.036f, h = 0.05f, d = 0.004f;
            var mesh = new Mesh { name = "CollarFlap" };
            var v = new List<Vector3>();
            var n = new List<Vector3>();
            foreach (var z in new[] { d, -d })
            {
                v.Add(new Vector3(-w, 0f, z));
                v.Add(new Vector3(w, 0f, z));
                v.Add(new Vector3(0f, -h, z));
                for (var i = 0; i < 3; i++) n.Add(new Vector3(0f, 0f, Mathf.Sign(z)));
            }
            mesh.SetVertices(v);
            mesh.SetNormals(n);
            mesh.SetTriangles(new[] { 0, 2, 1, 3, 4, 5 }, 0);
            mesh.RecalculateBounds();
            return Save(mesh);
        }

        /// <summary>White socks above the boots.</summary>
        static void Socks(Ctx c, string id)
        {
            var white = Fixed("Sock", "#F6F6F2", 0.1f);
            foreach (var (leg, tag) in new[] { (c.LegL, "L"), (c.LegR, "R") })
            {
                var sock = c.Item($"Outfit_{id}_Sock{tag}", leg);
                Part("Sock", sock, Capsule(LegRadius + 0.006f, 0.05f), white, new Vector3(0f, -HipsY + 0.13f, 0f), Quaternion.identity, null);
            }
        }

        static void SleevePair(Ctx c, string id, Material left, Material right, bool longSleeves, float extra = 0.008f)
        {
            foreach (var (shoulder, elbow, tag, mat) in new[] { (c.ShoulderL, c.ElbowL, "L", left), (c.ShoulderR, c.ElbowR, "R", right) })
            {
                var upper = c.Item($"Outfit_{id}_Arm{tag}", shoulder);
                Part("Sleeve", upper, Capsule(ArmRadius + extra, longSleeves ? c.UpperArm : c.UpperArm * 0.6f), mat, Vector3.zero, Quaternion.identity, null);
                if (!longSleeves) continue;
                var lower = c.Item($"Outfit_{id}_Fore{tag}", elbow);
                Part("Sleeve", lower, Capsule(ArmRadius * 0.95f + extra, c.Forearm * 0.75f), mat, Vector3.zero, Quaternion.identity, null);
            }
        }

        /// <summary>Shorts (or trousers): over the bottom of the torso and down the legs, with an optional belt.</summary>
        static Transform Shorts(Ctx c, string id, Material mat, Material belt, LegCover cover = LegCover.Short)
        {
            var shorts = c.Item($"Outfit_{id}_Shorts", c.Body);
            Part("Seat", shorts, Torso.Shell($"Outfit_{id}_shorts", Torso.Bottom - 1f, Waist, 0.016f), mat, Vector3.zero, Quaternion.identity, null);
            if (belt != null) Ring(Torso, "Belt", shorts, Waist, 0.018f, 0.014f, belt);
            LegSleeves(c, id, mat, cover == LegCover.Long ? 1f : 0.45f);
            return shorts;
        }

        /// <summary>Cloth over both legs, a fraction of the way down.</summary>
        static void LegSleeves(Ctx c, string id, Material mat, float fraction, float extra = 0.008f) => LegSleevesPair(c, id, mat, mat, fraction, extra);

        static void LegSleevesPair(Ctx c, string id, Material left, Material right, float fraction = 1f, float extra = 0.008f)
        {
            foreach (var (leg, tag, mat) in new[] { (c.LegL, "L", left), (c.LegR, "R", right) })
            {
                var cover = c.Item($"Outfit_{id}_Leg{tag}", leg);
                Part("Cloth", cover, Capsule(LegRadius + extra, LegLength * fraction), mat, Vector3.zero, Quaternion.identity, null);
            }
        }

        /// <summary>Every hat and fit a player can wear (everything in the catalog but "none").</summary>
        public static IEnumerable<string> CatalogIds(Slot slot) =>
            AvatarCatalog.For(slot).Select(i => i.Id).Where(id => id != AvatarCatalog.None);
    }
}
