using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Hindsight.UI
{
    /// <summary>
    /// A press-and-hold target. Sliding a finger off the button does not count as letting go,
    /// which is more forgiving for small hands; only lifting the finger releases it.
    /// </summary>
    public sealed class HoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private float pressedScale = 0.93f;

        private Vector3 baseScale;
        private bool interactable = true;

        public event Action Pressed;

        public event Action Released;

        public bool IsHeld { get; private set; }

        public bool Interactable
        {
            get => interactable;
            set
            {
                interactable = value;
                if (!value)
                {
                    Release();
                }
            }
        }

        private void Awake()
        {
            baseScale = transform.localScale;
        }

        private void OnDisable()
        {
            Release();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            Press();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            Release();
        }

        /// <summary>Begins a hold. Exposed for automated tests and accessibility input.</summary>
        public void Press()
        {
            if (!interactable || IsHeld)
            {
                return;
            }

            IsHeld = true;
            transform.localScale = baseScale * pressedScale;
            Pressed?.Invoke();
        }

        public void Release()
        {
            if (!IsHeld)
            {
                return;
            }

            IsHeld = false;
            transform.localScale = baseScale;
            Released?.Invoke();
        }
    }
}
