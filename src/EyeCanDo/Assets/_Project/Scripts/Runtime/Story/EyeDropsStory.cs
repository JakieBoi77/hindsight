using System.Collections;
using System.Collections.Generic;
using EyeCanDo.App;
using EyeCanDo.Art;
using EyeCanDo.Characters;
using EyeCanDo.Content;
using EyeCanDo.Core.Animation;
using EyeCanDo.Core.Content;
using EyeCanDo.Core.Interaction;
using EyeCanDo.UI;
using UnityEngine;
using UnityEngine.UI;

namespace EyeCanDo.Story
{
    /// <summary>
    /// The Special Drops story, in the spirit of the first PoC's walkthrough. Dr. Iris explains
    /// the drops and how they feel, the child watches Mia get her drops, then it is their turn:
    /// their own kid hops into the chair and the child does each step (head back, look up and
    /// hold, blink, wipe). Lines come from <see cref="StoryScript"/>; each line's beat says what
    /// happens on stage. Characters stay below the caption bar and the eye close-up sits beside
    /// them, so nothing covers anyone.
    /// </summary>
    public sealed class EyeDropsStory : MonoBehaviour
    {
        public static readonly Vector2 ChairPosition = new Vector2(-430f, -530f);
        public static readonly Vector2 DoctorPosition = new Vector2(-110f, -530f);
        public static readonly Vector2 KidSpot = new Vector2(-800f, -530f);
        public static readonly Vector2 OffStage = new Vector2(-1300f, -530f);
        public static readonly Vector2 CloseUpPosition = new Vector2(590f, -30f);
        public static readonly Vector2 StarPosition = new Vector2(-430f, 250f);
        public static readonly Vector2 TissueHome = new Vector2(300f, -360f);
        private const float KidScale = 0.68f;
        private const float DoctorScale = 0.74f;
        private const float CloseUpSize = 440f;
        private const float FastForward = 8f;
        private const float HurryBeat = 4f;
        private const float LookUpSeconds = 2.5f;

        [SerializeField] private StoryScript script;
        [SerializeField] private CaptionBank captions;

        private Stage stage;
        private RectTransform props;
        private RectTransform front;
        private ExamChair chair;
        private CharacterRig doctor;
        private CharacterRig mia;
        private CharacterRig you;
        private EyeCloseUp closeUp;
        private Image handBottle;
        private Image bigBottle;
        private RectTransform star;
        private Image starRing;
        private Image tissue;
        private RectTransform headArea;
        private RectTransform endButtons;
        private readonly List<ChunkyButton> quizCards = new List<ChunkyButton>();
        private readonly IdleHint idleHint = new IdleHint();
        private CoinCounter coins;

        private bool fast;
        private bool beatRunning;
        private bool actionDone;
        private int quizAnswer = -1;
        private int blinks;
        private StillHold hold;

        public int StepIndex { get; private set; } = -1;

        public StoryStepKind CurrentKind => script != null && StepIndex >= 0 && StepIndex < script.steps.Count ? script.steps[StepIndex].kind : StoryStepKind.Hello;

        public StoryBeat CurrentAction { get; private set; } = StoryBeat.None;

        public bool Finished { get; private set; }

        public Stage Stage => stage;

        public IReadOnlyList<ChunkyButton> QuizCards => quizCards;

        public ChunkyButton GamesButton { get; private set; }

        public ChunkyButton MapButton { get; private set; }

        private void Awake()
        {
            if (script == null)
            {
                script = DefaultContent.CreateStory();
            }

            if (captions == null)
            {
                captions = DefaultContent.CreateCaptionBank();
            }

            stage = Stage.Create("EyeDropsStory", true);
            stage.Captions.YouLook = GameServices.Progress.look;
            stage.Captions.Spoke += OnSpoke;
            stage.Captions.Hurried += OnHurried;
            stage.Chrome.HomePressed += () => GameServices.GoToMap(StopCatalog.EyeDropsId);
            stage.Chrome.RestartPressed += () => GameServices.GoTo(SceneNames.EyeDropsStory);
            stage.Chrome.SkipTriggered += Skip;
            Build();
            // The purse sits under the Skip button, so it never covers it.
            coins = CoinCounter.Create(stage.Overlay, GameServices.Progress.coins);
            ((RectTransform)coins.transform).anchoredPosition += new Vector2(0f, -LevelChrome.ButtonSize - 24f);
        }

