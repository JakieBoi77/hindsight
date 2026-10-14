using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hindsight.Ophthalmology.Procedure
{
    /// <summary>Plays a <see cref="ProcedureDefinition"/> step by step against a scene context.</summary>
    public sealed class ProcedureRunner : MonoBehaviour
    {
        [SerializeField] ProcedureDefinition procedure;
        [SerializeField] MonoBehaviour contextBehaviour; // must implement IProcedureContext
        [SerializeField] bool playOnStart = true;

        /// <summary>Raised with (stepIndex, stepCount, step) as each step starts.</summary>
        public event Action<int, int, ProcedureStep> StepStarted;
        public event Action Completed;

        public ProcedureDefinition Procedure => procedure;
        public bool IsRunning { get; private set; }

        void Start()
        {
            if (playOnStart) Play();
        }

        public void Play()
        {
            if (IsRunning || procedure == null) return;
            var context = contextBehaviour as IProcedureContext;
            if (context == null)
            {
                Debug.LogError($"{name}: context behaviour must implement {nameof(IProcedureContext)}.", this);
                return;
            }
            StartCoroutine(PlayRoutine(context));
        }

        IEnumerator PlayRoutine(IProcedureContext context)
        {
            IsRunning = true;
            yield return RunSteps(procedure.Steps, context, (i, n, s) => StepStarted?.Invoke(i, n, s));
            IsRunning = false;
            Completed?.Invoke();
        }

        /// <summary>Runs steps in order, skipping null entries. Static so it can be tested without a scene.</summary>
        public static IEnumerator RunSteps(IReadOnlyList<ProcedureStep> steps, IProcedureContext context,
            Action<int, int, ProcedureStep> onStepStarted = null)
        {
            for (var i = 0; i < steps.Count; i++)
            {
                var step = steps[i];
                if (step == null) continue;
                onStepStarted?.Invoke(i, steps.Count, step);
                yield return step.Run(context);
            }
        }

#if UNITY_EDITOR
        public void EditorInit(ProcedureDefinition value, MonoBehaviour context)
        {
            procedure = value;
            contextBehaviour = context;
        }
#endif
    }
}
