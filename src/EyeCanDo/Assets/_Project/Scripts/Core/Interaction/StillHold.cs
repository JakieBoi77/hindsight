using System;

namespace EyeCanDo.Core.Interaction
{
    /// <summary>
    /// "Look up and hold still": the child holds a finger down for a few seconds. Letting go only
    /// pauses. The time already held is kept and the doctor waits (FR_NoPenalty). Taps shorter
    /// than <see cref="AccidentalTapSeconds"/> do not count as pauses.
    /// </summary>
    public sealed class StillHold
    {
        public const float AccidentalTapSeconds = 0.4f;

        private float pressedFor;

        public StillHold(float seconds)
        {
            Seconds = Math.Max(0.1f, seconds);
        }

        public float Seconds { get; }

        public float Held { get; private set; }

        public bool IsHolding { get; private set; }

        public bool Done => Held >= Seconds;

        public float Progress => Math.Min(Held / Seconds, 1f);

        /// <summary>Times the child let go before the end, not counting accidental taps.</summary>
        public int Pauses { get; private set; }

        public void Press()
        {
            if (!Done)
            {
                IsHolding = true;
                pressedFor = 0f;
            }
        }

        public void Release()
        {
            if (!IsHolding)
            {
                return;
            }

            IsHolding = false;
            if (!Done && pressedFor >= AccidentalTapSeconds)
            {
                Pauses++;
            }
        }

        /// <summary>Advances the hold. Returns true on the frame the hold completes.</summary>
        public bool Tick(float deltaSeconds)
        {
            if (!IsHolding || Done || deltaSeconds <= 0f)
            {
                return false;
            }

            pressedFor += deltaSeconds;
            Held = Math.Min(Seconds, Held + deltaSeconds);
            if (!Done)
            {
                return false;
            }

            IsHolding = false;
            return true;
        }
    }
}
