using System;
using Hindsight.Core.Progress;
using Hindsight.Procedures;

namespace Hindsight.App
{
    /// <summary>Bridges procedure data assets with the engine-agnostic progress rules.</summary>
    public sealed class ProgressService
    {
        private readonly IProgressStore store;

        public ProgressService(IProgressStore store)
        {
            this.store = store ?? throw new ArgumentNullException(nameof(store));
            Current = store.Load();
        }

        public PlayerProgress Current { get; }

        public LevelAvailability GetAvailability(ProcedureDefinition procedure)
        {
            var requiredId = procedure.RequiredProcedure != null ? procedure.RequiredProcedure.Id : null;
            return UnlockRules.Evaluate(procedure.Id, procedure.IsPlayable, requiredId, Current);
        }

        public void RecordCompletion(ProcedureDefinition procedure)
        {
            var changed = Current.MarkCompleted(procedure.Id);
            changed |= Current.AddSticker(procedure.StickerId);
            if (changed)
            {
                store.Save(Current);
            }
        }
    }
}
