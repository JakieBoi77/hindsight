using UnityEngine;
using UnityEngine.EventSystems;

namespace Hindsight.Ophthalmology.UI
{
    /// <summary>
    /// Drag sideways over this (invisible) area to spin the kid around and see their back. The kid
    /// faces the camera again when the area is hidden (leaving Dress up).
    /// </summary>
    public sealed class KidSpinner : MonoBehaviour, IDragHandler, IEndDragHandler
    {
        [SerializeField] Transform target;
        [Tooltip("Degrees per screen pixel dragged.")]
        [SerializeField] float degreesPerPixel = 0.45f;
        [Tooltip("After a flick the kid keeps turning and slows down.")]
        [SerializeField] float friction = 4f;

        Quaternion start;
        float spin;
        bool dragging;

        void OnEnable()
        {
            if (target != null) start = target.rotation;
            spin = 0f;
        }

        void OnDisable()
        {
            if (target != null) target.rotation = start;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (target == null) return;
            dragging = true;
            var degrees = -eventData.delta.x * degreesPerPixel;
            target.Rotate(Vector3.up, degrees, Space.World);
            spin = Time.unscaledDeltaTime > 0f ? degrees / Time.unscaledDeltaTime : 0f;
        }

        public void OnEndDrag(PointerEventData eventData) => dragging = false;

        void Update()
        {
            if (dragging || target == null || Mathf.Abs(spin) < 1f) return;
            target.Rotate(Vector3.up, spin * Time.unscaledDeltaTime, Space.World);
            spin = Mathf.Lerp(spin, 0f, friction * Time.unscaledDeltaTime);
        }

#if UNITY_EDITOR
        public void EditorInit(Transform kid) => target = kid;
#endif
    }
}