        private void OnDestroy()
        {
            Time.timeScale = 1f;
        }

        private void Build()
        {
            var decor = stage.BuildClinicRoom();
            star = Stage.Prop(decor, "Props/ceiling_star", StarPosition, new Vector2(150f, 150f)).rectTransform;
            starRing = Ui.Image(star, "HoldRing", Shapes.Ring, UiTheme.Instance.success);
            Ui.Stretch(starRing.rectTransform, -34f);
            starRing.type = Image.Type.Filled;
            starRing.fillMethod = Image.FillMethod.Radial360;
            starRing.fillOrigin = (int)Image.Origin360.Top;
            starRing.fillAmount = 0f;
            var starPress = PressArea.AddTo(star.gameObject);
            starPress.Pressed += PressStar;
            starPress.Released += ReleaseStar;

            chair = ExamChair.Create(stage.Layer("Furniture"), ChairPosition, KidScale);
            var chairPress = PressArea.AddTo(chair.Art.gameObject);
            chair.Art.raycastTarget = true;
            chairPress.Tapped += TapChair;

            var people = stage.Layer("People");
            mia = CharacterRig.CreateKid(people, Core.Avatar.AvatarLook.Mia, "stand", KidScale);
            mia.Root.anchoredPosition = OffStage;
            mia.gameObject.SetActive(false);
            you = CharacterRig.CreateKid(people, GameServices.Progress.look, "stand", KidScale);
            you.Root.anchoredPosition = OffStage;
            you.gameObject.SetActive(false);
            doctor = CharacterRig.CreateDoctor(people, "idle", DoctorScale);
            doctor.Root.anchoredPosition = DoctorPosition;

            props = stage.Layer("Props");
            handBottle = Ui.Art(props, "HandBottle", "Props/bottle", new Vector2(90f, 135f));
            handBottle.gameObject.SetActive(false);

            bigBottle = Ui.Art(props, "BigBottle", "Props/bottle", new Vector2(150f, 225f));
            bigBottle.rectTransform.anchoredPosition = new Vector2(270f, -110f);
            bigBottle.raycastTarget = true;
            var bottlePress = bigBottle.gameObject.AddComponent<PressArea>();
            bottlePress.Tapped += TapBottle;
            bigBottle.gameObject.SetActive(false);

            headArea = Ui.Rect(props, "HeadArea");
            headArea.sizeDelta = new Vector2(340f, 360f);
            var headPress = PressArea.AddTo(headArea.gameObject);
            headPress.Dragged += delta => DragHead(delta.y);
            headPress.Released += ReleaseHead;
            headPress.Pressed += () => stage.Ghost.Stop();
            headArea.gameObject.SetActive(false);

            front = stage.Layer("Front");
            closeUp = EyeCloseUp.Create(front, CloseUpSize);
            closeUp.Root.anchoredPosition = CloseUpPosition;
            var eyePress = PressArea.AddTo(closeUp.EyeArea.gameObject);
            eyePress.Tapped += TapEye;
            closeUp.Show(false);

            tissue = Ui.Art(front, "Tissue", "Props/tissue", new Vector2(160f, 145f));
            tissue.raycastTarget = true;
            tissue.rectTransform.anchoredPosition = TissueHome;
            var tissuePress = tissue.gameObject.AddComponent<PressArea>();
            tissuePress.Pressed += () => stage.Ghost.Stop();
            tissuePress.Dragged += delta => tissue.rectTransform.anchoredPosition += delta;
            tissuePress.Released += ReleaseTissue;
            tissue.gameObject.SetActive(false);
        }

