using UnityEngine;
using UnityEngine.EventSystems;

namespace Hindsight.Ophthalmology.UI
{
    /// <summary>Reports whether a finger/mouse is currently held down on this UI element.</summary>
    public sealed class HoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        public bool IsHeld { get; private set; }

        public void OnPointerDown(PointerEventData eventData) => IsHeld = true;
        public void OnPointerUp(PointerEventData eventData) => IsHeld = false;
        public void OnPointerExit(PointerEventData eventData) => IsHeld = false;

        void OnDisable() => IsHeld = false;
    }
}
