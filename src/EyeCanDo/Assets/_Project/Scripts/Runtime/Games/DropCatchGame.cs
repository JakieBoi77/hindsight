using System;
using System.Collections;
using System.Collections.Generic;
using EyeCanDo.App;
using EyeCanDo.Art;
using EyeCanDo.Characters;
using EyeCanDo.Core.Animation;
using EyeCanDo.Core.Games;
using EyeCanDo.UI;
using UnityEngine;
using UnityEngine.UI;

namespace EyeCanDo.Games
{
    /// <summary>
    /// Drop Catch. Your kid sits in the exam chair with their head back and eyes open, and you
    /// slide the chair left and right to catch drops from friendly drop bottles. Every catch is a
    /// little cold (a shiver) and that is fine. When the eyes fill up, tap to blink for a bonus.
    /// Golden drops are worth more. Missing a drop only splashes the floor.
    /// </summary>
    public sealed class DropCatchGame : GameScene
    {
        private const float RailY = 200f;
        private const float ChairFloorY = -540f;
        private const float ChairScale = 0.62f;
        private const float MinX = -740f;
        private const float MaxX = 740f;
        private const float MissY = -470f;

        /// <summary>Where the chair waits while the level menu and results are showing.</summary>
        private const float RestX = -720f;

        private RectTransform play;
        private RectTransform dropsLayer;
        private RectTransform bottlesLayer;
        private ExamChair chair;
        private CharacterRig kid;
        private Image eyeMeterFill;
        private RectTransform eyeMeter;
        private PressArea pad;
        private readonly List<Bottle> bottles = new List<Bottle>();
        private readonly List<FallingDrop> drops = new List<FallingDrop>();
        private DropCatchRound round;
        private float targetX;
        private bool saidCold;
        private bool saidGolden;
        private bool saidBlink;
        private int lastTick = -1;

        public DropCatchRound Round => round;

        public ExamChair Chair => chair;

        public PressArea Pad => pad;

        /// <summary>World x of the patient's eyes, for tests and the auto-player.</summary>
        public float EyesX => chair != null ? chair.Root.anchoredPosition.x + EyesOffsetX : 0f;

        /// <summary>X of the drop closest to landing, or null when none is falling.</summary>
        public float? LowestDropX
        {
            get
            {
                FallingDrop lowest = null;
                foreach (var drop in drops)
                {
                    if (lowest == null || drop.Rect.anchoredPosition.y < lowest.Rect.anchoredPosition.y)
                    {
                        lowest = drop;
                    }
                }

                return lowest?.Rect.anchoredPosition.x;
            }
        }

        private float EyesOffsetX { get; set; }

        protected override GameId Game => GameId.DropCatch;

        protected override string SceneName => SceneNames.DropCatch;

        protected override string Title => "Drop Catch";

        private sealed class Bottle
        {
            public RectTransform Rect;
            public float Direction;
            public float Speed;
            public float TurnTimer;
        }

        private sealed class FallingDrop
        {
            public RectTransform Rect;
            public bool Golden;
            public float StartY;
            public float Time;
            public float Seconds;
        }

        protected override void BuildScene()
        {
            var decor = Stage.BuildClinicRoom(false);
            Stage.Prop(decor, "Room/cabinet", new Vector2(860f, -250f), new Vector2(200f, 200f));
            Stage.Prop(decor, "Room/plant", new Vector2(-870f, -270f), new Vector2(140f, 216f));
            Stage.Prop(decor, "Room/eye_chart", new Vector2(-860f, 40f), new Vector2(150f, 208f));

            // The rail the bottles glide along.
            var rail = Ui.Rounded(decor, "Rail", UiTheme.Hex("#9AA7BD"), 10f);
            Ui.Place(rail.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, RailY + 120f), new Vector2(1700f, 20f));

            play = Stage.Layer("Play");
            pad = PressArea.AddTo(play.gameObject);
            pad.Dragged += delta => targetX = Mathf.Clamp(targetX + delta.x, MinX, MaxX);
            pad.Pressed += () => Stage.Ghost.Stop();
            pad.Tapped += OnTap;

