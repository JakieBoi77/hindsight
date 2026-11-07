using System.IO;
using System.Collections.Generic;
using System.Linq;
using Hindsight.Ophthalmology.SceneObjects;
using TMPro;
using UnityEditor;
using UnityEngine;
using static Hindsight.Ophthalmology.EditorTools.BuilderUtil;

namespace Hindsight.Ophthalmology.EditorTools
{
    /// <summary>
    /// Builds the reusable prefabs. Convention: gameplay components and colliders live on the
    /// prefab root; the art lives under a "Visual" child so it can be swapped for better models later.
    /// </summary>
    internal static class PrefabBuilder
    {
        const string KenneyDir = "Assets/ThirdParty/Kenney/FurnitureKit";
        const string DoctorModel = "Assets/ThirdParty/KayKit/DoctorBase_Rogue.glb";
        const string DoctorAtlasSource = "Assets/ThirdParty/KayKit/rogue_texture.png";
        const string DoctorAtlas = MaterialsDir + "/DoctorAtlas.png";
        const string ChairModel = "Assets/ThirdParty/PolyPizza/ExamChair.glb";

        public const string RoomPath = PrefabsDir + "/ExamRoom.prefab";
        public const string ChairPath = PrefabsDir + "/ExamChair.prefab";
        public const string DoctorPath = PrefabsDir + "/Doctor.prefab";
        public const string BottlePath = PrefabsDir + "/EyeDropBottle.prefab";
        public const string ScopePath = PrefabsDir + "/Ophthalmoscope.prefab";
        public const string EyeModelPath = PrefabsDir + "/EyeModel.prefab";

        public const float RoomHalf = 3f;
        public const float RoomHeight = 3f;

        public static void BuildAll()
        {
            EnsureFolder(PrefabsDir);
            BuildRoom();
            BuildChair();
            BuildBottle();
            BuildOphthalmoscope();
            BuildDoctor(); // holds the bottle and the light, so it is built after them
            BuildEyeModel();
        }

        static void Save(GameObject go, string path)
        {
            PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
        }

        // ---------------- Room ----------------

        static float kenneyScale = -1f;

        /// <summary>Kenney models share a scale, so measure one known object and apply the same factor to all.</summary>
        static float KenneyScale()
        {
            if (kenneyScale > 0) return kenneyScale;
            var probe = InstantiateModel($"{KenneyDir}/kitchenCabinet.fbx", null);
            var h = WorldBounds(probe).size.y;
            Object.DestroyImmediate(probe);
            kenneyScale = h > 0.0001f ? 0.9f / h : 1f;
            return kenneyScale;
        }

        public static GameObject KenneyProp(string model, string name, Transform parent, Vector3 pos, float yaw)
        {
            var root = new GameObject(name);
            root.transform.SetParent(parent, false);
            root.transform.localPosition = pos;
            root.transform.localRotation = Quaternion.Euler(0, yaw, 0);
            var visual = InstantiateModel($"{KenneyDir}/{model}.fbx", root.transform);
            if (visual == null) return root;
            visual.name = "Visual";
            ScaleUniform(visual, KenneyScale());
            GroundAndCenter(visual);
            return root;
        }

