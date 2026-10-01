using System;

namespace Hindsight.Core.Procedures
{
    /// <summary>
    /// Tracks a child's position within a procedure's ordered steps.
    /// Kept free of Unity types so the flow rules can be unit tested.
    /// </summary>
    public sealed class ProcedureSession
    {
        public ProcedureSession(int stepCount)
        {
            if (stepCount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(stepCount), stepCount, "A procedure needs at least one step.");
            }

            StepCount = stepCount;
        }

        /// <summary>Raised with the new step index whenever the session moves forward.</summary>
        public event Action<int> StepChanged;

        /// <summary>Raised once, after the final step has been completed.</summary>
        public event Action Completed;

        public int StepCount { get; }

        public int CurrentIndex { get; private set; }

        public bool IsComplete { get; private set; }

        public bool IsLastStep => CurrentIndex == StepCount - 1;

        /// <summary>Fraction of steps finished, from 0 to 1.</summary>
        public float Progress => IsComplete ? 1f : (float)CurrentIndex / StepCount;

        /// <summary>Marks the current step finished and moves to the next one.</summary>
        public void CompleteCurrentStep()
        {
            if (IsComplete)
            {
                throw new InvalidOperationException("The procedure has already been completed.");
            }

            if (IsLastStep)
            {
                IsComplete = true;
                Completed?.Invoke();
                return;
            }

            CurrentIndex++;
            StepChanged?.Invoke(CurrentIndex);
        }
    }
}
