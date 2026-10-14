using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Hindsight.Ophthalmology.SceneObjects
{
    /// <summary>
    /// A tappable 3D object (needs a collider; the camera needs a PhysicsRaycaster).
    /// Works for touch and mouse through the EventSystem.
    /// </summary>
    public sealed class Interactable : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] string interactableId = "";
        [Tooltip("Shown while the object is waiting to be tapped (arrow, glow...).")]
        [SerializeField] GameObject highlight;

        public string Id => string.IsNullOrEmpty(interactableId) ? name : interactableId;
        public bool Accepting { get; private set; }
        public event Action Tapped;

        void Awake() => SetAccepting(false);

        public void SetAccepting(bool value)
        {
            Accepting = value;
            if (highlight != null) highlight.SetActive(value);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (Accepting) Tapped?.Invoke();
        }

#if UNITY_EDITOR
        public void EditorInit(string id, GameObject highlightObject)
        {
            interactableId = id;
            highlight = highlightObject;
        }
#endif
    }
}
