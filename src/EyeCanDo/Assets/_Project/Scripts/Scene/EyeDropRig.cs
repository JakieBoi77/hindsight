using System.Collections;
using UnityEngine;

namespace Hindsight.EyeCanDo.SceneObjects
{
    /// <summary>
    /// The eye-drop bottle and the drop that falls from it: over the player's eye in the first-person
    /// "tilt back" view, over Sam's eye when Sam goes first, and resting on the tool tray otherwise.
    /// </summary>
    public sealed class EyeDropRig : MonoBehaviour
    {
        [SerializeField] Transform bottle;
        [SerializeField] Transform drop;
        [SerializeField] Transform dropStart;
        [SerializeField] Transform dropTarget;
        [SerializeField] Transform restPoint; // where the bottle sits on the tool tray
        [SerializeField] Transform holdPoint; // over the player's eye

        Vector3 fallFrom;

        /// <summary>The bottle that moves between the tray and an eye.</summary>
        public Transform Bottle => bottle;

        /// <summary>True while the bottle is held up over an eye (the doctor's hand should be on it).</summary>
        public bool IsHeldUp { get; private set; }

        /// <summary>The eye the bottle is over while <see cref="IsHeldUp"/>.</summary>
        public Vector3 HeldOverEye { get; private set; }

        void Awake() => ShowAtRest();

        public void ShowAtRest()
        {
            IsHeldUp = false;
            if (drop != null) drop.gameObject.SetActive(false);
            if (bottle != null && restPoint != null) bottle.SetPositionAndRotation(restPoint.position, restPoint.rotation);
        }

        public void ShowHolding()
        {
            IsHeldUp = true;
            HeldOverEye = dropTarget != null ? dropTarget.position : transform.position;
            if (bottle != null && holdPoint != null) bottle.SetPositionAndRotation(holdPoint.position, holdPoint.rotation);
            if (drop != null) drop.gameObject.SetActive(false);
        }

        /// <summary>Starts a drop at the bottle tip. Move it with <see cref="SetDrop"/>; it only falls while the child holds still.</summary>
        public void BeginDrop()
        {
            if (drop == null || dropStart == null) return;
            fallFrom = dropStart.position;
            drop.position = fallFrom;
            drop.gameObject.SetActive(true);
        }

        /// <summary>Places the drop part way to the eye (0 = at the tip, 1 = landed), accelerating like gravity.</summary>
        public void SetDrop(float k)
        {
            if (drop == null || dropTarget == null) return;
            drop.position = Vector3.Lerp(fallFrom, dropTarget.position, k * k);
        }

        public void EndDrop()
        {
            if (drop != null) drop.gameObject.SetActive(false);
        }

        /// <summary>
        /// Holds the bottle over someone else's eye (Sam's) and lets one drop fall onto it. The bottle is
        /// held a little toward <paramref name="holderSide"/> (where the doctor stands), within her reach.
        /// </summary>
        public IEnumerator FallOnto(Vector3 eye, Vector3 holderSide = default, float seconds = 1.2f)
        {
            if (bottle == null) yield break;
            var side = holderSide - eye;
            side.y = 0f;
            var lean = side.sqrMagnitude > 1e-6f ? side.normalized * 0.05f : Vector3.zero;
            bottle.SetPositionAndRotation(eye + Vector3.up * 0.16f + lean, Quaternion.identity); // the bottle is modelled tip-down
            IsHeldUp = true;
            HeldOverEye = eye;
            yield return Wait(0.5f); // the doctor's hand comes to the bottle
            yield return Wait(0.5f);
            BeginDrop();
            var to = eye + Vector3.up * 0.01f;
            for (var t = 0f; t < seconds; t += Time.deltaTime)
            {
                var k = t / seconds;
                if (drop != null) drop.position = Vector3.Lerp(fallFrom, to, k * k);
                yield return null;
            }
            EndDrop();
            yield return Wait(0.4f);
            ShowAtRest();
        }

        static IEnumerator Wait(float seconds)
        {
            for (var t = 0f; t < seconds; t += Time.deltaTime) yield return null;
        }

#if UNITY_EDITOR
        public void EditorInit(Transform bottleT, Transform dropT, Transform start, Transform target, Transform rest, Transform hold)
        {
            bottle = bottleT;
            drop = dropT;
            dropStart = start;
            dropTarget = target;
            restPoint = rest;
            holdPoint = hold;
        }
#endif
    }
}
