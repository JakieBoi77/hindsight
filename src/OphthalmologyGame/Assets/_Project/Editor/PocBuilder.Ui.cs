using Hindsight.Ophthalmology.Core;
using Hindsight.Ophthalmology.SceneObjects;
using Hindsight.Ophthalmology.UI;
using TMPro;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using static Hindsight.Ophthalmology.EditorTools.BuilderUtil;
using static Hindsight.Ophthalmology.EditorTools.UiKit;

namespace Hindsight.Ophthalmology.EditorTools
{
    /// <summary>UI for the level and menu scenes, in the 2D prototype's style (see <see cref="UiKit"/>).</summary>
    public static partial class PocBuilder
    {
        struct LevelUi
        {
            public DialogueView Dialogue;
            public QuizView Quiz;
            public ActionButtonsView Buttons;
            public ScreenEffectsView Effects;
            public RewardView Reward;
            public LevelHud Hud;
            public TMP_Text HudLabel;
            public RectTransform HudDots;
            public Image HudDot;
            public Button HudHome;
        }

        static LevelUi BuildLevelUi(Transform canvas, ParticleSystem confetti)
        {
            var ui = new LevelUi();
            var root = (RectTransform)canvas;

            // ---- Eye overlays (under the rest of the UI)
            var fx = Stretch("ScreenEffects", canvas);
            var lidColor = new Color(0.22f, 0.1f, 0.09f);
            var upper = Rect("UpperLid", fx, new Vector2(0, 1), Vector2.one);
            Img(upper, lidColor, raycast: false);
            var lower = Rect("LowerLid", fx, Vector2.zero, new Vector2(1, 0));
            Img(lower, lidColor, raycast: false);
            var blur = Img(Stretch("Blur", fx), new Color(0.92f, 0.95f, 1f, 0f), raycast: false);
            var cold = Img(Stretch("ColdFlash", fx), new Color(0.55f, 0.85f, 1f, 0f), raycast: false);
            var snow = Rect("Snowflake", fx, new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.55f));
            snow.sizeDelta = Vector2.one * 380;
            Picture(snow, Icon("snowflake"));

            // ---- HUD: home button + a pill with the step name and progress dots
            var hud = Stretch("Hud", canvas);
            ui.Hud = hud.gameObject.AddComponent<LevelHud>();
            ui.HudHome = Round(hud, "ExitButton", Icon("home"), Palette.Coral, Palette.CoralDark, new Vector2(0, 1), 130, new Vector2(100, -96), 0.5f);
            var pill = Rect("StepPill", hud, new Vector2(0, 1), new Vector2(0, 1));
            pill.sizeDelta = new Vector2(760, 132);
            pill.anchoredPosition = new Vector2(190 + 380, -96);
            var pillFill = OutlinedCard(pill, new Color(1f, 1f, 1f, 0.96f), 56, 5);
            ui.HudLabel = Label(pillFill, "StepName", "", 40, Palette.Ink, TextAlignmentOptions.MidlineLeft, 26);
            ui.HudLabel.rectTransform.anchorMin = new Vector2(0, 0.5f);
            ui.HudLabel.rectTransform.offsetMin = new Vector2(34, 0);
            ui.HudLabel.rectTransform.offsetMax = new Vector2(-24, -4);
            ui.HudDots = Rect("Dots", pillFill, Vector2.zero, new Vector2(1, 0.5f), new Vector2(34, 10), new Vector2(-20, 0));
            var dotsLayout = ui.HudDots.gameObject.AddComponent<HorizontalLayoutGroup>();
            dotsLayout.spacing = 10;
            dotsLayout.childAlignment = TextAnchor.MiddleLeft;
            dotsLayout.childControlWidth = dotsLayout.childControlHeight = false;
            dotsLayout.childForceExpandWidth = dotsLayout.childForceExpandHeight = false;
            var dot = Rect("Dot", ui.HudDots, Vector2.zero, Vector2.zero);
            dot.sizeDelta = Vector2.one * 24;
            ui.HudDot = dot.gameObject.AddComponent<Image>();
            ui.HudDot.sprite = Circle;
            ui.HudDot.raycastTarget = false;
            var dotEdge = dot.gameObject.AddComponent<Outline>();
            dotEdge.effectColor = new Color(Palette.Ink.r, Palette.Ink.g, Palette.Ink.b, 0.35f);
            dotEdge.effectDistance = new Vector2(2, -2);