        private IEnumerator Start()
        {
            GameServices.Progress.StartStory();
            GameServices.Save();
            yield return null;
            for (var i = 0; i < script.steps.Count; i++)
            {
                StepIndex = i;
                var step = script.steps[i];
                stage.Chrome.SkipAllowed = step.skippable;
                foreach (var line in step.lines)
                {
                    yield return PlayLine(step, line);
                }

                EndFastForward();
            }

            stage.Chrome.SkipAllowed = false;
            Finished = true;
            ShowEndButtons();
        }

        private IEnumerator PlayLine(StoryStep step, CaptionLine line)
        {
            if (line == null)
            {
                yield break;
            }

            if (line.IsAction)
            {
                stage.Captions.Show(line);
                CurrentAction = line.beat;
                yield return RunAction(line.beat, step);
                CurrentAction = StoryBeat.None;
                stage.Ghost.Stop();
                yield break;
            }

            var beat = StartCoroutine(TimedBeat(line.beat));
            yield return stage.Captions.Say(line);
            yield return beat;
        }

        /// <summary>Runs a beat; if a tap hurried its line, the beat plays fast until it ends.</summary>
        private IEnumerator TimedBeat(StoryBeat beat)
        {
            beatRunning = true;
            yield return Beat(beat);
            beatRunning = false;
            if (!fast)
            {
                Time.timeScale = 1f;
            }
        }

        // Plain beats: things that happen on stage while a line is shown ------------------------

        private IEnumerator Beat(StoryBeat beat)
        {
            switch (beat)
            {
                case StoryBeat.DoctorWave:
                    yield return doctor.Wave(1.6f);
                    break;
                case StoryBeat.DoctorPoint:
                    doctor.SetPose("point");
                    break;
                case StoryBeat.DoctorCheer:
                    yield return Cheer();
                    break;
                case StoryBeat.MiaWalksIn:
                    yield return WalkIn(mia);
                    break;
                case StoryBeat.MiaWaves:
                    mia.SetMouth(Mouth.Grin);
                    yield return mia.Wave(1.4f);
                    mia.SetMouth(Mouth.Smile);
                    break;
                case StoryBeat.ShowBigEye:
                    closeUp.SetDemo();
                    closeUp.ResetEye();
                    doctor.SetPose("point");
                    bigBottle.gameObject.SetActive(true);
                    StartCoroutine(Tween.Pop(bigBottle.rectTransform, 0.4f));
                    yield return closeUp.Appear();
                    break;
                case StoryBeat.HideBigEye:
                    yield return Tween.Wait(1.2f);
                    yield return HideBigEye();
                    break;
                case StoryBeat.MiaSits:
                    yield return mia.WalkTo(ChairPosition.x - 230f);
                    yield return chair.HopIn(mia);
                    break;
                case StoryBeat.MiaTipsHead:
                    yield return TipHead(mia);
                    break;
                case StoryBeat.DropOnMia:
                    yield return GiveDrop(mia);
                    break;
                case StoryBeat.MiaShivers:
                    StartCoroutine(SquintCloseUp());
                    yield return mia.Shiver();
                    break;
                case StoryBeat.MiaBlinks:
                    StartCoroutine(mia.Blink(2));
                    yield return closeUp.Blink(2);
                    break;
                case StoryBeat.MiaWipes:
                    yield return AutoWipe();
                    break;
                case StoryBeat.MiaWalksOut:
                    yield return WalkOut(mia);
                    break;
                case StoryBeat.YouWalkIn:
                    yield return WalkIn(you);
                    break;
                case StoryBeat.YouShiver:
                    StartCoroutine(SquintCloseUp());
                    yield return you.Shiver();
                    break;
                case StoryBeat.Sticker:
                    yield return GiveSticker();
                    break;
            }
        }

        private IEnumerator Cheer()
        {
            doctor.SetPose("cheer");
            doctor.SetEyes(Eyes.Happy);
            doctor.SetMouth(Mouth.Grin);
            Sfx.Play(Sound.Cheer);
            StartCoroutine(Confetti.Burst(props, DoctorPosition + new Vector2(0f, 620f), 14, 360f));
            yield return Tween.Wait(1.6f);
            doctor.SetPose("idle");
            doctor.SetEyes(Eyes.Open);
            doctor.SetMouth(Mouth.Smile);
        }

