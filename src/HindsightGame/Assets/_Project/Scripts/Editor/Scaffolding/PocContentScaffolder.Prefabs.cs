using Hindsight.Menus;
using Hindsight.Procedures.Steps;
using Hindsight.Tweening;
using Hindsight.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Hindsight.Editor.Scaffolding.UiBuilder;

namespace Hindsight.Editor.Scaffolding
{
    public static partial class PocContentScaffolder
    {
        private const float StepEyeScale = 0.75f;

        private sealed class SharedPrefabs
        {
            public EyeView Eye;
            public HoldButton HoldButton;
            public LevelCardView LevelCard;
        }

        private sealed class StepPrefabs
        {
            public DialogueStepView Dialogue;
            public DilationDemoStepView Dilation;
            public TapTargetStepView TapTarget;
            public QuizStepView Quiz;
            public HoldStillStepView HoldStill;
            public TapCountStepView TapCount;
            public ExamineStepView Examine;
            public RewardStepView Reward;
        }

        private static SharedPrefabs BuildSharedPrefabs()
        {
            return new SharedPrefabs
            {
                Eye = BuildEyePrefab(),
                HoldButton = BuildHoldButtonPrefab(),
                LevelCard = BuildLevelCardPrefab(),
            };
        }

        private static EyeView BuildEyePrefab()
        {
            // Socket (skin + brow) behind an almond "white" that masks the iris, pupil and eyelid.
            var root = Rect("Eye", null);
            root.sizeDelta = new Vector2(760f, 480f);
            root.Image(ArtLibrary.Art("Eye/eye_socket.svg"), raycast: false, preserveAspect: false);

            var sclera = Rect("Sclera", root).Center(0f, -20f, 600f, 320f);
            sclera.Image(ArtLibrary.Art("Eye/eye_white.svg"), preserveAspect: false);
            sclera.gameObject.AddComponent<Mask>().showMaskGraphic = true;

            var iris = Rect("Iris", sclera).Center(0f, 0f, 240f, 240f);
            iris.Image(ArtLibrary.Art("Eye/iris.svg"));
            var pupil = Rect("Pupil", iris).Center(0f, 0f, 240f, 240f);
            pupil.Image(ArtLibrary.Art("Eye/pupil.svg"));
            pupil.localScale = Vector3.one * 0.4f;
            Rect("Highlight", iris).Center(-48f, 48f, 62f, 62f).Image(ArtLibrary.Art("Eye/eye_highlight.svg"));

            var lid = Rect("Eyelid", sclera).Place(new Vector2(0.5f, 1f), Vector2.zero, new Vector2(600f, 320f), new Vector2(0.5f, 1f));
            lid.Image(ArtLibrary.Art("Eye/eyelid.svg"), preserveAspect: false);
            lid.localScale = new Vector3(1f, 0f, 1f);

            Rect("Outline", root).Center(0f, -20f, 600f, 320f).Image(ArtLibrary.Art("Eye/eye_outline.svg"), preserveAspect: false);

            var eye = root.gameObject.AddComponent<EyeView>();
            SerializedWriter.For(eye).Ref("pupil", pupil).Ref("eyelid", lid).Apply();
            return SaveAsPrefab<EyeView>(root.gameObject, SharedPrefabsFolder + "Eye.prefab");
        }

        private static HoldButton BuildHoldButtonPrefab()
        {
            // The root receives input and squishes when held; the "Body" child carries the idle pulse,
            // so the two scale effects never fight over the same transform.
            var root = Rect("HoldButton", null);
            root.sizeDelta = new Vector2(300f, 300f);
            root.Image(ArtLibrary.Circle, new Color(1f, 1f, 1f, 0f), raycast: true, preserveAspect: false);
            var holdButton = root.gameObject.AddComponent<HoldButton>();

            var body = Rect("Body", root).Stretch();
            body.Idle(IdleKind.Pulse, 4f, 0.8f);
            Rect("Base", body).Stretch(18f, 18f, 18f, 18f).Image(ArtLibrary.Circle, Palette.Green, preserveAspect: false);
            Rect("Track", body).Stretch().Image(ArtLibrary.Ring, new Color(1f, 1f, 1f, 0.6f), preserveAspect: false);
            var ring = Rect("Ring", body).Stretch().Image(ArtLibrary.Ring, Palette.Yellow, preserveAspect: false);
            ring.type = Image.Type.Filled;
            ring.fillMethod = Image.FillMethod.Radial360;
            ring.fillOrigin = (int)Image.Origin360.Top;
            ring.fillClockwise = true;
            ring.fillAmount = 0f;
            Rect("Icon", body).Center(0f, 18f, 150f, 150f).Image(ArtLibrary.Art("Icons/quiz_head_back.svg"));
            Rect("Label", body).Center(0f, -78f, 220f, 60f).Text("Hold!", 46f, Palette.White);
            Rect("Hint", root).Center(0f, -195f, 420f, 60f).Text("Press and hold", 38f, Palette.Ink);

            return SaveAsPrefab<HoldButton>(root.gameObject, SharedPrefabsFolder + "HoldButton.prefab");
        }

