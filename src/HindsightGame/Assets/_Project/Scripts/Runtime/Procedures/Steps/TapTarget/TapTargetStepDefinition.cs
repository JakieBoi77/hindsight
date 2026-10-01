using System.Collections.Generic;
using UnityEngine;

namespace Hindsight.Procedures.Steps
{
    /// <summary>The child taps something in the room, e.g. the exam chair to sit down.</summary>
    [CreateAssetMenu(menuName = "Hindsight/Steps/Tap Target", fileName = "TapTargetStep")]
    public sealed class TapTargetStepDefinition : StepDefinition
    {
        [SerializeField] private DialogueLine prompt;
        [SerializeField] private List<DialogueLine> successLines = new List<DialogueLine>();

        public DialogueLine Prompt => prompt;

        public IReadOnlyList<DialogueLine> SuccessLines => successLines;

        public override void Validate(List<string> problems)
        {
            base.Validate(problems);
            RequireLine(prompt, nameof(prompt), name, problems);
            RequireLines(successLines, nameof(successLines), name, problems);
        }
    }
}