        private IEnumerator WalkIn(CharacterRig kid)
        {
            kid.gameObject.SetActive(true);
            kid.Root.anchoredPosition = OffStage;
            yield return kid.WalkTo(KidSpot.x);
        }

        private IEnumerator WalkOut(CharacterRig kid)
        {
            yield return HideDropTools();
            yield return Tween.Run(0.5f, Ease.InOutSine, t => kid.SetTilt(1f - t));
            kid.SetEyes(Eyes.Open);
            var seat = chair.SeatPosition;
            chair.Release(kid.Root.parent == chair.Seat ? stage.World.Find("People") : kid.Root.parent, seat);
            kid.Root.SetSiblingIndex(0);
            yield return kid.HopTo(KidSpot, 120f, 0.5f);
            StartCoroutine(kid.Wave(1f));
            yield return Tween.Wait(0.6f);
            yield return kid.WalkTo(OffStage.x);
            kid.gameObject.SetActive(false);
        }

        private IEnumerator TipHead(CharacterRig kid)
        {
            kid.SetEyes(Eyes.Up);
            StartCoroutine(Tween.Bump(star, 0.2f, 0.5f));
            Sfx.Play(Sound.Sparkle);
            yield return Tween.Run(0.8f, Ease.InOutSine, kid.SetTilt);
        }

        /// <summary>Dr. Iris holds the bottle over the kid's eye and a drop falls in.</summary>
        private IEnumerator GiveDrop(CharacterRig kid)
        {
            yield return ShowHandBottle(kid);
            closeUp.SetLook(kid.Look);
            closeUp.ResetEye();
            if (!closeUp.Root.gameObject.activeSelf)
            {
                yield return closeUp.Appear();
            }

            var drop = Ui.Art(props, "Drop", "Props/drop", new Vector2(36f, 48f));
            var from = (Vector2)stage.World.InverseTransformPoint(handBottle.rectTransform.position) + new Vector2(0f, -78f);
            var to = (Vector2)stage.World.InverseTransformPoint(kid.EyeWorld(1));
            StartCoroutine(Tween.Run(0.55f, Ease.InCubic, t =>
            {
                if (drop != null)
                {
                    drop.rectTransform.anchoredPosition = Vector2.Lerp(from, to, t);
                }
            }));
            yield return closeUp.DropIn();
            Destroy(drop.gameObject);
        }

        private IEnumerator ShowHandBottle(CharacterRig kid)
        {
            if (handBottle.gameObject.activeSelf)
            {
                yield break;
            }

            doctor.SetPose("hold");
            handBottle.gameObject.SetActive(true);
            handBottle.rectTransform.anchoredPosition = (Vector2)stage.World.InverseTransformPoint(doctor.HandWorld(0)) + new Vector2(0f, 20f);
            yield return Tween.Pop(handBottle.rectTransform, 0.3f);
        }

        private IEnumerator HideDropTools()
        {
            handBottle.gameObject.SetActive(false);
            doctor.SetPose("idle");
            if (closeUp.Root.gameObject.activeSelf)
            {
                yield return closeUp.Disappear();
            }
        }

        private IEnumerator HideBigEye()
        {
            if (bigBottle.gameObject.activeSelf)
            {
                yield return Tween.Scale(bigBottle.rectTransform, Vector3.zero, 0.2f, Ease.InCubic);
                bigBottle.gameObject.SetActive(false);
                bigBottle.rectTransform.localScale = Vector3.one;
            }

            doctor.SetPose("idle");
            if (closeUp.Root.gameObject.activeSelf)
            {
                yield return closeUp.Disappear();
            }
        }

        private IEnumerator SquintCloseUp()
        {
            yield return Tween.Run(0.15f, Ease.Linear, t => closeUp.SetLid(t * 0.6f));
            yield return Tween.Wait(0.3f);
            yield return Tween.Run(0.2f, Ease.Linear, t => closeUp.SetLid(0.6f * (1f - t)));
        }

