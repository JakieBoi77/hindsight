using System.Collections.Generic;
using UnityEngine;

namespace Hindsight.Procedures.Steps
{
    /// <summary>Shows what the drops do: tap the dropper and watch the pupil grow.</summary>
    [CreateAssetMenu(menuName = "Hindsight/Steps/Dilation Demo", fileName = "DilationDemoStep")]
    public sealed class DilationDemoStepDefinition : StepDefinition
    {
        [SerializeField] private DialogueLine tapPrompt;
        [SerializeField] private List<DialogueLine> explanationLines = new List<DialogueLine>();
        [SerializeField, Range(0.1f, 1f)] private float startPupilScale = 0.35f;
        [SerializeField, Range(0.1f, 1f)] private float endPupilScale = 0.85f;
        [SerializeField, Min(0.1f)] private float dilationSeconds = 2.5f;

        public DialogueLine TapPrompt => tapPrompt;

        public IReadOnlyList<DialogueLine> ExplanationLines => explanationLines;

        public float StartPupilScale => startPupilScale;

        public float EndPupilScale => endPupilScale;

        public float DilationSeconds => dilationSeconds;

        public override void Validate(List<string> problems)
        {
            base.Validate(problems);
            RequireLine(tapPrompt, nameof(tapPrompt), name, problems);
            RequireLines(explanationLines, nameof(explanationLines), name, problems);
            if (endPupilScale <= startPupilScale)
            {
                problems.Add($"{name}: end pupil scale must be larger than the start scale.");
            }
        }
    }
}