            // ---- Speech bubble
            var dlgPanel = Rect("DialoguePanel", canvas, new Vector2(0.04f, 0.03f), new Vector2(0.96f, 0.3f));
            var bubble = OutlinedCard(dlgPanel, Color.white, 56, 6);
            var nameTag = Rect("NameTag", dlgPanel, new Vector2(0, 1), new Vector2(0, 1));
            nameTag.pivot = new Vector2(0, 0.5f);
            nameTag.sizeDelta = new Vector2(280, 80);
            nameTag.anchoredPosition = new Vector2(44, 4);
            Panel(nameTag, Palette.Teal, 40);
            var speaker = Label(nameTag, "Speaker", Doctor, 44, Palette.White);
            var body = Label(bubble, "Body", "", 50, Palette.Ink, TextAlignmentOptions.MidlineLeft, 32);
            body.rectTransform.offsetMin = new Vector2(52, 24);
            body.rectTransform.offsetMax = new Vector2(-240, -40);
            var next = Round(bubble, "NextButton", Play, Palette.Green, Palette.GreenDark, new Vector2(1, 0.5f), 170, new Vector2(-120, 0), 0.42f);
            next.transform.Find("Face/Icon").GetComponent<RectTransform>().anchoredPosition = new Vector2(6, 0); // optical centre of the triangle
            WithIdle((RectTransform)next.transform, IdleMotion.Kind.Pulse, 0.06f, 1.1f);
            ui.Dialogue = canvas.gameObject.AddComponent<DialogueView>();
            ui.Dialogue.EditorInit(dlgPanel.gameObject, speaker, body, next);

            // ---- Quiz: dimmed backdrop, card, three picture answers
            var quizPanel = Stretch("QuizPanel", canvas);
            Img(quizPanel, new Color(Palette.Ink.r, Palette.Ink.g, Palette.Ink.b, 0.45f));
            var quizCard = Rect("Card", quizPanel, new Vector2(0.06f, 0.07f), new Vector2(0.94f, 0.9f));
            var quizFill = OutlinedCard(quizCard, Color.white, 64, 8);
            var question = Label(quizFill, "Question", "", 66, Palette.Ink, TextAlignmentOptions.Center, 40);
            question.rectTransform.anchorMin = new Vector2(0, 0.8f);
            question.rectTransform.offsetMin = new Vector2(40, 0);
            question.rectTransform.offsetMax = new Vector2(-40, -24);
            var options = new Button[3];
            var icons = new Image[3];
            var labels = new TMP_Text[3];
            for (var i = 0; i < 3; i++)
            {
                var x0 = 0.04f + i * 0.32f;
                var tile = Rect($"Option{i + 1}", quizFill, new Vector2(x0, 0.19f), new Vector2(x0 + 0.28f, 0.78f));
                Panel(tile, Hex("#9FC9EE"), 48, raycast: true);
                var face = Rect("Face", tile, Vector2.zero, Vector2.one, new Vector2(0, 14), Vector2.zero);
                var faceImg = Panel(face, Color.white, 48);
                var edge = face.gameObject.AddComponent<Outline>();
                edge.effectColor = Hex("#9FC9EE");
                edge.effectDistance = new Vector2(3, -3);
                var iconRt = Rect("Icon", face, new Vector2(0.15f, 0.38f), new Vector2(0.85f, 0.95f));
                icons[i] = Picture(iconRt, null);
                labels[i] = Label(face, "Label", "", 40, Palette.Ink, TextAlignmentOptions.Center, 24);
                labels[i].rectTransform.anchorMax = new Vector2(1, 0.38f);
                labels[i].rectTransform.offsetMin = new Vector2(18, 12);
                labels[i].rectTransform.offsetMax = new Vector2(-18, 0);
                var button = tile.gameObject.AddComponent<Button>();
                button.targetGraphic = faceImg;
                var colors = button.colors;
                colors.disabledColor = new Color(0.85f, 0.85f, 0.85f, 0.9f);
                button.colors = colors;
                button.navigation = new Navigation { mode = Navigation.Mode.None };
                tile.gameObject.AddComponent<ButtonFeedback>();
                options[i] = button;
            }
            var feedback = Label(quizFill, "Feedback", "", 50, Palette.Teal, TextAlignmentOptions.Center, 30);
            feedback.rectTransform.anchorMax = new Vector2(1, 0.17f);
            ui.Quiz = canvas.gameObject.AddComponent<QuizView>();
            ui.Quiz.EditorInit(quizPanel.gameObject, question, feedback, options, icons, labels);

