using System;
using System.Threading;
using UnityEngine;

namespace Hindsight.Procedures
{
    /// <summary>
    /// Presents one <see cref="StepDefinition"/> and completes when the child has finished it.
    /// Views are spawned per step by <see cref="ProcedureController"/> and destroyed afterwards.
    /// </summary>
    public abstract class StepView : MonoBehaviour
    {
        public abstract Awaitable RunAsync(StepDefinition definition, StepContext context, CancellationToken cancellationToken);
    }

    /// <summary>Typed base so each view works with its own definition type.</summary>
    public abstract class StepView<TDefinition> : StepView
        where TDefinition : StepDefinition
    {
        public sealed override Awaitable RunAsync(StepDefinition definition, StepContext context, CancellationToken cancellationToken)
        {
            if (!(definition is TDefinition typed))
            {
                throw new ArgumentException(
                    $"{GetType().Name} expects a {typeof(TDefinition).Name} but was given {definition?.GetType().Name ?? "null"}.",
                    nameof(definition));
            }

            return RunStepAsync(typed, context, cancellationToken);
        }

        protected abstract Awaitable RunStepAsync(TDefinition definition, StepContext context, CancellationToken cancellationToken);
    }
}