        static void BuildRoom()
        {
            var room = new GameObject("ExamRoom");
            var shell = new GameObject("Shell").transform;
            shell.SetParent(room.transform, false);

            var wall = Lit("Wall_Mint", new Color(0.80f, 0.93f, 0.89f));
            var accent = Lit("Wall_Sky", new Color(0.72f, 0.85f, 0.98f));
            var floor = Lit("Floor", new Color(0.86f, 0.80f, 0.70f), 0.35f);
            var ceiling = Lit("Ceiling", new Color(0.97f, 0.97f, 0.97f));
            var trim = Lit("Trim", new Color(0.35f, 0.62f, 0.78f));
            const float s = RoomHalf * 2f;

            Primitive(PrimitiveType.Cube, "Floor", shell, new Vector3(0, -0.05f, 0), new Vector3(s, 0.1f, s), floor, true);
            Primitive(PrimitiveType.Cube, "Ceiling", shell, new Vector3(0, RoomHeight + 0.05f, 0), new Vector3(s, 0.1f, s), ceiling);
            Primitive(PrimitiveType.Cube, "Wall_North", shell, new Vector3(0, RoomHeight / 2, RoomHalf + 0.05f), new Vector3(s, RoomHeight, 0.1f), accent);
            Primitive(PrimitiveType.Cube, "Wall_South", shell, new Vector3(0, RoomHeight / 2, -RoomHalf - 0.05f), new Vector3(s, RoomHeight, 0.1f), wall);
            Primitive(PrimitiveType.Cube, "Wall_East", shell, new Vector3(RoomHalf + 0.05f, RoomHeight / 2, 0), new Vector3(0.1f, RoomHeight, s), wall);
            Primitive(PrimitiveType.Cube, "Wall_West", shell, new Vector3(-RoomHalf - 0.05f, RoomHeight / 2, 0), new Vector3(0.1f, RoomHeight, s), wall);
            // Baseboards
            Primitive(PrimitiveType.Cube, "Base_N", shell, new Vector3(0, 0.06f, RoomHalf - 0.01f), new Vector3(s, 0.12f, 0.03f), trim);
            Primitive(PrimitiveType.Cube, "Base_S", shell, new Vector3(0, 0.06f, -RoomHalf + 0.01f), new Vector3(s, 0.12f, 0.03f), trim);
            Primitive(PrimitiveType.Cube, "Base_E", shell, new Vector3(RoomHalf - 0.01f, 0.06f, 0), new Vector3(0.03f, 0.12f, s), trim);
            Primitive(PrimitiveType.Cube, "Base_W", shell, new Vector3(-RoomHalf + 0.01f, 0.06f, 0), new Vector3(0.03f, 0.12f, s), trim);

            BuildDoor(shell);
            BuildEyeChart(shell);
            BuildPoster(shell);

            var props = new GameObject("Props").transform;
            props.SetParent(room.transform, false);

            // West wall: counter of cabinets with upper cupboards (front faces +X into the room).
            var x = -RoomHalf + 0.3f;
            KenneyProp("kitchenCabinetDrawer", "Cabinet_1", props, new Vector3(x, 0, -0.9f), 90);
            KenneyProp("kitchenCabinet", "Cabinet_2", props, new Vector3(x, 0, -0.3f), 90);
            KenneyProp("kitchenCabinet", "Cabinet_3", props, new Vector3(x, 0, 0.3f), 90);
            KenneyProp("kitchenCabinetUpper", "Upper_1", props, new Vector3(x, 1.55f, -0.9f), 90);
            KenneyProp("kitchenCabinetUpper", "Upper_2", props, new Vector3(x, 1.55f, -0.3f), 90);
            KenneyProp("kitchenCabinetUpper", "Upper_3", props, new Vector3(x, 1.55f, 0.3f), 90);
            KenneyProp("plantSmall1", "CounterPlant", props, new Vector3(x, 0.9f, 0.3f), 0);
            KenneyProp("books", "CounterBooks", props, new Vector3(x, 0.9f, -0.9f), 90);
            KenneyProp("bathroomSink", "Sink", props, new Vector3(x, 0, 1.4f), 90);

            // East wall: the doctor's desk.
            var desk = KenneyProp("desk", "Desk", props, new Vector3(RoomHalf - 0.45f, 0, -1.3f), -90);
            var deskTop = WorldBounds(desk).max.y;
            KenneyProp("computerScreen", "Monitor", props, new Vector3(RoomHalf - 0.35f, deskTop, -1.3f), -90);
            KenneyProp("computerKeyboard", "Keyboard", props, new Vector3(RoomHalf - 0.62f, deskTop, -1.3f), -90);
            KenneyProp("chairDesk", "DeskChair", props, new Vector3(RoomHalf - 1.05f, 0, -1.3f), 90);
            KenneyProp("trashcan", "Trash", props, new Vector3(RoomHalf - 0.3f, 0, -2.4f), 0);
            KenneyProp("coatRackStanding", "CoatRack", props, new Vector3(RoomHalf - 0.35f, 0, 2.5f), 0);

            // South wall (behind the chair): bookcase with a teddy bear on top.
            var shelf = KenneyProp("bookcaseClosed", "Bookcase", props, new Vector3(-1.6f, 0, -RoomHalf + 0.3f), 0);
            KenneyProp("books", "ShelfBooks", props, new Vector3(-1.75f, WorldBounds(shelf).max.y, -RoomHalf + 0.3f), 0);
            KenneyProp("plantSmall2", "ShelfPlant", props, new Vector3(-1.35f, WorldBounds(shelf).max.y, -RoomHalf + 0.3f), 0);

            // Corners and floor.
            KenneyProp("pottedPlant", "Plant_NW", props, new Vector3(-RoomHalf + 0.4f, 0, RoomHalf - 0.4f), 0);
            KenneyProp("pottedPlant", "Plant_SE", props, new Vector3(RoomHalf - 0.4f, 0, -RoomHalf + 0.4f), 45);
            KenneyProp("rugRound", "Rug", props, new Vector3(1.2f, 0.002f, 1.3f), 0);

            // Next to the exam chair: tool tray (drawers) and the doctor's stool.
            KenneyProp("sideTableDrawers", "ToolTray", props, new Vector3(-0.85f, 0, -0.55f), 90);
            KenneyProp("stoolBar", "DoctorStool", props, new Vector3(-1.6f, 0, 1.7f), 0);

            // Ceiling lamp.
            var lamp = KenneyProp("lampSquareCeiling", "CeilingLamp", props, Vector3.zero, 0);
            var lampH = WorldBounds(lamp).size.y;
            lamp.transform.localPosition = new Vector3(0.6f, RoomHeight - lampH, 1.6f);

            Save(room, RoomPath);
        }