        private static LevelCardView BuildLevelCardPrefab()
        {
            var root = Rect("LevelCard", null);
            root.sizeDelta = new Vector2(500f, 330f);
            root.Panel(Palette.Ink, raycast: true);
            var face = Rect("Face", root).Stretch(6f, 6f, 6f, 6f).Panel(Palette.White);
            var button = root.MakeButton(face);

            var content = Rect("Content", root).Stretch();
            var contentGroup = content.Group();
            var icon = Rect("Icon", content).Center(0f, 45f, 180f, 180f).Image(null);
            var title = Rect("Title", content).Place(new Vector2(0.5f, 0f), new Vector2(0f, 82f), new Vector2(460f, 64f))
                .Text("Level", 46f, Palette.Ink, TextAlignmentOptions.Center, 30f);
            var status = Rect("Status", content).Place(new Vector2(0.5f, 0f), new Vector2(0f, 34f), new Vector2(460f, 44f))
                .Text("Play!", 32f, Palette.GreyDark);

            var lockBadge = Rect("LockBadge", root).Place(new Vector2(1f, 1f), new Vector2(-20f, -20f), new Vector2(110f, 110f), new Vector2(1f, 1f));
            lockBadge.Image(ArtLibrary.Circle, Palette.White, preserveAspect: false);
            Rect("Padlock", lockBadge).Center(0f, 0f, 80f, 80f).Image(ArtLibrary.Art("UI/padlock.svg"));

            var completed = Rect("CompletedBadge", root).Place(new Vector2(0f, 1f), new Vector2(18f, -18f), new Vector2(96f, 96f), new Vector2(0f, 1f));
            completed.Image(ArtLibrary.KenneySprite("yellow_star.png"));

            var card = root.gameObject.AddComponent<LevelCardView>();
            SerializedWriter.For(card)
                .Ref("button", button)
                .Ref("background", face)
                .Ref("icon", icon)
                .Ref("title", title)
                .Ref("status", status)
                .Ref("lockBadge", lockBadge.gameObject)
                .Ref("completedBadge", completed.gameObject)
                .Ref("contentGroup", contentGroup)
                .Apply();
            return SaveAsPrefab<LevelCardView>(root.gameObject, SharedPrefabsFolder + "LevelCard.prefab");
        }

        private static StepPrefabs BuildStepPrefabs(SharedPrefabs shared)
        {
            return new StepPrefabs
            {
                Dialogue = BuildDialogueStep(),
                Dilation = BuildDilationStep(shared),
                TapTarget = BuildTapTargetStep(),
                Quiz = BuildQuizStep(),
                HoldStill = BuildHoldStillStep(shared),
                TapCount = BuildTapCountStep(shared),
                Examine = BuildExamineStep(shared),
                Reward = BuildRewardStep(),
            };
        }

        private static RectTransform StepRoot(string name)
        {
            return Rect(name, null).Stretch();
        }

        private static EyeView PlaceEye(SharedPrefabs shared, Transform parent, float x, float y, float scale = StepEyeScale)
        {
            var instance = InstantiatePrefab(shared.Eye.gameObject, parent);
            var rect = (RectTransform)instance.transform;
            rect.anchoredPosition = new Vector2(x, y);
            rect.localScale = Vector3.one * scale;
            return instance.GetComponent<EyeView>();
        }

        private static (HoldButton Button, Image Ring) PlaceHoldButton(SharedPrefabs shared, Transform parent, float x, float y, float scale = 1f)
        {
            var instance = InstantiatePrefab(shared.HoldButton.gameObject, parent);
            var rect = (RectTransform)instance.transform;
            rect.anchoredPosition = new Vector2(x, y);
            rect.localScale = Vector3.one * scale;
            return (instance.GetComponent<HoldButton>(), instance.transform.Find("Body/Ring").GetComponent<Image>());
        }

