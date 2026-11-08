using System.Collections.Generic;
using System.IO;
using System.Linq;
using Hindsight.Ophthalmology.Core;
using Hindsight.Ophthalmology.Core.Avatar;
using Hindsight.Ophthalmology.Procedure;
using Hindsight.Ophthalmology.Procedure.Steps;
using Hindsight.Ophthalmology.SceneObjects;
using Hindsight.Ophthalmology.UI;
using TMPro;
using Unity.Cinemachine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using static Hindsight.Ophthalmology.EditorTools.BuilderUtil;

namespace Hindsight.Ophthalmology.EditorTools
{
    /// <summary>
    /// Generates the whole POC (settings, data, prefabs, scenes) from code so it can be rebuilt
    /// headless: Unity -batchmode -projectPath . -executeMethod Hindsight.Ophthalmology.EditorTools.PocBuilder.BuildAll -quit
    /// </summary>
    public static partial class PocBuilder
    {
        public const string MenuScenePath = ScenesDir + "/Menu.unity";
        public const string EyeDropsScenePath = ScenesDir + "/EyeDrops.unity";
        // Seated child in the exam chair: where the bottom of the torso rests (the cushion is at about
        // 0.646 m; see POC -> Debug Captures) and the backrest recline (degrees).
        const float SeatHeight = 0.655f;
        const float SeatZ = -0.17f;
        const float SeatRecline = 0f; // the spine leans back (KidActor.seatRecline), not the whole body

        const string NarrationDir = Root + "/Audio/Narration";
        const string SfxDir = Root + "/Audio/Sfx";

        static readonly Color Ink = new(0.13f, 0.2f, 0.3f);
        static readonly Color Teal = new(0.1f, 0.55f, 0.6f);
        static readonly Color Orange = new(1f, 0.58f, 0.2f);
        static readonly Color Green = new(0.3f, 0.75f, 0.4f);
        static readonly Color Purple = new(0.55f, 0.4f, 0.85f);
        static readonly Color Card = new(1f, 1f, 1f, 0.95f);

        [MenuItem("POC/Rebuild All")]
        public static void BuildAll()
        {
            if (TmpFont == null)
            {
                Debug.LogError($"PocBuilder: TMP font missing at {FontPath} (TMP Essential Resources).");
                return;
            }
            ConfigurePlayer();
            UiKit.Prepare();
            PrefabBuilder.BuildAll();
            BuildSoundLibrary();
            var (catalog, eyeDrops) = BuildLevelData();
            var procedure = BuildEyeDropsProcedure();
            WriteNarrationScript(procedure);
            BuildEyeDropsScene(eyeDrops, procedure);
            BuildMenuScene(catalog);
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(MenuScenePath, true),
                new EditorBuildSettingsScene(EyeDropsScenePath, true),
            };
            AssetDatabase.SaveAssets();
            EditorSceneManager.OpenScene(MenuScenePath);
            Debug.Log("PocBuilder: build complete.");
        }