        private IEnumerator AutoWipe()
        {
            yield return closeUp.ShowDrip();
            tissue.gameObject.SetActive(true);
            tissue.rectTransform.anchoredPosition = TissueHome;
            yield return Tween.Pop(tissue.rectTransform, 0.3f);
            var target = (Vector2)front.InverseTransformPoint(closeUp.DripPoint.position);
            yield return Tween.Move(tissue.rectTransform, target, 0.7f, Ease.InOutSine);
            yield return closeUp.WipeDrip();
            yield return Tween.Scale(tissue.rectTransform, Vector3.zero, 0.25f, Ease.InCubic);
            tissue.gameObject.SetActive(false);
            tissue.rectTransform.localScale = Vector3.one;
        }

        private IEnumerator GiveSticker()
        {
            yield return HideDropTools();
            var before = GameServices.Progress.coins;
            var firstTime = GameServices.Progress.FinishStory();
            GameServices.Save();
            var earned = GameServices.Progress.coins - before;
            var sticker = Ui.Art(front, "Sticker", "Props/sticker_brave_eyes", new Vector2(380f, 380f));
            sticker.rectTransform.anchoredPosition = CloseUpPosition + new Vector2(0f, 20f);
            Sfx.Play(Sound.Reward);
            StartCoroutine(Confetti.Burst(front, CloseUpPosition, 24, 520f));
            yield return Tween.Pop(sticker.rectTransform, 0.6f, 1f);
            sticker.gameObject.AddComponent<Bob>().Height = 10f;
            // Let the jingle ring out before the coins land, so the two never clash.
            yield return Tween.Wait(1.4f);
            yield return coins.Earn(sticker.rectTransform.position, earned, front);
            yield return Tween.Wait(0.3f);
            stage.Captions.Show(firstTime ? captions.storyCoins : captions.storyAgain);
        }

        // Actions: the child does these -------------------------------------------------------

        private IEnumerator RunAction(StoryBeat beat, StoryStep step)
        {
            actionDone = false;
            idleHint.Restart();
            switch (beat)
            {
                case StoryBeat.TapBottle:
                    stage.Ghost.ShowTap(bigBottle.rectTransform);
                    yield return WaitForAction(() => stage.Ghost.ShowTap(bigBottle.rectTransform));
                    yield return BottleMagic();
                    break;
                case StoryBeat.Quiz:
                    yield return Quiz(step);
                    break;
                case StoryBeat.YouHopUp:
                    stage.Ghost.ShowTap(chair.Art.rectTransform);
                    yield return WaitForAction(() => stage.Ghost.ShowTap(chair.Art.rectTransform));
                    yield return you.WalkTo(ChairPosition.x - 230f);
                    yield return chair.HopIn(you);
                    you.SetMouth(Mouth.Grin);
                    break;
                case StoryBeat.YouTipHead:
                    yield return TipHeadAction();
                    break;
                case StoryBeat.YouLookUp:
                    yield return LookUpAction();
                    break;
                case StoryBeat.YouBlink:
                    blinks = 0;
                    stage.Ghost.ShowTap(closeUp.EyeArea);
                    yield return WaitForAction(() => stage.Ghost.ShowTap(closeUp.EyeArea));
                    break;
                case StoryBeat.YouWipe:
                    yield return closeUp.ShowDrip();
                    tissue.gameObject.SetActive(true);
                    tissue.rectTransform.anchoredPosition = TissueHome;
                    yield return Tween.Pop(tissue.rectTransform, 0.3f);
                    stage.Ghost.ShowDrag(tissue.rectTransform, closeUp.DripPoint.position);
                    yield return WaitForAction(() => stage.Ghost.ShowDrag(tissue.rectTransform, closeUp.DripPoint.position));
                    tissue.gameObject.SetActive(false);
                    break;
            }
        }

        /// <summary>Waits until the action is done, repeating the demonstration when the child is idle.</summary>
        private IEnumerator WaitForAction(System.Action hint)
        {
            while (!actionDone)
            {
                if (fast)
                {
                    CompleteActionForChild();
                }

                if (idleHint.Tick(Time.deltaTime) && !fast)
                {
                    hint();
                }

                yield return null;
            }
        }