            // ---- Action buttons: hold (with a ring that fills as the drop falls) and blink
            var actions = Stretch("ActionButtons", canvas);
            var holdWrap = Rect("HoldButtonWrap", actions, new Vector2(0.83f, 0.58f), new Vector2(0.83f, 0.58f));
            holdWrap.sizeDelta = Vector2.one * 420;
            var track = Rect("RingTrack", holdWrap, Vector2.zero, Vector2.one);
            var trackImg = track.gameObject.AddComponent<Image>();
            trackImg.sprite = Ring;
            trackImg.color = new Color(1f, 1f, 1f, 0.55f);
            trackImg.raycastTarget = false;
            var ring = Rect("Ring", holdWrap, Vector2.zero, Vector2.one);
            var ringImg = ring.gameObject.AddComponent<Image>();
            ringImg.sprite = Ring;
            ringImg.color = Palette.Yellow;
            ringImg.type = Image.Type.Filled;
            ringImg.fillMethod = Image.FillMethod.Radial360;
            ringImg.fillOrigin = (int)Image.Origin360.Top;
            ringImg.fillClockwise = true;
            ringImg.fillAmount = 0f;
            ringImg.raycastTarget = false;
            var hold = Round(holdWrap, "HoldButton", Icon("quiz_big_pupil"), Palette.Green, Palette.GreenDark, new Vector2(0.5f, 0.5f), 330, Vector2.zero, 0.4f);
            var holdButton = hold.gameObject.AddComponent<HoldButton>();
            var holdIcon = (RectTransform)hold.transform.Find("Face/Icon");
            holdIcon.anchoredPosition = new Vector2(0, 46);
            var holdLabel = Label((RectTransform)hold.transform.Find("Face"), "Label", "", 36, Palette.White, TextAlignmentOptions.Center, 22);
            holdLabel.rectTransform.anchorMin = new Vector2(0.12f, 0.12f);
            holdLabel.rectTransform.anchorMax = new Vector2(0.88f, 0.45f);
            holdWrap.gameObject.AddComponent<IdleMotion>().EditorInit(IdleMotion.Kind.Pulse, 0.04f, 0.9f);

            var tap = Round(actions, "BlinkButton", Icon("quiz_eyes_shut"), Palette.Purple, Palette.PurpleDark, new Vector2(0.83f, 0.58f), 330, Vector2.zero, 0.4f);
            ((RectTransform)tap.transform.Find("Face/Icon")).anchoredPosition = new Vector2(0, 48);
            var tapLabel = Label((RectTransform)tap.transform.Find("Face"), "Label", "", 40, Palette.White, TextAlignmentOptions.Center, 24);
            tapLabel.rectTransform.anchorMin = new Vector2(0.12f, 0.12f);
            tapLabel.rectTransform.anchorMax = new Vector2(0.88f, 0.45f);
            WithIdle((RectTransform)tap.transform, IdleMotion.Kind.Pulse, 0.05f, 1.2f);
            ui.Buttons = actions.gameObject.AddComponent<ActionButtonsView>();
            ui.Buttons.EditorInit(holdButton, holdLabel, ringImg, tap, tapLabel);

