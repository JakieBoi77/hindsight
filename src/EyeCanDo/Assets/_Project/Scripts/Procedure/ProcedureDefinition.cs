using System.Collections.Generic;
using UnityEngine;

namespace Hindsight.EyeCanDo.Procedure
{
    /// <summary>An ordered list of steps that make up one clinic visit.</summary>
    [CreateAssetMenu(menuName = "EyeCanDo/Procedure Definition", fileName = "Procedure")]
    public sealed class ProcedureDefinition : ScriptableObject
    {
        [SerializeField] string displayName = "";
        [SerializeField] List<ProcedureStep> steps = new();

        public string DisplayName => displayName;
        public IReadOnlyList<ProcedureStep> Steps => steps;

#if UNITY_EDITOR
        public void EditorInit(string name, List<ProcedureStep> value)
        {
            displayName = name;
            steps = value;
        }
#endif
    }
}