        private IEnumerator BottleMagic()
        {
            stage.Ghost.Stop();
            yield return Tween.Bump(bigBottle.rectTransform, 0.2f, 0.25f);
            var sparkle = Ui.Art(front, "Sparkle", "Icons/sparkle", new Vector2(90f, 90f));
            var from = bigBottle.rectTransform.anchoredPosition;
            sparkle.color = UiTheme.Instance.primary;
            Sfx.Play(Sound.Sparkle);
            yield return Tween.Run(0.5f, Ease.InOutSine, t =>
            {
                sparkle.rectTransform.anchoredPosition = Vector2.Lerp(from, CloseUpPosition, t) + Vector2.up * (Mathf.Sin(t * Mathf.PI) * 140f);
                sparkle.rectTransform.localRotation = Quaternion.Euler(0f, 0f, t * 180f);
            });
            Destroy(sparkle.gameObject);
            yield return closeUp.Dilate();
        }

        private IEnumerator Quiz(StoryStep step)
        {
            yield return HideBigEye();
            doctor.SetPose("point");
            var drops = step.kind == StoryStepKind.QuizDrops;
            var options = drops
                ? new[] { ("Icons/eye_rainbow", "Rainbow eye"), ("Icons/eye_big_pupil", "Big pupil"), ("Icons/sleepy", "Sleepy") }
                : new[] { ("Icons/eyes_shut", "Eyes shut"), ("Icons/wiggle", "Wiggle around"), ("Icons/head_back", "Head back, eyes open") };
            var correct = drops ? 1 : 2;
            var theme = UiTheme.Instance;
            quizCards.Clear();
            for (var i = 0; i < options.Length; i++)
            {
                var index = i;
                var card = ChunkyButton.Create(props, "Answer" + i, null, null, theme.paper, new Vector2(240f, 300f), 36f);
                Ui.Place((RectTransform)card.transform, new Vector2(0.5f, 0.5f), new Vector2(330f + i * 255f, -40f), new Vector2(240f, 300f));
                var face = (RectTransform)card.transform.Find("Face");
                var picture = Ui.Art(face, "Picture", options[i].Item1, new Vector2(190f, 190f));
                picture.rectTransform.anchoredPosition = new Vector2(0f, 36f);
                var label = Ui.Text(face, options[i].Item2, 32f, theme.ink);
                Ui.Place(label.rectTransform, new Vector2(0.5f, 0f), new Vector2(0f, 46f), new Vector2(220f, 76f));
                card.Clicked += () => quizAnswer = index;
                quizCards.Add(card);
                StartCoroutine(Tween.Pop(card.transform, 0.35f + i * 0.08f));
            }

            quizAnswer = -1;
            while (true)
            {
                if (fast)
                {
                    quizAnswer = correct;
                }

                if (quizAnswer >= 0)
                {
                    var answer = quizAnswer;
                    quizAnswer = -1;
                    if (answer == correct)
                    {
                        Sfx.Play(Sound.Correct);
                        var check = Ui.Disc(quizCards[answer].transform, "Check", theme.success);
                        Ui.Place((RectTransform)check.parent, new Vector2(1f, 1f), new Vector2(-10f, -10f), new Vector2(80f, 80f));
                        var tick = Ui.Art(check, "Tick", "Icons/check", new Vector2(56f, 56f));
                        tick.color = Color.white;
                        yield return Tween.Bump(quizCards[answer].transform, 0.15f, 0.3f);
                        break;
                    }

                    Sfx.Play(Sound.TryAgain);
                    StartCoroutine(Tween.Wiggle(quizCards[answer].transform));
                    stage.Captions.Show(step.hint);
                }

                yield return null;
            }

            yield return Tween.Wait(0.5f);
            foreach (var card in quizCards)
            {
                StartCoroutine(Tween.Scale(card.transform, Vector3.zero, 0.2f, Ease.InCubic));
            }

            yield return Tween.Wait(0.25f);
            foreach (var card in quizCards)
            {
                Destroy(card.gameObject);
            }

            quizCards.Clear();
            doctor.SetPose("idle");
        }

