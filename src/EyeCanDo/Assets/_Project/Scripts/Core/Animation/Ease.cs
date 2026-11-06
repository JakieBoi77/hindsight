using System;

namespace EyeCanDo.Core.Animation
{
    /// <summary>Easing curves for UI motion. Input and output are 0 to 1.</summary>
    public static class Ease
    {
        public static float Linear(float t)
        {
            return Clamp01(t);
        }

        public static float OutCubic(float t)
        {
            t = Clamp01(t) - 1f;
            return t * t * t + 1f;
        }

        public static float InOutSine(float t)
        {
            return (float)(-(Math.Cos(Math.PI * Clamp01(t)) - 1) / 2);
        }

        public static float InCubic(float t)
        {
            return t * t * t;
        }

        public static float InQuad(float t)
        {
            t = Clamp01(t);
            return t * t;
        }

        /// <summary>Overshoots slightly, then settles. Used for pop-ins.</summary>
        public static float OutBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            t = Clamp01(t) - 1f;
            return 1f + c3 * t * t * t + c1 * t * t;
        }

        private static float Clamp01(float t)
        {
            return Math.Max(0f, Math.Min(1f, t));
        }
    }
}
