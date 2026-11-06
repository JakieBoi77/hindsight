using System;
using System.Collections;
using EyeCanDo.Core.Animation;
using UnityEngine;

namespace EyeCanDo.UI
{
    /// <summary>
    /// Coroutine tweens on scaled time, so pausing the game (FR_Suspend) pauses every animation.
    /// Start them with StartCoroutine on any MonoBehaviour.
    /// </summary>
    public static class Tween
    {
        public static IEnumerator Run(float duration, Func<float, float> ease, Action<float> apply)
        {
            if (duration <= 0f)
            {
                apply(1f);
                yield break;
            }

            var time = 0f;
            while (time < duration)
            {
                apply(ease(time / duration));
                yield return null;
                time += Time.deltaTime;
            }

            apply(1f);
        }

        public static IEnumerator Move(RectTransform rect, Vector2 to, float duration, Func<float, float> ease = null)
        {
            var from = rect.anchoredPosition;
            return Run(duration, ease ?? Ease.OutCubic, t => rect.anchoredPosition = Vector2.LerpUnclamped(from, to, t));
        }

        public static IEnumerator Scale(Transform target, Vector3 to, float duration, Func<float, float> ease = null)
        {
            var from = target.localScale;
            return Run(duration, ease ?? Ease.OutCubic, t => target.localScale = Vector3.LerpUnclamped(from, to, t));
        }

        public static IEnumerator Fade(CanvasGroup group, float to, float duration)
        {
            var from = group.alpha;
            return Run(duration, Ease.Linear, t => group.alpha = Mathf.Lerp(from, to, t));
        }

        /// <summary>Grows from nothing with a small overshoot.</summary>
        public static IEnumerator Pop(Transform target, float duration = 0.35f, float size = 1f)
        {
            target.localScale = Vector3.zero;
            return Run(duration, Ease.OutBack, t => target.localScale = Vector3.one * (size * t));
        }

        /// <summary>A short side-to-side wiggle, for "not yet" feedback that is never a failure.</summary>
        public static IEnumerator Wiggle(Transform target, float degrees = 8f, float duration = 0.45f)
        {
            var start = target.localRotation;
            yield return Run(duration, Ease.Linear, t =>
                target.localRotation = start * Quaternion.Euler(0f, 0f, Mathf.Sin(t * Mathf.PI * 4f) * degrees * (1f - t)));
            target.localRotation = start;
        }

        /// <summary>
        /// A quick grow-and-settle, for "yes, that worked". It always settles at normal size, so
        /// bumps that overlap cannot make something keep growing.
        /// </summary>
        public static IEnumerator Bump(Transform target, float amount = 0.15f, float duration = 0.3f)
        {
            yield return Run(duration, Ease.Linear, t =>
            {
                if (target != null)
                {
                    target.localScale = Vector3.one * (1f + Mathf.Sin(t * Mathf.PI) * amount);
                }
            });
            if (target != null)
            {
                target.localScale = Vector3.one;
            }
        }

        public static IEnumerator Wait(float seconds)
        {
            var time = 0f;
            while (time < seconds)
            {
                yield return null;
                time += Time.deltaTime;
            }
        }
    }
}