            bottlesLayer = Stage.Layer("Bottles");
            chair = ExamChair.Create(Stage.Layer("Chair"), new Vector2(RestX, ChairFloorY), ChairScale);
            kid = CharacterRig.CreateKid(chair.Seat, GameServices.Progress.look, "seated", ChairScale);
            chair.SeatKid(kid);
            kid.SetTilt(1f);
            kid.SetEyes(Eyes.Up);
            dropsLayer = Stage.Layer("Drops");

            eyeMeter = Ui.Rect(chair.Root, "EyeMeter");
            Ui.Place(eyeMeter, new Vector2(1f, 1f), new Vector2(40f, 40f), new Vector2(110f, 110f));
            var meterFace = Ui.Disc(eyeMeter, "Meter", UiTheme.Instance.paper);
            Ui.Stretch((RectTransform)meterFace.parent);
            var meterBack = Ui.Art(meterFace, "Empty", "Icons/drop", new Vector2(70f, 70f));
            meterBack.color = new Color(1f, 1f, 1f, 0.3f);
            eyeMeterFill = Ui.Art(meterFace, "Fill", "Icons/drop", new Vector2(70f, 70f));
            eyeMeterFill.type = Image.Type.Filled;
            eyeMeterFill.fillMethod = Image.FillMethod.Vertical;
            eyeMeterFill.fillAmount = 0f;
        }

        protected override IEnumerator PlayRound(int level, Action<int, int> finished)
        {
            var settings = DropCatchSettings.ForLevel(level);
            round = new DropCatchRound(settings, Environment.TickCount);
            round.DropReleased += Release;
            targetX = 0f;
            EyesOffsetX = Stage.World.InverseTransformPoint(kid.EyesCentreWorld).x - chair.Root.anchoredPosition.x;
            CreateBottles(settings);
            ShowHud("Icons/clock");
            SetScore(0, false);
            SetTimer(Mathf.CeilToInt(settings.RoundSeconds).ToString());

            // The chair glides to the middle during the countdown, ready for the first drop.
            yield return GetReady(captions.catchIntro, captions.catchHowTo, MoveChair);
            Stage.Ghost.ShowDrag(chair.Root, chair.Root.position + Vector3.right * 380f * Stage.WorldUnit);

            while (!round.TimeUp || drops.Count > 0)
            {
                var dt = Time.deltaTime;
                round.Tick(dt);
                MoveChair(dt);
                MoveBottles(dt);
                MoveDrops(dt);
                UpdateTimer();
                yield return null;
            }

            foreach (var bottle in bottles)
            {
                StartCoroutine(Tween.Scale(bottle.Rect, Vector3.zero, 0.3f, Ease.InCubic));
            }

            kid.SetEyes(Eyes.Happy);
            kid.SetMouth(Mouth.Grin);
            // Glide back to the side so the results card does not cover your character.
            eyeMeter.gameObject.SetActive(false);
            targetX = RestX;
            for (var t = 0f; t < 0.8f; t += Time.deltaTime)
            {
                MoveChair(Time.deltaTime);
                yield return null;
            }

            yield return Stage.Captions.Say(captions.timeUp, false);
            finished(round.Stars, round.Score);
        }

        private void CreateBottles(DropCatchSettings settings)
        {
            foreach (var bottle in bottles)
            {
                Destroy(bottle.Rect.gameObject);
            }

            bottles.Clear();
            for (var i = 0; i < settings.Bottles; i++)
            {
                var image = Ui.Art(bottlesLayer, "Bottle", "Props/bottle_buddy", new Vector2(130f, 195f));
                var rect = image.rectTransform;
                rect.anchoredPosition = new Vector2(settings.Bottles == 1 ? -300f : (i == 0 ? -500f : 500f), RailY);
                bottles.Add(new Bottle
                {
                    Rect = rect,
                    Direction = i == 0 ? 1f : -1f,
                    Speed = settings.BottleSpeed * 1920f,
                    TurnTimer = UnityEngine.Random.Range(1.5f, 3.5f),
                });
                StartCoroutine(Tween.Pop(rect, 0.4f));
            }
        }

