using System;

namespace Hindsight.Core.Interactions
{
    public enum HoldStillState
    {
        Idle,
        Holding,
        Interrupted,
        Succeeded,
    }

    /// <summary>
    /// Rules for "keep still / keep your eyes open" moments: the child must hold for
    /// <see cref="RequiredSeconds"/> without letting go. Letting go early is treated as a
    /// blink and the attempt resets, so a child can always try again rather than fail.
    /// </summary>
    public sealed class HoldStillChallenge
    {
        private float heldSeconds;

        public HoldStillChallenge(float requiredSeconds)
        {
            if (requiredSeconds <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(requiredSeconds), requiredSeconds, "Hold duration must be positive.");
            }

            RequiredSeconds = requiredSeconds;
        }

        public float RequiredSeconds { get; }

        public HoldStillState State { get; private set; } = HoldStillState.Idle;

        /// <summary>Number of times the hold was interrupted before succeeding.</summary>
        public int InterruptionCount { get; private set; }

        public float Progress => Math.Min(heldSeconds / RequiredSeconds, 1f);

        public void BeginHold()
        {
            if (State == HoldStillState.Succeeded)
            {
                return;
            }

            heldSeconds = 0f;
            State = HoldStillState.Holding;
        }

        /// <summary>Advances the hold timer. Returns the resulting state.</summary>
        public HoldStillState Tick(float deltaSeconds)
        {
            if (State != HoldStillState.Holding)
            {
                return State;
            }

            heldSeconds += Math.Max(deltaSeconds, 0f);
            if (heldSeconds >= RequiredSeconds)
            {
                State = HoldStillState.Succeeded;
            }

            return State;
        }

        public void ReleaseHold()
        {
            if (State != HoldStillState.Holding)
            {
                return;
            }

            InterruptionCount++;
            heldSeconds = 0f;
            State = HoldStillState.Interrupted;
        }
    }
}
