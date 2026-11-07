using System.Linq;
using Hindsight.Ophthalmology.Core;
using Hindsight.Ophthalmology.Core.Avatar;
using Hindsight.Ophthalmology.Core.Progress;
using Hindsight.Ophthalmology.SceneObjects;
using Hindsight.Ophthalmology.UI;
using TMPro;
using UnityEditor;
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
            public CaptionView Captions;
            public NarrationPlayer Narrator;
            public ActionButtonsView Buttons;
            public ScreenEffectsView Effects;
            public RewardView Reward;
            public LevelHud Hud;
            public RectTransform HudDots;
            public Image HudDot;
            public Button HudHome;
            public Button HudGear;
            public SettingsView Settings;
        }

        static readonly Color DimInk = new(0.18f, 0.23f, 0.35f, 0.5f);

        static LevelUi BuildLevelUi(Transform canvas, ParticleSystem confetti, Texture[] portraits)
        {
            var ui = new LevelUi();

            // ---- Narration voice
            var voice = new GameObject("Narration");
            var narrator = voice.AddComponent<NarrationPlayer>();
            ui.Narrator = narrator;

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

            // ---- HUD: home, a pill of progress dots (no words), settings
            var hud = Stretch("Hud", canvas);
            ui.Hud = hud.gameObject.AddComponent<LevelHud>();
            ui.HudHome = Round(hud, "ExitButton", Icon("home"), Palette.Coral, Palette.CoralDark, new Vector2(0, 1), 130, new Vector2(100, -96), 0.5f);
            ui.HudGear = Round(hud, "SettingsButton", Icon("gear"), Palette.Grey, Hex("#6F7A90"), new Vector2(1, 1), 120, new Vector2(-96, -96), 0.55f);
            var pill = Rect("StepPill", hud, new Vector2(0.5f, 1), new Vector2(0.5f, 1));
            pill.sizeDelta = new Vector2(560, 84);
            pill.anchoredPosition = new Vector2(0, -96);
            var pillFill = OutlinedCard(pill, new Color(1f, 1f, 1f, 0.96f), 42, 5);
            ui.HudDots = Rect("Dots", pillFill, Vector2.zero, Vector2.one, new Vector2(24, 0), new Vector2(-24, 0));
            var dotsLayout = ui.HudDots.gameObject.AddComponent<HorizontalLayoutGroup>();
            dotsLayout.spacing = 12;
            dotsLayout.childAlignment = TextAnchor.MiddleCenter;
            dotsLayout.childControlWidth = dotsLayout.childControlHeight = false;
            dotsLayout.childForceExpandWidth = dotsLayout.childForceExpandHeight = false;
            var dot = Rect("Dot", ui.HudDots, Vector2.zero, Vector2.zero);
            dot.sizeDelta = Vector2.one * 26;
            ui.HudDot = dot.gameObject.AddComponent<Image>();
            ui.HudDot.sprite = Circle;
            ui.HudDot.raycastTarget = false;
            var dotEdge = dot.gameObject.AddComponent<Outline>();
            dotEdge.effectColor = new Color(Palette.Ink.r, Palette.Ink.g, Palette.Ink.b, 0.35f);
            dotEdge.effectDistance = new Vector2(2, -2);

            // ---- Caption bubble: portrait, one short line, replay, next
            var capPanel = Rect("CaptionPanel", canvas, new Vector2(0.1f, 0.03f), new Vector2(0.9f, 0.21f));
            Panel(capPanel, Palette.Ink, 56, raycast: true);
            var bubbleRt = Rect("Fill", capPanel, Vector2.zero, Vector2.one, new Vector2(6, 6), new Vector2(-6, -6));
            var bubble = Panel(bubbleRt, Color.white, 50);
            var portraitRing = Rect("Portrait", capPanel, new Vector2(0, 0.5f), new Vector2(0, 0.5f));
            portraitRing.sizeDelta = Vector2.one * 200;
            portraitRing.anchoredPosition = new Vector2(70, 30);
            var ringImg = portraitRing.gameObject.AddComponent<Image>();
            ringImg.sprite = Circle;
            ringImg.color = Palette.Ink;
            ringImg.raycastTarget = false;
            var portraitBg = Rect("Bg", portraitRing, Vector2.zero, Vector2.one, new Vector2(7, 7), new Vector2(-7, -7));
            var bgImg = portraitBg.gameObject.AddComponent<Image>();
            bgImg.sprite = Circle;
            bgImg.color = Palette.Sky;
            bgImg.raycastTarget = false;
            // The speaker's live 3D portrait (a render texture), clipped to the circle.
            portraitBg.gameObject.AddComponent<Mask>().showMaskGraphic = true;
            var portrait = Rect("Face", portraitBg, Vector2.zero, Vector2.one).gameObject.AddComponent<RawImage>();
            portrait.raycastTarget = false;
            var caption = Label(bubbleRt, "Caption", "", 64, Palette.Ink, TextAlignmentOptions.MidlineLeft, 36);
            caption.rectTransform.offsetMin = new Vector2(200, 12);
            caption.rectTransform.offsetMax = new Vector2(-330, -12);
            var replay = Round(bubbleRt, "ReplayButton", Icon("replay"), Palette.Blue, Palette.BlueDark, new Vector2(1, 0.5f), 130, new Vector2(-270, 0), 0.5f);
            var next = Round(bubbleRt, "NextButton", Play, Palette.Green, Palette.GreenDark, new Vector2(1, 0.5f), 160, new Vector2(-110, 0), 0.42f);
            next.transform.Find("Face/Icon").GetComponent<RectTransform>().anchoredPosition = new Vector2(6, 0); // optical centre of the triangle
            WithIdle((RectTransform)next.transform, IdleMotion.Kind.Pulse, 0.06f, 1.1f);
            ui.Captions = canvas.gameObject.AddComponent<CaptionView>();
            ui.Captions.EditorInit(capPanel.gameObject, bubble, portrait, caption, next, replay, narrator,
                portraits,
                new[] { Color.white, Hex("#E9F8EC"), Color.white });

            // ---- Action buttons: hold (with a ring that fills as the drop falls) and blink
            var actions = Stretch("ActionButtons", canvas);
            var holdWrap = Rect("HoldButtonWrap", actions, new Vector2(0.83f, 0.6f), new Vector2(0.83f, 0.6f));
            holdWrap.sizeDelta = Vector2.one * 420;
            var track = Rect("RingTrack", holdWrap, Vector2.zero, Vector2.one);
            var trackImg = track.gameObject.AddComponent<Image>();
            trackImg.sprite = Ring;
            trackImg.color = new Color(1f, 1f, 1f, 0.55f);
            trackImg.raycastTarget = false;
            var ring = Rect("Ring", holdWrap, Vector2.zero, Vector2.one);
            var ringFill = ring.gameObject.AddComponent<Image>();
            ringFill.sprite = Ring;
            ringFill.color = Palette.Yellow;
            ringFill.type = Image.Type.Filled;
            ringFill.fillMethod = Image.FillMethod.Radial360;
            ringFill.fillOrigin = (int)Image.Origin360.Top;
            ringFill.fillClockwise = true;
            ringFill.fillAmount = 0f;
            ringFill.raycastTarget = false;
            var hold = Round(holdWrap, "HoldButton", Icon("quiz_big_pupil"), Palette.Green, Palette.GreenDark, new Vector2(0.5f, 0.5f), 330, Vector2.zero, 0.5f);
            var holdButton = hold.gameObject.AddComponent<HoldButton>();
            var holdLabel = Label((RectTransform)hold.transform.Find("Face"), "Label", "", 36, Palette.White, TextAlignmentOptions.Center, 22);
            holdLabel.rectTransform.anchorMin = new Vector2(0.12f, 0.08f);
            holdLabel.rectTransform.anchorMax = new Vector2(0.88f, 0.3f);
            holdWrap.gameObject.AddComponent<IdleMotion>().EditorInit(IdleMotion.Kind.Pulse, 0.04f, 0.9f);

            var tap = Round(actions, "BlinkButton", Icon("quiz_eyes_shut"), Palette.Purple, Palette.PurpleDark, new Vector2(0.83f, 0.6f), 330, Vector2.zero, 0.4f);
            ((RectTransform)tap.transform.Find("Face/Icon")).anchoredPosition = new Vector2(0, 40);
            var tapLabel = Label((RectTransform)tap.transform.Find("Face"), "Label", "", 64, Palette.White, TextAlignmentOptions.Center, 30);
            tapLabel.rectTransform.anchorMin = new Vector2(0.12f, 0.1f);
            tapLabel.rectTransform.anchorMax = new Vector2(0.88f, 0.42f);
            WithIdle((RectTransform)tap.transform, IdleMotion.Kind.Pulse, 0.05f, 1.2f);
            ui.Buttons = actions.gameObject.AddComponent<ActionButtonsView>();
            ui.Buttons.EditorInit(holdButton, holdLabel, ringFill, tap, tapLabel);

            // ---- Reward: the sticker, coins flying into the purse, then Home and Play again
            var rewardPanel = Stretch("RewardPanel", canvas);
            Img(rewardPanel, new Color(Palette.Ink.r, Palette.Ink.g, Palette.Ink.b, 0.3f));
            var sticker = Rect("Sticker", rewardPanel, new Vector2(0.3f, 0.6f), new Vector2(0.3f, 0.6f));
            sticker.sizeDelta = Vector2.one * 520;
            var stickerImg = Picture(sticker, Icon("sticker_brave_eyes"));
            stickerImg.raycastTarget = true;
            var stickerButton = sticker.gameObject.AddComponent<Button>();
            stickerButton.targetGraphic = stickerImg;
            stickerButton.transition = Selectable.Transition.None;
            stickerButton.navigation = new Navigation { mode = Navigation.Mode.None };
            var stickerWrap = WithIdle(sticker, IdleMotion.Kind.Pulse, 0.05f, 1f);
            stickerWrap.name = "StickerWrap";
            var title = Label(rewardPanel, "Title", "", 110, Palette.White, TextAlignmentOptions.Center, 60);
            title.rectTransform.anchorMin = new Vector2(0.02f, 0.17f);
            title.rectTransform.anchorMax = new Vector2(0.58f, 0.32f);
            TextOutline(title, 0.25f);
            var chip = Rect("CoinChip", rewardPanel, new Vector2(0.3f, 0.12f), new Vector2(0.3f, 0.12f));
            chip.sizeDelta = new Vector2(300, 110);
            OutlinedCard(chip, Palette.Yellow, 55, 5);
            var chipIcon = Rect("Icon", chip, new Vector2(0, 0.5f), new Vector2(0, 0.5f));
            chipIcon.sizeDelta = Vector2.one * 84;
            chipIcon.anchoredPosition = new Vector2(64, 0);
            Picture(chipIcon, Icon("coin"));
            var chipLabel = Label(chip, "Label", "+40", 64, Palette.Ink, TextAlignmentOptions.Center);
            chipLabel.rectTransform.offsetMin = new Vector2(100, 0);
            var purse = Purse(rewardPanel, new Vector2(1, 1), new Vector2(-190, -96));
            var endGroup = Rect("EndButtons", rewardPanel, new Vector2(0.78f, 0.22f), new Vector2(0.78f, 0.22f));
            endGroup.sizeDelta = new Vector2(500, 220);
            var home = Round(endGroup, "HomeButton", Icon("home"), Palette.Coral, Palette.CoralDark, new Vector2(0.22f, 0.5f), 190, Vector2.zero, 0.5f);
            var again = Round(endGroup, "ReplayLevelButton", Play, Palette.Green, Palette.GreenDark, new Vector2(0.78f, 0.5f), 190, Vector2.zero, 0.42f);
            again.transform.Find("Face/Icon").GetComponent<RectTransform>().anchoredPosition = new Vector2(8, 0);
            var flyingCoin = Rect("FlyingCoin", rewardPanel, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            flyingCoin.sizeDelta = Vector2.one * 84;
            Picture(flyingCoin, Icon("coin"));
            ui.Reward = canvas.gameObject.AddComponent<RewardView>();
            ui.Reward.EditorInit(rewardPanel.gameObject, sticker, stickerButton, title, chip.gameObject, chipLabel, endGroup.gameObject,
                home, again, purse, confetti, flyingCoin, (RectTransform)purse.transform.Find("Icon"));

            // ---- Settings (paused from the HUD) and its confirm dialog
            var confirm = BuildConfirmDialog(canvas);
            ui.Settings = BuildSettingsPanel(canvas, confirm);

            // ---- Fader on top of everything
            var fader = Img(Stretch("Fader", canvas), new Color(0f, 0f, 0f, 0f), raycast: false);
            ui.Effects = fx.gameObject.AddComponent<ScreenEffectsView>();
            ui.Effects.EditorInit(upper, lower, blur, cold, fader, snow);
            confirm.transform.SetAsLastSibling();
            return ui;
        }

        // ------------------------------------------------------------------ shared widgets

        /// <summary>The coin purse pill: coin icon and the current player's coins.</summary>
        static CoinCounter Purse(RectTransform parent, Vector2 anchor, Vector2 offset)
        {
            var rt = Rect("Purse", parent, anchor, anchor);
            rt.sizeDelta = new Vector2(260, 104);
            rt.anchoredPosition = offset;
            OutlinedCard(rt, Color.white, 52, 5);
            var icon = Rect("Icon", rt, new Vector2(0, 0.5f), new Vector2(0, 0.5f));
            icon.sizeDelta = Vector2.one * 80;
            icon.anchoredPosition = new Vector2(58, 0);
            Picture(icon, Icon("coin"));
            var label = Label(rt, "Coins", "0", 56, Palette.Ink, TextAlignmentOptions.Center);
            label.rectTransform.offsetMin = new Vector2(96, 0);
            label.rectTransform.offsetMax = new Vector2(-16, 0);
            var counter = rt.gameObject.AddComponent<CoinCounter>();
            counter.EditorInit(label);
            return counter;
        }

        static ConfirmDialog BuildConfirmDialog(Transform canvas)
        {
            var panel = Stretch("ConfirmPanel", canvas);
            Img(panel, DimInk);
            var card = Rect("Card", panel, new Vector2(0.24f, 0.24f), new Vector2(0.76f, 0.76f));
            var fill = OutlinedCard(card, Color.white, 56, 7);
            var title = Label(fill, "Title", "", 66, Palette.Ink, TextAlignmentOptions.Center, 40);
            title.rectTransform.anchorMin = new Vector2(0, 0.68f);
            title.rectTransform.offsetMin = new Vector2(40, 0);
            title.rectTransform.offsetMax = new Vector2(-40, -24);
            var body = Label(fill, "Body", "", 40, Palette.Grey, TextAlignmentOptions.Center, 26);
            body.rectTransform.anchorMin = new Vector2(0, 0.36f);
            body.rectTransform.anchorMax = new Vector2(1, 0.7f);
            body.rectTransform.offsetMin = new Vector2(50, 0);
            body.rectTransform.offsetMax = new Vector2(-50, 0);
            var yes = Chunky(fill, "YesButton", "Yes", Palette.Coral, Palette.CoralDark, new Vector2(0.28f, 0.17f), new Vector2(380, 130), Vector2.zero, null, 46);
            var no = Chunky(fill, "NoButton", "No", Palette.Green, Palette.GreenDark, new Vector2(0.72f, 0.17f), new Vector2(380, 130), Vector2.zero, null, 46);
            var dialog = panel.gameObject.AddComponent<ConfirmDialog>();
            dialog.EditorInit(panel.gameObject, title, body, yes, yes.transform.Find("Face/Label").GetComponent<TMP_Text>(),
                no, no.transform.Find("Face/Label").GetComponent<TMP_Text>());
            return dialog;
        }

        static SettingsView BuildSettingsPanel(Transform canvas, ConfirmDialog confirm)
        {
            var panel = Stretch("SettingsPanel", canvas);
            Img(panel, DimInk);
            var card = Rect("Card", panel, new Vector2(0.17f, 0.05f), new Vector2(0.83f, 0.95f));
            var fill = OutlinedCard(card, Color.white, 60, 7);
            var header = Label(fill, "Header", "Settings", 72, Palette.Ink, TextAlignmentOptions.Center);
            header.rectTransform.anchorMin = new Vector2(0, 0.88f);
            var close = Round(fill, "CloseButton", Icon("close"), Palette.Coral, Palette.CoralDark, new Vector2(1, 1), 120, new Vector2(-84, -78), 0.42f);

            var names = new[] { "Sound", "Voice", "Captions", "Big text", "Move on by itself", "Less motion" };
            var rows = names.Select((n, i) =>
            {
                var y = 0.8f - i * 0.112f;
                var row = Rect("Row_" + n, fill, new Vector2(0.06f, y - 0.1f), new Vector2(0.94f, y));
                var label = Label(row, "Label", n, 46, Palette.Ink, TextAlignmentOptions.MidlineLeft, 28);
                label.rectTransform.anchorMax = new Vector2(0.48f, 1);
                return row;
            }).ToArray();

            var sound = MakeSlider(rows[0]);
            var voice = MakeSlider(rows[1]);
            var captions = MakeOnOff(rows[2]);
            var big = MakeOnOff(rows[3]);
            var auto = MakeOnOff(rows[4]);
            var calm = MakeOnOff(rows[5]);
            var reset = Chunky(fill, "ResetButton", "Reset game", Palette.Coral, Palette.CoralDark, new Vector2(0.5f, 0.07f), new Vector2(460, 120), Vector2.zero, null, 44);

            var view = panel.gameObject.AddComponent<SettingsView>();
            view.EditorInit(panel.gameObject, sound, voice, captions, big, auto, calm, reset, close, confirm);
            return view;
        }

        static Slider MakeSlider(RectTransform row)
        {
            var root = Rect("Slider", row, new Vector2(0.5f, 0.2f), new Vector2(1f, 0.8f));
            var slider = root.gameObject.AddComponent<Slider>();
            var bg = Rect("Background", root, new Vector2(0, 0.32f), new Vector2(1, 0.68f));
            Panel(bg, Palette.TileEdge, 20, raycast: true);
            var fillArea = Rect("Fill Area", root, new Vector2(0, 0.32f), new Vector2(1, 0.68f), new Vector2(10, 0), new Vector2(-30, 0));
            var fillRt = Rect("Fill", fillArea, Vector2.zero, Vector2.one, new Vector2(-10, 0), new Vector2(10, 0));
            Panel(fillRt, Palette.Teal, 20);
            var handleArea = Rect("Handle Slide Area", root, Vector2.zero, Vector2.one, new Vector2(30, 0), new Vector2(-30, 0));
            var handle = Rect("Handle", handleArea, Vector2.zero, new Vector2(0, 1));
            handle.sizeDelta = new Vector2(70, 0);
            var handleImg = handle.gameObject.AddComponent<Image>();
            handleImg.sprite = Circle;
            handleImg.color = Palette.Yellow;
            handleImg.preserveAspect = true;
            var edge = handle.gameObject.AddComponent<Outline>();
            edge.effectColor = Palette.Ink;
            edge.effectDistance = new Vector2(3, -3);
            slider.fillRect = fillRt;
            slider.handleRect = handle;
            slider.targetGraphic = handleImg;
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 1f;
            slider.navigation = new Navigation { mode = Navigation.Mode.None };
            return slider;
        }

        static OnOffButton MakeOnOff(RectTransform row)
        {
            var button = Chunky(row, "Switch", "On", Palette.Green, Palette.GreenDark, new Vector2(0.78f, 0.5f), new Vector2(260, 100), Vector2.zero, Check, 44, 34);
            var onOff = button.gameObject.AddComponent<OnOffButton>();
            onOff.EditorInit(button, button.transform.Find("Face").GetComponent<Image>(), button.GetComponent<Image>(),
                button.transform.Find("Face/Label").GetComponent<TMP_Text>(), button.transform.Find("Face/Icon").gameObject);
            return onOff;
        }

        // ------------------------------------------------------------------ Menu scene

        const string PortraitDir = Root + "/Art/UI";

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
            // The kid takes the doctor's spot on level select and dress-up (MenuController swaps them).
            var kidPos = doctorPos;
            var kid = Place(PrefabBuilder.KidPath, "PlayerKid", kidPos,
                Quaternion.LookRotation(new Vector3(camPos.x - kidPos.x, 0f, camPos.z - kidPos.z)), world);
            var cam = MainCamera(camPos, Look(camPos, new Vector3(-0.5f, 1.0f, -0.5f)), false);

            // Camera poses: the room, and a "mirror" close-up for Dress Up with the kid on the left.
            var views = new GameObject("CameraViews").transform;
            var roomView = new GameObject("RoomView").transform;
            roomView.SetParent(views, false);
            roomView.SetPositionAndRotation(cam.transform.position, cam.transform.rotation);
            var toCam = new Vector3(camPos.x - kidPos.x, 0f, camPos.z - kidPos.z).normalized;
            var mirrorPos = kidPos + toCam * 1.9f + Vector3.up * 0.8f;
            var camRight = Vector3.Cross(Vector3.up, -toCam);
            var mirrorView = new GameObject("MirrorView").transform;
            mirrorView.SetParent(views, false);
            mirrorView.SetPositionAndRotation(mirrorPos, Look(mirrorPos, kidPos + Vector3.up * 0.55f + camRight * 0.6f));
            // Level select: the kid on the right, clear of the level cards on the left.
            var kidView = new GameObject("KidView").transform;
            kidView.SetParent(views, false);
            kidView.SetPositionAndRotation(camPos, Look(camPos, kidPos + Vector3.up * 0.45f - camRight * 0.75f));

            var studio = BuildPortraitStudio(out var portraits);

            var canvas = (RectTransform)MakeCanvas().transform;
            var confirm = BuildConfirmDialog(canvas);
            var settings = BuildSettingsPanel(canvas, confirm);

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
            var gear = Round(titlePanel, "SettingsButton", Icon("gear"), Palette.Grey, Hex("#6F7A90"), new Vector2(1, 1), 130, new Vector2(-100, -100), 0.55f);
            Decor(titlePanel, "drop", new Vector2(0.4f, 0.22f), 150, IdleMotion.Kind.Bob, 14f, 0.6f);
            var notice = Rect("ResetNotice", titlePanel, new Vector2(0.5f, 0.06f), new Vector2(0.5f, 0.06f));
            notice.sizeDelta = new Vector2(820, 100);
            OutlinedCard(notice, Palette.Yellow, 50, 5);
            Label(notice, "Label", "Everything was reset.", 46, Palette.Ink);
            notice.gameObject.SetActive(false);

            // ---- Save files
            var savesPanel = Stretch("SavesPanel", canvas);
            Header(savesPanel, "Who's playing?");
            var saveView = BuildSaveCards(savesPanel, portraits, studio, confirm);
            var savesBack = Chunky(savesPanel, "BackButton", "", Palette.Coral, Palette.CoralDark,
                new Vector2(0.04f, 0.08f), new Vector2(170, 130), new Vector2(85, 0), Icon("home"), 54);

            // ---- Level select
            var selectPanel = Stretch("LevelSelectPanel", canvas);
            Header(selectPanel, "Pick a visit!");
            Purse(selectPanel, new Vector2(1, 1), new Vector2(-190, -96));
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
            var dressButton = Chunky(selectPanel, "DressUpButton", "Dress up", Palette.Purple, Palette.PurpleDark,
                new Vector2(0.84f, 0.1f), new Vector2(440, 150), Vector2.zero, Icon("shirt"), 56);
            WithIdle((RectTransform)dressButton.transform, IdleMotion.Kind.Pulse, 0.04f, 0.8f);

            // ---- Dress up
            var dressPanel = Stretch("DressUpPanel", canvas);
            var dressView = BuildDressUp(dressPanel, kid.GetComponent<KidActor>());

            var controller = canvas.gameObject.AddComponent<MenuController>();
            controller.EditorInit(catalog, titlePanel.gameObject, savesPanel.gameObject, selectPanel.gameObject, dressPanel.gameObject,
                notice.gameObject, play, gear, savesBack, back, dressButton, list, card, saveView, dressView, settings,
                doctor.GetComponent<DoctorActor>(), kid.GetComponent<KidActor>(), cam.transform, roomView, mirrorView, kidView);
            savesPanel.gameObject.SetActive(false);
            selectPanel.gameObject.SetActive(false);
            dressPanel.gameObject.SetActive(false);
            settings.transform.SetAsLastSibling();
            confirm.transform.SetAsLastSibling();

            SaveScene(MenuScenePath);
        }

        static void Header(RectTransform parent, string text)
        {
            var header = Rect("Header", parent, new Vector2(0.04f, 0.9f), new Vector2(0.04f, 0.9f));
            header.pivot = new Vector2(0, 0.5f);
            header.sizeDelta = new Vector2(620, 120);
            Panel(header, Palette.Teal, 60);
            Label(header, "Label", text, 66, Palette.White);
        }

        /// <summary>Three kids far from the room, each with its own camera rendering into a texture for its save card.</summary>
        static PortraitStudio BuildPortraitStudio(out RenderTexture[] textures)
        {
            var root = new GameObject("PortraitStudio").transform;
            root.position = new Vector3(60f, 0f, 0f);
            Primitive(PrimitiveType.Cube, "Backdrop", root, new Vector3(2f, 1.5f, 1.2f), new Vector3(9f, 4f, 0.1f), Lit("PortraitBackdrop", Hex("#E1F1FF")));
            Primitive(PrimitiveType.Cube, "Floor", root, new Vector3(2f, -0.05f, 0f), new Vector3(9f, 0.1f, 4f), Lit("PortraitFloor", Hex("#CDE8FF")));
            textures = new RenderTexture[SaveSlots.Count];
            var kids = new KidActor[SaveSlots.Count];
            for (var i = 0; i < SaveSlots.Count; i++)
            {
                var pos = root.position + new Vector3(i * 2f, 0f, 0f);
                var kid = Place(PrefabBuilder.KidPath, $"PortraitKid{i + 1}", pos, Quaternion.Euler(0, 180f, 0), root);
                kids[i] = kid.GetComponent<KidActor>();

                var path = $"{PortraitDir}/Portrait_{i + 1}.renderTexture";
                var rt = AssetDatabase.LoadAssetAtPath<RenderTexture>(path);
                if (rt == null)
                {
                    rt = new RenderTexture(512, 512, 24) { name = $"Portrait_{i + 1}", antiAliasing = 4 };
                    AssetDatabase.CreateAsset(rt, path);
                }
                textures[i] = rt;

                var camGo = new GameObject($"PortraitCamera{i + 1}");
                camGo.transform.SetParent(root, true);
                var camPos = pos + new Vector3(0f, 0.72f, -1.5f);
                camGo.transform.SetPositionAndRotation(camPos, Look(camPos, pos + Vector3.up * 0.62f));
                var cam = camGo.AddComponent<Camera>();
                cam.fieldOfView = 34f;
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = Hex("#E1F1FF");
                cam.targetTexture = rt;
                cam.nearClipPlane = 0.05f;
                cam.farClipPlane = 8f;

                var light = new GameObject($"PortraitLight{i + 1}").AddComponent<Light>();
                light.transform.SetParent(root, true);
                light.transform.position = pos + new Vector3(-0.5f, 1.6f, -1.2f);
                light.type = LightType.Point;
                light.range = 4f;
                light.intensity = 1.6f;
            }
            var studio = root.gameObject.AddComponent<PortraitStudio>();
            studio.EditorInit(kids);
            return studio;
        }

        static SaveSelectView BuildSaveCards(RectTransform panel, RenderTexture[] portraits, PortraitStudio studio, ConfirmDialog confirm)
        {
            var n = SaveSlots.Count;
            var cards = new Button[n];
            var titles = new TMP_Text[n];
            var coinRows = new GameObject[n];
            var coinLabels = new TMP_Text[n];
            var stickers = new GameObject[n];
            var newBadges = new GameObject[n];
            var deletes = new Button[n];
            for (var i = 0; i < n; i++)
            {
                var x = 0.08f + i * 0.3f;
                var root = Rect($"SaveCard{i + 1}", panel, new Vector2(x, 0.24f), new Vector2(x + 0.26f, 0.8f));
                Panel(root, Hex("#9FC9EE"), 48, raycast: true);
                var face = Rect("Face", root, Vector2.zero, Vector2.one, new Vector2(0, 14), Vector2.zero);
                var faceImg = Panel(face, Color.white, 48);

                var frame = Rect("Portrait", face, new Vector2(0.1f, 0.34f), new Vector2(0.9f, 0.95f));
                Panel(frame, Palette.Sky, 36);
                var raw = Rect("Image", frame, Vector2.zero, Vector2.one, new Vector2(8, 8), new Vector2(-8, -8)).gameObject.AddComponent<RawImage>();
                raw.texture = portraits[i];
                raw.raycastTarget = false;
                var plus = Rect("NewBadge", frame, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
                plus.sizeDelta = Vector2.one * 150;
                var plusBg = plus.gameObject.AddComponent<Image>();
                plusBg.sprite = Circle;
                plusBg.color = Palette.Green;
                plusBg.raycastTarget = false;
                var plusIcon = Rect("Icon", plus, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
                plusIcon.sizeDelta = Vector2.one * 70;
                plusIcon.anchoredPosition = new Vector2(6, 0);
                Picture(plusIcon, Play);

                titles[i] = Label(face, "Title", "", 52, Palette.Ink, TextAlignmentOptions.Center, 30);
                titles[i].rectTransform.anchorMin = new Vector2(0.04f, 0.18f);
                titles[i].rectTransform.anchorMax = new Vector2(0.96f, 0.33f);

                var coinRow = Rect("Coins", face, new Vector2(0.25f, 0.04f), new Vector2(0.75f, 0.17f));
                var coinIcon = Rect("Icon", coinRow, new Vector2(0, 0.5f), new Vector2(0, 0.5f));
                coinIcon.sizeDelta = Vector2.one * 60;
                coinIcon.anchoredPosition = new Vector2(30, 0);
                Picture(coinIcon, Icon("coin"));
                coinLabels[i] = Label(coinRow, "Label", "0", 46, Palette.Ink, TextAlignmentOptions.Center);
                coinLabels[i].rectTransform.offsetMin = new Vector2(64, 0);

                var sticker = Rect("Sticker", root, Vector2.one, Vector2.one);
                sticker.sizeDelta = Vector2.one * 120;
                sticker.anchoredPosition = new Vector2(-30, -30);
                Picture(sticker, Icon("sticker_brave_eyes"));

                var del = Round(root, "DeleteButton", Icon("close"), Palette.Coral, Palette.CoralDark, new Vector2(0.5f, 0f), 100, new Vector2(0, -80), 0.42f);

                var button = root.gameObject.AddComponent<Button>();
                button.targetGraphic = faceImg;
                button.navigation = new Navigation { mode = Navigation.Mode.None };
                root.gameObject.AddComponent<ButtonFeedback>();

                cards[i] = button;
                coinRows[i] = coinRow.gameObject;
                stickers[i] = sticker.gameObject;
                newBadges[i] = plus.gameObject;
                deletes[i] = del;
            }
            var view = panel.gameObject.AddComponent<SaveSelectView>();
            view.EditorInit(cards, titles, coinRows, coinLabels, stickers, newBadges, deletes, studio, confirm);
            return view;
        }

        static DressUpView BuildDressUp(RectTransform panel, KidActor kid)
        {
            var card = Rect("Card", panel, new Vector2(0.47f, 0.04f), new Vector2(0.97f, 0.96f));
            var fill = OutlinedCard(card, new Color(1f, 1f, 1f, 0.97f), 56, 7);
            var purse = Purse(panel, new Vector2(0, 1), new Vector2(190, -96));

            // Clothes first: they are what children change most.
            var tabNames = new[] { "Outfits", "Hats", "Capes", "Hair", "Shirt", "Shoes", "Hair colour", "Skin" };
            var tabSlots = new[] { Slot.Outfit, Slot.Hat, Slot.Cape, Slot.HairStyle, Slot.Shirt, Slot.Shoes, Slot.Hair, Slot.Skin };
            var tabs = new Button[tabNames.Length];
            for (var i = 0; i < tabNames.Length; i++)
            {
                var col = i % 4;
                var row = i / 4;
                var anchor = new Vector2(0.135f + col * 0.243f, 0.92f - row * 0.085f);
                tabs[i] = Chunky(fill, "Tab_" + tabNames[i].Replace(" ", ""), tabNames[i], Color.white, Palette.TileEdge, anchor, new Vector2(164, 78), Vector2.zero, null, 30, 30);
                tabs[i].transform.Find("Face/Label").GetComponent<TMP_Text>().color = Palette.Ink;
            }

            var gridRt = Rect("Grid", fill, new Vector2(0.04f, 0.2f), new Vector2(0.96f, 0.8f));
            var grid = gridRt.gameObject.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(170, 170);
            grid.spacing = new Vector2(18, 18);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 4;
            grid.childAlignment = TextAnchor.UpperCenter;

            // Item template: a tile with a colour swatch or a name, a price chip and a "worn" ring.
            var item = Rect("ItemTemplate", gridRt, Vector2.zero, Vector2.zero);
            Panel(item, Palette.TileEdge, 36, raycast: true);
            var face = Rect("Face", item, Vector2.zero, Vector2.one, new Vector2(0, 10), Vector2.zero);
            var faceImg = Panel(face, Color.white, 36);
            var swatch = Rect("Swatch", face, new Vector2(0.18f, 0.18f), new Vector2(0.82f, 0.82f));
            var swatchImg = swatch.gameObject.AddComponent<Image>();
            swatchImg.sprite = Circle;
            swatchImg.raycastTarget = false;
            var swatchEdge = swatch.gameObject.AddComponent<Outline>();
            swatchEdge.effectColor = Palette.Ink;
            swatchEdge.effectDistance = new Vector2(3, -3);
            var label = Label(face, "Label", "", 34, Palette.Ink, TextAlignmentOptions.Center, 20);
            label.rectTransform.offsetMin = new Vector2(10, 10);
            label.rectTransform.offsetMax = new Vector2(-10, -10);
            var price = Rect("Price", item, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f));
            price.sizeDelta = new Vector2(120, 52);
            price.anchoredPosition = new Vector2(0, 6);
            OutlinedCard(price, Palette.Yellow, 26, 3);
            var priceIcon = Rect("Icon", price, new Vector2(0, 0.5f), new Vector2(0, 0.5f));
            priceIcon.sizeDelta = Vector2.one * 38;
            priceIcon.anchoredPosition = new Vector2(26, 0);
            Picture(priceIcon, Icon("coin"));
            var priceLabel = Label(price, "Label", "0", 30, Palette.Ink, TextAlignmentOptions.Center);
            priceLabel.rectTransform.offsetMin = new Vector2(44, 0);
            var selected = Rect("Selected", item, new Vector2(1, 1), new Vector2(1, 1));
            selected.sizeDelta = Vector2.one * 56;
            selected.anchoredPosition = new Vector2(-12, -12);
            var selBg = selected.gameObject.AddComponent<Image>();
            selBg.sprite = Circle;
            selBg.color = Palette.Green;
            selBg.raycastTarget = false;
            Picture(Rect("Tick", selected, new Vector2(0.15f, 0.15f), new Vector2(0.85f, 0.85f)), Check);
            var itemButton = item.gameObject.AddComponent<Button>();
            itemButton.targetGraphic = faceImg;
            itemButton.navigation = new Navigation { mode = Navigation.Mode.None };
            item.gameObject.AddComponent<ButtonFeedback>();

            var buy = Chunky(fill, "BuyButton", "Buy", Palette.Green, Palette.GreenDark, new Vector2(0.7f, 0.09f), new Vector2(380, 130), Vector2.zero, Icon("coin"), 50);
            var buyLabel = buy.transform.Find("Face/Label").GetComponent<TMP_Text>();
            var note = Label(fill, "Note", "", 34, Palette.Grey, TextAlignmentOptions.Center, 22);
            note.rectTransform.anchorMin = new Vector2(0.3f, 0.155f);
            note.rectTransform.anchorMax = new Vector2(1f, 0.2f);
            var done = Chunky(fill, "DoneButton", "Done", Palette.Coral, Palette.CoralDark, new Vector2(0.22f, 0.09f), new Vector2(300, 130), Vector2.zero, Check, 50);

            // Drag over the kid (left half) to spin them round; a turning-arrows hint sits at their feet.
            var spin = Rect("SpinArea", panel, new Vector2(0f, 0.12f), new Vector2(0.46f, 0.86f));
            Img(spin, new Color(1f, 1f, 1f, 0f));
            spin.gameObject.AddComponent<KidSpinner>().EditorInit(kid.transform);
            var spinHint = Rect("SpinHint", panel, new Vector2(0.23f, 0.08f), new Vector2(0.23f, 0.08f));
            spinHint.sizeDelta = Vector2.one * 110;
            OutlinedCard(spinHint, Palette.Blue, 55, 5);
            Picture(Rect("Icon", spinHint, new Vector2(0.18f, 0.18f), new Vector2(0.82f, 0.82f)), Icon("spin"));
            WithIdle(spinHint, IdleMotion.Kind.Pulse, 0.06f, 1f);

            var view = panel.gameObject.AddComponent<DressUpView>();
            view.EditorInit(tabs, tabSlots, gridRt, itemButton, buy, buyLabel, note, done, purse, kid);
            return view;
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
