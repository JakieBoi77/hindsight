using System.Threading;
using Hindsight.Core.Animation;
using Hindsight.Tweening;
using UnityEngine;

namespace Hindsight.Procedures.Steps
{
    /// <summary>
    /// The illustrated eye shared by several steps. The pupil scales to show dilation and the
    /// eyelid (pivoted at its top edge) scales vertically to blink.
    /// </summary>
    public sealed class EyeView : MonoBehaviour
    {
        [SerializeField] private RectTransform pupil;
        [SerializeField] private RectTransform eyelid;
        [SerializeField] private float blinkSeconds = 0.12f;

        public float PupilScale => pupil.localScale.x;

        public bool IsClosed => eyelid.localScale.y > 0.5f;

        private void Awake()
        {
            eyelid.localScale = new Vector3(1f, 0f, 1f);
        }

        public void SetPupilScale(float scale)
        {
            pupil.localScale = Vector3.one * scale;
        }

        public Awaitable DilateAsync(float toScale, float seconds, CancellationToken cancellationToken)
        {
            return Tween.ScaleAsync(pupil, Vector3.one * toScale, seconds, EaseType.InOutQuad, cancellationToken);
        }

        public Awaitable CloseAsync(CancellationToken cancellationToken)
        {
            return Tween.ScaleAsync(eyelid, Vector3.one, blinkSeconds, EaseType.InQuad, cancellationToken);
        }

        public Awaitable OpenAsync(CancellationToken cancellationToken)
        {
            return Tween.ScaleAsync(eyelid, new Vector3(1f, 0f, 1f), blinkSeconds * 1.5f, EaseType.OutQuad, cancellationToken);
        }

        public async Awaitable BlinkAsync(CancellationToken cancellationToken)
        {
            await CloseAsync(cancellationToken);
            await OpenAsync(cancellationToken);
        }
    }
}