        private void MoveChair(float dt)
        {
            var position = chair.Root.anchoredPosition;
            var x = Mathf.Lerp(position.x, targetX, 1f - Mathf.Exp(-14f * dt));
            chair.Root.anchoredPosition = new Vector2(x, position.y);
            // Lean a little into the movement, for fun.
            chair.Root.localRotation = Quaternion.Euler(0f, 0f, Mathf.Clamp((position.x - x) * 0.4f, -6f, 6f));
        }

        private void MoveBottles(float dt)
        {
            foreach (var bottle in bottles)
            {
                bottle.TurnTimer -= dt;
                if (bottle.TurnTimer <= 0f)
                {
                    bottle.TurnTimer = UnityEngine.Random.Range(1.2f, 3.2f);
                    if (UnityEngine.Random.value < 0.5f)
                    {
                        bottle.Direction *= -1f;
                    }
                }

                var position = bottle.Rect.anchoredPosition;
                position.x += bottle.Direction * bottle.Speed * dt;
                if (position.x < MinX || position.x > MaxX)
                {
                    bottle.Direction *= -1f;
                    position.x = Mathf.Clamp(position.x, MinX, MaxX);
                }

                position.y = RailY + Mathf.Sin(Time.time * 4f + position.x * 0.01f) * 6f;
                bottle.Rect.anchoredPosition = position;
                bottle.Rect.localRotation = Quaternion.Euler(0f, 0f, -bottle.Direction * 6f);
            }
        }

        private void Release(int bottleIndex, bool golden)
        {
            if (bottleIndex >= bottles.Count)
            {
                return;
            }

            var bottle = bottles[bottleIndex];
            StartCoroutine(Tween.Bump(bottle.Rect, 0.12f, 0.2f));
            var image = Ui.Art(dropsLayer, golden ? "GoldenDrop" : "Drop", golden ? "Props/drop_gold" : "Props/drop", new Vector2(60f, 80f));
            image.rectTransform.anchoredPosition = bottle.Rect.anchoredPosition + new Vector2(0f, -110f);
            drops.Add(new FallingDrop
            {
                Rect = image.rectTransform,
                Golden = golden,
                StartY = image.rectTransform.anchoredPosition.y,
                Seconds = round.Settings.FallSeconds,
            });
            Sfx.Play(Sound.DropFall, golden ? 1.25f : 1f);
        }

        private void MoveDrops(float dt)
        {
            var eyes = Stage.World.InverseTransformPoint(kid.EyesCentreWorld);
            var catchRadius = round.Settings.CatchRadius;
            for (var i = drops.Count - 1; i >= 0; i--)
            {
                var drop = drops[i];
                drop.Time += dt;
                var fallen = drop.Time / drop.Seconds;
                var y = drop.StartY - (drop.StartY - eyes.y) * fallen * (0.6f + 0.4f * fallen);
                var position = drop.Rect.anchoredPosition;
                var wasAbove = position.y > eyes.y;
                position.y = y;
                drop.Rect.anchoredPosition = position;
                if (wasAbove && y <= eyes.y && Mathf.Abs(position.x - eyes.x) <= catchRadius)
                {
                    Caught(drop, eyes);
                    drops.RemoveAt(i);
                    continue;
                }

                if (y <= MissY)
                {
                    Missed(drop);
                    drops.RemoveAt(i);
                }
            }
        }

        private void Caught(FallingDrop drop, Vector3 eyes)
        {
            var points = round.Catch(drop.Golden);
            Destroy(drop.Rect.gameObject);
            SetScore(round.Score);
            Sfx.Play(drop.Golden ? Sound.Golden : Sound.Catch, 1f + Mathf.Min(round.Combo, 8) * 0.04f);
            var splash = Ui.Art(dropsLayer, "Splash", "Props/splash", new Vector2(220f, 132f));
            splash.rectTransform.anchoredPosition = new Vector2(eyes.x, eyes.y + 40f);
            StartCoroutine(FadeAndDestroy(splash, 0.4f));
            FloatText(dropsLayer, "+" + points, new Vector2(eyes.x + 120f, eyes.y + 120f), drop.Golden ? UiTheme.Instance.primary : UiTheme.Instance.drop);
            if (round.Combo > 0 && round.Combo % 5 == 0)
            {
                FloatText(dropsLayer, round.Combo + " in a row!", new Vector2(eyes.x, eyes.y + 260f), UiTheme.Instance.success, 56f);
                StartCoroutine(Confetti.Burst(dropsLayer, new Vector2(eyes.x, eyes.y + 120f), 10, 260f));
            }

            StartCoroutine(kid.Shiver());
            if (drop.Golden)
            {
                StartCoroutine(Confetti.Burst(dropsLayer, new Vector2(eyes.x, eyes.y + 80f), 10, 240f));
                if (!saidGolden)
                {
                    saidGolden = true;
                    Stage.Captions.Show(captions.catchGolden);
                }
            }
            else if (!saidCold)
            {
                saidCold = true;
                Stage.Captions.Show(captions.catchCold);
            }

            UpdateEyeMeter();
        }

