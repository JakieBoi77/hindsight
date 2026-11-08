using System;
using System.Collections;
using Hindsight.Ophthalmology.Core.Avatar;
using UnityEngine;

namespace Hindsight.Ophthalmology.SceneObjects
{
    public enum DoctorPose
    {
        None,
        Idle,
        Wave,
        Interact,
        Walk,
        Cheer,
        UseItem,
    }

    public enum DoctorProp
    {
        None,
        EyeDrops,
        Light,
    }

    /// <summary>
    /// Dr. Iris: a big bean (<see cref="BeanRig"/>) in a white coat and head mirror. Steps ask for
    /// poses, props and moves; this maps them onto the rig's animation layers.
    /// </summary>
    [RequireComponent(typeof(BeanRig))]
    [DefaultExecutionOrder(50)] // after the rig has posed her, so the held light follows her head this frame
    public sealed class DoctorActor : MonoBehaviour
    {
        [SerializeField] BeanRig rig;
        [Tooltip("Held in the left hand.")]
        [SerializeField] GameObject propEyeDrops;
        [Tooltip("Held in the right hand.")]
        [SerializeField] GameObject propLight;

        [Header("Movement")]
        [SerializeField] float walkSpeed = 2.6f;
        [SerializeField] float turnSpeed = 720f;

        [Header("Holding the bottle")]
        [Tooltip("How far she turns her shoulder toward the bottle (degrees) when it is close and when it is far, so her arm reaches it without leaning over the child.")]
        [SerializeField] Vector2 holdTurn = new(40f, 85f);
        [Tooltip("How much she leans toward the bottle (0 upright, 1 a full lean).")]
        [SerializeField] float holdLean = 0.25f;

        DoctorProp prop;
        Quaternion? turnGoal;

        // The exam light: held in the hand, or up at her right eye (fixed to her head) while she looks.
        Transform lightAtEye, lightGrip;
        bool lightUp;
        float lightBlend;

        public BeanRig Rig => rig != null ? rig : rig = GetComponent<BeanRig>();

        public bool IsTalking => Rig.IsTalking;

        /// <summary>True between <see cref="HoldBottle"/> and <see cref="FinishTask"/> (or a teleport).</summary>
        public bool IsHoldingBottle { get; private set; }

        void Awake()
        {
            Rig.ApplyLook(AvatarLook.Doctor);
            Rig.SetMood(Mood.Happy);
            SetProp(DoctorProp.None);
        }

        void LateUpdate() => PoseLight(Time.deltaTime);

        /// <summary>
        /// Moves the exam light between her hand and her eye. Up at the eye it is fixed to her head
        /// (lens forward, handle down, in front of her right eye), so it holds perfectly still while
        /// her hand grips the handle. Called after the rig poses her.
        /// </summary>
        public void PoseLight(float dt)
        {
            if (propLight == null || lightAtEye == null) return;
            lightBlend = Mathf.MoveTowards(lightBlend, lightUp ? 1f : 0f, dt * 2f);
            if (lightBlend <= 0f) return;
            var light = propLight.transform;
            var parent = light.parent;
            var handPos = parent.TransformPoint(lightHandPos);
            var handRot = parent.rotation * lightHandRot;
            var k = Mathf.SmoothStep(0f, 1f, lightBlend);
            light.SetPositionAndRotation(Vector3.Lerp(handPos, lightAtEye.position, k), Quaternion.Slerp(handRot, lightAtEye.rotation, k));
        }

        Vector3 lightHandPos;
        Quaternion lightHandRot;