        static void BuildDoor(Transform parent)
        {
            var door = Lit("Door", new Color(0.62f, 0.45f, 0.30f));
            var knob = Lit("Knob", new Color(0.9f, 0.8f, 0.4f), 0.8f);
            var z = -RoomHalf + 0.02f;
            Primitive(PrimitiveType.Cube, "Door", parent, new Vector3(1.6f, 1.05f, z), new Vector3(1f, 2.1f, 0.04f), door);
            Primitive(PrimitiveType.Sphere, "DoorKnob", parent, new Vector3(1.2f, 1.0f, z + 0.04f), Vector3.one * 0.07f, knob);
        }

        static void BuildEyeChart(Transform parent)
        {
            var board = Lit("ChartWhite", Color.white, 0.1f);
            var frame = Lit("ChartFrame", new Color(0.2f, 0.3f, 0.45f));
            var chart = new GameObject("EyeChart").transform;
            chart.SetParent(parent, false);
            chart.localPosition = new Vector3(0, 1.55f, RoomHalf - 0.02f);
            // Identity rotation: local -Z faces into the room, which is the side TextMeshPro renders on.
            Primitive(PrimitiveType.Cube, "Frame", chart, new Vector3(0, 0, 0.012f), new Vector3(0.72f, 1.02f, 0.02f), frame);
            Primitive(PrimitiveType.Cube, "Board", chart, Vector3.zero, new Vector3(0.66f, 0.96f, 0.02f), board);

            var textGo = new GameObject("Letters");
            textGo.transform.SetParent(chart, false);
            textGo.transform.localPosition = new Vector3(0, 0, -0.012f); // in front of the board
            var tmp = textGo.AddComponent<TextMeshPro>();
            tmp.font = TmpFont;
            tmp.rectTransform.sizeDelta = new Vector2(0.6f, 0.9f);
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.black;
            tmp.fontSize = 1f;
            tmp.text = "<size=200%><b>E</b></size>\n<size=130%>F P</size>\nT O Z\n<size=80%>L P E D</size>\n<size=60%>P E C F D</size>\n<size=45%>E D F C Z P</size>\n<size=35%>F E L O P Z D</size>";
        }

