using System.Collections.Generic;
using UnityEngine;

namespace Hindsight.Procedures
{
    /// <summary>
    /// Data for one screen of a procedure. Each subclass pairs with a <see cref="StepView"/>
    /// prefab that knows how to present it, so new procedures are mostly authored as data.
    /// </summary>
    public abstract class StepDefinition : ScriptableObject
    {
        [SerializeField] private StepView viewPrefab;

        public StepView ViewPrefab => viewPrefab;

        /// <summary>True for the step at which the procedure counts as finished (e.g. the reward).</summary>
        public virtual bool MarksProcedureComplete => false;

        /// <summary>Appends human-readable authoring problems; used by the editor validator and tests.</summary>
        public virtual void Validate(List<string> problems)
        {
            if (viewPrefab == null)
            {
                problems.Add($"{name}: no view prefab assigned.");
            }
        }

        protected static void RequireLine(DialogueLine line, string label, string owner, List<string> problems)
        {
            if (line.IsEmpty)
            {
                problems.Add($"{owner}: '{label}' text is empty.");
            }
        }

        protected static void RequireLines(IReadOnlyList<DialogueLine> lines, string label, string owner, List<string> problems)
        {
            if (lines == null || lines.Count == 0)
            {
                problems.Add($"{owner}: '{label}' has no lines.");
                return;
            }

            for (var i = 0; i < lines.Count; i++)
            {
                RequireLine(lines[i], $"{label}[{i}]", owner, problems);
            }
        }
    }
}
