using System.Linq;
using Hindsight.Ophthalmology.Core.Avatar;
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
        const string ChairModel = "Assets/ThirdParty/PolyPizza/ExamChair.glb";
        const float KidHeight = 1.0f;
        const float DoctorHeight = 1.7f;

        public const string KidPath = PrefabsDir + "/Kid.prefab";
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
            BuildKid();
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
            BuildDropsChart(shell);

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

        /// <summary>
        /// The drops chart on the east wall (where the eye poster was): a big board with two picture
        /// panels (<see cref="ChartArt"/>) that light up as Dr. Iris explains them (<see cref="TeachingChart"/>).
        /// </summary>
        static void BuildDropsChart(Transform parent)
        {
            const float width = 1.66f;
            var height = width * ChartArt.Height / ChartArt.Width;
            var tex = ChartArt.Texture();
            var chart = new GameObject("DropsChart").transform;
            chart.SetParent(parent, false);
            chart.localPosition = new Vector3(RoomHalf - 0.02f, 1.32f, 0.9f);
            chart.localRotation = Quaternion.Euler(0, 90, 0); // a quad shows its face along -Z: into the room (-X)

            Primitive(PrimitiveType.Cube, "Frame", chart, new Vector3(0, 0, 0.012f), new Vector3(width + 0.06f, height + 0.06f, 0.02f), Lit("ChartFrame", new Color(0.2f, 0.3f, 0.45f)));
            Quad("Board", chart, Vector3.zero, new Vector2(width, height), PictureMaterial("ChartBoard", tex, new Rect(0, 0, 1, 1)));

            var glow = Lit("ChartGlow", new Color(1f, 0.8f, 0.2f), 0.2f, new Color(1f, 0.75f, 0.1f) * 2f);
            var panels = new Transform[ChartArt.PanelCount];
            var glows = new Renderer[ChartArt.PanelCount];
            for (var i = 0; i < ChartArt.PanelCount; i++)
            {
                var r = ChartArt.PanelRect(i);
                var uv = new Rect((float)r.x / ChartArt.Width, (float)r.y / ChartArt.Height, (float)r.width / ChartArt.Width, (float)r.height / ChartArt.Height);
                var centre = new Vector3((uv.center.x - 0.5f) * width, (uv.center.y - 0.5f) * height, 0f);
                var size = new Vector2(uv.width * width, uv.height * height);
                glows[i] = Quad($"Glow{i + 1}", chart, centre + new Vector3(0, 0, -0.003f), size * 1.07f, glow).GetComponent<Renderer>();
                glows[i].enabled = false;
                panels[i] = Quad($"Panel{i + 1}", chart, centre + new Vector3(0, 0, -0.006f), size, PictureMaterial($"ChartPanel{i + 1}", tex, uv)).transform;
            }
            chart.gameObject.AddComponent<TeachingChart>().EditorInit(panels, glows);
        }

        static GameObject Quad(string name, Transform parent, Vector3 localPos, Vector2 size, Material mat)
        {
            var q = GameObject.CreatePrimitive(PrimitiveType.Quad);
            q.name = name;
            Object.DestroyImmediate(q.GetComponent<Collider>());
            q.transform.SetParent(parent, false);
            q.transform.localPosition = localPos;
            q.transform.localScale = new Vector3(size.x, size.y, 1f);
            var r = q.GetComponent<Renderer>();
            r.sharedMaterial = mat;
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            return q;
        }

        /// <summary>A lit material showing part of a picture (<paramref name="uv"/>), slightly self-lit so it reads on the wall.</summary>
        static Material PictureMaterial(string name, Texture2D tex, Rect uv)
        {
            var path = $"{MaterialsDir}/{name}.mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                AssetDatabase.CreateAsset(mat, path);
            }
            mat.SetTexture("_BaseMap", tex);
            mat.SetTextureScale("_BaseMap", uv.size);
            mat.SetTextureOffset("_BaseMap", uv.position);
            mat.SetColor("_BaseColor", Color.white);
            mat.SetFloat("_Smoothness", 0.1f);
            mat.EnableKeyword("_EMISSION");
            mat.SetTexture("_EmissionMap", tex);
            mat.SetColor("_EmissionColor", Color.white * 0.25f);
            EditorUtility.SetDirty(mat);
            return mat;
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

        /// <summary>Dr. Iris: a grown-up scout (1.7 times a kid) in a white coat and head mirror, holding the drops and the light.</summary>
        static void BuildDoctor()
        {
            var root = new GameObject("Doctor");
            var rig = BeanBuilder.Build(root, DoctorHeight / KidHeight, new[] { AvatarLook.DoctorHat }, new[] { AvatarLook.DoctorOutfit }, AvatarLook.Doctor,
                armLength: 1.15f); // a grown-up's arms: the wrists still reach the hips
            // Props in the mitten hands, hidden until a step asks for them. Built with the arms hanging down.
            var drops = HeldProp(BottlePath, rig.HandL, "HeldEyeDrops", new Vector3(0f, 0.03f, 0.08f), Quaternion.Euler(180f, 0f, 0f), 2.4f);
            // The light's handle in the fist and its head out past it (along the forearm), lens forward:
            // raised to her face, the head is in front of her eye.
            var light = HeldProp(ScopePath, rig.HandR, "HeldLight", new Vector3(0f, -0.075f, 0.02f), Quaternion.Euler(180f, 0f, 0f), 2f);
            if (drops != null) drops.SetActive(false);
            if (light != null) light.SetActive(false);
            root.AddComponent<DoctorActor>().EditorInit(rig, drops, light);
            Save(root, DoctorPath);
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

            // The pupil is something to tap: a generous collider over the iris and a glowing ring while it waits.
            var tap = new GameObject("PupilTap");
            tap.transform.SetParent(root.transform, false);
            tap.transform.localPosition = new Vector3(0, 0, -0.5f);
            tap.AddComponent<SphereCollider>().radius = 0.3f;
            // A halo just around the pupil (between the iris and the pupil), so the iris stays visible.
            var glow = Primitive(PrimitiveType.Cylinder, "Glow", tap.transform, new Vector3(0, 0, -0.0115f), new Vector3(0.2f, 0.001f, 0.2f),
                Lit("PupilGlow", new Color(1f, 0.85f, 0.2f, 1f), 0.5f, new Color(1f, 0.75f, 0.1f) * 1.2f));
            glow.transform.localRotation = Quaternion.Euler(90, 0, 0);
            glow.AddComponent<PulseScale>();
            tap.AddComponent<Interactable>().EditorInit("Pupil", glow);

            root.AddComponent<EyeCloseupRig>().EditorInit(pupilGo.transform);
            Save(root, EyeModelPath);
        }

        // ---------------- Kid (Sam and the player's own character) ----------------

        /// <summary>A kid scout wearing (switchably) every hat and fit the shop has. Sam and the player's kid share it.</summary>
        static void BuildKid()
        {
            var root = new GameObject("Kid");
            var rig = BeanBuilder.Build(root, 1f, BeanBuilder.CatalogIds(Slot.Hat), BeanBuilder.CatalogIds(Slot.Outfit), AvatarLook.Default);

            // Tappable (Sam waves when tapped): a capsule around the body and a bobbing diamond above the head.
            var col = root.AddComponent<CapsuleCollider>();
            col.center = new Vector3(0, KidHeight * 0.5f, 0);
            col.height = KidHeight;
            col.radius = 0.3f;
            var highlight = new GameObject("Highlight");
            highlight.transform.SetParent(root.transform, false);
            highlight.transform.localPosition = new Vector3(0, KidHeight + 0.25f, 0);
            Primitive(PrimitiveType.Cube, "Diamond", highlight.transform, Vector3.zero, Vector3.one * 0.13f, Lit("Highlight", new Color(1f, 0.85f, 0.2f), 0.5f))
                .transform.localRotation = Quaternion.Euler(45, 0, 45);
            highlight.AddComponent<Bobber>();
            root.AddComponent<Interactable>().EditorInit("Kid", highlight);

            root.AddComponent<KidActor>().EditorInit(rig);
            Save(root, KidPath);
        }
    }
}