        private IEnumerator TipHeadAction()
        {
            you.SetTilt(0f);
            PlaceHeadArea();
            headArea.gameObject.SetActive(true);
            var arrow = Ui.Art(props, "Arrow", "Icons/arrow_up", new Vector2(90f, 90f));
            arrow.color = UiTheme.Instance.success;
            arrow.rectTransform.anchoredPosition = headArea.anchoredPosition + new Vector2(-200f, 30f);
            arrow.gameObject.AddComponent<Bob>().Height = 14f;
            System.Action demo = () => stage.Ghost.ShowDrag(headArea, headArea.position + Vector3.up * 220f * stage.WorldUnit);
            demo();
            yield return WaitForAction(demo);
            Destroy(arrow.gameObject);
            headArea.gameObject.SetActive(false);
            you.SetEyes(Eyes.Up);
            Sfx.Play(Sound.Correct);
        }

        private void PlaceHeadArea()
        {
            headArea.anchoredPosition = (Vector2)stage.World.InverseTransformPoint(you.Head.position) + new Vector2(0f, 60f);
        }

        private IEnumerator LookUpAction()
        {
            hold = new StillHold(LookUpSeconds);
            you.SetEyes(Eyes.Up);
            starRing.fillAmount = 0f;
            stage.Ghost.ShowHold(star, 1.5f);
            var bottleShown = false;
            var saidPaused = false;
            while (!hold.Done)
            {
                if (fast)
                {
                    hold.Press();
                    hold.Tick(LookUpSeconds);
                }

                if (hold.Tick(Time.deltaTime))
                {
                    break;
                }

                starRing.fillAmount = hold.Progress;
                if (hold.IsHolding && !bottleShown)
                {
                    bottleShown = true;
                    StartCoroutine(ShowHandBottle(you));
                }

                if (!hold.IsHolding && hold.Pauses > 0 && !saidPaused)
                {
                    saidPaused = true;
                    stage.Captions.Show(captions.storyPaused);
                }

                if (hold.IsHolding)
                {
                    saidPaused = false;
                    idleHint.Reset();
                }
                else if (idleHint.Tick(Time.deltaTime) && !fast)
                {
                    stage.Ghost.ShowHold(star, 1.5f);
                }

                yield return null;
            }

            starRing.fillAmount = 1f;
            stage.Ghost.Stop();
            Sfx.Play(Sound.Correct);
            yield return GiveDrop(you);
            starRing.fillAmount = 0f;
        }

        private void CompleteActionForChild()
        {
            switch (CurrentAction)
            {
                case StoryBeat.YouTipHead:
                    you.SetTilt(1f);
                    actionDone = true;
                    break;
                case StoryBeat.YouBlink:
                    blinks = 3;
                    actionDone = true;
                    break;
                case StoryBeat.YouWipe:
                    StartCoroutine(closeUp.WipeDrip());
                    actionDone = true;
                    break;
                default:
                    actionDone = true;
                    break;
            }
        }

        // Touch handlers (also used by the PlayMode auto-player) ------------------------------

        public void TapBottle()
        {
            if (CurrentAction == StoryBeat.TapBottle)
            {
                actionDone = true;
            }
        }

        public void TapChair()
        {
            if (CurrentAction == StoryBeat.YouHopUp)
            {
                actionDone = true;
            }
        }

        public void AnswerQuiz(int index)
        {
            if (CurrentAction == StoryBeat.Quiz)
            {
                quizAnswer = index;
            }
        }

        public void DragHead(float deltaY)
        {
            if (CurrentAction != StoryBeat.YouTipHead || actionDone)
            {
                return;
            }

            idleHint.Reset();
            you.SetTilt(you.Tilt + deltaY / 200f);
            if (you.Tilt >= 0.99f)
            {
                actionDone = true;
            }
        }

        private void ReleaseHead()
        {
            if (CurrentAction == StoryBeat.YouTipHead && !actionDone && you.Tilt >= 0.6f)
            {
                StartCoroutine(Tween.Run(0.2f, Ease.OutCubic, t => you.SetTilt(Mathf.Lerp(you.Tilt, 1f, t))));
                actionDone = true;
            }
        }