        private void Missed(FallingDrop drop)
        {
            round.Miss();
            Sfx.Play(Sound.Plip, UnityEngine.Random.Range(0.9f, 1.1f));
            var splash = Ui.Art(dropsLayer, "FloorSplash", "Props/splash", new Vector2(140f, 84f));
            splash.rectTransform.anchoredPosition = drop.Rect.anchoredPosition + new Vector2(0f, 20f);
            Destroy(drop.Rect.gameObject);
            StartCoroutine(FadeAndDestroy(splash, 0.5f));
        }

        private void OnTap()
        {
            if (round == null || !round.EyesFull)
            {
                return;
            }

            var bonus = round.Blink();
            StartCoroutine(kid.Blink());
            if (bonus > 0)
            {
                Stage.Ghost.Stop();
                SetScore(round.Score);
                var eyes = Stage.World.InverseTransformPoint(kid.EyesCentreWorld);
                FloatText(dropsLayer, "+" + bonus, new Vector2(eyes.x, eyes.y + 160f), UiTheme.Instance.success);
                StartCoroutine(Confetti.Burst(dropsLayer, new Vector2(eyes.x, eyes.y + 60f), 12, 280f));
                Sfx.Play(Sound.Sparkle);
                Stage.Captions.Show(captions.catchBlinkDone);
                UpdateEyeMeter();
            }
        }

        private void UpdateEyeMeter()
        {
            var full = round.EyesFull;
            eyeMeterFill.fillAmount = Mathf.Clamp01(round.CatchesSinceBlink / (float)round.Settings.CatchesUntilBlink);
            if (full)
            {
                if (!eyeMeter.GetComponent<Bob>())
                {
                    eyeMeter.gameObject.AddComponent<Bob>().Height = 10f;
                    Sfx.Play(Sound.Pop);
                }

                if (!saidBlink)
                {
                    saidBlink = true;
                    Stage.Captions.Show(captions.catchBlink);
                    Stage.Ghost.ShowDoubleTap(kid.EyePoint(0));
                }
            }
            else
            {
                var bob = eyeMeter.GetComponent<Bob>();
                if (bob != null)
                {
                    Destroy(bob);
                    eyeMeter.anchoredPosition = new Vector2(40f, 40f);
                }
            }
        }

        private void UpdateTimer()
        {
            var seconds = Mathf.CeilToInt(round.TimeLeft);
            SetTimer(seconds.ToString());
            if (seconds <= 5 && seconds > 0 && seconds != lastTick)
            {
                lastTick = seconds;
                Sfx.Play(Sound.Tick);
            }
        }

        private static IEnumerator FadeAndDestroy(Image image, float seconds)
        {
            var rect = image.rectTransform;
            yield return Tween.Run(seconds, Ease.OutCubic, t =>
            {
                if (image != null)
                {
                    rect.localScale = Vector3.one * Mathf.Lerp(0.6f, 1.2f, t);
                    image.color = new Color(1f, 1f, 1f, 1f - t);
                }
            });
            if (image != null)
            {
                Destroy(image.gameObject);
            }
        }

        // Test hooks: the PlayMode auto-player steers the chair the way a drag would.
        public void SteerTo(float x)
        {
            targetX = Mathf.Clamp(x - EyesOffsetX, MinX, MaxX);
        }

        public void TapToBlink()
        {
            OnTap();
        }
    }
}