        /// <summary>Wrapper carrying an idle pulse, with a button inside it (see HoldButton for why).</summary>
        private static (RectTransform Wrapper, IdleMotion Pulse) PulseWrapper(Transform parent, string name, float x, float y, float width, float height, float amplitude = 5f)
        {
            var wrapper = Rect(name, parent).Center(x, y, width, height);
            var pulse = wrapper.Idle(IdleKind.Pulse, amplitude, 0.9f, enabled: false);
            return (wrapper, pulse);
        }

        private static Button ImageButton(Transform parent, string name, Sprite sprite)
        {
            var rect = Rect(name, parent).Stretch();
            var image = rect.Image(sprite, raycast: true);
            return rect.MakeButton(image);
        }

        private static DialogueStepView BuildDialogueStep()
        {
            var root = StepRoot("DialogueStep");
            var illustration = Rect("Illustration", root).Center(0f, -10f, 520f, 520f).Image(null);
            var view = root.gameObject.AddComponent<DialogueStepView>();
            SerializedWriter.For(view).Ref("illustration", illustration).Apply();
            return SaveAsPrefab<DialogueStepView>(root.gameObject, StepPrefabsFolder + "DialogueStep.prefab");
        }

        private static DilationDemoStepView BuildDilationStep(SharedPrefabs shared)
        {
            var root = StepRoot("DilationDemoStep");

            // Glow sits behind the eye so the growing pupil stays clearly visible.
            var glow = Rect("BrightLightGlow", root).Center(-80f, -120f, 900f, 900f);
            glow.Image(ArtLibrary.Art("Clinic/light_glow.svg"));
            var glowGroup = glow.Group(0f);
            glowGroup.blocksRaycasts = false;
            var eye = PlaceEye(shared, root, -80f, -120f);

            var dropTarget = Rect("DropTarget", root).Center(-80f, -120f, 10f, 10f);
            var drop = Rect("Drop", root).Center(-80f, 92f, 46f, 63f);
            drop.Image(ArtLibrary.Art("Clinic/drop.svg"));

            var (dropperWrapper, dropperPulse) = PulseWrapper(root, "Dropper", -80f, 205f, 124f, 217f);
            var dropper = ImageButton(dropperWrapper, "DropperButton", ArtLibrary.Art("Clinic/eye_drop_bottle.svg"));
            var tapHint = Rect("TapHint", root).Center(130f, 220f, 260f, 70f);
            tapHint.Text("Tap!", 54f, Palette.Coral);

            var view = root.gameObject.AddComponent<DilationDemoStepView>();
            SerializedWriter.For(view)
                .Ref("eye", eye)
                .Ref("dropperButton", dropper)
                .Ref("dropperPulse", dropperPulse)
                .Ref("tapHint", tapHint.gameObject)
                .Ref("drop", drop)
                .Ref("dropTarget", dropTarget)
                .Ref("brightLightGlow", glowGroup)
                .Apply();
            return SaveAsPrefab<DilationDemoStepView>(root.gameObject, StepPrefabsFolder + "DilationDemoStep.prefab");
        }

        private static TapTargetStepView BuildTapTargetStep()
        {
            var root = StepRoot("TapTargetStep");
            var highlight = Rect("Highlight", root).Center(150f, -40f, 720f, 720f);
            highlight.Image(ArtLibrary.Art("Clinic/light_glow.svg"));
            var highlightGroup = highlight.Group(0f);
            highlightGroup.blocksRaycasts = false;

            var (chairWrapper, chairPulse) = PulseWrapper(root, "Chair", 150f, -40f, 480f, 560f, 3f);
            var chair = ImageButton(chairWrapper, "ChairButton", ArtLibrary.Art("Clinic/exam_chair.svg"));

            var avatar = Rect("ChildAvatar", root).Center(150f, -40f, 480f, 560f);
            avatar.Image(ArtLibrary.Art("Characters/child_seated.svg"));
            var avatarGroup = avatar.Group(0f);
            avatarGroup.blocksRaycasts = false;

            var view = root.gameObject.AddComponent<TapTargetStepView>();
            SerializedWriter.For(view)
                .Ref("target", chair)
                .Ref("targetPulse", chairPulse)
                .Ref("highlight", highlightGroup)
                .Ref("avatar", avatar)
                .Ref("avatarGroup", avatarGroup)
                .Apply();
            return SaveAsPrefab<TapTargetStepView>(root.gameObject, StepPrefabsFolder + "TapTargetStep.prefab");
        }

