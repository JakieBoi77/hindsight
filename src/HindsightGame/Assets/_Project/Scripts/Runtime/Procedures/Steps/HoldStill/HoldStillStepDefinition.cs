using UnityEngine;

namespace Hindsight.Procedures.Steps
{
    /// <summary>
    /// The eye drop itself: head tilted back, eyes open. Holding the button keeps the eye open
    /// while the drop falls; letting go is a blink and the drop misses.
    /// </summary>
    [CreateAssetMenu(menuName = "Hindsight/Steps/Hold Still (Eye Drop)", fileName = "HoldStillStep")]
    public sealed class HoldStillStepDefinition : HoldStepDefinitionBase
    {
        [SerializeField] private string sensationCaption = "Brrr! Cold!";

        /// <summary>Short caption shown when the drop lands, reinforcing that cold is expected.</summary>
        public string SensationCaption => sensationCaption;
    }
}
