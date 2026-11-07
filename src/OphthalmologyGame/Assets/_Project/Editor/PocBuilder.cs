using System.Collections.Generic;
using System.IO;
using System.Linq;
using Hindsight.Ophthalmology.Core;
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
        const string Doctor = "Dr. Iris";

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
            var (catalog, eyeDrops) = BuildLevelData();
            var procedure = BuildEyeDropsProcedure();
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
                locationPathName = "Builds/Android/OphthalmologyGame.apk",
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
                locationPathName = "Builds/Windows/OphthalmologyGame.exe",
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
            PlayerSettings.productName = "Ophthalmology Game";
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

        static ProcedureDefinition BuildEyeDropsProcedure()
        {
            var dir = DataDir + "/EyeDrops/Steps";
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
                Step<DialogueStep>("Greeting", "Say hello").EditorInit("DoctorGreeting", "", DoctorPose.Wave,
                    "Hi there! I'm Dr. Iris, your eye doctor.",
                    "Welcome to my clinic! Today I'm going to take a super close look at your eyes."),
                Step<DialogueStep>("Explain", "Why eye drops?").EditorInit("DoctorExplain", "", DoctorPose.UseItem,
                    "To see inside your eye, I need the dark circle in the middle - your pupil - to get BIG.",
                    "Special eye drops make that happen. Your job is to tilt your head back, keep your eyes open, and stay nice and still.")
                    .EditorProp(DoctorProp.EyeDrops),
                Step<ShowCloseupStep>("Dilation", "What drops do").EditorInit(
                    "This is your pupil before the drops. It's small, like a tiny window.",
                    "After the drops, it gets nice and big - now I can see all the way inside!",
                    "DoctorExplain"),
                Step<TapTargetStep>("SitInChair", "Sit in the chair").EditorInit("ChairApproach", "ExamChair",
                    "Let's get started! Can you hop up into the big chair? Tap the chair to sit down.", "FirstPerson"),
                Step<DialogueStep>("Seated", "Get comfy").EditorInit("FirstPerson", "DoctorExamPoint", DoctorPose.Wave,
                    "Great job! Nice and comfy. Before we start, let's see what you remember."),
                Step<QuizStep>("QuizWhy", "Quiz: why?").EditorInit("FirstPerson",
                    "Why do we use eye drops?", 0,
                    "That's right! Great listening!",
                    "Not quite! The drops make your pupil bigger. Try again!",
                    "Make my pupils big so the doctor can see inside",
                    "Change the color of my eyes",
                    "Help me fall asleep")
                    .EditorIcons(UiKit.Icon("quiz_big_pupil"), UiKit.Icon("quiz_rainbow_eye"), UiKit.Icon("quiz_sleepy")),
                Step<QuizStep>("QuizJob", "Quiz: your job").EditorInit("FirstPerson",
                    "What is YOUR job when the drops go in?", 1,
                    "Yes! You've got it!",
                    "Hmm, not that one. Remember: head back and eyes open!",
                    "Squeeze my eyes shut tight",
                    "Tilt my head back and don't blink",
                    "Wiggle around")
                    .EditorIcons(UiKit.Icon("quiz_eyes_shut"), UiKit.Icon("quiz_head_back"), UiKit.Icon("quiz_wiggle")),
                Step<DialogueStep>("HeadsUp", "Heads up").EditorInit("FirstPerson", "", DoctorPose.Idle,
                    "Just so you know: the drops might feel a little cold, and they might sting for a few seconds.",
                    "That's totally normal, and it goes away quickly. You're going to do great!")
                    .EditorProp(DoctorProp.EyeDrops),
                Step<DialogueStep>("TiltBack", "Tilt back").EditorInit("FirstPersonTiltBack", "DoctorDropPoint", DoctorPose.Idle,
                    "Now tilt your head back and look up at the ceiling.",
                    "Keep your eye wide open and try not to blink. If you blink, the drop will miss and we'll have to do it again.")
                    .EditorTeleport(),
                Step<HoldStillStep>("EyeDrop", "The eye drop"),
                Step<BlinkStep>("Blink", "Blink, blink, blink"),
                Step<DialogueStep>("Wait", "Let the drops work").EditorInit("FirstPerson", "DoctorExamPoint", DoctorPose.Idle,
                    "The drops take a little while to work, so let's wait together...",
                    "...and done! Your pupils are nice and big now.").EditorTeleport(),
                Step<ExamStep>("Exam", "Look inside"),
                Step<RewardStep>("Reward", "Reward"),
            };

            var procedure = LoadOrCreate<ProcedureDefinition>(DataDir + "/EyeDrops/EyeDropsProcedure.asset");
            procedure.EditorInit("Eye Drops", steps);
            EditorUtility.SetDirty(procedure);
            return procedure;
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
            var exam = Anchor(anchors, "DoctorExamPoint", headFlat + new Vector3(0.3f, 0f, 1.25f), headFlat);
            var dropPoint = Anchor(anchors, "DoctorDropPoint", headFlat + new Vector3(0.5f, 0f, 0.12f), headFlat);

            var doctor = Place(PrefabBuilder.DoctorPath, "Doctor", stand.position, stand.rotation, world);

            // ---- Camera shots
            var shotsRoot = new GameObject("Shots").transform;
            var standFace = stand.position + Vector3.up * 1.45f;
            var widePos = new Vector3(0.2f, 2.2f, -2.8f);
            Shot(shotsRoot, "RoomWide", widePos, Look(widePos, new Vector3(0.4f, 0.8f, 1.0f)), 60f);
            // The doctor is 1.5 m and chibi-proportioned, so frame head-to-belt with room above for waving.
            // Aim below the doctor's centre: the speech bubble covers the bottom ~30% of the screen,
            // so this keeps the face, hands (wave, held bottle) and coat above it.
            var greetPos = stand.position + stand.forward * 2.5f + Vector3.up * 0.95f;
            Shot(shotsRoot, "DoctorGreeting", greetPos, Look(greetPos, stand.position + Vector3.up * 0.55f), 50f);
            var explainPos = stand.position + Quaternion.Euler(0, -25f, 0) * stand.forward * 2.3f + Vector3.up * 0.95f;
            Shot(shotsRoot, "DoctorExplain", explainPos, Look(explainPos, stand.position + Vector3.up * 0.55f + stand.right * 0.45f), 50f);
            var approachPos = new Vector3(head.x - 2.1f, 1.5f, head.z + 1.1f);
            Shot(shotsRoot, "ChairApproach", approachPos, Look(approachPos, chair.transform.position + new Vector3(0f, 0.75f, 0.2f)), 50f);

            var fpRot = Look(head, new Vector3(head.x + 0.2f, 1.35f, head.z + 2f));
            Shot(shotsRoot, "FirstPerson", head, fpRot, 60f);
            var tiltRot = Quaternion.Euler(-68f, 0f, 0f);
            Shot(shotsRoot, "FirstPersonTiltBack", head, tiltRot, 60f);
            var examRot = Look(head, exam.position + Vector3.up * 1.1f); // the doctor is 1.5 m and leans in
            Shot(shotsRoot, "ExamCloseFP", head, examRot, 55f);
            var rewardPos = stand.position + stand.forward * 2.3f + Vector3.up * 1.15f;
            var rewardShot = Shot(shotsRoot, "Reward", rewardPos, Look(rewardPos, stand.position + Vector3.up * 1.1f + stand.right * 0.75f), 50f);

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
            hold.position = head + tiltFwd * 0.17f + tiltRot * new Vector3(0.03f, 0.06f, 0f);
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

            // ---- Confetti for the reward shot
            var confetti = MakeConfetti(rewardShot.transform);

            // ---- Camera + director
            var roomWide = shotsRoot.Find("RoomWide");
            var cam = MainCamera(roomWide.position, roomWide.rotation, true);
            var director = shotsRoot.gameObject.AddComponent<ShotDirector>();
            director.EditorInit(cam.GetComponent<CinemachineBrain>(), "RoomWide");

            // ---- UI
            var canvas = MakeCanvas();
            var ui = BuildLevelUi(canvas.transform, confetti);

            // ---- Procedure wiring
            var systems = new GameObject("Procedure");
            var context = systems.AddComponent<ExamRoomContext>();
            context.EditorInit(level, anchors, director, doctor.GetComponent<DoctorActor>(), eye.GetComponent<EyeCloseupRig>(),
                dropRig, scopeRig, ui.Dialogue, ui.Quiz, ui.Buttons, ui.Effects, ui.Reward);
            var runner = systems.AddComponent<ProcedureRunner>();
            runner.EditorInit(procedure, context);
            ui.Hud.EditorInit(runner, ui.HudLabel, ui.HudDots, ui.HudDot, ui.HudHome);

            SaveScene(EyeDropsScenePath);
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
