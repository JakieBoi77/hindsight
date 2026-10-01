using System;

namespace Hindsight.Core.Interactions
{
    /// <summary>Counts taps toward a target, e.g. "blink five times".</summary>
    public sealed class TapGoal
    {
        public TapGoal(int requiredTaps)
        {
            if (requiredTaps <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(requiredTaps), requiredTaps, "Tap goal must be positive.");
            }

            RequiredTaps = requiredTaps;
        }

        public int RequiredTaps { get; }

        public int TapCount { get; private set; }

        public bool IsReached => TapCount >= RequiredTaps;

        public float Progress => (float)TapCount / RequiredTaps;

        /// <summary>Registers a tap. Extra taps after the goal is reached are ignored.</summary>
        /// <returns>True if this tap reached the goal.</returns>
        public bool RegisterTap()
        {
            if (IsReached)
            {
                return false;
            }

            TapCount++;
            return IsReached;
        }
    }
}