        private static QuizStepView BuildQuizStep()
        {
            var root = StepRoot("QuizStep");
            var container = Rect("Answers", root).Center(0f, -10f, 1360f, 480f);
            var layout = container.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 40f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            var card = Rect("AnswerTemplate", container);
            card.sizeDelta = new Vector2(400f, 470f);
            card.Panel(Palette.Ink, raycast: true);
            var face = Rect("Face", card).Stretch(6f, 6f, 6f, 6f).Panel(Palette.White);
            var button = card.MakeButton(face);
            var icon = Rect("Icon", card).Center(0f, 70f, 250f, 250f).Image(null);
            var label = Rect("Label", card).Place(new Vector2(0.5f, 0f), new Vector2(0f, 85f), new Vector2(360f, 130f))
                .Text("Answer", 42f, Palette.Ink, TextAlignmentOptions.Center, 28f);
            var badge = Rect("CorrectBadge", card).Place(new Vector2(1f, 1f), new Vector2(-12f, -12f), new Vector2(90f, 90f), new Vector2(1f, 1f));
            badge.Image(ArtLibrary.KenneySprite("green_icon_checkmark.png"));

            var answerView = card.gameObject.AddComponent<QuizAnswerView>();
            SerializedWriter.For(answerView)
                .Ref("button", button)
                .Ref("background", face)
                .Ref("icon", icon)
                .Ref("label", label)
                .Ref("correctBadge", badge.gameObject)
                .Apply();

            var view = root.gameObject.AddComponent<QuizStepView>();
            SerializedWriter.For(view).Ref("answerTemplate", answerView).Ref("answerContainer", container).Apply();
            return SaveAsPrefab<QuizStepView>(root.gameObject, StepPrefabsFolder + "QuizStep.prefab");
        }

        private static HoldStillStepView BuildHoldStillStep(SharedPrefabs shared)
        {
            var root = StepRoot("HoldStillStep");

            var tilt = Rect("TiltDemo", root).Stretch();
            var tiltGroup = tilt.Group();
            var tiltArt = Rect("HeadTilt", tilt).Center(0f, -10f, 520f, 520f);
            tiltArt.Image(ArtLibrary.Art("Characters/child_head_tilt.svg"));
            tiltArt.Idle(IdleKind.Sway, 5f, 0.5f);

            var scene = Rect("EyeScene", root).Stretch();
            var sceneGroup = scene.Group(0f);
            const float eyeX = -230f;
            var eye = PlaceEye(shared, scene, eyeX, -130f);
            Rect("Dropper", scene).Center(eyeX, 205f, 124f, 217f).Image(ArtLibrary.Art("Clinic/eye_drop_bottle.svg"));
            var dropStart = Rect("DropStart", scene).Center(eyeX, 92f, 10f, 10f);
            var dropTarget = Rect("DropTarget", scene).Center(eyeX, -130f, 10f, 10f);
            var splashTarget = Rect("SplashTarget", scene).Center(eyeX, -40f, 10f, 10f);
            var drop = Rect("Drop", scene).Center(eyeX, 92f, 46f, 63f);
            drop.Image(ArtLibrary.Art("Clinic/drop.svg"));

            var (holdButton, holdRing) = PlaceHoldButton(shared, scene, 430f, -90f);
            var holdPulse = holdButton.transform.Find("Body").GetComponent<IdleMotion>();

            var burst = Rect("SensationBurst", scene).Center(110f, 170f, 420f, 150f);
            Rect("Snowflake", burst).Place(new Vector2(0f, 0.5f), new Vector2(75f, 0f), new Vector2(140f, 140f)).Image(ArtLibrary.Art("Clinic/snowflake.svg"));
            var sensationLabel = Rect("Caption", burst).Stretch(150f, 0f, 0f, 0f).Text("Brrr! Cold!", 56f, Palette.BlueDark, TextAlignmentOptions.Left, 36f);

            var view = root.gameObject.AddComponent<HoldStillStepView>();
            SerializedWriter.For(view)
                .Ref("tiltDemo", tiltGroup)
                .Ref("eyeScene", sceneGroup)
                .Ref("eye", eye)
                .Ref("drop", drop)
                .Ref("dropStart", dropStart)
                .Ref("dropTarget", dropTarget)
                .Ref("splashTarget", splashTarget)
                .Ref("holdButton", holdButton)
                .Ref("holdRing", holdRing)
                .Ref("holdButtonPulse", holdPulse)
                .Ref("sensationBurst", burst)
                .Ref("sensationLabel", sensationLabel)
                .Apply();
            return SaveAsPrefab<HoldStillStepView>(root.gameObject, StepPrefabsFolder + "HoldStillStep.prefab");
        }

