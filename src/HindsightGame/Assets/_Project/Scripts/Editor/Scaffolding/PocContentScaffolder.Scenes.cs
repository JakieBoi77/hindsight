using Hindsight.App;
using Hindsight.Menus;
using Hindsight.Procedures;
using Hindsight.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static Hindsight.Editor.Scaffolding.UiBuilder;

namespace Hindsight.Editor.Scaffolding
{
    public static partial class PocContentScaffolder
    {
        private static readonly Vector2 ReferenceResolution = new Vector2(1920f, 1080f);

        private static void BuildBootstrapper(ProcedureCatalog catalog)
        {
            var root = new GameObject("GameBootstrapper");
            var bootstrapper = root.AddComponent<GameBootstrapper>();

            var effects = new GameObject("EffectsAudio").AddComponent<AudioSource>();
            effects.transform.SetParent(root.transform, false);
            effects.playOnAwake = false;
            var narration = new GameObject("NarrationAudio").AddComponent<AudioSource>();
            narration.transform.SetParent(root.transform, false);
            narration.playOnAwake = false;

            var audio = root.AddComponent<UiAudio>();
            SerializedWriter.For(audio)
                .Ref("effectsSource", effects)
                .Ref("narrationSource", narration)
                .Ref("tapClip", ArtLibrary.KenneySound("tap-a.ogg"))
                .Ref("successClip", ArtLibrary.KenneySound("switch-a.ogg"))
                .Ref("gentleErrorClip", ArtLibrary.KenneySound("click-a.ogg"))
                .Apply();

            var faderCanvas = CreateCanvas("FaderCanvas", root.transform, sortingOrder: 1000);
            var faderGroup = faderCanvas.gameObject.AddComponent<CanvasGroup>();
            faderGroup.alpha = 0f;
            faderGroup.blocksRaycasts = false;
            Rect("Overlay", faderCanvas.transform).Stretch().Image(null, Palette.Ink, raycast: true, preserveAspect: false);
            var fader = faderCanvas.gameObject.AddComponent<ScreenFader>();

            SerializedWriter.For(bootstrapper)
                .Ref("catalog", catalog)
                .Ref("screenFader", fader)
                .Ref("uiAudio", audio)
                .Apply();

            SaveAsPrefab<GameBootstrapper>(root, ResourcesFolder + GameBootstrapper.ResourcePath + ".prefab");
        }

        private static void BuildScenes()
        {
            // Each NewScene unloads unused assets, so scenes look up assets by path after creation
            // rather than holding references from earlier scaffolding stages.
            var mainMenu = BuildMainMenuScene();
            var levelSelect = BuildLevelSelectScene();
            var procedure = BuildProcedureScene();

            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(mainMenu, true),
                new EditorBuildSettingsScene(levelSelect, true),
                new EditorBuildSettingsScene(procedure, true),
            };
        }

        private static string BuildMainMenuScene()
        {
            var scene = NewScene();
            var canvas = CreateCanvas("UI", null);
            AddBackground(canvas.transform, "Backgrounds/title_background.svg");
            var safe = SafeArea(canvas.transform);

            Rect("Title", safe).Place(new Vector2(0.5f, 1f), new Vector2(0f, -150f), new Vector2(1600f, 200f))
                .Text("Ophthalmology Game", 130f, Palette.Ink, TextAlignmentOptions.Center, 70f);

            // The game's own cast previews the experience: Dr. Iris waving, and a child in the exam chair.
            var doctor = Rect("Doctor", safe).Place(Vector2.zero, new Vector2(230f, 40f), new Vector2(400f, 720f), Vector2.zero);
            doctor.Image(ArtLibrary.Art("Characters/doctor_wave.svg"));
            doctor.Idle(IdleKind.Bob, 8f, 0.4f);

            var chair = Rect("ExamChair", safe).Place(new Vector2(1f, 0f), new Vector2(-200f, 40f), new Vector2(480f, 560f), new Vector2(1f, 0f));
            chair.Image(ArtLibrary.Art("Clinic/exam_chair.svg"));
            Rect("Child", chair).Stretch().Image(ArtLibrary.Art("Characters/child_seated.svg"));

            var playWrapper = Rect("Play", safe).Place(new Vector2(0.5f, 0f), new Vector2(0f, 170f), new Vector2(560f, 180f));
            playWrapper.Idle(IdleKind.Pulse, 3f, 0.8f);
            var play = ChunkyButton(playWrapper, "PlayButton", "Play", Palette.Green, Palette.GreenDark, new Vector2(560f, 180f), 92f, ArtLibrary.KenneySprite("icon_play_light.png"));
            ((RectTransform)play.transform).Stretch();

            var controller = canvas.gameObject.AddComponent<MainMenuController>();
            SerializedWriter.For(controller).Ref("playButton", play).Apply();
            return SaveScene(scene, SceneIds.MainMenu);
        }

