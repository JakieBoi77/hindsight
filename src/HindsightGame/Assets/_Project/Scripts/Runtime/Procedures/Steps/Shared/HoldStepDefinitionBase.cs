using System.Collections.Generic;
using UnityEngine;

namespace Hindsight.Procedures.Steps
{
    /// <summary>Common data for "press and hold to keep still" steps.</summary>
    public abstract class HoldStepDefinitionBase : StepDefinition
    {
        [SerializeField] private List<DialogueLine> introLines = new List<DialogueLine>();
        [SerializeField] private DialogueLine holdPrompt;
        [SerializeField, Min(0.5f)] private float holdSeconds = 3f;
        [Tooltip("Shown when the child lets go early. Keep it gentle: explain, then invite another try.")]
        [SerializeField] private DialogueLine interruptedLine;
        [SerializeField] private List<DialogueLine> successLines = new List<DialogueLine>();

        public IReadOnlyList<DialogueLine> IntroLines => introLines;

        public DialogueLine HoldPrompt => holdPrompt;

        public float HoldSeconds => holdSeconds;

        public DialogueLine InterruptedLine => interruptedLine;

        public IReadOnlyList<DialogueLine> SuccessLines => successLines;

        public override void Validate(List<string> problems)
        {
            base.Validate(problems);
            RequireLine(holdPrompt, nameof(holdPrompt), name, problems);
            RequireLine(interruptedLine, nameof(interruptedLine), name, problems);
            RequireLines(successLines, nameof(successLines), name, problems);
        }
    }
}