        static void BuildPoster(Transform parent)
        {
            var paper = Lit("PosterPaper", new Color(1f, 0.96f, 0.85f));
            var sclera = Lit("EyeWhite", Color.white, 0.6f);
            var iris = Lit("EyeIris", new Color(0.25f, 0.55f, 0.85f), 0.6f);
            var pupil = Lit("EyePupil", new Color(0.03f, 0.03f, 0.05f), 0.8f);

            var poster = new GameObject("EyePoster").transform;
            poster.SetParent(parent, false);
            poster.localPosition = new Vector3(RoomHalf - 0.02f, 1.6f, 0.9f);
            poster.localRotation = Quaternion.Euler(0, 90, 0); // local -Z faces into the room (-X)
            Primitive(PrimitiveType.Cube, "Paper", poster, Vector3.zero, new Vector3(0.8f, 1f, 0.01f), paper);
            Primitive(PrimitiveType.Sphere, "Eye", poster, new Vector3(0, 0.05f, -0.01f), new Vector3(0.5f, 0.3f, 0.02f), sclera);
            Primitive(PrimitiveType.Cylinder, "Iris", poster, new Vector3(0, 0.05f, -0.025f), new Vector3(0.24f, 0.004f, 0.24f), iris)
                .transform.localRotation = Quaternion.Euler(90, 0, 0);
            Primitive(PrimitiveType.Cylinder, "Pupil", poster, new Vector3(0, 0.05f, -0.032f), new Vector3(0.1f, 0.004f, 0.1f), pupil)
                .transform.localRotation = Quaternion.Euler(90, 0, 0);

            var title = new GameObject("Title");
            title.transform.SetParent(poster, false);
            title.transform.localPosition = new Vector3(0, 0.36f, -0.01f);
            var tmp = title.AddComponent<TextMeshPro>();
            tmp.font = TmpFont;
            tmp.rectTransform.sizeDelta = new Vector2(0.75f, 0.2f);
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontSize = 0.9f;
            tmp.color = new Color(0.15f, 0.3f, 0.5f);
            tmp.text = "<b>Your Amazing Eyes!</b>";

            var caption = Object.Instantiate(title, poster);
            caption.name = "Caption";
            caption.transform.localPosition = new Vector3(0, -0.32f, -0.01f);
            var cap = caption.GetComponent<TextMeshPro>();
            cap.fontSize = 0.55f;
            cap.text = "The pupil is the window\ninto your eye";
        }

        // ---------------- Exam chair ----------------

        static void BuildChair()
        {
            var root = new GameObject("ExamChair");
            var visual = InstantiateModel(ChairModel, root.transform);
            visual.name = "Visual";

            // Fit the longest horizontal side to 1.8 m.
            var b = WorldBounds(visual);
            ScaleUniform(visual, 1.8f / Mathf.Max(b.size.x, b.size.z));

            // Turn it so the headrest (highest point) is at -Z and the footrest points +Z.
            var top = HighestVertex(visual);
            b = WorldBounds(visual);
            var toHead = new Vector3(top.x - b.center.x, 0, top.z - b.center.z);
            visual.transform.rotation = Quaternion.AngleAxis(Vector3.SignedAngle(toHead, Vector3.back, Vector3.up), Vector3.up) * visual.transform.rotation;
            GroundAndCenter(visual);

            b = WorldBounds(visual);
            top = HighestVertex(visual);
            var col = root.AddComponent<BoxCollider>();
            col.center = b.center - root.transform.position;
            col.size = b.size;

            // Where the seated child's eyes are: just in front of and a little above the headrest top.
            var head = new GameObject("Head").transform;
            head.SetParent(root.transform, false);
            head.localPosition = new Vector3(0, top.y + 0.08f, top.z + 0.18f);

            var highlight = new GameObject("Highlight");
            highlight.transform.SetParent(root.transform, false);
            highlight.transform.localPosition = new Vector3(0, b.max.y + 0.25f, b.center.z);
            var glow = Lit("Highlight", new Color(1f, 0.85f, 0.2f), 0.5f, new Color(1f, 0.75f, 0.1f) * 1.5f);
            Primitive(PrimitiveType.Cube, "Diamond", highlight.transform, Vector3.zero, Vector3.one * 0.16f, glow)
                .transform.localRotation = Quaternion.Euler(45, 0, 45);
            highlight.AddComponent<Bobber>();

            root.AddComponent<Interactable>().EditorInit("ExamChair", highlight);
            Save(root, ChairPath);
        }

        static Vector3 HighestVertex(GameObject go)
        {
            var best = new Vector3(0, float.MinValue, 0);
            foreach (var mf in go.GetComponentsInChildren<MeshFilter>())
            {
                var mesh = mf.sharedMesh;
                if (mesh == null || !mesh.isReadable) continue;
                var m = mf.transform.localToWorldMatrix;
                foreach (var v in mesh.vertices)
                {
                    var w = m.MultiplyPoint3x4(v);
                    if (w.y > best.y) best = w;
                }
            }
            if (best.y > float.MinValue) return best;

            // Meshes not readable: use the top of the tallest renderer instead.
            var tallest = go.GetComponentsInChildren<Renderer>().OrderByDescending(r => r.bounds.max.y).FirstOrDefault();
            if (tallest == null) return WorldBounds(go).center;
            var tb = tallest.bounds;
            return new Vector3(tb.center.x, tb.max.y, tb.center.z);
        }