        private static string BuildLevelSelectScene()
        {
            var scene = NewScene();
            var levelCard = AssetDatabase.LoadAssetAtPath<LevelCardView>(SharedPrefabsFolder + "LevelCard.prefab");
            var canvas = CreateCanvas("UI", null);
            AddBackground(canvas.transform, "Backgrounds/title_background.svg");
            var safe = SafeArea(canvas.transform);

            Rect("Header", safe).Place(new Vector2(0.5f, 1f), new Vector2(0f, -95f), new Vector2(1200f, 130f))
                .Text("Choose a visit!", 96f, Palette.Ink, TextAlignmentOptions.Center, 60f);

            var back = RoundButton(safe, "BackButton", ArtLibrary.KenneySprite("grey_arrow_basic_w.png"), Palette.Blue, 130f);
            ((RectTransform)back.transform).Place(new Vector2(0f, 1f), new Vector2(100f, -95f), new Vector2(130f, 130f));

            var grid = Rect("Levels", safe).Region(new Vector2(0.04f, 0.04f), new Vector2(0.96f, 0.8f));
            var layout = grid.gameObject.AddComponent<GridLayoutGroup>();
            layout.cellSize = new Vector2(500f, 330f);
            layout.spacing = new Vector2(50f, 50f);
            layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            layout.constraintCount = 3;
            layout.childAlignment = TextAnchor.MiddleCenter;

            var controller = canvas.gameObject.AddComponent<LevelSelectController>();
            SerializedWriter.For(controller)
                .Ref("cardPrefab", levelCard)
                .Ref("cardContainer", grid)
                .Ref("backButton", back)
                .Apply();
            return SaveScene(scene, SceneIds.LevelSelect);
        }

        private static string BuildProcedureScene()
        {
            var scene = NewScene();
            var fallback = AssetDatabase.LoadAssetAtPath<ProcedureCatalog>(DataFolder + "ProcedureCatalog.asset").Procedures[0];
            var canvas = CreateCanvas("UI", null);
            AddBackground(canvas.transform, "Backgrounds/exam_room.svg");
            var safe = SafeArea(canvas.transform);

            // Doctor stands bottom-left throughout; steps play out on a stage to the right.
            var doctorRect = Rect("Doctor", safe).Place(Vector2.zero, new Vector2(30f, 10f), new Vector2(400f, 720f), Vector2.zero);
            var doctorImage = doctorRect.Image(ArtLibrary.Art("Characters/doctor_smile.svg"));
            doctorRect.Idle(IdleKind.Bob, 6f, 0.4f);
            var doctor = doctorRect.gameObject.AddComponent<DoctorCharacterView>();
            SerializedWriter.For(doctor)
                .Ref("image", doctorImage)
                .Poses("poses",
                    (DoctorPose.Smile, ArtLibrary.Art("Characters/doctor_smile.svg")),
                    (DoctorPose.Wave, ArtLibrary.Art("Characters/doctor_wave.svg")),
                    (DoctorPose.Point, ArtLibrary.Art("Characters/doctor_point.svg")),
                    (DoctorPose.Cheer, ArtLibrary.Art("Characters/doctor_cheer.svg")))
                .Apply();

            var stage = Rect("StepStage", safe).Region(new Vector2(0.25f, 0.03f), new Vector2(0.98f, 0.64f));
            var stageGroup = stage.Group();

            var dialogue = BuildDialogue(safe);
            var topBar = Rect("TopBar", safe).Stretch();
            var home = RoundButton(topBar, "HomeButton", ArtLibrary.Art("UI/home.svg"), Palette.Blue, 120f);
            ((RectTransform)home.transform).Place(new Vector2(0f, 1f), new Vector2(90f, -75f), new Vector2(120f, 120f));
            var dots = BuildProgressDots(topBar);
            var skip = ChunkyButton(topBar, "DeveloperSkipButton", "Skip", Palette.Grey, Palette.GreyDark, new Vector2(190f, 90f), 40f);
            ((RectTransform)skip.transform).Place(new Vector2(1f, 1f), new Vector2(-120f, -70f), new Vector2(190f, 90f));

            var exitDialog = BuildExitDialog(canvas.transform);

            var controller = canvas.gameObject.AddComponent<ProcedureController>();
            SerializedWriter.For(controller)
                .Ref("fallbackProcedure", fallback)
                .Ref("stepStage", stage)
                .Ref("stepStageGroup", stageGroup)
                .Ref("doctor", doctor)
                .Ref("dialogue", dialogue)
                .Ref("progressDots", dots)
                .Ref("homeButton", home)
                .Ref("exitDialog", exitDialog)
                .Ref("developerSkipButton", skip)
                .Apply();
            return SaveScene(scene, SceneIds.Procedure);
        }