        public void PressStar()
        {
            if (CurrentAction == StoryBeat.YouLookUp && hold != null)
            {
                stage.Ghost.Stop();
                hold.Press();
            }
        }

        public void ReleaseStar()
        {
            hold?.Release();
        }

        public void TapEye()
        {
            if (CurrentAction != StoryBeat.YouBlink || actionDone)
            {
                return;
            }

            stage.Ghost.Stop();
            idleHint.Reset();
            blinks++;
            StartCoroutine(closeUp.Blink());
            StartCoroutine(you.Blink());
            if (blinks >= 3)
            {
                actionDone = true;
            }
        }

        private void ReleaseTissue()
        {
            if (CurrentAction != StoryBeat.YouWipe || actionDone)
            {
                return;
            }

            var target = (Vector2)front.InverseTransformPoint(closeUp.DripPoint.position);
            if (Vector2.Distance(tissue.rectTransform.anchoredPosition, target) < 150f)
            {
                WipeNow();
            }
            else
            {
                StartCoroutine(Tween.Move(tissue.rectTransform, TissueHome, 0.3f));
            }
        }

        public void WipeNow()
        {
            if (CurrentAction != StoryBeat.YouWipe || actionDone)
            {
                return;
            }

            StartCoroutine(closeUp.WipeDrip());
            actionDone = true;
        }

        // Skip, talking and the end ----------------------------------------------------------

        /// <summary>
        /// Hold-to-skip fast-forwards the rest of the current step: lines continue on their own
        /// and actions are done for the child. It is never allowed on the Sensation step.
        /// </summary>
        private void Skip()
        {
            var step = StepIndex >= 0 && StepIndex < script.steps.Count ? script.steps[StepIndex] : null;
            if (step == null || !step.skippable || fast)
            {
                return;
            }

            fast = true;
            stage.Ghost.Stop();
            stage.Captions.AutoContinue = true;
            Time.timeScale = FastForward;
        }

        public void SkipStep()
        {
            Skip();
        }

        private void EndFastForward()
        {
            if (!fast)
            {
                return;
            }

            fast = false;
            stage.Captions.AutoContinue = false;
            Time.timeScale = 1f;
        }

        private void OnSpoke(Speaker speaker, float seconds)
        {
            var rig = speaker == Speaker.Doctor ? doctor : speaker == Speaker.Mia ? mia : speaker == Speaker.You ? you : null;
            if (rig != null && rig.gameObject.activeInHierarchy)
            {
                rig.Talk(seconds);
            }
        }

        /// <summary>A tap cut the line short: everyone stops talking and the animation catches up.</summary>
        private void OnHurried()
        {
            foreach (var rig in new[] { doctor, mia, you })
            {
                rig.StopTalking();
            }

            if (beatRunning && !fast)
            {
                Time.timeScale = HurryBeat;
            }
        }

        private void ShowEndButtons()
        {
            var theme = UiTheme.Instance;
            endButtons = Ui.Rect(front, "EndButtons");
            Ui.Stretch(endButtons);
            GamesButton = ChunkyButton.Create(endButtons, "Games", "Play a game!", ArtLibrary.Get("Icons/play"), theme.primary, new Vector2(460f, 130f), 60f);
            Ui.Place((RectTransform)GamesButton.transform, new Vector2(0.5f, 0.5f), new Vector2(400f, -420f), new Vector2(460f, 130f));
            GamesButton.gameObject.AddComponent<Bob>().Height = 8f;
            GamesButton.Clicked += () => GameServices.GoToMap(StopCatalog.EyeDropsId);
            MapButton = ChunkyButton.Create(endButtons, "Map", "Map", ArtLibrary.Get("Icons/home"), theme.paper, new Vector2(240f, 130f), 60f);
            Ui.Place((RectTransform)MapButton.transform, new Vector2(0.5f, 0.5f), new Vector2(780f, -420f), new Vector2(240f, 130f));
            MapButton.Clicked += () => GameServices.GoToMap();
            StartCoroutine(Tween.Pop(GamesButton.transform, 0.4f));
            StartCoroutine(Tween.Pop(MapButton.transform, 0.4f));
        }
    }
}
