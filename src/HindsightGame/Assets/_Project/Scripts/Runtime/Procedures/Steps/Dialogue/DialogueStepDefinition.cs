using System.Collections.Generic;
using UnityEngine;

namespace Hindsight.Procedures.Steps
{
    /// <summary>The doctor talks through one or more lines, optionally beside an illustration.</summary>
    [CreateAssetMenu(menuName = "Hindsight/Steps/Dialogue", fileName = "DialogueStep")]
    public sealed class DialogueStepDefinition : StepDefinition
    {
        [SerializeField] private Sprite illustration;
        [SerializeField] private List<DialogueLine> lines = new List<DialogueLine>();

        public Sprite Illustration => illustration;

        public IReadOnlyList<DialogueLine> Lines => lines;

        public override void Validate(List<string> problems)
        {
            base.Validate(problems);
            RequireLines(lines, nameof(lines), name, problems);
        }
    }
}