        // ---------------- Doctor ----------------

        static void BuildDoctor()
        {
            EnsureLegacyAnimation(DoctorModel);
            var root = new GameObject("Doctor");
            var visual = InstantiateModel(DoctorModel, root.transform);
            visual.name = "Visual";
            PrefabUtility.UnpackPrefabInstance(visual, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);

            // The KayKit rogue, minus the adventuring gear.
            foreach (var junk in new[] { "Rogue_Cape", "Knife_Offhand", "1H_Crossbow", "2H_Crossbow", "Knife", "Throwable" })
            {
                var t = FindDeep(visual.transform, junk);
                if (t != null) Object.DestroyImmediate(t.gameObject);
            }

            // Tunic -> white coat, via a recoloured copy of the gradient atlas.
            AssetDatabase.DeleteAsset(MaterialsDir + "/Doctor.mat");
            var coat = new Material(Shader.Find("Universal Render Pipeline/Lit")) { name = "Doctor" };
            coat.SetTexture("_BaseMap", BuildDoctorAtlas());
            coat.SetColor("_BaseColor", Color.white);
            coat.SetFloat("_Smoothness", 0.25f);
            AssetDatabase.CreateAsset(coat, MaterialsDir + "/Doctor.mat");
            foreach (var r in visual.GetComponentsInChildren<Renderer>(true))
                r.sharedMaterials = Enumerable.Repeat(coat, r.sharedMaterials.Length).ToArray();

            var b = WorldBounds(visual);
            ScaleUniform(visual, 1.5f / b.size.y);
            GroundAndCenter(visual);

            var head = FindDeep(visual.transform, "head");
            var chest = FindDeep(visual.transform, "chest");
            var headMesh = visual.GetComponentsInChildren<SkinnedMeshRenderer>().FirstOrDefault(r => r.name.Contains("Head"));
            var bodyMesh = visual.GetComponentsInChildren<SkinnedMeshRenderer>().FirstOrDefault(r => r.name.Contains("Body"));

            Transform mouth = null;
            if (headMesh != null && head != null)
            {
                var hb = headMesh.bounds;
                var faceProbe = Probe(headMesh);

                // Mouth: a soft dark oval on the face, below the nose. Scaled vertically while talking.
                if (SurfacePoint(faceProbe, hb, 0.30f, out var mouthPoint, out var mouthNormal))
                {
                    var m = Primitive(PrimitiveType.Sphere, "Mouth", head, Vector3.zero, Vector3.one, Lit("DoctorMouth", new Color(0.3f, 0.1f, 0.12f), 0.3f));
                    m.transform.position = mouthPoint - mouthNormal * 0.005f; // mostly sunk into the face so it never sticks out
                    m.transform.rotation = Quaternion.LookRotation(mouthNormal, Vector3.up);
                    SetWorldScale(m.transform, new Vector3(hb.size.x * 0.11f, hb.size.y * 0.035f, 0.012f));
                    mouth = m.transform;
                }

                // Classic head mirror on the forehead.
                if (SurfacePoint(faceProbe, hb, 0.74f, out var browPoint, out var browNormal))
                {
                    var mirror = Primitive(PrimitiveType.Cylinder, "HeadMirror", head, Vector3.zero, Vector3.one, Lit("HeadMirror", new Color(0.86f, 0.9f, 0.95f), 0.9f));
                    mirror.transform.position = browPoint + browNormal * 0.015f;
                    mirror.transform.rotation = Quaternion.FromToRotation(Vector3.up, browNormal);
                    SetWorldScale(mirror.transform, new Vector3(hb.size.x * 0.19f, 0.012f, hb.size.x * 0.19f));
                    var hole = Primitive(PrimitiveType.Cylinder, "MirrorHole", mirror.transform, Vector3.zero, Vector3.one, Lit("HeadBand", new Color(0.18f, 0.22f, 0.32f), 0.3f));
                    hole.transform.localPosition = new Vector3(0, 0.6f, 0);
                    hole.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
                }
                Object.DestroyImmediate(faceProbe.gameObject);
            }

            // Name badge on the coat.
            if (bodyMesh != null && chest != null)
            {
                var probe = Probe(bodyMesh);
                var bb = bodyMesh.bounds;
                if (SurfacePoint(probe, bb, 0.72f, out var p, out var n, bb.center.x + bb.size.x * 0.18f))
                {
                    var badge = Primitive(PrimitiveType.Cube, "Badge", chest, Vector3.zero, Vector3.one, Lit("Badge", new Color(0.29f, 0.56f, 0.89f), 0.5f));
                    badge.transform.position = p + n * 0.006f;
                    badge.transform.rotation = Quaternion.LookRotation(n, Vector3.up);
                    SetWorldScale(badge.transform, new Vector3(0.09f, 0.055f, 0.012f));
                    var white = Lit("ChartWhite", Color.white, 0.1f);
                    Primitive(PrimitiveType.Cube, "CrossH", badge.transform, new Vector3(0, 0, 0.6f), new Vector3(0.45f, 0.2f, 0.2f), white);
                    Primitive(PrimitiveType.Cube, "CrossV", badge.transform, new Vector3(0, 0, 0.6f), new Vector3(0.12f, 0.75f, 0.2f), white);
                }
                Object.DestroyImmediate(probe.gameObject);
            }

            // Props in the hand slots, hidden until a step asks for them.
            // Placed in front of the fist in the bind pose (arms out, facing +Z), then parented to the hand slot.
            var drops = HeldProp(BottlePath, FindDeep(visual.transform, "handslot.l"), "HeldEyeDrops",
                new Vector3(0f, 0.09f, 0.07f), Quaternion.Euler(180f, 0f, 0f), 2.4f);
            var light = HeldProp(ScopePath, FindDeep(visual.transform, "handslot.r"), "HeldLight",
                new Vector3(0f, 0.1f, 0.07f), Quaternion.Euler(0f, 180f, 0f), 2f);

            var anim = visual.GetComponentInChildren<Animation>();
            if (anim != null) anim.playAutomatically = false;

            root.AddComponent<DoctorActor>().EditorInit(head, chest, FindDeep(visual.transform, "upperarm.r"),
                FindDeep(visual.transform, "lowerarm.r"), mouth, drops, light);
            Save(root, DoctorPath);
        }