        /// <summary>Where the light sits at her eye, and where her hand holds it then, as points fixed to her head.</summary>
        void PlaceLightMarks()
        {
            if (propLight == null || Rig.Eyes == null) return;
            var head = Rig.Eyes.parent;
            if (lightAtEye == null)
            {
                lightHandPos = propLight.transform.localPosition;
                lightHandRot = propLight.transform.localRotation;
                lightAtEye = new GameObject("LightAtEye").transform;
                lightAtEye.SetParent(head, false);
                lightGrip = new GameObject("LightGrip").transform;
                lightGrip.SetParent(head, false);
            }
            // Her right eye (the eyes node is between the two), just in front of it. The scope's lens
            // faces its local -Z and sits a little above its middle; its handle hangs below (-Y).
            var eye = Rig.Eyes.position + head.right * (FaceArt.EyeCellX * FaceArt.EyesHeight * 0.5f * Rig.Scale);
            var pos = eye + head.forward * 0.06f - head.up * 0.016f;
            lightAtEye.SetPositionAndRotation(pos, Quaternion.LookRotation(-head.forward, head.up));
            // The middle of the handle (the scope is built at twice its model size).
            lightGrip.position = pos - head.up * 0.17f - head.forward * 0.04f;
        }

        /// <summary>Turns in place toward a goal set by holding the bottle (and back after).</summary>
        void Update()
        {
            if (!turnGoal.HasValue) return;
            transform.rotation = Quaternion.RotateTowards(transform.rotation, turnGoal.Value, turnSpeed * 0.35f * Time.deltaTime);
            if (Quaternion.Angle(transform.rotation, turnGoal.Value) < 0.1f) turnGoal = null;
        }

        /// <summary>Finishes a turn at once (editor captures, where Update doesn't run).</summary>
        public void CompleteTurn()
        {
            if (turnGoal.HasValue) transform.rotation = turnGoal.Value;
            turnGoal = null;
        }

        /// <summary>Her brows show how she feels (<see cref="Mood.Keep"/> changes nothing).</summary>
        public void SetMood(Mood mood) => Rig.SetMood(mood);

        /// <summary>Plays a pose. One-shot poses (wave, cheer, reach) return to standing by themselves.</summary>
        public void Play(DoctorPose pose)
        {
            switch (pose)
            {
                case DoctorPose.Idle: Rig.SetMode(BeanMode.Idle); break;
                case DoctorPose.Walk: Rig.SetMode(BeanMode.Walk); break;
                case DoctorPose.Wave:
                    Rig.Wave(2.2f);
                    Rig.SetMood(Mood.Happy);
                    break;
                case DoctorPose.Cheer:
                    Rig.Cheer();
                    Rig.SetMood(Mood.Happy);
                    break;
                case DoctorPose.Interact:
                case DoctorPose.UseItem:
                    // Reach with the hand holding something (the drops are in the left).
                    Rig.Reach(1.6f, left: prop == DoctorProp.EyeDrops);
                    break;
            }
        }

        /// <summary>Mouth moves and the body nods while true.</summary>
        public void SetTalking(bool value) => Rig.SetTalking(value);

        /// <summary>Where the mouth gets its loudness from while talking (the narration).</summary>
        public void SetVoice(Func<float> level) => Rig.Voice = level;

        public void SetProp(DoctorProp value)
        {
            prop = value;
            if (propEyeDrops != null) propEyeDrops.SetActive(value == DoctorProp.EyeDrops);
            if (propLight != null) propLight.SetActive(value == DoctorProp.Light);
            if (value != DoctorProp.Light) LowerLight(snap: true);
        }

        /// <summary>
        /// Looks into an eye with the light: she leans in, raises the light to her own right eye (lens
        /// toward the child) and holds it there, steady, peering at <paramref name="eye"/> from
        /// <paramref name="standoff"/> metres. Ends with <see cref="FinishTask"/>.
        /// </summary>
        public void Examine(Vector3 eye, float standoff)
        {
            Rig.ClearHands();
            SetProp(DoctorProp.Light);
            Rig.SetMood(Mood.Focused);
            Rig.SetFocus(eye, standoff);
            PlaceLightMarks();
            lightUp = true;
            if (lightGrip != null) Rig.SetHandTarget(false, lightGrip, Vector3.zero, steadyHead: true);
            else Rig.SetHandToFace(left: false);
        }

        /// <summary>Takes the light back down into her hand (at once with <paramref name="snap"/>).</summary>
        void LowerLight(bool snap)
        {
            lightUp = false;
            if (!snap || propLight == null || lightAtEye == null) return;
            lightBlend = 0f;
            propLight.transform.localPosition = lightHandPos;
            propLight.transform.localRotation = lightHandRot;
        }

