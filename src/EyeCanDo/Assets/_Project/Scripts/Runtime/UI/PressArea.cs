using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace EyeCanDo.UI
{
    /// <summary>
    /// A touch area that reports press, release, drag and tap. It follows one finger at a time,
    /// so a second finger cannot confuse an exercise. Needs a raycast-target Graphic.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public sealed class PressArea : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        private const int NoPointer = int.MinValue;
        private const float TapMaxSeconds = 0.6f;

        private int pointerId = NoPointer;
        private float pressTime;

        public event Action Pressed;

        public event Action Released;

        public event Action Tapped;

        /// <summary>Drag movement in canvas units since the last event.</summary>
        public event Action<Vector2> Dragged;

        public bool IsPressed => pointerId != NoPointer;

        public bool Interactable { get; set; } = true;

        public static PressArea AddTo(GameObject target, bool visible = false)
        {
            if (target.GetComponent<Graphic>() == null)
            {
                var image = target.AddComponent<Image>();
                image.color = visible ? Color.white : Color.clear;
            }

            target.GetComponent<Graphic>().raycastTarget = true;
            return target.AddComponent<PressArea>();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!Interactable || pointerId != NoPointer)
            {
                return;
            }

            pointerId = eventData.pointerId;
            pressTime = Time.unscaledTime;
            Pressed?.Invoke();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.pointerId != pointerId)
            {
                return;
            }

            pointerId = NoPointer;
            Released?.Invoke();
            if (Interactable && Time.unscaledTime - pressTime <= TapMaxSeconds)
            {
                Tapped?.Invoke();
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (eventData.pointerId != pointerId || !Interactable)
            {
                return;
            }

            var canvas = GetComponentInParent<Canvas>();
            var scale = canvas != null ? canvas.rootCanvas.scaleFactor : 1f;
            Dragged?.Invoke(eventData.delta / Mathf.Max(scale, 0.0001f));
        }

        public void OnEndDrag(PointerEventData eventData)
        {
        }

        /// <summary>Lets go of any held finger, for example when the screen changes.</summary>
        public void Cancel()
        {
            if (pointerId == NoPointer)
            {
                return;
            }

            pointerId = NoPointer;
            Released?.Invoke();
        }

        private void OnDisable()
        {
            Cancel();
        }

        // Test hooks: the PlayMode auto-player drives exercises through these, the same way touches do.
        public void SimulatePress()
        {
            OnPointerDown(new PointerEventData(EventSystem.current) { pointerId = 0 });
        }

        public void SimulateRelease()
        {
            OnPointerUp(new PointerEventData(EventSystem.current) { pointerId = 0 });
        }

        public void SimulateDrag(Vector2 canvasDelta)
        {
            if (IsPressed && Interactable)
            {
                Dragged?.Invoke(canvasDelta);
            }
        }
    }
}