        /// <summary>
        /// glTFast's editor import defaults to Mecanim clips with an empty Animator. DoctorActor drives
        /// legacy clips through an Animation component, so force the legacy method on the model.
        /// </summary>
        static void EnsureLegacyAnimation(string modelPath)
        {
            var importer = AssetImporter.GetAtPath(modelPath);
            var so = new SerializedObject(importer);
            var method = so.FindProperty("importSettings.animationMethod");
            if (method == null || method.intValue == 1) return; // 1 = GLTFast.AnimationMethod.Legacy
            method.intValue = 1;
            so.ApplyModifiedPropertiesWithoutUndo();
            importer.SaveAndReimport();
        }

        /// <summary>Copies the KayKit gradient atlas and repaints the swatches the tunic uses.</summary>
        static Texture2D BuildDoctorAtlas()
        {
            var srcImporter = (TextureImporter)AssetImporter.GetAtPath(DoctorAtlasSource);
            if (!srcImporter.isReadable)
            {
                srcImporter.isReadable = true;
                srcImporter.textureCompression = TextureImporterCompression.Uncompressed;
                srcImporter.SaveAndReimport();
            }
            var src = AssetDatabase.LoadAssetAtPath<Texture2D>(DoctorAtlasSource);
            var tex = new Texture2D(src.width, src.height, TextureFormat.RGBA32, false);
            tex.SetPixels(src.GetPixels());

            // Swatches are 128x256 cells on an 8x4 grid; (col, row) counted from the top-left like the PNG.
            Repaint(tex, 0, 1, 1, new Color(1f, 1f, 1f), new Color(0.8f, 0.85f, 0.93f));           // tunic body -> white coat
            Repaint(tex, 1, 1, 1, new Color(0.45f, 0.65f, 0.95f), new Color(0.26f, 0.43f, 0.76f)); // collar -> blue scrubs at the V-neck
            Repaint(tex, 1, 2, 1, new Color(0.45f, 0.65f, 0.95f), new Color(0.26f, 0.43f, 0.76f));

            File.WriteAllBytes(DoctorAtlas, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(DoctorAtlas, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(DoctorAtlas);
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(DoctorAtlas);
        }

        static void Repaint(Texture2D tex, int col, int row, int cols, Color top, Color bottom)
        {
            const int w = 128, h = 256;
            for (var y = 0; y < h; y++)
            {
                var c = Color.Lerp(top, bottom, (float)y / (h - 1));
                var ty = tex.height - 1 - (row * h + y); // PNG rows go down, texture rows go up
                for (var x = col * w; x < (col + cols) * w; x++) tex.SetPixel(x, ty, c);
            }
        }

        static Transform FindDeep(Transform t, string name)
        {
            if (t.name == name) return t;
            foreach (Transform c in t)
            {
                var hit = FindDeep(c, name);
                if (hit != null) return hit;
            }
            return null;
        }

        /// <summary>Temporary collider shaped like the posed skinned mesh, for placing things on its surface.</summary>
        static MeshCollider Probe(SkinnedMeshRenderer smr)
        {
            var baked = new Mesh();
            smr.BakeMesh(baked, true); // the bake leaves out the parents' scale, so the probe gets it below
            var go = new GameObject("Probe");
            go.transform.SetPositionAndRotation(smr.transform.position, smr.transform.rotation);
            go.transform.localScale = smr.transform.lossyScale;
            var col = go.AddComponent<MeshCollider>();
            col.sharedMesh = baked;
            Physics.SyncTransforms();
            return col;
        }

        /// <summary>Casts from in front of the model (it faces +Z) at a height fraction of the bounds.</summary>
        static bool SurfacePoint(Collider probe, Bounds b, float heightFraction, out Vector3 point, out Vector3 normal, float? x = null)
        {
            var origin = new Vector3(x ?? b.center.x, b.min.y + b.size.y * heightFraction, b.max.z + 1f);
            if (probe.Raycast(new Ray(origin, Vector3.back), out var hit, 5f))
            {
                point = hit.point;
                normal = hit.normal;
                return true;
            }
            Debug.LogWarning($"PrefabBuilder: no surface found on {probe.name} at height {heightFraction}");
            point = origin;
            normal = Vector3.forward;
            return false;
        }

        static void SetWorldScale(Transform t, Vector3 world)
        {
            var parent = t.parent != null ? t.parent.lossyScale : Vector3.one;
            t.localScale = new Vector3(world.x / parent.x, world.y / parent.y, world.z / parent.z);
        }

        static GameObject HeldProp(string prefabPath, Transform slot, string name, Vector3 worldOffset, Quaternion worldRotation, float scale)
        {
            if (slot == null) return null;
            var go = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath), slot);
            PrefabUtility.UnpackPrefabInstance(go, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            go.name = name;
            foreach (var c in go.GetComponentsInChildren<Collider>(true)) Object.DestroyImmediate(c);
            foreach (var c in go.GetComponentsInChildren<Interactable>(true)) Object.DestroyImmediate(c);
            foreach (var c in go.GetComponentsInChildren<Light>(true)) Object.DestroyImmediate(c.gameObject);
            go.transform.SetPositionAndRotation(slot.position + worldOffset, worldRotation);
            SetWorldScale(go.transform, Vector3.one * scale);
            return go;
        }

        // ---------------- Eye drop bottle ----------------

        static void BuildBottle()
        {
            var root = new GameObject("EyeDropBottle");
            var visual = new GameObject("Visual").transform;
            visual.SetParent(root.transform, false);
            var plastic = Lit("BottlePlastic", new Color(0.95f, 0.97f, 1f), 0.7f);
            var label = Lit("BottleLabel", new Color(0.95f, 0.45f, 0.55f), 0.4f);
            var cap = Lit("BottleTip", new Color(0.92f, 0.92f, 0.95f), 0.6f);
            // Built pointing down (-Y) since it is held upside down over the eye; the rest pose flips it.
            Primitive(PrimitiveType.Cylinder, "Body", visual, new Vector3(0, 0.045f, 0), new Vector3(0.036f, 0.04f, 0.036f), plastic);
            Primitive(PrimitiveType.Cylinder, "Label", visual, new Vector3(0, 0.05f, 0), new Vector3(0.038f, 0.022f, 0.038f), label);
            Primitive(PrimitiveType.Sphere, "Shoulder", visual, new Vector3(0, 0.005f, 0), new Vector3(0.036f, 0.022f, 0.036f), plastic);
            Primitive(PrimitiveType.Cylinder, "Nozzle", visual, new Vector3(0, -0.012f, 0), new Vector3(0.012f, 0.012f, 0.012f), cap);
            var tip = new GameObject("DropStart").transform;
            tip.SetParent(root.transform, false);
            tip.localPosition = new Vector3(0, -0.03f, 0);
            Save(root.gameObject, BottlePath);
        }

        // ---------------- Ophthalmoscope ----------------

        static void BuildOphthalmoscope()
        {
            var root = new GameObject("Ophthalmoscope");
            var visual = new GameObject("Visual").transform;
            visual.SetParent(root.transform, false);
            var handle = Lit("ScopeHandle", new Color(0.25f, 0.27f, 0.3f), 0.5f);
            var head = Lit("ScopeHead", new Color(0.1f, 0.1f, 0.12f), 0.6f);
            var lens = Lit("ScopeLens", new Color(1f, 0.95f, 0.8f), 0.9f, new Color(1f, 0.92f, 0.7f) * 4f);
            Primitive(PrimitiveType.Cylinder, "Handle", visual, new Vector3(0, -0.07f, 0.02f), new Vector3(0.025f, 0.06f, 0.025f), handle);
            Primitive(PrimitiveType.Cube, "Head", visual, Vector3.zero, new Vector3(0.04f, 0.05f, 0.03f), head);
            Primitive(PrimitiveType.Sphere, "Lens", visual, new Vector3(0, 0.008f, -0.016f), new Vector3(0.016f, 0.016f, 0.008f), lens);

            var lightGo = new GameObject("Beam");
            lightGo.transform.SetParent(root.transform, false);
            lightGo.transform.localPosition = new Vector3(0, 0.008f, -0.25f); // well in front of the lens so it does not wash out the tool
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1f, 0.93f, 0.75f);
            light.range = 0.6f;
            light.intensity = 0.08f;

            var col = root.AddComponent<SphereCollider>();
            col.radius = 0.07f; // generous for small fingers
            root.AddComponent<Interactable>().EditorInit("Ophthalmoscope", null);
            Save(root, ScopePath);
        }