        /// <summary>
        /// Holds the eye-drop bottle over an eye from where she stands: she turns a shoulder toward
        /// the bottle and reaches over with that arm, staying (nearly) upright so she never leans into
        /// the child, and looks at the eye. The bottle is the scene's, so her own prop is put away.
        /// Ends with <see cref="FinishTask"/>.
        /// </summary>
        public void HoldBottle(Transform bottle, Vector3 eye)
        {
            if (bottle == null) return;
            if (!IsHoldingBottle) restRotation = transform.rotation;
            IsHoldingBottle = true;
            SetProp(DoctorProp.None);
            Rig.SetMood(Mood.Focused);
            var toBottle = bottle.position - transform.position;
            toBottle.y = 0f;
            if (toBottle.sqrMagnitude < 1e-6f) toBottle = transform.forward;
            // The hand on the bottle's side (the right one when it is straight ahead).
            var left = Vector3.Dot(restRotation * Vector3.right, toBottle) < -0.05f;
            var size = Rig.Scale / 1.7f;
            var turn = Mathf.Lerp(holdTurn.x, holdTurn.y, Mathf.InverseLerp(0.45f * size, 0.75f * size, toBottle.magnitude));
            turnGoal = Quaternion.LookRotation(toBottle) * Quaternion.Euler(0f, left ? turn : -turn, 0f);
            Rig.SetFocus(eye, 0.4f * Rig.Scale / 1.7f, leanTarget: bottle, lean: holdLean);
            // Grip the bottle from her side of it, a little above its middle.
            Rig.SetHandTarget(left, bottle, -toBottle.normalized * 0.045f + Vector3.up * 0.05f);
        }

        Quaternion restRotation;

        /// <summary>Stands back up from an exam or a drop: lets go of the focus and the hand goals (and turns back after a drop).</summary>
        public void FinishTask()
        {
            if (IsHoldingBottle) turnGoal = restRotation;
            IsHoldingBottle = false;
            LowerLight(snap: false);
            Rig.ClearFocus();
            Rig.ClearHands();
            Rig.SetMood(Mood.Neutral);
        }

        /// <summary>Walks to <paramref name="target"/> and turns to match its rotation.</summary>
        public IEnumerator MoveTo(Transform target)
        {
            if (target == null) yield break;
            turnGoal = null;
            var flatTarget = new Vector3(target.position.x, transform.position.y, target.position.z);
            if (Vector3.Distance(transform.position, flatTarget) > 0.05f)
            {
                Rig.SetMoveSpeed(walkSpeed);
                Rig.SetMode(BeanMode.Walk);
                while (Vector3.Distance(transform.position, flatTarget) > 0.02f)
                {
                    var dir = flatTarget - transform.position;
                    if (dir.sqrMagnitude > 0.0001f)
                        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(dir), turnSpeed * Time.deltaTime);
                    transform.position = Vector3.MoveTowards(transform.position, flatTarget, walkSpeed * Time.deltaTime);
                    yield return null;
                }
                Rig.SetMode(BeanMode.Idle);
            }
            while (Quaternion.Angle(transform.rotation, target.rotation) > 1f)
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, target.rotation, turnSpeed * Time.deltaTime);
                yield return null;
            }
            transform.rotation = target.rotation;
        }

        /// <summary>Teleports without walking (used when the camera is looking elsewhere).</summary>
        public void PlaceAt(Transform target)
        {
            if (target == null) return;
            transform.SetPositionAndRotation(target.position, target.rotation);
            turnGoal = null;
            LowerLight(snap: true);
            Rig.SetMode(BeanMode.Idle);
            Rig.ResetLayers(); // a cut: no half-finished lean or reach carries over
            IsHoldingBottle = false;
        }

#if UNITY_EDITOR
        public void EditorInit(BeanRig beanRig, GameObject drops, GameObject light)
        {
            rig = beanRig;
            propEyeDrops = drops;
            propLight = light;
        }
#endif
    }
}
