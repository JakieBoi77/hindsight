using UnityEngine;

namespace Hindsight.Tweening
{
    /// <summary>
    /// Looping ambient motion (gentle bob or pulse) to make characters and tap targets feel alive.
    /// Cheap per-frame math only; disable the component when the motion is not needed.
    /// </summary>
    public sealed class IdleMotion : MonoBehaviour
    {
        private enum MotionKind
        {
            Bob,
            Pulse,
            Sway,
        }

        [SerializeField] private MotionKind kind = MotionKind.Bob;
        [SerializeField] private float amplitude = 10f;
        [SerializeField] private float cyclesPerSecond = 0.5f;

        private RectTransform rectTransform;
        private Vector2 baseAnchoredPosition;
        private Vector3 baseScale;
        private float baseRotation;
        private float phase;

        private void Awake()
        {
            rectTransform = (RectTransform)transform;
        }

        private void OnEnable()
        {
            baseAnchoredPosition = rectTransform.anchoredPosition;
            baseScale = rectTransform.localScale;
            baseRotation = rectTransform.localEulerAngles.z;
            phase = 0f;
        }

        private void Update()
        {
            phase += Time.unscaledDeltaTime * cyclesPerSecond * Mathf.PI * 2f;
            var wave = Mathf.Sin(phase);
            switch (kind)
            {
                case MotionKind.Bob:
                    rectTransform.anchoredPosition = baseAnchoredPosition + new Vector2(0f, wave * amplitude);
                    break;
                case MotionKind.Pulse:
                    // Amplitude is a percentage for pulses, e.g. 6 = +/-6% scale.
                    rectTransform.localScale = baseScale * (1f + (wave * amplitude * 0.01f));
                    break;
                case MotionKind.Sway:
                    rectTransform.localEulerAngles = new Vector3(0f, 0f, baseRotation + (wave * amplitude));
                    break;
            }
        }

        private void OnDisable()
        {
            if (rectTransform == null)
            {
                return;
            }

            rectTransform.anchoredPosition = baseAnchoredPosition;
            rectTransform.localScale = baseScale;
            rectTransform.localEulerAngles = new Vector3(0f, 0f, baseRotation);
        }
    }
}
