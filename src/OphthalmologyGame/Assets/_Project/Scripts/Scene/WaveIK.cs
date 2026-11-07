using UnityEngine;

namespace Hindsight.Ophthalmology.SceneObjects
{
    /// <summary>
    /// A friendly wave for the chibi characters, done with two-bone IK: the hand goes up and out from
    /// the shoulder, always clear of the head, and the forearm swings side to side around the elbow, and the elbow bends
    /// outwards and down. The KayKit pack has no wave clip, and fixed joint angles clip into the
    /// characters' big heads.
    /// </summary>
    public sealed class WaveIK : MonoBehaviour
    {
        [SerializeField] Transform upperArm;
        [SerializeField] Transform lowerArm;
        [SerializeField] Transform hand;
        [SerializeField] Transform headCenter;
        [SerializeField] float headRadius = 0.2f;
        [Tooltip("Hand target from the shoulder, in arm lengths: out to the side, up, forward.")]
        [SerializeField] float outward = 0.6f;
        [SerializeField] float upward = 0.85f;
        [SerializeField] float forward = 0.15f;
        [Tooltip("How far the hand swings side to side around the elbow, in degrees.")]
        [SerializeField] float swingDegrees = 28f;
        [Tooltip("The hand never comes closer to the head's centre line than this, in head radii.")]
        [SerializeField] float clearance = 1.45f;

        /// <summary>Poses the arm. Call after the animation has posed the skeleton (in LateUpdate).</summary>
        public void Apply(float weight, float swing)
        {
            if (weight <= 0f || upperArm == null || lowerArm == null || hand == null || headCenter == null) return;
            var root = transform;
            var up = root.up;
            var s = upperArm.position;
            var centre = headCenter.position;

            // The side the arm is on, flattened.
            var side = Vector3.ProjectOnPlane(s - centre, up);
            side = side.sqrMagnitude > 1e-6f ? side.normalized : root.right;
            var e0 = lowerArm.position;
            var h0 = hand.position;
            float a = Vector3.Distance(s, e0), b = Vector3.Distance(e0, h0);
            var reach = a + b;
            var target = s + side * reach * outward + up * reach * upward + root.forward * reach * forward;
            // Keep the hand clear of the (big) head, sideways.
            var sideOffset = Vector3.Dot(target - centre, side);
            if (sideOffset < headRadius * clearance) target += side * (headRadius * clearance - sideOffset);

            var toTarget = target - s;
            var d = Mathf.Clamp(toTarget.magnitude, 0.001f, a + b - 0.0005f);
            var n = toTarget.normalized;
            var cos = Mathf.Clamp((a * a + d * d - b * b) / (2f * a * d), -1f, 1f);
            var sin = Mathf.Sqrt(1f - cos * cos);
            // Elbow points out to the side and down.
            var pole = Vector3.ProjectOnPlane(side - up * 0.6f, n);
            pole = pole.sqrMagnitude > 1e-6f ? pole.normalized : side;
            var elbow = s + n * (a * cos) + pole * (a * sin);
            var handGoal = s + n * d;

            var q1 = Quaternion.FromToRotation(e0 - s, elbow - s);
            upperArm.rotation = Quaternion.Slerp(Quaternion.identity, q1, weight) * upperArm.rotation;
            var e1 = lowerArm.position;
            var q2 = Quaternion.FromToRotation(hand.position - e1, handGoal - e1);
            lowerArm.rotation = Quaternion.Slerp(Quaternion.identity, q2, weight) * lowerArm.rotation;
            // The wave itself: the forearm swings around the elbow, only ever outwards (away from the
            // head), between the IK pose and swingDegrees further out.
            var r = hand.position - lowerArm.position;
            var outwardSign = Vector3.Dot(Vector3.Cross(root.forward, r), side) >= 0f ? 1f : -1f;
            var angle = outwardSign * swingDegrees * (0.5f + 0.5f * swing) * weight;
            lowerArm.rotation = Quaternion.AngleAxis(angle, root.forward) * lowerArm.rotation;
        }

#if UNITY_EDITOR
        /// <summary>Where the hand goes, in arm lengths from the shoulder (kids' short arms wave lower and wider).</summary>
        public void EditorShape(float outwards, float upwards, float forwards)
        {
            outward = outwards;
            upward = upwards;
            forward = forwards;
        }

        public void EditorInit(Transform upper, Transform lower, Transform handBone, Transform centre, float radius)
        {
            upperArm = upper;
            lowerArm = lower;
            hand = handBone;
            headCenter = centre;
            headRadius = radius;
        }
#endif
    }
}
