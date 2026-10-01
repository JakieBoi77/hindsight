using System.Collections.Generic;
using UnityEngine;

namespace Hindsight.Procedures.Steps
{
    /// <summary>The doctor looks into the eye with a light (ophthalmoscope); the child keeps still.</summary>
    [CreateAssetMenu(menuName = "Hindsight/Steps/Examine", fileName = "ExamineStep")]
    public sealed class ExamineStepDefinition : HoldStepDefinitionBase
    {
        [SerializeField] private DialogueLine tapToolPrompt;

        public DialogueLine TapToolPrompt => tapToolPrompt;

        public override void Validate(List<string> problems)
        {
            base.Validate(problems);
            RequireLine(tapToolPrompt, nameof(tapToolPrompt), name, problems);
        }
    }
}
