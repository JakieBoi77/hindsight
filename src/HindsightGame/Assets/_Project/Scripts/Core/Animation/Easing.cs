using System;

namespace Hindsight.Core.Animation
{
    public enum EaseType
    {
        Linear,
        InQuad,
        OutQuad,
        InOutQuad,
        OutCubic,
        OutBack,
        OutBounce,
    }

    /// <summary>Standard easing curves (see easings.net). Input and output are normalised 0..1.</summary>
    public static class Easing
    {
        private const float BackOvershoot = 1.70158f;

        public static float Evaluate(EaseType ease, float t)
        {
            t = Math.Clamp(t, 0f, 1f);
            switch (ease)
            {
                case EaseType.Linear:
                    return t;
                case EaseType.InQuad:
                    return t * t;
                case EaseType.OutQuad:
                    return 1f - ((1f - t) * (1f - t));
                case EaseType.InOutQuad:
                    return t < 0.5f ? 2f * t * t : 1f - ((float)Math.Pow((-2f * t) + 2f, 2) / 2f);
                case EaseType.OutCubic:
                    return 1f - (float)Math.Pow(1f - t, 3);
                case EaseType.OutBack:
                    {
                        var c3 = BackOvershoot + 1f;
                        return 1f + (c3 * (float)Math.Pow(t - 1f, 3)) + (BackOvershoot * (float)Math.Pow(t - 1f, 2));
                    }

                case EaseType.OutBounce:
                    return OutBounce(t);
                default:
                    throw new ArgumentOutOfRangeException(nameof(ease), ease, "Unknown ease type.");
            }
        }

        private static float OutBounce(float t)
        {
            const float n1 = 7.5625f;
            const float d1 = 2.75f;
            if (t < 1f / d1)
            {
                return n1 * t * t;
            }

            if (t < 2f / d1)
            {
                t -= 1.5f / d1;
                return (n1 * t * t) + 0.75f;
            }

            if (t < 2.5f / d1)
            {
                t -= 2.25f / d1;
                return (n1 * t * t) + 0.9375f;
            }

            t -= 2.625f / d1;
            return (n1 * t * t) + 0.984375f;
        }
    }
}
