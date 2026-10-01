using System.Collections.Generic;
using UnityEngine;

namespace Hindsight.Procedures
{
    /// <summary>A playable level: one clinic experience broken into ordered steps.</summary>
    [CreateAssetMenu(menuName = "Hindsight/Procedure", fileName = "Procedure")]
    public sealed class ProcedureDefinition : ScriptableObject
    {
        [Tooltip("Stable save-game id. Never change once shipped.")]
        [SerializeField] private string id;
        [SerializeField] private string displayName;
        [SerializeField] private Sprite icon;
        [SerializeField] private Color accentColor = Color.white;

        [Tooltip("False for levels shown as 'coming soon' whose content is not built yet.")]
        [SerializeField] private bool isPlayable;
        [SerializeField] private ProcedureDefinition requiredProcedure;

        [SerializeField] private string stickerId;
        [SerializeField] private Sprite stickerSprite;
        [SerializeField] private List<StepDefinition> steps = new List<StepDefinition>();

        public string Id => id;

        public string DisplayName => displayName;

        public Sprite Icon => icon;

        public Color AccentColor => accentColor;

        public bool IsPlayable => isPlayable;

        public ProcedureDefinition RequiredProcedure => requiredProcedure;

        public string StickerId => stickerId;

        public Sprite StickerSprite => stickerSprite;

        public IReadOnlyList<StepDefinition> Steps => steps;

        public void Validate(List<string> problems)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                problems.Add($"{name}: id is empty.");
            }

            if (string.IsNullOrWhiteSpace(displayName))
            {
                problems.Add($"{name}: display name is empty.");
            }

            if (!isPlayable)
            {
                return;
            }

            if (steps.Count == 0)
            {
                problems.Add($"{name}: playable procedure has no steps.");
            }

            for (var i = 0; i < steps.Count; i++)
            {
                if (steps[i] == null)
                {
                    problems.Add($"{name}: step {i} is missing.");
                    continue;
                }

                steps[i].Validate(problems);
            }
        }
    }
}
