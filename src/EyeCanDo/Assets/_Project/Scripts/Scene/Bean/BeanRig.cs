using System;
using System.Collections.Generic;
using Hindsight.EyeCanDo.Core.Avatar;
using UnityEngine;

namespace Hindsight.EyeCanDo.SceneObjects
{
    /// <summary>What a scout's legs are doing.</summary>
    public enum BeanMode
    {
        Idle,
        Walk,
        Sit,
    }

    /// <summary>
    /// A scout character: a big round head on a small torso, long arms with mitten hands and short
    /// legs, built by the editor's BeanBuilder. Every joint is built with no rotation, so each frame
    /// the pose is written from scratch.
    ///
    /// Looks: <see cref="ApplyLook"/> switches hats and fits (child objects named
    /// "&lt;Slot&gt;_&lt;id&gt;[_&lt;part&gt;]"), colours the skin, and picks the eye, mouth and
    /// brow cells of the face atlases (<see cref="FaceArt"/>).
    ///
    /// Animation is procedural: a base (idle, walking, sitting) with layers on top (waving, cheering,
    /// dancing, reaching, talking, head back, shivering, squashing, blinking). Then two world-space
    /// layers: a focus (the body leans and the head turns toward a point, as when looking into an eye)
    /// and hand goals (two-bone arm IK to a target, or up to the character's own eye). While talking,
    /// the mouth opens as wide as the voice is loud (<see cref="Voice"/>). Time is the rig's own clock,
    /// so editor captures can step it.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class BeanRig : MonoBehaviour
    {
        [Header("Joints (built with no rotation)")]
        [SerializeField] Transform hips;
        [SerializeField] Transform body;
        [SerializeField] Transform head;
        [SerializeField] Transform shoulderL;
        [SerializeField] Transform shoulderR;
        [SerializeField] Transform elbowL;
        [SerializeField] Transform elbowR;
        [SerializeField] Transform legL;
        [SerializeField] Transform legR;
        [SerializeField] Vector3 hipsRest;

        [Header("Anchors")]
        [SerializeField] Transform eyes;
        [SerializeField] Transform headCenter;
        [SerializeField] Transform handL;
        [SerializeField] Transform handR;
        [Tooltip("The bottom of the torso: what rests on a seat.")]
        [SerializeField] Transform seatPoint;

        [Header("Looks")]
        [SerializeField] Renderer eyesPatch;
        [SerializeField] Renderer mouthPatch;
        [SerializeField] Renderer browsPatch;
        [SerializeField] Renderer cheeksPatch;
        [SerializeField] Renderer[] skin = new Renderer[0];
        [Tooltip("Switchable parts named <Slot>_<id>[_<part>].")]
        [SerializeField] GameObject[] items = new GameObject[0];
        [Tooltip("Parts that keep turning (the propeller).")]
        [SerializeField] Transform[] spinners = new Transform[0];

        [Header("Moves")]
        [Tooltip("Visual scale (the doctor is bigger), so strides and reaches match the world.")]
        [SerializeField] float scale = 1f;
        [Tooltip("How far the body leans back when seated in the exam chair.")]
        [SerializeField] float seatRecline = 30f;
        [Tooltip("How far the head tips back for 'head back'.")]
        [SerializeField] float tiltDegrees = 34f;
        [Tooltip("How far the body leans at most toward a focus point.")]
        [SerializeField] float maxLean = 50f;
        [Tooltip("How loud the voice must be for each mouth shape (1, 2 and 3).")]
        [SerializeField] Vector3 mouthThresholds = new(0.12f, 0.4f, 0.7f);

        static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        static readonly int BaseMapStId = Shader.PropertyToID("_BaseMap_ST");
        static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

        MaterialPropertyBlock block;
        float clock;
        BeanMode mode;
        float moveSpeed;
        float walkPhase;

        float wWalk, wSit, wTalk, wWave, wCheer, wDance, wReach, wShiver, wFocus;
        float tilt, tiltTarget;
        float waveUntil = -1f, cheerStart = -10f, cheerUntil = -1f, danceUntil = -1f, reachUntil = -1f, shiverUntil = -1f;
        bool reachLeft;
        bool talking;
        float squash;
        bool eyesForcedShut;
        float nextBlink = -1f, blinkUntil = -1f;
        float mouthLevel;
        int mouthFrame, shownMouthFrame = -1;
        float mouthHeldUntil;
        int eyeStyle, mouthStyle, browStyle = -1;
        int shownEyeStyle = -1, shownMouthStyle = -1, shownBrowStyle = -2;
        Mood mood = Mood.Neutral, shownMood = Mood.Keep;
        bool shownShut;

        Vector3 focusPoint;
        Transform leanToward;
        bool focusOn;
        float focusLean = 1f;
        float focusStandoff;
        Func<Vector3> focusWander;

        struct HandGoal
        {
            public bool On;
            public bool ToFace;
            public bool Steady;
            public Transform Target;
            public Vector3 Offset;
            public float Weight;
        }

        readonly HandGoal[] hands = new HandGoal[2];

        const float CheerSeconds = 1.5f;

        /// <summary>How loud the voice is right now (0..1), read while talking. Without one, the mouth babbles.</summary>
        public Func<float> Voice { get; set; }

        public Transform Hips => hips;
        public Transform SeatPoint => seatPoint != null ? seatPoint : hips;
        public Transform Eyes => eyes;
        public Transform HeadCenter => headCenter;
        public Transform HandL => handL;
        public Transform HandR => handR;
        public BeanMode Mode => mode;
        public bool IsTalking => talking;
        public float Scale => scale;

        /// <summary>Which mouth shape is showing (0 closed .. 3 wide open).</summary>
        public int MouthFrame => mouthFrame;

        // ------------------------------------------------------------------ looks

        /// <summary>Dresses the scout: hat and fit objects, skin colour, eyes, mouth and brows.</summary>
        public void ApplyLook(AvatarLook look)
        {
            if (look == null) return;
            foreach (var item in items)
            {
                if (item == null) continue;
                var parts = item.name.Split('_');
                if (parts.Length < 2 || !Enum.TryParse<Slot>(parts[0], out var slot)) continue;
                item.SetActive(look.Get(slot) == parts[1]);
            }

            var skinColour = Parse(look.skin, Color.white);
            Tint(skin, skinColour, skinColour * 0.16f);
            if (cheeksPatch != null) SetCell(cheeksPatch, FaceArt.Cheeks);

            eyeStyle = FaceArt.EyeIndex(look.eyes);
            mouthStyle = FaceArt.MouthIndex(look.mouth);
            browStyle = FaceArt.BrowIndex(look.brows);
            ShowFace(force: true);
        }

        /// <summary>How the scout feels right now: the brows change to that mood. <see cref="Mood.Keep"/> changes nothing.</summary>
        public Mood Mood => mood;

        public void SetMood(Mood value)
        {
            if (value != Mood.Keep) mood = value;
        }

        static Color Parse(string hex, Color fallback) => ColorUtility.TryParseHtmlString(hex, out var c) ? c : fallback;

        void Tint(Renderer[] renderers, Color colour, Color? glow = null)
        {
            block ??= new MaterialPropertyBlock();
            foreach (var r in renderers)
            {
                if (r == null) continue;
                r.GetPropertyBlock(block);
                block.SetColor(BaseColorId, colour);
                if (glow.HasValue) block.SetColor(EmissionColorId, glow.Value);
                r.SetPropertyBlock(block);
            }
        }

        void SetCell(Renderer r, Rect cell)
        {
            if (r == null) return;
            block ??= new MaterialPropertyBlock();
            r.GetPropertyBlock(block);
            block.SetVector(BaseMapStId, FaceArt.ST(cell));
            r.SetPropertyBlock(block);
        }

        void ShowFace(bool force = false)
        {
            var shut = eyesForcedShut || clock < blinkUntil;
            if (force || shut != shownShut || eyeStyle != shownEyeStyle)
            {
                SetCell(eyesPatch, FaceArt.Eyes(eyeStyle, shut));
                shownShut = shut;
                shownEyeStyle = eyeStyle;
            }
            if (force || browStyle != shownBrowStyle || mood != shownMood)
            {
                if (browsPatch != null)
                {
                    browsPatch.enabled = browStyle >= 0;
                    if (browStyle >= 0) SetCell(browsPatch, FaceArt.Brows(browStyle, mood));
                }
                shownBrowStyle = browStyle;
                shownMood = mood;
            }
            if (force || mouthFrame != shownMouthFrame || mouthStyle != shownMouthStyle)
            {
                SetCell(mouthPatch, FaceArt.Mouth(mouthStyle, mouthFrame));
                shownMouthFrame = mouthFrame;
                shownMouthStyle = mouthStyle;
            }
        }

        // ------------------------------------------------------------------ moves

        public void SetMode(BeanMode value) => mode = value;

        /// <summary>Walking speed in metres per second, so the legs keep up with the ground.</summary>
        public void SetMoveSpeed(float metresPerSecond) => moveSpeed = metresPerSecond;

        public void SetTalking(bool value) => talking = value;

        /// <summary>Raises the right hand beside the head and waves it.</summary>
        public void Wave(float seconds = 2.2f) => waveUntil = clock + seconds;

        /// <summary>Both arms up and two happy hops.</summary>
        public void Cheer()
        {
            cheerStart = clock;
            cheerUntil = clock + CheerSeconds;
        }

        /// <summary>Bounces, sways and pumps the arms until the time is up.</summary>
        public void Dance(float seconds) => danceUntil = clock + seconds;

        /// <summary>Reaches one arm forward.</summary>
        public void Reach(float seconds, bool left = false)
        {
            reachUntil = clock + seconds;
            reachLeft = left;
        }

        public void Shiver(float seconds = 0.8f) => shiverUntil = clock + seconds;

        /// <summary>Head back so the face looks up (1), or upright again (0). Eased.</summary>
        public void SetTilt(float amount) => tiltTarget = Mathf.Clamp01(amount);

        /// <summary>Squashes the head (1) and back (0), as when squeezing the eyes shut.</summary>
        public void SetSquash(float amount) => squash = Mathf.Clamp01(amount);

        public void SetEyesShut(bool shut) => eyesForcedShut = shut;

        /// <summary>
        /// Leans toward a point and looks at it, bringing the head to <paramref name="standoff"/> metres
        /// from it (as close as the lean allows). An optional wander moves the point around a little
        /// (looking at different spots of an eye). With a lean target, the body leans toward that
        /// instead (a bottle held above the eye) while the head still looks at the point.
        /// <paramref name="lean"/> scales how far the body leans (0: stays upright and only looks).
        /// </summary>
        public void SetFocus(Vector3 point, float standoff, Func<Vector3> wander = null, Transform leanTarget = null, float lean = 1f)
        {
            focusLean = Mathf.Clamp01(lean);
            focusPoint = point;
            leanToward = leanTarget;
            focusStandoff = standoff;
            focusWander = wander;
            focusOn = true;
        }

        public void ClearFocus() => focusOn = false;

        /// <summary>
        /// Moves a hand to a target (plus a world offset), bending the arm to reach it. With
        /// <paramref name="steadyHead"/> the head and body stop swaying (the target is on the head:
        /// an instrument held to the eye).
        /// </summary>
        public void SetHandTarget(bool left, Transform target, Vector3 offset, bool steadyHead = false)
        {
            var i = left ? 0 : 1;
            hands[i].On = target != null;
            hands[i].ToFace = false;
            hands[i].Steady = steadyHead;
            hands[i].Target = target;
            hands[i].Offset = offset;
        }

        /// <summary>Holds a hand up in front of the character's own eye (an instrument to look through).</summary>
        public void SetHandToFace(bool left)
        {
            var i = left ? 0 : 1;
            hands[i].On = true;
            hands[i].ToFace = true;
            hands[i].Target = null;
        }

        public void ClearHand(bool left) => hands[left ? 0 : 1].On = false;

        public void ClearHands()
        {
            hands[0].On = false;
            hands[1].On = false;
        }

        /// <summary>Drops every layer at once and holds the plain pose: for teleports and cuts.</summary>
        public void ResetLayers()
        {
            waveUntil = cheerUntil = danceUntil = reachUntil = shiverUntil = -1f;
            tilt = tiltTarget = 0f;
            squash = 0f;
            eyesForcedShut = false;
            focusOn = false;
            ClearHands();
            Snap();
        }

        /// <summary>Jumps every blend to where it is heading and poses the character now.</summary>
        public void Snap()
        {
            wWalk = mode == BeanMode.Walk ? 1f : 0f;
            wSit = mode == BeanMode.Sit ? 1f : 0f;
            wTalk = talking ? 1f : 0f;
            wWave = clock < waveUntil ? 1f : 0f;
            wCheer = clock < cheerUntil ? 1f : 0f;
            wDance = clock < danceUntil ? 1f : 0f;
            wReach = clock < reachUntil ? 1f : 0f;
            wShiver = clock < shiverUntil ? 1f : 0f;
            wFocus = focusOn ? 1f : 0f;
            for (var i = 0; i < 2; i++) hands[i].Weight = hands[i].On ? 1f : 0f;
            tilt = tiltTarget;
            Evaluate(0f);
        }

        void LateUpdate() => Evaluate(Time.deltaTime);

        /// <summary>Advances the rig's clock by <paramref name="dt"/> and poses the character.</summary>
        public void Evaluate(float dt)
        {
            if (hips == null || body == null) return;
            clock += dt;
            var t = clock;

            wWalk = Mathf.MoveTowards(wWalk, mode == BeanMode.Walk ? 1f : 0f, dt * 6f);
            wSit = Mathf.MoveTowards(wSit, mode == BeanMode.Sit ? 1f : 0f, dt * 6f);
            wTalk = Mathf.MoveTowards(wTalk, talking ? 1f : 0f, dt * 6f);
            wWave = Mathf.MoveTowards(wWave, t < waveUntil ? 1f : 0f, dt * 5f);
            wCheer = Mathf.MoveTowards(wCheer, t < cheerUntil ? 1f : 0f, dt * 7f);
            wDance = Mathf.MoveTowards(wDance, t < danceUntil ? 1f : 0f, dt * 5f);
            wReach = Mathf.MoveTowards(wReach, t < reachUntil ? 1f : 0f, dt * 4f);
            wShiver = Mathf.MoveTowards(wShiver, t < shiverUntil ? 1f : 0f, dt * 8f);
            wFocus = Mathf.MoveTowards(wFocus, focusOn ? 1f : 0f, dt * 1.6f);
            for (var i = 0; i < 2; i++) hands[i].Weight = Mathf.MoveTowards(hands[i].Weight, hands[i].On ? 1f : 0f, dt * 2f);
            tilt = Mathf.MoveTowards(tilt, tiltTarget, dt * 2f);

            // While a hand holds something up to the face (the exam light), the head and body keep
            // still, so the hand doesn't wobble with them.
            var steady = 1f;
            for (var i = 0; i < 2; i++)
                if (hands[i].ToFace || hands[i].Steady) steady = Mathf.Min(steady, 1f - 0.9f * hands[i].Weight);

            // ---- Idle: breathing, a slow sway, a little look around, arms hanging by the sides.
            var breathe = Mathf.Sin(t * 2.2f) * steady;
            var hipsOffset = new Vector3(Mathf.Sin(t * 0.9f) * 0.006f * steady, 0f, 0f);
            var bodyEuler = new Vector3(-3f, 0f, Mathf.Sin(t * 0.9f) * 1.5f * steady);
            var bodyScale = new Vector3(1f + 0.012f * breathe, 1f + 0.01f * breathe, 1f + 0.012f * breathe);
            var headEuler = steady * new Vector3(Mathf.Sin(t * 0.7f) * 2f, Mathf.Sin(t * 0.43f) * 6f, Mathf.Sin(t * 0.55f) * 2.5f);
            var armSway = Mathf.Sin(t * 1.3f) * 2f;
            var shL = new Vector3(3f, 0f, -(14f + armSway));
            var shR = new Vector3(3f, 0f, 14f + armSway);
            var elL = new Vector3(-16f, 0f, 12f);
            var elR = new Vector3(-16f, 0f, -12f);
            var lgL = Vector3.zero;
            var lgR = Vector3.zero;

            // ---- Walking: legs swing, arms swing the other way, a bob and a waddle.
            var stride = 0.62f * scale; // metres per full step cycle
            walkPhase += dt * Mathf.PI * 2f * Mathf.Max(moveSpeed, 0.4f * scale) / stride * wWalk;
            if (wWalk > 0f)
            {
                var w = wWalk;
                float s = Mathf.Sin(walkPhase), c = Mathf.Cos(walkPhase);
                lgL = Vector3.Lerp(lgL, new Vector3(-34f * s, 0f, 0f), w);
                lgR = Vector3.Lerp(lgR, new Vector3(34f * s, 0f, 0f), w);
                shL = Vector3.Lerp(shL, new Vector3(40f * s, 0f, -8f), w);
                shR = Vector3.Lerp(shR, new Vector3(-40f * s, 0f, 8f), w);
                elL = Vector3.Lerp(elL, new Vector3(-28f, 0f, 0f), w);
                elR = Vector3.Lerp(elR, new Vector3(-28f, 0f, 0f), w);
                hipsOffset.y += w * 0.025f * (Mathf.Abs(c) - 0.4f);
                bodyEuler += w * new Vector3(6f, 6f * s, 4f * s);
                headEuler += w * new Vector3(-3f, -4f * s, -3f * s);
            }

            // ---- Seated in the exam chair: legs out along the leg rest, leaning back, hands in the lap.
            if (wSit > 0f)
            {
                var w = wSit;
                lgL = Vector3.Lerp(lgL, new Vector3(-84f, 0f, -3f), w);
                lgR = Vector3.Lerp(lgR, new Vector3(-84f, 0f, 3f), w);
                shL = Vector3.Lerp(shL, new Vector3(-28f, 0f, -14f), w);
                shR = Vector3.Lerp(shR, new Vector3(-28f, 0f, 14f), w);
                elL = Vector3.Lerp(elL, new Vector3(-50f, 0f, 0f), w);
                elR = Vector3.Lerp(elR, new Vector3(-50f, 0f, 0f), w);
                bodyEuler.x -= seatRecline * w;
            }

            // ---- Head back: the head tips back so the face looks up.
            var tiltEase = Mathf.SmoothStep(0f, 1f, tilt);
            headEuler.x -= tiltDegrees * tiltEase;
            bodyEuler.x -= 6f * tiltEase;

            // ---- Talking: the mouth follows the voice; the head nods along.
            UpdateMouth(dt, t);
            if (wTalk > 0f)
            {
                var lively = 0.5f + mouthLevel;
                var nod = wTalk * steady;
                headEuler.x += nod * lively * (Mathf.Sin(t * 7.3f) * 3f + Mathf.Sin(t * 2.9f) * 2f);
                headEuler.z += nod * Mathf.Sin(t * 1.7f) * 4f;
                bodyEuler.y += nod * Mathf.Sin(t * 1.1f) * 3f;
                shR = Vector3.Lerp(shR, shR + new Vector3(-22f, 0f, 6f), wTalk * Mathf.Clamp01(Mathf.Sin(t * 0.8f) * 1.5f));
            }

            // ---- Reaching forward with one arm.
            if (wReach > 0f)
            {
                var w = Mathf.SmoothStep(0f, 1f, wReach);
                if (reachLeft)
                {
                    shL = Vector3.Lerp(shL, new Vector3(-78f, 0f, -6f), w);
                    elL = Vector3.Lerp(elL, new Vector3(-12f, 0f, 0f), w);
                }
                else
                {
                    shR = Vector3.Lerp(shR, new Vector3(-78f, 0f, 6f), w);
                    elR = Vector3.Lerp(elR, new Vector3(-12f, 0f, 0f), w);
                }
                bodyEuler.x += 6f * w;
            }

            // ---- Waving: right hand up beside the head, forearm swinging.
            if (wWave > 0f)
            {
                var w = Mathf.SmoothStep(0f, 1f, wWave);
                shR = Vector3.Lerp(shR, new Vector3(-10f, 0f, 140f), w);
                elR = Vector3.Lerp(elR, new Vector3(0f, 0f, 15f + 30f * Mathf.Sin(t * 9f)), w);
                headEuler.z += w * Mathf.Sin(t * 4.5f) * 4f;
            }

            // ---- Cheering: both arms up and two hops.
            if (wCheer > 0f)
            {
                var w = Mathf.SmoothStep(0f, 1f, wCheer);
                var k = Mathf.Clamp01((t - cheerStart) / CheerSeconds);
                var hop = Mathf.Abs(Mathf.Sin(k * Mathf.PI * 2f));
                var wiggle = Mathf.Sin(t * 14f) * 10f;
                shL = Vector3.Lerp(shL, new Vector3(-10f, 0f, -155f + wiggle), w);
                shR = Vector3.Lerp(shR, new Vector3(-10f, 0f, 155f + wiggle), w);
                elL = Vector3.Lerp(elL, new Vector3(0f, 0f, 10f), w);
                elR = Vector3.Lerp(elR, new Vector3(0f, 0f, -10f), w);
                hipsOffset.y += w * 0.09f * hop;
                headEuler.x -= w * 10f * hop;
                var land = w * (1f - hop) * Mathf.Sin(k * Mathf.PI);
                bodyScale += new Vector3(0.05f, -0.07f, 0.05f) * land;
            }

            // ---- Dancing: bounce on the beat, sway, arms pumping in turn, little kicks, head bopping.
            if (wDance > 0f)
            {
                var w = Mathf.SmoothStep(0f, 1f, wDance);
                const float beat = 7f;
                var b = Mathf.Sin(t * beat);
                hipsOffset.y += w * 0.04f * Mathf.Abs(b);
                bodyEuler += w * new Vector3(0f, 18f * Mathf.Sin(t * beat * 0.5f), 8f * b);
                headEuler += w * new Vector3(6f * Mathf.Abs(b), 0f, -10f * b);
                shL = Vector3.Lerp(shL, new Vector3(-20f, 0f, -(100f + 45f * b)), w);
                shR = Vector3.Lerp(shR, new Vector3(-20f, 0f, 100f - 45f * b), w);
                elL = Vector3.Lerp(elL, new Vector3(-30f, 0f, 25f), w);
                elR = Vector3.Lerp(elR, new Vector3(-30f, 0f, -25f), w);
                lgL = Vector3.Lerp(lgL, new Vector3(-16f * Mathf.Max(0f, Mathf.Sin(t * beat * 0.5f)), 0f, -6f), w);
                lgR = Vector3.Lerp(lgR, new Vector3(-16f * Mathf.Max(0f, -Mathf.Sin(t * beat * 0.5f)), 0f, 6f), w);
            }

            // ---- Shivering (the cold drop).
            if (wShiver > 0f)
            {
                headEuler.z += wShiver * Mathf.Sin(t * 60f) * 5f;
                bodyEuler.y += wShiver * Mathf.Sin(t * 47f) * 3f;
                bodyScale.x += wShiver * Mathf.Sin(t * 53f) * 0.015f;
            }

            hips.localPosition = hipsRest + hipsOffset;
            body.localRotation = Quaternion.Euler(bodyEuler);
            body.localScale = bodyScale;
            if (head != null)
            {
                head.localRotation = Quaternion.Euler(headEuler);
                head.localScale = new Vector3(1f + 0.06f * squash, 1f - 0.1f * squash, 1f + 0.06f * squash);
            }
            Set(shoulderL, shL);
            Set(shoulderR, shR);
            Set(elbowL, elL);
            Set(elbowR, elR);
            Set(legL, lgL);
            Set(legR, lgR);

            // ---- World-space layers: lean and look toward a focus point, then hands to their goals.
            if (wFocus > 0f) ApplyFocus(Mathf.SmoothStep(0f, 1f, wFocus));
            for (var i = 0; i < 2; i++)
                if (hands[i].Weight > 0f) ApplyHand(i == 0, hands[i], Mathf.SmoothStep(0f, 1f, hands[i].Weight));

            var spin = (360f + 900f * Mathf.Max(wWalk, wDance)) * dt;
            foreach (var s in spinners)
                if (s != null) s.localRotation = Quaternion.AngleAxis(spin, Vector3.up) * s.localRotation;

            // ---- Blinking now and then (each character on its own rhythm).
            if (nextBlink < 0f) nextBlink = t + UnityEngine.Random.Range(0.8f, 4f);
            if (t >= nextBlink)
            {
                blinkUntil = t + 0.13f;
                nextBlink = t + UnityEngine.Random.Range(2.2f, 5f);
            }
            ShowFace();
        }

        static void Set(Transform joint, Vector3 euler)
        {
            if (joint != null) joint.localRotation = Quaternion.Euler(euler);
        }

        /// <summary>Leans the body so the head comes toward the focus point, then turns the head to look at it.</summary>
        void ApplyFocus(float w)
        {
            if (headCenter == null) return;
            var target = focusPoint + (focusWander != null ? focusWander() : Vector3.zero);
            var leanPoint = leanToward != null ? leanToward.position : target;
            var hipsPos = hips.position;
            var away = hipsPos - leanPoint;
            away.y = 0f;
            away = away.sqrMagnitude > 1e-6f ? away.normalized : -transform.forward;
            // Where the head should be: standoff away from the point, on the character's side of it.
            var goal = leanPoint + (away * 0.85f + Vector3.up * 0.25f).normalized * focusStandoff;
            var current = headCenter.position - hipsPos;
            var wanted = goal - hipsPos;
            var lean = Quaternion.FromToRotation(current, wanted);
            lean.ToAngleAxis(out var angle, out var axis);
            if (angle > 180f) angle -= 360f;
            angle = Mathf.Clamp(angle, -maxLean, maxLean) * focusLean;
            body.rotation = Quaternion.AngleAxis(angle * w, axis) * body.rotation;

            if (head == null || eyes == null) return;
            var look = Quaternion.FromToRotation(head.forward, target - eyes.position);
            look.ToAngleAxis(out var lookAngle, out var lookAxis);
            if (lookAngle > 180f) lookAngle -= 360f;
            lookAngle = Mathf.Clamp(lookAngle, -55f, 55f);
            head.rotation = Quaternion.AngleAxis(lookAngle * w, lookAxis) * head.rotation;
        }

        void ApplyHand(bool left, HandGoal goal, float w)
        {
            var shoulder = left ? shoulderL : shoulderR;
            var elbow = left ? elbowL : elbowR;
            var hand = left ? handL : handR;
            if (shoulder == null || elbow == null || hand == null) return;
            Vector3 target;
            if (goal.ToFace)
            {
                if (eyes == null || head == null) return;
                // Just below and in front of the eye on that side: the instrument in the mitten covers the eye.
                var side = left ? -1f : 1f;
                target = eyes.position + head.forward * (0.04f * scale) - head.up * (0.035f * scale) + head.right * (side * 0.035f * scale);
            }
            else
            {
                if (goal.Target == null) return;
                target = goal.Target.position + goal.Offset;
            }
            var outward = (left ? -body.right : body.right) - body.up * 0.6f - body.forward * 0.3f;
            SolveArm(shoulder, elbow, hand, target, outward, w);
        }

        /// <summary>Two-bone IK: bends the arm (elbow toward the pole) so the hand reaches the target, as far as it can.</summary>
        static void SolveArm(Transform upper, Transform lower, Transform end, Vector3 target, Vector3 pole, float w)
        {
            var s = upper.position;
            var e = lower.position;
            var h = end.position;
            float a = Vector3.Distance(s, e), b = Vector3.Distance(e, h);
            var toTarget = target - s;
            var d = Mathf.Clamp(toTarget.magnitude, 0.001f, a + b - 0.0005f);
            var n = toTarget.normalized;
            var cos = Mathf.Clamp((a * a + d * d - b * b) / (2f * a * d), -1f, 1f);
            var sin = Mathf.Sqrt(1f - cos * cos);
            var poleDir = Vector3.ProjectOnPlane(pole, n);
            poleDir = poleDir.sqrMagnitude > 1e-6f ? poleDir.normalized : Vector3.down;
            var elbowGoal = s + n * (a * cos) + poleDir * (a * sin);
            upper.rotation = Quaternion.Slerp(Quaternion.identity, Quaternion.FromToRotation(e - s, elbowGoal - s), w) * upper.rotation;
            var e1 = lower.position;
            lower.rotation = Quaternion.Slerp(Quaternion.identity, Quaternion.FromToRotation(end.position - e1, s + n * d - e1), w) * lower.rotation;
        }

        /// <summary>
        /// Picks the mouth shape from how loud the voice is. A shape is held for a moment so the
        /// mouth doesn't flicker between two of them.
        /// </summary>
        void UpdateMouth(float dt, float t)
        {
            float target = 0f;
            if (talking) target = Voice != null ? Mathf.Clamp01(Voice()) : Babble(t);
            var rate = target > mouthLevel ? 30f : 14f;
            mouthLevel = Mathf.Lerp(mouthLevel, target, 1f - Mathf.Exp(-rate * dt));
            if (!talking && wTalk <= 0f) mouthLevel = 0f;

            var frame = mouthLevel < mouthThresholds.x ? 0 : mouthLevel < mouthThresholds.y ? 1 : mouthLevel < mouthThresholds.z ? 2 : 3;
            if (frame != mouthFrame && t >= mouthHeldUntil)
            {
                mouthFrame = frame;
                mouthHeldUntil = t + 0.05f;
            }
        }

        /// <summary>Syllable-like opening for talking without a voice to follow: two sines at odd ratios.</summary>
        public static float Babble(float t) => Mathf.Clamp01(0.45f + 0.55f * Mathf.Sin(t * 17f) * Mathf.Sin(t * 5.3f + 1f));

#if UNITY_EDITOR
        public void EditorInit(Transform hipsT, Transform bodyT, Transform headT, Transform shL, Transform shR, Transform elL, Transform elR,
            Transform lgL, Transform lgR, Transform eyesT, Transform headCentre, Transform handLeft, Transform handRight, Transform seat,
            Renderer eyesR, Renderer mouthR, Renderer browsR, Renderer cheeksR, Renderer[] skinR, GameObject[] parts,
            Transform[] spin, float visualScale)
        {
            hips = hipsT;
            hipsRest = hipsT.localPosition;
            body = bodyT;
            head = headT;
            shoulderL = shL;
            shoulderR = shR;
            elbowL = elL;
            elbowR = elR;
            legL = lgL;
            legR = lgR;
            eyes = eyesT;
            headCenter = headCentre;
            handL = handLeft;
            handR = handRight;
            seatPoint = seat;
            eyesPatch = eyesR;
            mouthPatch = mouthR;
            browsPatch = browsR;
            cheeksPatch = cheeksR;
            skin = skinR;
            items = parts;
            spinners = spin;
            scale = visualScale;
        }
#endif
    }
}
