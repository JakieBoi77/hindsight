using System;
using System.Threading;
using Hindsight.Core.Animation;
using UnityEngine;
using UnityEngine.UI;

namespace Hindsight.Tweening
{
    /// <summary>
    /// Minimal Awaitable-based tweens. Avoids a third-party dependency (e.g. DOTween) while
    /// keeping step scripts readable as linear "do this, then that" sequences.
    /// Pass the owning object's destroyCancellationToken so tweens stop when it is destroyed.
    /// </summary>
    public static class Tween
    {
        /// <summary>Calls <paramref name="apply"/> each frame with an eased 0..1 value.</summary>
        public static async Awaitable RunAsync(float duration, Action<float> apply, EaseType ease, CancellationToken cancellationToken)
        {
            if (duration <= 0f)
            {
                apply(Easing.Evaluate(ease, 1f));
                return;
            }

            var elapsed = 0f;
            apply(Easing.Evaluate(ease, 0f));
            while (elapsed < duration)
            {
                await Awaitable.NextFrameAsync(cancellationToken);

                // Unscaled time keeps UI animation independent of any future pause/slow-motion.
                elapsed += Time.unscaledDeltaTime;
                apply(Easing.Evaluate(ease, elapsed / duration));
            }
        }

        public static Awaitable ScaleAsync(Transform target, Vector3 to, float duration, EaseType ease, CancellationToken cancellationToken)
        {
            var from = target.localScale;
            return RunAsync(duration, t => target.localScale = Vector3.LerpUnclamped(from, to, t), ease, cancellationToken);
        }

        public static Awaitable MoveAsync(RectTransform target, Vector2 to, float duration, EaseType ease, CancellationToken cancellationToken)
        {
            var from = target.anchoredPosition;
            return RunAsync(duration, t => target.anchoredPosition = Vector2.LerpUnclamped(from, to, t), ease, cancellationToken);
        }

        public static Awaitable FadeAsync(CanvasGroup target, float to, float duration, EaseType ease, CancellationToken cancellationToken)
        {
            var from = target.alpha;
            return RunAsync(duration, t => target.alpha = Mathf.LerpUnclamped(from, to, t), ease, cancellationToken);
        }

        public static Awaitable ColorAsync(Graphic target, Color to, float duration, EaseType ease, CancellationToken cancellationToken)
        {
            var from = target.color;
            return RunAsync(duration, t => target.color = Color.LerpUnclamped(from, to, t), ease, cancellationToken);
        }

        public static Awaitable RotateAsync(Transform target, float toDegrees, float duration, EaseType ease, CancellationToken cancellationToken)
        {
            var from = target.localEulerAngles.z;
            var delta = Mathf.DeltaAngle(from, toDegrees);
            return RunAsync(duration, t => target.localEulerAngles = new Vector3(0f, 0f, from + (delta * t)), ease, cancellationToken);
        }

        /// <summary>Briefly grows then returns to the original scale; good for "tap me" feedback.</summary>
        public static async Awaitable PunchScaleAsync(Transform target, float amount, float duration, CancellationToken cancellationToken)
        {
            var baseScale = target.localScale;
            try
            {
                await RunAsync(duration, t => target.localScale = baseScale * (1f + (amount * Mathf.Sin(t * Mathf.PI))), EaseType.Linear, cancellationToken);
            }
            finally
            {
                if (target != null)
                {
                    target.localScale = baseScale;
                }
            }
        }

        /// <summary>Side-to-side wobble, used for gentle "not this one" feedback.</summary>
        public static async Awaitable ShakeAsync(RectTransform target, float amplitude, float duration, CancellationToken cancellationToken)
        {
            const float wobbles = 3f;
            var origin = target.anchoredPosition;
            try
            {
                await RunAsync(
                    duration,
                    t => target.anchoredPosition = origin + new Vector2(Mathf.Sin(t * Mathf.PI * 2f * wobbles) * amplitude * (1f - t), 0f),
                    EaseType.Linear,
                    cancellationToken);
            }
            finally
            {
                if (target != null)
                {
                    target.anchoredPosition = origin;
                }
            }
        }
    }
}