        [MenuItem("POC/Build Android APK")]
        public static void BuildAndroid()
        {
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { MenuScenePath, EyeDropsScenePath },
                locationPathName = "Builds/Android/EyeCanDo.apk",
                target = BuildTarget.Android,
                options = BuildOptions.None,
            });
            Debug.Log($"PocBuilder: Android build {report.summary.result}, {report.summary.totalSize / (1024 * 1024)} MB");
            if (Application.isBatchMode && report.summary.result != BuildResult.Succeeded) EditorApplication.Exit(1);
        }

        /// <summary>Desktop build used for the autopilot smoke test (run it with -autopilot &lt;dir&gt;).</summary>
        [MenuItem("POC/Build Windows (smoke test)")]
        public static void BuildWindows()
        {
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { MenuScenePath, EyeDropsScenePath },
                locationPathName = "Builds/Windows/EyeCanDo.exe",
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None,
            });
            Debug.Log($"PocBuilder: Windows build {report.summary.result}");
            if (Application.isBatchMode && report.summary.result != BuildResult.Succeeded) EditorApplication.Exit(1);
        }

        // ------------------------------------------------------------------ settings

        static void ConfigurePlayer()
        {
            PlayerSettings.companyName = "Hindsight";
            PlayerSettings.productName = "EyeCanDo";
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.hindsight.ophthalmologygame");
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.iOS, "com.hindsight.ophthalmologygame");
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true;
            PlayerSettings.allowedAutorotateToLandscapeRight = true;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.iOS.targetDevice = iOSTargetDevice.iPhoneAndiPad;
        }

        // ------------------------------------------------------------------ data

        static (LevelCatalog, LevelDefinition) BuildLevelData()
        {
            var dir = DataDir + "/Levels";
            LevelDefinition Level(string file) => LoadOrCreate<LevelDefinition>($"{dir}/{file}.asset");

            var eyeDrops = Level("EyeDrops");
            var chart = Level("EyeChart");
            var puff = Level("AirPuff");
            var photo = Level("EyePhoto");
            var lamp = Level("BigLight");
            var glasses = Level("Glasses");
            eyeDrops.EditorInit("eye-drops", "Eye Drops", "Help the doctor put in special drops.", "EyeDrops", true, chart, UiKit.Icon("level_eye_drops"));
            chart.EditorInit("eye-chart", "Read the Chart", "How many letters can you read?", "", false, puff, UiKit.Icon("level_vision_test"));
            puff.EditorInit("air-puff", "Puff of Air", "A tiny puff checks your eye pressure.", "", false, photo, UiKit.Icon("level_eye_pressure"));
            photo.EditorInit("eye-photo", "Eye Photo", "Say cheese... with your eyeball!", "", false, lamp, UiKit.Icon("level_eye_photo"));
            lamp.EditorInit("big-light", "The Big Light", "Rest your chin and look at the light.", "", false, glasses, UiKit.Icon("level_slit_lamp"));
            glasses.EditorInit("glasses", "Picking Glasses", "Find a cool pair of frames.", "", false, null, UiKit.Icon("level_glasses"));
            foreach (var l in new[] { eyeDrops, chart, puff, photo, lamp, glasses }) EditorUtility.SetDirty(l);

            var catalog = LoadOrCreate<LevelCatalog>(DataDir + "/LevelCatalog.asset");
            catalog.EditorSetLevels(new List<LevelDefinition> { eyeDrops, chart, puff, photo, lamp, glasses });
            EditorUtility.SetDirty(catalog);
            return (catalog, eyeDrops);
        }

        /// <summary>A narrated line. Its clip is Audio/Narration/&lt;id&gt;.wav (or .ogg/.mp3) when that file exists.</summary>
        static StoryLine L(string id, Speaker speaker, string narration, string caption, Mood mood = Mood.Keep)
        {
            AudioClip clip = null;
            foreach (var ext in new[] { "wav", "ogg", "mp3" })
            {
                clip = AssetDatabase.LoadAssetAtPath<AudioClip>($"{NarrationDir}/{id}.{ext}");
                if (clip != null) break;
            }
            return new StoryLine(id, speaker, narration, caption, clip, mood);
        }

        /// <summary>A line of Dr. Iris's; her brows show the mood while she says it.</summary>
        static StoryLine Dr(string id, string narration, string caption, Mood mood = Mood.Keep) => L(id, Speaker.Doctor, narration, caption, mood);

        static StoryLine Kid(string id, string narration, string caption, Mood mood = Mood.Keep) => L(id, Speaker.Sam, narration, caption, mood);

        /// <summary>
        /// The storyboard: Dr. Iris and Sam show how eye drops go, then it is the player's turn.
        /// Each panel is one shot with one or two spoken lines and at most one action. Captions are
        /// 8 words or fewer; the narration says the full sentence.
        /// </summary>
        static ProcedureDefinition BuildEyeDropsProcedure()
        {
            var dir = DataDir + "/EyeDrops/Steps";
            AssetDatabase.DeleteAsset(dir); // step files change between versions; the procedure is rebuilt below
            var index = 0;
            T Step<T>(string file, string title) where T : ProcedureStep
            {
                var step = LoadOrCreate<T>($"{dir}/{++index:00}_{file}.asset");
                step.EditorSetTitle(title);
                EditorUtility.SetDirty(step);
                return step;
            }

            var steps = new List<ProcedureStep>
            {
                Step<PanelStep>("Hello", "1. Dr. Iris says hi").EditorInit("DoctorGreeting",
                    Dr("p01_hello", "Hey, I'm Dr. Iris. I'm going to be looking at your eyes today.", "Hi! I'm Dr. Iris.", Mood.Happy), StoryBeat.DoctorWave),
                Step<TapTargetStep>("MeetSam", "2. Meet Sam").EditorInit("SamWaiting", "Sam",
                    Dr("p02_sam", "This is Sam. He is also getting eye drops today. Click on Sam to say hi.", "This is Sam. Click to say hi!", Mood.Happy),
                    onTap: StoryBeat.SamWave, reaction: Kid("p02_hi", "Hi! I'm Sam, nice to meet you.", "Hi! I'm Sam. Nice to meet you!", Mood.Happy)),
                Step<ChartStep>("Chart", "3. What the drops do").EditorInit(
                    Dr("p03_chart", "In order to see into your eye, I'll need to use some special eye drops. The drops make your pupil nice and big, so that I can see all the way inside.",
                        "Drops make your pupil big", Mood.Neutral), "special", "big"),
                Step<PanelStep>("SamFirst", "4. Sam goes first").EditorInit("ChartTalk",
                        Dr("p04_first", "Sam, you're up first. Hop into the chair so we can see how it's done.", "Sam goes first. Hop in!", Mood.Happy))
                    .EditorFollowUp(Kid("p04_willdo", "Will do!", "Will do!", Mood.Happy))
                    .EditorAfter(StoryBeat.SamWalkToChair),
                Step<PanelStep>("SamHeadBack", "5. Head back, eyes open").EditorInit("SamChair",
                        Dr("p05_cold", "When I put the drops in, they will be cold and may sting for a little bit. Lots of kids say it's not so bad. You're going to do great!",
                            "Cold, may sting a bit. You'll do great!", Mood.Worried),
                        StoryBeat.SamTiltBack, setup: StoryBeat.SamSit),
                Step<PanelStep>("DropOnSam", "6. Sam's drop").EditorInit("SamChair",
                        Kid("p06_ooh", "Oooh, cold!", "Ooh, cold!", Mood.Surprised), StoryBeat.DropOnSam)
                    .EditorFollowUp(Dr("p06_blink", "Make sure to blink to spread the drop around. Then I can take a look inside your eye.", "Blink to spread the drop", Mood.Neutral))
                    .EditorBeatFirst()
                    .EditorAfter(StoryBeat.SamBlink),
                Step<PanelStep>("LookAtSam", "7. A look inside Sam's eye").EditorInit("SamChair",
                        Dr("p07_good", "Looking good, Sam!", "Looking good, Sam!", Mood.Happy), StoryBeat.DoctorExamSam)
                    .EditorBeatFirst(),
                Step<PanelStep>("SamDance", "8. Sam's happy dance").EditorInit("ChairApproach",
                        Kid("p08_yay", "Yay, that wasn't so bad!", "Yay! That wasn't so bad!", Mood.Happy), StoryBeat.SamDance, setup: StoryBeat.SamStandUp)
                    .EditorFollowUp(Dr("p08_set", "You're all set!", "You're all set!", Mood.Happy))
                    .EditorAfter(StoryBeat.SamLeave),
                Step<TapTargetStep>("YourTurn", "9. Your turn").EditorInit("ChairApproach", "ExamChair",
                    Dr("p09_turn", "Now it's your turn! Click on the chair to have a seat.", "Your turn! Click the chair.", Mood.Happy), "FirstPerson",
                    setup: StoryBeat.KidAppear, afterSetup: StoryBeat.KidHide),
                Step<PanelStep>("LookUp", "10. Head back, eyes open").EditorInit("FirstPersonTiltBack",
                        Dr("p10_hold", "Make sure to keep your head back and eyes open. Remember, the drops will feel cold and may sting for a little bit.",
                            "Head back, eyes open. May sting a bit.", Mood.Worried), setup: StoryBeat.DoctorToDropPoint)
                    // The button and the bottle only appear with this line, once the explaining is done.
                    .EditorFollowUp(Dr("p10_button", "Hold the button to keep your eye open!", "Hold the button!", Mood.Neutral))
                    .EditorEndOnHold(showDropper: true),
                // Letting go mid-drop: the drop waits and Dr. Iris repeats her reminder.
                Step<HoldStillStep>("EyeDrop", "11. The drop").EditorInit(new StoryLine(),
                    Dr("p11_again", "Make sure to keep your head back and eyes open.", "Head back, eyes open.", Mood.Worried), new StoryLine()),
                Step<BlinkStep>("Blink", "12. Blink").EditorInit(
                    Dr("p12_did", "You did it! Now blink! The drops need to spread around.", "You did it! Now blink!", Mood.Happy), null, 3),
                Step<ExamStep>("LookInside", "13. A look inside your eye").EditorInit(
                    Dr("p13_look", "Amazing! Now I'll just take a quick look into your eye.", "Amazing! Now a quick look.", Mood.Focused),
                    Dr("p13_great", "Fantastic! Your eyes look great!", "Your eyes look great!", Mood.Happy)),
                Step<RewardStep>("Reward", "14. Brave eyes").EditorInit("Brave Eyes!"),
            };
            foreach (var step in steps) EditorUtility.SetDirty(step);

            var procedure = LoadOrCreate<ProcedureDefinition>(DataDir + "/EyeDrops/EyeDropsProcedure.asset");
            procedure.EditorInit("Eye Drops", steps);
            EditorUtility.SetDirty(procedure);
            AssetDatabase.SaveAssets();
            return procedure;
        }

        /// <summary>
        /// Writes the recording script: every line with its file name. Recording a line and saving it
        /// as Audio/Narration/&lt;id&gt;.wav, then running POC → Rebuild All, wires it in.
        /// </summary>
        static void WriteNarrationScript(ProcedureDefinition procedure)
        {
            EnsureFolder(NarrationDir);
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("# Eye Drops narration script");
            sb.AppendLine();
            sb.AppendLine("Generated by `PocBuilder` (POC → Rebuild All). Do not edit by hand: change the lines in `PocBuilder.BuildEyeDropsProcedure`.");
            sb.AppendLine();
            sb.AppendLine("To add a recording, save it in this folder as `<id>.wav` (or `.ogg` / `.mp3`) and run **POC → Rebuild All**.");
            sb.AppendLine("Lines without a recording are timed silently, with their caption on screen.");
            sb.AppendLine();
            sb.AppendLine("| Panel | id | Speaker | Narration (spoken) | Caption | Recorded |");
            sb.AppendLine("| --- | --- | --- | --- | --- | --- |");
            foreach (var step in procedure.Steps)
            foreach (var line in step.Lines)
                sb.AppendLine($"| {step.Title} | `{line.id}` | {line.speaker} | {line.narration} | {line.caption} | {(line.clip != null ? "yes" : "no")} |");
            File.WriteAllText(NarrationDir + "/NarrationScript.md", sb.ToString());
            AssetDatabase.ImportAsset(NarrationDir + "/NarrationScript.md");

            // The same lines for Tools/VoiceGen/generate_voices.py (outside Assets, so Unity ignores it).
            var json = new System.Text.StringBuilder("[\n");
            var all = procedure.Steps.SelectMany(st => st.Lines).ToList();
            for (var i = 0; i < all.Count; i++)
                json.Append($"  {{\"id\": \"{all[i].id}\", \"speaker\": \"{all[i].speaker}\", \"text\": \"{all[i].narration.Replace("\"", "\\\"")}\"}}{(i < all.Count - 1 ? "," : "")}\n");
            json.Append("]\n");
            Directory.CreateDirectory("Tools/VoiceGen");
            File.WriteAllText("Tools/VoiceGen/lines.json", json.ToString());
        }

        /// <summary>The CC0 sound effects (copied from the 2D PoC), in <see cref="SfxId"/> order.</summary>
        static void BuildSoundLibrary()
        {
            var files = new (string file, float volume)[]
            {
                ("tap", 0.5f), ("coin", 0.8f), ("cheer", 0.8f), ("blink", 0.6f), ("shiver", 0.7f), ("correct", 0.9f),
                ("try_again", 0.7f), ("reward", 0.9f), ("purchase", 0.9f), ("drop_fall", 0.8f), ("pop_1", 0.6f), ("sparkle", 0.7f),
            };
            var clips = files.Select(f => AssetDatabase.LoadAssetAtPath<AudioClip>($"{SfxDir}/{f.file}.wav")).ToArray();
            for (var i = 0; i < clips.Length; i++)
                if (clips[i] == null) Debug.LogError($"PocBuilder: missing sound {SfxDir}/{files[i].file}.wav");
            var library = LoadOrCreate<SoundLibrary>(Root + "/Resources/SoundLibrary.asset");
            library.EditorInit(clips, files.Select(f => f.volume).ToArray());
            EditorUtility.SetDirty(library);
        }

        // ------------------------------------------------------------------ shared scene pieces

        static void SetupLighting()
        {
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.88f, 0.9f, 0.95f);
            RenderSettings.ambientEquatorColor = new Color(0.72f, 0.72f, 0.72f);
            RenderSettings.ambientGroundColor = new Color(0.42f, 0.4f, 0.38f);
            RenderSettings.skybox = null;
            RenderSettings.fog = false;

            var sun = new GameObject("Key Light").AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = new Color(1f, 0.97f, 0.92f);
            sun.intensity = 1.1f;
            sun.shadows = LightShadows.Soft;
            sun.shadowStrength = 0.6f;
            sun.transform.rotation = Quaternion.Euler(55f, -35f, 0f);

            var fill = new GameObject("Ceiling Light").AddComponent<Light>();
            fill.type = LightType.Point;
            fill.color = new Color(1f, 0.96f, 0.9f);
            fill.range = 7f;
            fill.intensity = 1.2f;
            fill.shadows = LightShadows.None;
            fill.transform.position = new Vector3(-0.8f, 1.9f, 1.2f);

            var volumeGo = new GameObject("Post Processing");
            var volume = volumeGo.AddComponent<Volume>();
            volume.isGlobal = true;
            var profilePath = MaterialsDir + "/PostProfile.asset";
            var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(profilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<VolumeProfile>();
                AssetDatabase.CreateAsset(profile, profilePath);
                var bloom = profile.Add<Bloom>(true);
                bloom.threshold.Override(1.2f);
                bloom.intensity.Override(0.6f);
                var tone = profile.Add<Tonemapping>(true);
                tone.mode.Override(TonemappingMode.Neutral);
                EditorUtility.SetDirty(profile);
            }
            volume.sharedProfile = profile;
        }

        static GameObject Place(string prefabPath, string name, Vector3 pos, Quaternion rot, Transform parent = null)
        {
            var go = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath), parent);
            go.name = name;
            go.transform.SetPositionAndRotation(pos, rot);
            return go;
        }

        static Transform Anchor(Transform parent, string name, Vector3 pos, Vector3 faceTowards)
        {
            var t = new GameObject(name).transform;
            t.SetParent(parent, false);
            t.position = pos;
            var dir = faceTowards - pos;
            dir.y = 0;
            t.rotation = Quaternion.LookRotation(dir.normalized, Vector3.up);
            return t;
        }

        static Camera MainCamera(Vector3 pos, Quaternion rot, bool withBrain)
        {
            var go = new GameObject("Main Camera") { tag = "MainCamera" };
            go.transform.SetPositionAndRotation(pos, rot);
            var cam = go.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.55f, 0.75f, 0.9f);
            cam.nearClipPlane = 0.01f;
            cam.fieldOfView = 50f;
            go.AddComponent<AudioListener>();
            go.AddComponent<PhysicsRaycaster>();
            cam.GetUniversalAdditionalCameraData().renderPostProcessing = true;
            if (withBrain)
            {
                var brain = go.AddComponent<CinemachineBrain>();
                brain.DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.EaseInOut, 1.3f);
            }
            return cam;
        }

        static CinemachineCamera Shot(Transform parent, string id, Vector3 pos, Quaternion rot, float fov = 50f)
        {
            var go = new GameObject(id);
            go.transform.SetParent(parent, false);
            go.transform.SetPositionAndRotation(pos, rot);
            var cam = go.AddComponent<CinemachineCamera>();
            var lens = LensSettings.Default;
            lens.FieldOfView = fov;
            lens.NearClipPlane = 0.01f;
            lens.FarClipPlane = 100f;
            cam.Lens = lens;
            cam.Priority.Value = 0;
            return cam;
        }

        static Quaternion Look(Vector3 from, Vector3 to) => Quaternion.LookRotation(to - from, Vector3.up);

        static Canvas MakeCanvas()
        {
            var go = new GameObject("Canvas") { layer = 5 };
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1200);
            scaler.matchWidthOrHeight = 0.5f;
            go.AddComponent<GraphicRaycaster>();

            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<InputSystemUIInputModule>();
            return canvas;
        }

        static void SaveScene(string path)
        {
            EnsureFolder(ScenesDir);
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), path);
        }

        // ------------------------------------------------------------------ Eye Drops scene

        /// <summary>Where Dr. Iris stands around the chair (metres from the chair's head, on the floor).</summary>
        const float ExamDistance = 1.5f, DropSide = 0.46f, DropBehind = -0.42f, SamSide = 0.66f;

        static void BuildEyeDropsScene(LevelDefinition level, ProcedureDefinition procedure)
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            SetupLighting();

            var world = new GameObject("World").transform;
            var room = Place(PrefabBuilder.RoomPath, "ExamRoom", Vector3.zero, Quaternion.identity, world);
            var chair = Place(PrefabBuilder.ChairPath, "ExamChair", new Vector3(0f, 0f, 0.2f), Quaternion.identity, world);
            var head = chair.transform.Find("Head").position;
            var headFlat = new Vector3(head.x, 0f, head.z);

            // ---- Anchors (where the doctor and props go during the procedure)
            var anchors = new GameObject("Anchors").transform;
            var stand = Anchor(anchors, "DoctorStandPoint", new Vector3(1.3f, 0f, 1.3f), new Vector3(1.4f, 0f, -2f)); // faces the door side, clear of the chair
            // First person: she stands past the end of the leg rest and leans in to look. For the drop
            // she stands beside and behind the chair's head, out of sight of the child looking up at
            // the ceiling, and reaches over with the bottle.
            var exam = Anchor(anchors, "DoctorExamPoint", headFlat + new Vector3(0.2f, 0f, ExamDistance), headFlat);
            var dropPoint = Anchor(anchors, "DoctorDropPoint", headFlat + new Vector3(DropSide, 0f, DropBehind), headFlat + new Vector3(0f, 0f, 0.3f));
            // Beside Sam's head with room to spare (she reaches over him, never leans into him), so the
            // front-left camera sees both faces.
            Anchor(anchors, "DoctorSamPoint", headFlat + new Vector3(SamSide, 0f, 0.1f), headFlat);
            // Where a seated child's eyes are; facing the footrest (+Z).
            var chairEyes = Anchor(anchors, "ChairEyes", head, head + Vector3.forward);
            chairEyes.position = head;
            // Where a seated child's hips go, facing the footrest and leaning back with the backrest
            // (measured from the chair's top profile with POC → Debug Captures).
            var seat = new GameObject("SamSeat").transform;
            seat.SetParent(anchors, false);
            seat.SetPositionAndRotation(new Vector3(head.x, SeatHeight, SeatZ), Quaternion.Euler(-SeatRecline, 0f, 0f));
            var samWait = Anchor(anchors, "SamWaitPoint", new Vector3(1.25f, 0f, -2.2f), new Vector3(0.3f, 0f, -0.6f));
            // At the drops chart on the east wall (it spans z 0.1 to 1.7): Dr. Iris on its right, Sam on its left,
            // both turned half toward the chart and half toward the camera.
            var chartCam = new Vector3(0.05f, 1.3f, 0.8f);
            Anchor(anchors, "DoctorChartPoint", new Vector3(2.6f, 0f, -0.45f), new Vector3(0.4f, 0f, 1.3f));
            Anchor(anchors, "SamChartPoint", new Vector3(2.55f, 0f, 1.95f), new Vector3(0.4f, 0f, 0.3f));
            // Sam's way out: round the chair to the door.
            Anchor(anchors, "SamExitWay", new Vector3(1.4f, 0f, 0.55f), new Vector3(1.6f, 0f, -2.6f));
            Anchor(anchors, "SamExitPoint", new Vector3(1.6f, 0f, -2.6f), new Vector3(1.6f, 0f, -4f));
            var approachPos = new Vector3(head.x - 2.1f, 1.5f, head.z + 1.1f);
            // Just past the chair's footrest, so nothing hides the kid from the approach camera.
            var kidStand = Anchor(anchors, "KidStandPoint", new Vector3(0.15f, 0f, 1.55f), approachPos);
            var rewardPos = stand.position + stand.forward * 2.3f + Vector3.up * 1.15f;
            Anchor(anchors, "KidRewardPoint", stand.position + stand.right * 0.8f + stand.forward * 0.15f, rewardPos);

            var doctor = Place(PrefabBuilder.DoctorPath, "Doctor", stand.position, stand.rotation, world);

            // ---- Sam (the peer model) waits by the door; the player's own kid appears for "your turn".
            var sam = Place(PrefabBuilder.KidPath, "Sam", samWait.position, samWait.rotation, world);
            sam.GetComponent<Interactable>().EditorInit("Sam", sam.transform.Find("Highlight").gameObject);
            var playerKid = Place(PrefabBuilder.KidPath, "PlayerKid", kidStand.position, kidStand.rotation, world);
            playerKid.GetComponent<Interactable>().EditorInit("PlayerKid", playerKid.transform.Find("Highlight").gameObject);

            // ---- Things to discover: the eye chart and the drops on the tray glow and play a fact when tapped.
            var chart = room.transform.Find("Shell/EyeChart");
            chart.gameObject.AddComponent<BoxCollider>().size = new Vector3(0.72f, 1.02f, 0.06f);
            chart.gameObject.AddComponent<Interactable>().EditorInit("EyeChart", Sparkle(chart, new Vector3(0f, 0.68f, -0.05f), 0.12f));

            // ---- Camera shots
            var shotsRoot = new GameObject("Shots").transform;
            var standFace = stand.position + Vector3.up * 1.45f;
            var widePos = new Vector3(0.2f, 2.2f, -2.8f);
            Shot(shotsRoot, "RoomWide", widePos, Look(widePos, new Vector3(0.4f, 0.8f, 1.0f)), 60f);
            // The doctor is 1.5 m and chibi-proportioned, so frame head-to-belt with room above for waving.
            // Aim below the doctor's centre: the speech bubble covers the bottom ~30% of the screen,
            // so this keeps the face, hands (wave, held bottle) and coat above it.
            // Dr. Iris is 1.7 m: aim at her chest from about her eye height, so her head sits well below the progress bar.
            var greetPos = stand.position + stand.forward * 2.9f + Vector3.up * 1.3f;
            Shot(shotsRoot, "DoctorGreeting", greetPos, Look(greetPos, stand.position + Vector3.up * 1.0f), 50f);
            var explainPos = stand.position + Quaternion.Euler(0, -25f, 0) * stand.forward * 2.7f + Vector3.up * 1.3f;
            Shot(shotsRoot, "DoctorExplain", explainPos, Look(explainPos, stand.position + Vector3.up * 1.0f + stand.right * 0.45f), 50f);
            Shot(shotsRoot, "ChairApproach", approachPos, Look(approachPos, chair.transform.position + new Vector3(-0.25f, 0.7f, 0.35f)), 50f);
            var samWaitShot = new Vector3(0.35f, 1.0f, -0.8f);
            Shot(shotsRoot, "SamWaiting", samWaitShot, Look(samWaitShot, samWait.position + Vector3.up * 0.55f), 50f);
            // From the front-left, so the doctor (on Sam's right) never stands between the camera and Sam.
            var samChairShot = head + new Vector3(-1.15f, 0.3f, 1.5f);
            Shot(shotsRoot, "SamChair", samChairShot, Look(samChairShot, head + new Vector3(0.1f, -0.25f, 0.25f)), 48f);
            // The chart: the camera pans from Sam at the door over to the east wall.
            Shot(shotsRoot, "ChartTalk", chartCam, Look(chartCam, new Vector3(2.9f, 1.08f, 0.8f)), 50f);
            // Sam leaving: looking from beside the chair over to the door.
            var doorCam = new Vector3(-0.7f, 1.45f, 1.7f);
            Shot(shotsRoot, "DoorView", doorCam, Look(doorCam, new Vector3(1.4f, 0.75f, -2.6f)), 50f);

            var fpRot = Look(head, new Vector3(head.x + 0.2f, 1.35f, head.z + 2f));
            Shot(shotsRoot, "FirstPerson", head, fpRot, 60f);
            var tiltRot = Quaternion.Euler(-68f, 0f, 0f);
            Shot(shotsRoot, "FirstPersonTiltBack", head, tiltRot, 60f);
            var examRot = Look(head, exam.position + Vector3.up * 1.1f + (headFlat - exam.position) * 0.25f); // she is 1.7 m and leans in
            Shot(shotsRoot, "ExamCloseFP", head, examRot, 55f);
            var rewardShot = Shot(shotsRoot, "Reward", rewardPos, Look(rewardPos, stand.position + Vector3.up * 0.95f + stand.right * 0.6f), 50f);

            // ---- A smiley sun painted on the ceiling to look at while the drop goes in
            var tiltDir = tiltRot * Vector3.forward;
            var sunPos = head + tiltDir * ((PrefabBuilder.RoomHeight - 0.004f - head.y) / tiltDir.y);
            BuildCeilingSun(world, sunPos, tiltRot * Vector3.up);

            // ---- Eye close-up "stage", far from the room so it never shows up in other shots
            var stage = new GameObject("EyeCloseupStage").transform;
            stage.position = new Vector3(40f, 0f, 0f);
            var eye = Place(PrefabBuilder.EyeModelPath, "EyeModel", stage.position + new Vector3(0f, 1.5f, 0f), Quaternion.identity, stage);
            Primitive(PrimitiveType.Cube, "Backdrop", stage, new Vector3(0f, 1.5f, 1.4f), new Vector3(8f, 5f, 0.1f), Lit("Backdrop", new Color(1f, 0.82f, 0.86f)));
            var stageLight = new GameObject("StageLight").AddComponent<Light>();
            stageLight.transform.SetParent(stage, false);
            stageLight.transform.localPosition = new Vector3(-0.6f, 2.4f, -1.6f);
            stageLight.type = LightType.Point;
            stageLight.range = 6f;
            stageLight.intensity = 3f;
            var closeupPos = stage.position + new Vector3(0f, 1.5f, -1.6f);
            Shot(shotsRoot, "EyeCloseup", closeupPos, Quaternion.identity, 40f);

            // ---- Eye drop rig: bottle over the eye in the tilt-back view, resting on the tool tray otherwise
            var tiltFwd = tiltRot * Vector3.forward;
            var dropRigGo = new GameObject("EyeDropRig");
            dropRigGo.transform.SetParent(world, false);
            var hold = new GameObject("BottleHold").transform;
            hold.SetParent(dropRigGo.transform, false);
            hold.position = head + tiltFwd * 0.17f + tiltRot * new Vector3(0.07f, 0.05f, 0f); // a little toward the doctor's side
            var toEye = (head - hold.position).normalized;
            var axisDown = (toEye + tiltRot * Vector3.right * 1.1f).normalized; // tip toward the eye, body off to the side
            hold.rotation = Quaternion.FromToRotation(Vector3.down, axisDown);
            var tray = room.transform.Find("Props/ToolTray");
            var trayBounds = WorldBounds(tray.gameObject);
            var rest = new GameObject("BottleRest").transform;
            rest.SetParent(dropRigGo.transform, false);
            rest.position = new Vector3(trayBounds.center.x, trayBounds.max.y + 0.087f, trayBounds.center.z);
            rest.rotation = Quaternion.Euler(180f, 0f, 0f);
            var bottle = Place(PrefabBuilder.BottlePath, "EyeDropBottle", rest.position, rest.rotation, dropRigGo.transform);
            bottle.AddComponent<SphereCollider>().radius = 0.09f; // generous for small fingers
            bottle.AddComponent<Interactable>().EditorInit("Bottle", Sparkle(bottle.transform, new Vector3(0f, -0.16f, 0f), 0.05f));
            var drop = Primitive(PrimitiveType.Sphere, "Drop", dropRigGo.transform, Vector3.zero, Vector3.one * 0.011f,
                Lit("Drop", new Color(0.6f, 0.85f, 1f), 0.95f, new Color(0.3f, 0.5f, 0.8f)));
            var dropTarget = new GameObject("DropTarget").transform;
            dropTarget.SetParent(dropRigGo.transform, false);
            dropTarget.position = head + tiltFwd * 0.02f;
            var dropRig = dropRigGo.AddComponent<EyeDropRig>();
            dropRig.EditorInit(bottle.transform, drop.transform, bottle.transform.Find("DropStart"), dropTarget, rest, hold);

            // ---- Ophthalmoscope rig: hops between points in front of the exam camera
            var scopeRigGo = new GameObject("OphthalmoscopeRig");
            scopeRigGo.transform.SetParent(world, false);
            var offsets = new[] { new Vector2(-0.07f, 0.035f), new Vector2(0.075f, 0.03f), new Vector2(0f, -0.02f) };
            var points = offsets.Select((o, i) =>
            {
                var p = new GameObject($"LookPoint_{i + 1}").transform;
                p.SetParent(scopeRigGo.transform, false);
                p.position = head + examRot * new Vector3(o.x, o.y, 0.3f);
                p.rotation = examRot;
                return p;
            }).ToArray();
            var scope = Place(PrefabBuilder.ScopePath, "Ophthalmoscope", points[0].position, points[0].rotation, scopeRigGo.transform);
            var scopeRig = scopeRigGo.AddComponent<OphthalmoscopeRig>();
            scopeRig.EditorInit(scope.transform, scope.GetComponentInChildren<Light>(), points);

            // ---- The light that glints in Sam's eye when Dr. Iris looks in
            var glint = new GameObject("ExamGlint").AddComponent<Light>();
            glint.transform.SetParent(world, false);
            glint.type = LightType.Point;
            glint.color = new Color(1f, 0.95f, 0.8f);
            glint.range = 0.35f;
            glint.intensity = 1.2f;
            glint.shadows = LightShadows.None;

            // ---- Confetti for the reward shot
            var confetti = MakeConfetti(rewardShot.transform);

            // ---- Camera + director
            var roomWide = shotsRoot.Find("RoomWide");
            var cam = MainCamera(roomWide.position, roomWide.rotation, true);
            var director = shotsRoot.gameObject.AddComponent<ShotDirector>();
            director.EditorInit(cam.GetComponent<CinemachineBrain>(), "RoomWide");

            // ---- UI
            var canvas = MakeCanvas();
            var portraits = new[] { PortraitTexture("Speaker_Doctor"), PortraitTexture("Speaker_Sam") };
            var ui = BuildLevelUi(canvas.transform, confetti, new Texture[] { portraits[0], portraits[1], null });
            BuildSpeakerStudio(ui.Narrator, portraits);

            // ---- Procedure wiring
            var systems = new GameObject("Procedure");
            var context = systems.AddComponent<ExamRoomContext>();
            context.EditorInit(level, anchors, director, doctor.GetComponent<DoctorActor>(), sam.GetComponent<KidActor>(),
                playerKid.GetComponent<KidActor>(), eye.GetComponent<EyeCloseupRig>(), dropRig, scopeRig,
                room.GetComponentInChildren<TeachingChart>(), glint, ui.Captions,
                ui.Buttons, ui.Effects, ui.Reward);
            var runner = systems.AddComponent<ProcedureRunner>();
            runner.EditorInit(procedure, context);
            ui.Hud.EditorInit(runner, ui.HudDots, ui.HudDot, ui.HudHome, ui.HudGear, ui.Settings);

            SaveScene(EyeDropsScenePath);
        }

        /// <summary>A small glowing diamond that shows while an object can be tapped.</summary>
        static GameObject Sparkle(Transform parent, Vector3 localPos, float size)
        {
            var go = new GameObject("Highlight");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            var glow = Lit("Highlight", new Color(1f, 0.85f, 0.2f), 0.5f, new Color(1f, 0.75f, 0.1f) * 1.5f);
            var d = Primitive(PrimitiveType.Cube, "Diamond", go.transform, Vector3.zero, Vector3.one, glow);
            d.transform.localRotation = Quaternion.Euler(45, 0, 45);
            d.transform.localScale = new Vector3(size / parent.lossyScale.x, size / parent.lossyScale.y, size / parent.lossyScale.z);
            go.AddComponent<Bobber>();
            go.SetActive(false);
            return go;
        }

        /// <summary>
        /// A smiley sun with clouds and a rainbow, painted on the ceiling (a flat unlit picture facing
        /// down, turned so it reads upright in the tilt-back view).
        /// </summary>
        static void BuildCeilingSun(Transform parent, Vector3 pos, Vector3 viewUp)
        {
            var picture = GameObject.CreatePrimitive(PrimitiveType.Quad);
            picture.name = "CeilingSun";
            Object.DestroyImmediate(picture.GetComponent<Collider>());
            picture.transform.SetParent(parent, false);
            var up = Vector3.ProjectOnPlane(viewUp, Vector3.up).normalized;
            // A quad shows its face along -Z: point +Z up so the picture faces down into the room.
            picture.transform.SetPositionAndRotation(pos, Quaternion.LookRotation(Vector3.up, up));
            picture.transform.localScale = Vector3.one * 1.3f;
            picture.GetComponent<Renderer>().sharedMaterial = CeilingArt.Material();
        }

        static RenderTexture PortraitTexture(string name)
        {
            var path = $"{Root}/Art/UI/{name}.renderTexture";
            var rt = AssetDatabase.LoadAssetAtPath<RenderTexture>(path);
            if (rt != null) return rt;
            rt = new RenderTexture(384, 384, 24) { name = name, antiAliasing = 4 };
            AssetDatabase.CreateAsset(rt, path);
            return rt;
        }

        /// <summary>
        /// Copies of the doctor and Sam far from the room, each filmed head-and-shoulders into a render
        /// texture for the caption bubble (<see cref="SpeakerPortraits"/> makes them talk).
        /// </summary>
        static void BuildSpeakerStudio(NarrationPlayer narrator, RenderTexture[] textures)
        {
            var root = new GameObject("SpeakerStudio").transform;
            root.position = new Vector3(-60f, 0f, 0f);
            Primitive(PrimitiveType.Cube, "Backdrop", root, new Vector3(1.5f, 1.5f, -1.2f), new Vector3(6f, 4f, 0.1f), Lit("PortraitBackdrop", UiKit.Hex("#E1F1FF")));
            var doctor = Place(PrefabBuilder.DoctorPath, "DoctorPortrait", root.position, Quaternion.identity, root);
            var sam = Place(PrefabBuilder.KidPath, "SamPortrait", root.position + new Vector3(3f, 0f, 0f), Quaternion.identity, root);
            sam.GetComponent<Interactable>().EditorInit("SamPortrait", null);
            var actors = new[] { doctor, sam };
            for (var i = 0; i < actors.Length; i++)
            {
                var centre = actors[i].GetComponentsInChildren<Transform>(true).First(t => t.name == "HeadCenter").position;
                var camGo = new GameObject(actors[i].name + "Camera");
                camGo.transform.SetParent(root, true);
                // The models face +Z: film from in front, a little below eye level, so the face fills the circle.
                var distance = i == 0 ? 1.85f : 1.1f; // the doctor is 1.7 times Sam's size
                var camPos = centre + new Vector3(0f, -0.04f, distance);
                camGo.transform.SetPositionAndRotation(camPos, Look(camPos, centre + Vector3.down * 0.06f));
                var cam = camGo.AddComponent<Camera>();
                cam.fieldOfView = 24f;
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = UiKit.Hex("#E1F1FF");
                cam.targetTexture = textures[i];
                cam.nearClipPlane = 0.05f;
                cam.farClipPlane = 4f;
                var light = new GameObject(actors[i].name + "Light").AddComponent<Light>();
                light.transform.SetParent(root, true);
                light.transform.position = centre + new Vector3(-0.4f, 0.5f, 1f);
                light.type = LightType.Point;
                light.range = 3f;
                light.intensity = 1.4f;
            }
            root.gameObject.AddComponent<SpeakerPortraits>().EditorInit(narrator, doctor.GetComponent<DoctorActor>(), sam.GetComponent<KidActor>());
        }

        static ParticleSystem MakeConfetti(Transform rewardCamera)
        {
            var go = new GameObject("Confetti");
            go.transform.position = rewardCamera.position + rewardCamera.forward * 1.6f - Vector3.up * 0.9f;
            go.transform.rotation = Quaternion.Euler(-90f, 0f, 0f); // emit upwards
            var ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var main = ps.main;
            main.playOnAwake = false;
            main.loop = false;
            main.duration = 2f;
            main.startLifetime = new ParticleSystem.MinMaxCurve(2.5f, 3.5f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(2.2f, 3.6f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.03f, 0.06f);
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            main.gravityModifier = 0.45f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 400;
            var colors = new Gradient();
            colors.SetKeys(
                new[]
                {
                    new GradientColorKey(new Color(1f, 0.3f, 0.4f), 0f), new GradientColorKey(new Color(1f, 0.8f, 0.2f), 0.25f),
                    new GradientColorKey(new Color(0.3f, 0.85f, 0.45f), 0.5f), new GradientColorKey(new Color(0.3f, 0.6f, 1f), 0.75f),
                    new GradientColorKey(new Color(0.7f, 0.4f, 1f), 1f),
                },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) });
            main.startColor = new ParticleSystem.MinMaxGradient(colors) { mode = ParticleSystemGradientMode.RandomColor };

            var emission = ps.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 160), new ParticleSystem.Burst(0.5f, 120) });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(2.2f, 0.6f, 0.1f);

            var rot = ps.rotationOverLifetime;
            rot.enabled = true;
            rot.z = new ParticleSystem.MinMaxCurve(-4f, 4f);

            go.GetComponent<ParticleSystemRenderer>().sharedMaterial = ParticleUnlit("Confetti");
            return ps;
        }
    }
}