        // ---------------- Close-up eye model ----------------

        static void BuildEyeModel()
        {
            var root = new GameObject("EyeModel");
            var sclera = Lit("EyeWhite", Color.white, 0.6f);
            var iris = Lit("EyeIris", new Color(0.25f, 0.55f, 0.85f), 0.6f);
            var irisRing = Lit("EyeIrisRing", new Color(0.15f, 0.35f, 0.6f), 0.6f);
            var pupil = Lit("EyePupil", new Color(0.03f, 0.03f, 0.05f), 0.8f);
            var shine = Lit("EyeShine", Color.white, 1f, Color.white * 1.5f);
            var visual = new GameObject("Visual").transform;
            visual.SetParent(root.transform, false);

            Primitive(PrimitiveType.Sphere, "Sclera", visual, Vector3.zero, Vector3.one, sclera);
            var ring = Primitive(PrimitiveType.Cylinder, "IrisRing", visual, new Vector3(0, 0, -0.5f), new Vector3(0.46f, 0.004f, 0.46f), irisRing);
            ring.transform.localRotation = Quaternion.Euler(90, 0, 0);
            var irisGo = Primitive(PrimitiveType.Cylinder, "Iris", visual, new Vector3(0, 0, -0.506f), new Vector3(0.4f, 0.004f, 0.4f), iris);
            irisGo.transform.localRotation = Quaternion.Euler(90, 0, 0);
            // Pupil is a child of the iris, so its X/Z scale is "fraction of the iris".
            var pupilGo = Primitive(PrimitiveType.Cylinder, "Pupil", irisGo.transform, new Vector3(0, -1.5f, 0), new Vector3(0.3f, 1f, 0.3f), pupil);
            Primitive(PrimitiveType.Sphere, "Shine", visual, new Vector3(-0.07f, 0.08f, -0.515f), new Vector3(0.05f, 0.05f, 0.01f), shine);

            root.AddComponent<EyeCloseupRig>().EditorInit(pupilGo.transform);
            Save(root, EyeModelPath);
        }
    }
}