        private static TapCountStepView BuildTapCountStep(SharedPrefabs shared)
        {
            var root = StepRoot("TapCountStep");
            var (wrapper, pulse) = PulseWrapper(root, "EyeTarget", 0f, -80f, 646f, 408f, 3f);
            var hitArea = wrapper.gameObject.AddComponent<Image>();
            hitArea.color = new Color(1f, 1f, 1f, 0f);
            var button = wrapper.MakeButton(hitArea, feedback: false);
            var eye = PlaceEye(shared, wrapper, 0f, 0f, 0.85f);

            var counters = Rect("Counters", root).Center(0f, 250f, 700f, 100f);
            var layout = counters.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 18f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            var counterTemplate = Rect("CounterTemplate", counters);
            counterTemplate.sizeDelta = new Vector2(84f, 84f);
            var counterImage = counterTemplate.Image(ArtLibrary.KenneySprite("grey_star_outline_depth.png"));

            var view = root.gameObject.AddComponent<TapCountStepView>();
            SerializedWriter.For(view)
                .Ref("eye", eye)
                .Ref("tapTarget", button)
                .Ref("tapTargetPulse", pulse)
                .Ref("counterTemplate", counterImage)
                .Ref("counterEmpty", ArtLibrary.KenneySprite("grey_star_outline_depth.png"))
                .Ref("counterFilled", ArtLibrary.KenneySprite("yellow_star.png"))
                .Apply();
            return SaveAsPrefab<TapCountStepView>(root.gameObject, StepPrefabsFolder + "TapCountStep.prefab");
        }

        private static ExamineStepView BuildExamineStep(SharedPrefabs shared)
        {
            var root = StepRoot("ExamineStep");
            const float eyeX = -180f;
            const float eyeY = -110f;
            var eye = PlaceEye(shared, root, eyeX, eyeY);

            var glow = Rect("LightGlow", root).Center(eyeX, eyeY, 700f, 700f);
            glow.Image(ArtLibrary.Art("Clinic/light_glow.svg"));
            var glowGroup = glow.Group(0f);
            glowGroup.blocksRaycasts = false;

            var toolTarget = Rect("ToolTarget", root).Center(200f, -60f, 10f, 10f);
            var (toolWrapper, toolPulse) = PulseWrapper(root, "Ophthalmoscope", 470f, -60f, 180f, 291f);
            var toolButton = ImageButton(toolWrapper, "ToolButton", ArtLibrary.Art("Clinic/ophthalmoscope.svg"));

            var (holdButton, holdRing) = PlaceHoldButton(shared, root, 520f, -150f, 0.85f);

            var view = root.gameObject.AddComponent<ExamineStepView>();
            SerializedWriter.For(view)
                .Ref("eye", eye)
                .Ref("toolButton", toolButton)
                .Ref("tool", toolWrapper)
                .Ref("toolTarget", toolTarget)
                .Ref("toolPulse", toolPulse)
                .Ref("lightGlow", glowGroup)
                .Ref("holdButton", holdButton)
                .Ref("holdRing", holdRing)
                .Apply();
            return SaveAsPrefab<ExamineStepView>(root.gameObject, StepPrefabsFolder + "ExamineStep.prefab");
        }

        private static RewardStepView BuildRewardStep()
        {
            var root = StepRoot("RewardStep");
            var headline = Rect("Headline", root).Center(0f, 250f, 1100f, 150f).Text("You did it!", 110f, Palette.Ink, TextAlignmentOptions.Center, 60f);
            var stickerRoot = Rect("Sticker", root).Center(0f, 10f, 330f, 343f);
            var sticker = stickerRoot.Image(null);
            var finish = ChunkyButton(root, "FinishButton", "Finish", Palette.Green, Palette.GreenDark, new Vector2(420f, 140f), 64f);
            ((RectTransform)finish.transform).Center(0f, -250f, 420f, 140f);
            var finishLabel = finish.transform.Find("Face/Label").GetComponent<TMP_Text>();

            var confettiRect = Rect("Confetti", root).Stretch();
            var pieceTemplate = Rect("PieceTemplate", confettiRect).Center(0f, 0f, 20f, 30f).Image(null);
            var confetti = confettiRect.gameObject.AddComponent<UiConfetti>();
            SerializedWriter.For(confetti).Ref("pieceTemplate", pieceTemplate).Apply();

            var view = root.gameObject.AddComponent<RewardStepView>();
            SerializedWriter.For(view)
                .Ref("headline", headline)
                .Ref("sticker", sticker)
                .Ref("stickerRoot", stickerRoot)
                .Ref("confetti", confetti)
                .Ref("finishButton", finish)
                .Ref("finishLabel", finishLabel)
                .Apply();
            return SaveAsPrefab<RewardStepView>(root.gameObject, StepPrefabsFolder + "RewardStep.prefab");
        }
    }
}