            // ---- Reward
            var rewardPanel = Stretch("RewardPanel", canvas);
            Img(rewardPanel, new Color(Palette.Ink.r, Palette.Ink.g, Palette.Ink.b, 0.3f));
            var sticker = Rect("Sticker", rewardPanel, new Vector2(0.28f, 0.62f), new Vector2(0.28f, 0.62f));
            sticker.sizeDelta = Vector2.one * 540;
            Picture(sticker, Icon("sticker_brave_eyes"));
            var title = Label(rewardPanel, "Title", "", 96, Palette.White, TextAlignmentOptions.Center, 60);
            title.rectTransform.anchorMin = new Vector2(0.02f, 0.22f);
            title.rectTransform.anchorMax = new Vector2(0.54f, 0.36f);
            TextOutline(title, 0.25f);
            var rewardBody = Label(rewardPanel, "Body", "", 44, Palette.White, TextAlignmentOptions.Center, 28);
            rewardBody.rectTransform.anchorMin = new Vector2(0.05f, 0.13f);
            rewardBody.rectTransform.anchorMax = new Vector2(0.51f, 0.23f);
            TextOutline(rewardBody, 0.2f);
            var cont = Chunky(rewardPanel, "ContinueButton", "Back to visits", Palette.Green, Palette.GreenDark,
                new Vector2(0.28f, 0.075f), new Vector2(560, 140), Vector2.zero, Icon("home"), 52);
            WithIdle((RectTransform)cont.transform, IdleMotion.Kind.Pulse, 0.04f, 1f);
            ui.Reward = canvas.gameObject.AddComponent<RewardView>();
            ui.Reward.EditorInit(rewardPanel.gameObject, sticker, title, rewardBody, cont, confetti);

            // ---- Fader on top of everything
            var fader = Img(Stretch("Fader", canvas), new Color(0f, 0f, 0f, 0f), raycast: false);
            ui.Effects = fx.gameObject.AddComponent<ScreenEffectsView>();
            ui.Effects.EditorInit(upper, lower, blur, cold, fader, snow);
            return ui;
        }

        // ------------------------------------------------------------------ Menu scene

        static void BuildMenuScene(LevelCatalog catalog)
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            SetupLighting();

            var world = new GameObject("World").transform;
            Place(PrefabBuilder.RoomPath, "ExamRoom", Vector3.zero, Quaternion.identity, world);
            Place(PrefabBuilder.ChairPath, "ExamChair", new Vector3(0f, 0f, 0.2f), Quaternion.identity, world);
            var camPos = new Vector3(2.2f, 1.4f, 2.3f);
            var doctorPos = new Vector3(0.35f, 0f, 1.45f);
            var doctor = Place(PrefabBuilder.DoctorPath, "Doctor", doctorPos,
                Quaternion.LookRotation(new Vector3(camPos.x - doctorPos.x, 0f, camPos.z - doctorPos.z)), world);
            MainCamera(camPos, Look(camPos, new Vector3(-0.5f, 1.0f, -0.5f)), false);

            var canvas = (RectTransform)MakeCanvas().transform;

            // ---- Title
            var titlePanel = Stretch("TitlePanel", canvas);
            var title = Label(titlePanel, "Title", "Ophthalmology\nGame", 150, Palette.White, TextAlignmentOptions.BottomLeft);
            title.rectTransform.anchorMin = new Vector2(0.05f, 0.5f);
            title.rectTransform.anchorMax = new Vector2(0.7f, 0.93f);
            title.lineSpacing = -18;
            TextOutline(title, 0.28f);
            var sub = Rect("Subtitle", titlePanel, new Vector2(0.05f, 0.44f), new Vector2(0.05f, 0.44f));
            sub.pivot = new Vector2(0, 0.5f);
            sub.sizeDelta = new Vector2(640, 96);
            Panel(sub, Palette.Yellow, 48);
            Label(sub, "Label", "A visit to the eye doctor!", 46, Palette.Ink);
            var play = Chunky(titlePanel, "PlayButton", "Play", Palette.Green, Palette.GreenDark,
                new Vector2(0.2f, 0.2f), new Vector2(560, 200), Vector2.zero, Play, 96, 56);
            WithIdle((RectTransform)play.transform, IdleMotion.Kind.Pulse, 0.05f, 0.9f);
            Decor(titlePanel, "drop", new Vector2(0.4f, 0.22f), 150, IdleMotion.Kind.Bob, 14f, 0.6f);

            // ---- Level select
            var selectPanel = Stretch("LevelSelectPanel", canvas);
            var header = Rect("Header", selectPanel, new Vector2(0.04f, 0.9f), new Vector2(0.04f, 0.9f));
            header.pivot = new Vector2(0, 0.5f);
            header.sizeDelta = new Vector2(560, 120);
            Panel(header, Palette.Teal, 60);
            Label(header, "Label", "Pick a visit!", 66, Palette.White);

