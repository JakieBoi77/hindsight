using System.Collections;
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
    /// Facade over the doctor's model. Steps ask for poses, props and moves; this class maps them
    /// onto the model's animation clips (legacy Animation from the glTF import) and adds procedural
    /// layers the clip set doesn't have: talking (mouth + head bob) and waving.
    /// Swapping the model means swapping the Visual child and the clip/bone names below.
    /// </summary>
    public sealed class DoctorActor : MonoBehaviour
    {
        [Header("Clips (matched by exact name, or by suffix after '|')")]
        [SerializeField] string idleClip = "Idle";
        [SerializeField] string walkClip = "Walking_A";
        [SerializeField] string interactClip = "Interact";
        [SerializeField] string cheerClip = "Cheer";
        [SerializeField] string useItemClip = "Use_Item";
        [SerializeField] float crossFade = 0.25f;

        [Header("Bones and parts")]
        [SerializeField] Transform headBone;
        [SerializeField] Transform chestBone;
        [SerializeField] Transform upperArmR;
        [SerializeField] Transform lowerArmR;
        [SerializeField] Transform mouth;
        [SerializeField] GameObject propEyeDrops;
        [SerializeField] GameObject propLight;

        [Header("Movement")]
        [SerializeField] float walkSpeed = 1.1f;
        [SerializeField] float turnSpeed = 300f;

        Animation anim;
        bool talking;
        float talkWeight;
        float waveWeight;
        float waveUntil;
        Vector3 mouthClosedScale;

        public bool IsTalking => talking;

        void Awake()
        {
            anim = GetComponentInChildren<Animation>();
            if (anim != null)
            {
                foreach (AnimationState state in anim)
                    state.wrapMode = Matches(state.name, idleClip) || Matches(state.name, walkClip) ? WrapMode.Loop : WrapMode.Once;
                anim.playAutomatically = false; // the glTF importer makes the first clip the default
                var idle = FindClip(DoctorPose.Idle);
                if (idle != null) anim.Play(idle);
            }
            if (mouth != null) mouthClosedScale = mouth.localScale;
            SetProp(DoctorProp.None);
        }

        /// <summary>Plays a pose. One-shot poses return to idle afterwards. Wave is procedural on top of idle.</summary>
        public void Play(DoctorPose pose)
        {
            if (pose == DoctorPose.None) return;
            if (pose == DoctorPose.Wave)
            {
                waveUntil = Time.time + 4f;
                pose = DoctorPose.Idle;
            }
            if (anim == null) return;
            var clip = FindClip(pose);
            if (clip == null) return;
            anim.CrossFade(clip, crossFade);
            if (pose is DoctorPose.Interact or DoctorPose.Cheer or DoctorPose.UseItem)
            {
                var idle = FindClip(DoctorPose.Idle);
                if (idle != null) anim.CrossFadeQueued(idle, crossFade);
            }
        }

        /// <summary>Mouth flaps and head bobs while true. Driven by the dialogue typewriter.</summary>
        public void SetTalking(bool value) => talking = value;

        public void SetProp(DoctorProp prop)
        {
            if (propEyeDrops != null) propEyeDrops.SetActive(prop == DoctorProp.EyeDrops);
            if (propLight != null) propLight.SetActive(prop == DoctorProp.Light);
        }

        /// <summary>Walks to <paramref name="target"/> and turns to match its rotation.</summary>
        public IEnumerator MoveTo(Transform target)
        {
            if (target == null) yield break;
            var flatTarget = new Vector3(target.position.x, transform.position.y, target.position.z);
            if (Vector3.Distance(transform.position, flatTarget) > 0.05f)
            {
                Play(DoctorPose.Walk);
                while (Vector3.Distance(transform.position, flatTarget) > 0.02f)
                {
                    var dir = flatTarget - transform.position;
                    if (dir.sqrMagnitude > 0.0001f)
                        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(dir), turnSpeed * Time.deltaTime);
                    transform.position = Vector3.MoveTowards(transform.position, flatTarget, walkSpeed * Time.deltaTime);
                    yield return null;
                }
                Play(DoctorPose.Idle);
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
            Play(DoctorPose.Idle);
        }

        // Procedural layers run after the Animation component has posed the skeleton this frame.
        void LateUpdate()
        {
            var dt = Time.deltaTime;
            talkWeight = Mathf.MoveTowards(talkWeight, talking ? 1f : 0f, dt * 6f);
            waveWeight = Mathf.MoveTowards(waveWeight, Time.time < waveUntil ? 1f : 0f, dt * 4f);
            var t = Time.time;

            if (talkWeight > 0f)
            {
                var up = transform.up;
                var right = transform.right;
                if (headBone != null)
                {
                    var nod = (Mathf.Sin(t * 7.3f) * 5f + Mathf.Sin(t * 2.9f) * 3f) * talkWeight;
                    var tilt = Mathf.Sin(t * 1.7f) * 4f * talkWeight;
                    headBone.rotation = Quaternion.AngleAxis(nod, right) * Quaternion.AngleAxis(tilt, transform.forward) * headBone.rotation;
                }
                if (chestBone != null)
                    chestBone.rotation = Quaternion.AngleAxis(Mathf.Sin(t * 2.1f) * 3f * talkWeight, up) * chestBone.rotation;
            }

            if (mouth != null)
            {
                // Two sines at odd ratios read as syllables.
                var open = talkWeight * Mathf.Clamp01(0.5f + 0.5f * Mathf.Sin(t * 17f) * Mathf.Sin(t * 5.3f + 1f));
                mouth.localScale = new Vector3(mouthClosedScale.x, mouthClosedScale.y * (1f + 1.1f * open), mouthClosedScale.z);
            }

            if (waveWeight > 0f) ApplyWave(waveWeight, Mathf.Sin(t * 9f));
        }

        /// <summary>
        /// Right arm out to the side with the forearm angled up, hand swinging. Chibi arms are short and the
        /// head is wide: folding the forearm straight up would tuck the hand behind the head, so it stays
        /// at ~45 degrees and the hand waves beside the cheek.
        /// </summary>
        public void ApplyWave(float weight, float swing)
        {
            if (upperArmR == null || lowerArmR == null) return;
            var fwd = transform.forward;
            // Swing forward first (so the hand is in front of the hair), then out to the side.
            var raise = Quaternion.AngleAxis(120f, fwd) * Quaternion.AngleAxis(-12f, transform.right);
            upperArmR.rotation = Quaternion.Slerp(upperArmR.rotation, raise * upperArmR.rotation, weight);
            lowerArmR.rotation = Quaternion.Slerp(lowerArmR.rotation, Quaternion.AngleAxis(35f + 25f * swing, fwd) * lowerArmR.rotation, weight);
        }

        string FindClip(DoctorPose pose)
        {
            var wanted = pose switch
            {
                DoctorPose.Idle => idleClip,
                DoctorPose.Walk => walkClip,
                DoctorPose.Interact => interactClip,
                DoctorPose.Cheer => cheerClip,
                DoctorPose.UseItem => useItemClip,
                _ => null,
            };
            if (wanted == null || anim == null) return null;
            foreach (AnimationState state in anim)
                if (Matches(state.name, wanted)) return state.name;
            return null;
        }

        static bool Matches(string clipName, string wanted) =>
            clipName == wanted || clipName.EndsWith("|" + wanted, System.StringComparison.OrdinalIgnoreCase);

#if UNITY_EDITOR
        public void EditorInit(Transform head, Transform chest, Transform upperArm, Transform lowerArm, Transform mouthT,
            GameObject drops, GameObject light)
        {
            headBone = head;
            chestBone = chest;
            upperArmR = upperArm;
            lowerArmR = lowerArm;
            mouth = mouthT;
            propEyeDrops = drops;
            propLight = light;
        }
#endif
    }
}
