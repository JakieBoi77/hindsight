using UnityEngine;

namespace Hindsight.Ophthalmology.UI
{
    /// <summary>Gentle looping motion that draws a child's eye to an element (pulse, bob or sway).</summary>
    public sealed class IdleMotion : MonoBehaviour
    {
        public enum Kind
        {
            Pulse,
            Bob,
            Sway,
        }

        [SerializeField] Kind kind = Kind.Pulse;
        [SerializeField] float amplitude = 0.05f;
        [SerializeField] float cyclesPerSecond = 0.8f;

        RectTransform rect;
        Vector2 basePosition;

        void Awake()
        {
            rect = (RectTransform)transform;
            basePosition = rect.anchoredPosition;
        }

        void OnDisable()
        {
            if (rect == null) return;
            rect.anchoredPosition = basePosition;
            rect.localRotation = Quaternion.identity;
        }

        void Update()
        {
            var s = Mathf.Sin(Time.unscaledTime * cyclesPerSecond * Mathf.PI * 2f);
            switch (kind)
            {
                case Kind.Pulse:
                    // Put this on a wrapper, not on the button itself: ButtonFeedback also drives the button's scale.
                    rect.localScale = Vector3.one * (1f + amplitude * s);
                    break;
                case Kind.Bob:
                    rect.anchoredPosition = basePosition + Vector2.up * (amplitude * s);
                    break;
                case Kind.Sway:
                    rect.localRotation = Quaternion.Euler(0f, 0f, amplitude * s);
                    break;
            }
        }

#if UNITY_EDITOR
        public void EditorInit(Kind motion, float amp, float speed)
        {
            kind = motion;
            amplitude = amp;
            cyclesPerSecond = speed;
        }
#endif
    }
}