        private static DoctorDialogueView BuildDialogue(Transform parent)
        {
            var root = Rect("Dialogue", parent).Region(new Vector2(0.25f, 0.665f), new Vector2(0.97f, 0.9f));
            var group = root.Group();

            var bubble = Rect("Bubble", root).Stretch();
            bubble.Panel(Palette.Ink);
            var body = Rect("Body", bubble).Stretch(6f, 6f, 6f, 6f);
            var bodyImage = body.Panel(Palette.White, raycast: true);
            var bubbleButton = body.MakeButton(bodyImage, feedback: false);
            bubbleButton.transition = Selectable.Transition.None;

            var label = Rect("Label", body).Stretch(44f, 16f, 190f, 16f)
                .Text(string.Empty, 58f, Palette.Ink, TextAlignmentOptions.Left, 36f);

            // Tail pointing down toward the doctor. Drawn after the bubble so its white top
            // covers the bubble border where they join.
            Rect("Tail", bubble).Place(Vector2.zero, new Vector2(70f, 14f), new Vector2(110f, 90f), new Vector2(0f, 1f))
                .Image(ArtLibrary.Art("UI/bubble_tail.svg"), preserveAspect: false);

            var continueWrapper = Rect("Continue", body).Place(new Vector2(1f, 0.5f), new Vector2(-95f, 0f), new Vector2(130f, 130f));
            continueWrapper.Idle(IdleKind.Pulse, 6f, 1f);
            var continueButton = RoundButton(continueWrapper, "ContinueButton", ArtLibrary.KenneySprite("icon_play_light.png"), Palette.Green, 130f, 0.5f);
            ((RectTransform)continueButton.transform).Stretch();

            var view = root.gameObject.AddComponent<DoctorDialogueView>();
            SerializedWriter.For(view)
                .Ref("bubbleGroup", group)
                .Ref("bubble", bubble)
                .Ref("label", label)
                .Ref("bubbleButton", bubbleButton)
                .Ref("continueButton", continueButton)
                .Apply();
            return view;
        }

        private static ProgressDotsView BuildProgressDots(Transform parent)
        {
            var root = Rect("ProgressDots", parent).Place(new Vector2(0.5f, 1f), new Vector2(0f, -55f), new Vector2(900f, 60f));
            var layout = root.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 22f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            var template = Rect("DotTemplate", root);
            template.sizeDelta = new Vector2(34f, 34f);
            var dotImage = template.Image(ArtLibrary.Circle, preserveAspect: false);

            var view = root.gameObject.AddComponent<ProgressDotsView>();
            SerializedWriter.For(view).Ref("dotTemplate", dotImage).Apply();
            return view;
        }

        private static ConfirmDialog BuildExitDialog(Transform parent)
        {
            var root = Rect("ExitDialog", parent).Stretch();
            root.Image(null, new Color(0f, 0f, 0f, 0.45f), raycast: true, preserveAspect: false);
            root.Group(0f);

            var panel = Rect("Panel", root).Center(0f, 0f, 1000f, 560f);
            panel.Panel(Palette.Ink);
            Rect("Face", panel).Stretch(8f, 8f, 8f, 8f).Panel(Palette.White);
            Rect("Title", panel).Center(0f, 150f, 900f, 110f).Text("Leave this visit?", 76f, Palette.Ink);
            Rect("Subtitle", panel).Center(0f, 50f, 860f, 80f).Text("You can start it again any time.", 42f, Palette.GreyDark);

            var stay = ChunkyButton(panel, "KeepGoingButton", "Keep going", Palette.Green, Palette.GreenDark, new Vector2(400f, 140f), 54f);
            ((RectTransform)stay.transform).Center(-220f, -140f, 400f, 140f);
            var leave = ChunkyButton(panel, "LeaveButton", "Leave", Palette.Coral, Palette.CoralDark, new Vector2(400f, 140f), 54f);
            ((RectTransform)leave.transform).Center(220f, -140f, 400f, 140f);

            var dialog = root.gameObject.AddComponent<ConfirmDialog>();
            SerializedWriter.For(dialog)
                .Ref("confirmButton", leave)
                .Ref("cancelButton", stay)
                .Ref("panel", panel)
                .Apply();
            return dialog;
        }

        private static Scene NewScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var cameraObject = new GameObject("Main Camera") { tag = "MainCamera" };
            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Palette.Sky;
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);

            var eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<InputSystemUIInputModule>();
            return scene;
        }

        private static string SaveScene(Scene scene, string sceneName)
        {
            var path = ScenesFolder + sceneName + ".unity";
            if (!EditorSceneManager.SaveScene(scene, path))
            {
                throw new System.InvalidOperationException($"Failed to save scene {path}.");
            }

            return path;
        }

        private static Canvas CreateCanvas(string name, Transform parent, int sortingOrder = 0)
        {
            var go = new GameObject(name, typeof(RectTransform)) { layer = UiLayer };
            if (parent != null)
            {
                go.transform.SetParent(parent, false);
            }

            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;

            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = ReferenceResolution;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            go.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        private static void AddBackground(Transform canvas, string artPath)
        {
            // Envelope keeps the 16:9 art undistorted on 4:3 tablets and tall phones (cropping edges).
            var background = Rect("Background", canvas).Center(0f, 0f, ReferenceResolution.x, ReferenceResolution.y);
            background.Image(ArtLibrary.Art(artPath), preserveAspect: false);
            var fitter = background.gameObject.AddComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            fitter.aspectRatio = ReferenceResolution.x / ReferenceResolution.y;
        }

        private static RectTransform SafeArea(Transform canvas)
        {
            var safe = Rect("SafeArea", canvas).Stretch();
            safe.gameObject.AddComponent<SafeAreaFitter>();
            return safe;
        }
    }
}
