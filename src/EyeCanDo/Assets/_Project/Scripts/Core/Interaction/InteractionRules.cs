using System;

namespace EyeCanDo.Core.Interaction
{
    /// <summary>
    /// Lets one navigation input through per screen transition and ignores the rest, so
    /// repeated taps cannot skip a screen (FR_IgnoreRepeatedTouches).
    /// </summary>
    public sealed class InputGate
    {
        public bool IsOpen { get; private set; } = true;

        /// <summary>Returns true for the first input, then false until <see cref="Open"/>.</summary>
        public bool TryPass()
        {
            if (!IsOpen)
            {
                return false;
            }

            IsOpen = false;
            return true;
        }

        /// <summary>Called when the next screen is shown and ready for input.</summary>
        public void Open()
        {
            IsOpen = true;
        }
    }

    /// <summary>
    /// The skip control acts only after a deliberate hold of <see cref="HoldSeconds"/>, so a
    /// repeated tap cannot skip content. Disabled on sensation steps (FR_DescribeSensations).
    /// </summary>
    public sealed class SkipHold
    {
        /// <summary>SKIP_HOLD_MIN_S in the SRS.</summary>
        public const float HoldSeconds = 1f;

        private float held;
        private bool pressing;

        public bool Enabled { get; set; } = true;

        public float Progress => Math.Min(held / HoldSeconds, 1f);

        public void Press()
        {
            if (Enabled)
            {
                pressing = true;
                held = 0f;
            }
        }

        public void Release()
        {
            pressing = false;
            held = 0f;
        }

        /// <summary>Returns true once, at the moment the hold completes.</summary>
        public bool Tick(float deltaSeconds)
        {
            if (!pressing || !Enabled)
            {
                return false;
            }

            held += Math.Max(deltaSeconds, 0f);
            if (held < HoldSeconds)
            {
                return false;
            }

            pressing = false;
            held = 0f;
            return true;
        }
    }

    /// <summary>
    /// Asks for a demonstration when the child has not done the expected action for a while
    /// (FR_HintOnMiss). The first hint comes after <see cref="FirstHintSeconds"/>, later ones
    /// after <see cref="RepeatHintSeconds"/>.
    /// </summary>
    public sealed class IdleHint
    {
        public const float FirstHintSeconds = 4f;
        public const float RepeatHintSeconds = 7f;

        private float idle;
        private bool hinted;

        public bool Enabled { get; set; } = true;

        /// <summary>Returns true when a hint should be shown now.</summary>
        public bool Tick(float deltaSeconds)
        {
            if (!Enabled)
            {
                return false;
            }

            idle += Math.Max(deltaSeconds, 0f);
            var threshold = hinted ? RepeatHintSeconds : FirstHintSeconds;
            if (idle < threshold)
            {
                return false;
            }

            idle = 0f;
            hinted = true;
            return true;
        }

        /// <summary>Called whenever the child makes progress.</summary>
        public void Reset()
        {
            idle = 0f;
        }

        /// <summary>Called when moving to a new action, so the first hint is quick again.</summary>
        public void Restart()
        {
            idle = 0f;
            hinted = false;
        }
    }
}
