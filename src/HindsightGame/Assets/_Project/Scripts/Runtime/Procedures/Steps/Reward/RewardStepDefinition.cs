using System.Collections.Generic;
using UnityEngine;

namespace Hindsight.Procedures.Steps
{
    /// <summary>Celebration and sticker at the end of a procedure. Completing it records progress.</summary>
    [CreateAssetMenu(menuName = "Hindsight/Steps/Reward", fileName = "RewardStep")]
    public sealed class RewardStepDefinition : StepDefinition
    {
        [SerializeField] private string headline = "You did it!";
        [SerializeField] private DialogueLine celebrationLine;
        [SerializeField] private string finishLabel = "Finish";

        public string Headline => headline;

        public DialogueLine CelebrationLine => celebrationLine;

        public string FinishLabel => finishLabel;

        public override bool MarksProcedureComplete => true;

        public override void Validate(List<string> problems)
        {
            base.Validate(problems);
            RequireLine(celebrationLine, nameof(celebrationLine), name, problems);
        }
    }
}
