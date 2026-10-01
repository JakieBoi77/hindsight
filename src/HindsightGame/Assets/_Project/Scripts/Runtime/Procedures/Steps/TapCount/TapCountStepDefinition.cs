using System.Collections.Generic;
using UnityEngine;

namespace Hindsight.Procedures.Steps
{
    /// <summary>Repeated taps on a target, e.g. "blink lots to spread the drops around".</summary>
    [CreateAssetMenu(menuName = "Hindsight/Steps/Tap Count (Blink)", fileName = "TapCountStep")]
    public sealed class TapCountStepDefinition : StepDefinition
    {
        [SerializeField] private DialogueLine prompt;
        [SerializeField, Range(1, 10)] private int requiredTaps = 5;
        [SerializeField] private List<DialogueLine> successLines = new List<DialogueLine>();

        public DialogueLine Prompt => prompt;

        public int RequiredTaps => requiredTaps;

        public IReadOnlyList<DialogueLine> SuccessLines => successLines;

        public override void Validate(List<string> problems)
        {
            base.Validate(problems);
            RequireLine(prompt, nameof(prompt), name, problems);
            RequireLines(successLines, nameof(successLines), name, problems);
        }
    }
}
