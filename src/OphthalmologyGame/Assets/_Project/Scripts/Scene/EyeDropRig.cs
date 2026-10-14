using System;
using System.Collections;
using UnityEngine;

namespace Hindsight.Ophthalmology.SceneObjects
{
    /// <summary>
    /// The eye-drop bottle held over the player's eye and the drop that falls from it.
    /// Positioned (by the scene builder) over the first-person "tilt back" camera.
    /// </summary>
    public sealed class EyeDropRig : MonoBehaviour
    {
        [SerializeField] Transform bottle;
        [SerializeField] Transform drop;
        [SerializeField] Transform dropStart;
        [SerializeField] Transform dropTarget;
        [SerializeField] Transform restPoint; // where the bottle sits on the tool tray
        [SerializeField] Transform holdPoint; // over the player's eye

        void Awake() => ShowAtRest();

        public void ShowAtRest()
        {
            if (drop != null) drop.gameObject.SetActive(false);
            if (bottle != null && restPoint != null) bottle.SetPositionAndRotation(restPoint.position, restPoint.rotation);
        }

        public void ShowHolding()
        {
            if (bottle != null && holdPoint != null) bottle.SetPositionAndRotation(holdPoint.position, holdPoint.rotation);
            if (drop != null) drop.gameObject.SetActive(false);
        }

        /// <summary>
        /// Lets one drop fall. Returns early (and reports a miss) if <paramref name="stillOk"/> turns false.
        /// </summary>
        public IEnumerator Fall(float seconds, Func<bool> stillOk, Action<bool> onDone, Action<float> onProgress = null)
        {
            if (drop == null || dropStart == null || dropTarget == null)
            {
                onDone?.Invoke(true);
                yield break;
            }
            drop.gameObject.SetActive(true);
            for (var t = 0f; t < seconds; t += Time.deltaTime)
            {
                if (!stillOk())
                {
                    drop.gameObject.SetActive(false);
                    onDone?.Invoke(false);
                    yield break;
                }
                var k = t / seconds;
                onProgress?.Invoke(k);
                drop.position = Vector3.Lerp(dropStart.position, dropTarget.position, k * k); // accelerate like gravity
                yield return null;
            }
            drop.gameObject.SetActive(false);
            onDone?.Invoke(true);
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