            var list = Rect("LevelList", selectPanel, new Vector2(0.04f, 0.17f), new Vector2(0.62f, 0.83f));
            var grid = list.gameObject.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(336, 300);
            grid.spacing = new Vector2(28, 28);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 3;
            grid.childAlignment = TextAnchor.UpperLeft;
            var card = BuildLevelCard(list);

            var back = Chunky(selectPanel, "BackButton", "Back", Palette.Coral, Palette.CoralDark,
                new Vector2(0.04f, 0.08f), new Vector2(300, 130), new Vector2(150, 0), null, 54);

            var controller = canvas.gameObject.AddComponent<MenuController>();
            controller.EditorInit(catalog, titlePanel.gameObject, selectPanel.gameObject, play, back, list, card, doctor.GetComponent<DoctorActor>());
            selectPanel.gameObject.SetActive(false);

            SaveScene(MenuScenePath);
        }

        static void Decor(RectTransform parent, string icon, Vector2 anchor, float size, IdleMotion.Kind kind, float amplitude, float speed)
        {
            var rt = Rect("Decor_" + icon, parent, anchor, anchor);
            rt.sizeDelta = Vector2.one * size;
            Picture(rt, Icon(icon));
            rt.gameObject.AddComponent<IdleMotion>().EditorInit(kind, amplitude, speed);
        }

        static LevelCardView BuildLevelCard(RectTransform list)
        {
            var root = Rect("LevelCard", list, Vector2.zero, Vector2.zero);
            Panel(root, Hex("#9FC9EE"), 44, raycast: true);
            var face = Rect("Face", root, Vector2.zero, Vector2.one, new Vector2(0, 12), Vector2.zero);
            var faceImg = Panel(face, Color.white, 44);

            var bubble = Rect("IconBubble", face, new Vector2(0.5f, 0.64f), new Vector2(0.5f, 0.64f));
            bubble.sizeDelta = Vector2.one * 168;
            var bubbleImg = bubble.gameObject.AddComponent<Image>();
            bubbleImg.sprite = Circle;
            bubbleImg.color = Palette.Sky;
            bubbleImg.raycastTarget = false;
            var iconRt = Rect("Icon", bubble, new Vector2(0.12f, 0.12f), new Vector2(0.88f, 0.88f));
            var icon = Picture(iconRt, null);

            var title = Label(face, "Title", "", 38, Palette.Ink, TextAlignmentOptions.Center, 26);
            title.rectTransform.anchorMin = new Vector2(0.04f, 0.2f);
            title.rectTransform.anchorMax = new Vector2(0.96f, 0.36f);
            var blurb = Label(face, "Blurb", "", 24, Palette.Grey, TextAlignmentOptions.Center, 16);
            blurb.rectTransform.anchorMin = new Vector2(0.06f, 0.03f);
            blurb.rectTransform.anchorMax = new Vector2(0.94f, 0.2f);

            GameObject Badge(string name, Color color, Sprite sprite, float iconScale)
            {
                var b = Rect(name, root, Vector2.one, Vector2.one);
                b.sizeDelta = Vector2.one * 92;
                b.anchoredPosition = new Vector2(-34, -34);
                var bg = b.gameObject.AddComponent<Image>();
                bg.sprite = Circle;
                bg.color = color;
                bg.raycastTarget = false;
                var edge = b.gameObject.AddComponent<Outline>();
                edge.effectColor = Palette.Ink;
                edge.effectDistance = new Vector2(3, -3);
                var i = Rect("Icon", b, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
                i.sizeDelta = Vector2.one * 92 * iconScale;
                Picture(i, sprite);
                return b.gameObject;
            }
            var lockBadge = Badge("LockBadge", Color.white, Icon("padlock"), 0.62f);
            var doneBadge = Badge("DoneBadge", Palette.Green, Check, 0.62f);

            var button = root.gameObject.AddComponent<Button>();
            button.targetGraphic = faceImg;
            var colors = button.colors;
            colors.disabledColor = Color.white; // LevelCardView greys locked cards itself
            button.colors = colors;
            button.navigation = new Navigation { mode = Navigation.Mode.None };
            root.gameObject.AddComponent<ButtonFeedback>();

            var view = root.gameObject.AddComponent<LevelCardView>();
            view.EditorInit(button, faceImg, icon, title, blurb, lockBadge, doneBadge);
            return view;
        }
    }
}
