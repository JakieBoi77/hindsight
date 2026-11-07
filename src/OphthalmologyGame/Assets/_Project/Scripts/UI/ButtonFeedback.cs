using UnityEngine;
using UnityEngine.EventSystems;

namespace Hindsight.Ophthalmology.UI
{
    /// <summary>Squishes a button while pressed, springs it back on release, and plays a soft tap.</summary>
    public sealed class ButtonFeedback : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        [SerializeField] float pressedScale = 0.92f;
        [SerializeField] float speed = 18f;
        [SerializeField] bool tapSound = true;

        float target = 1f;
        float current = 1f;

        public void OnPointerDown(PointerEventData eventData)
        {
            target = pressedScale;
            if (tapSound) Sfx.Play(SfxId.Tap);
        }

        public void OnPointerUp(PointerEventData eventData) => target = 1f;
        public void OnPointerExit(PointerEventData eventData) => target = 1f;

        void OnDisable()
        {
            target = current = 1f;
            transform.localScale = Vector3.one;
        }

        void Update()
        {
            if (Mathf.Approximately(current, target)) return;
            current = Mathf.MoveTowards(current, target, speed * Time.unscaledDeltaTime);
            transform.localScale = Vector3.one * current;
        }
    }
}
