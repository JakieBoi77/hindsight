using System.Collections.Generic;
using UnityEngine;

namespace Hindsight.Procedures
{
    /// <summary>Ordered list of levels shown on the level select screen.</summary>
    [CreateAssetMenu(menuName = "Hindsight/Procedure Catalog", fileName = "ProcedureCatalog")]
    public sealed class ProcedureCatalog : ScriptableObject
    {
        [SerializeField] private List<ProcedureDefinition> procedures = new List<ProcedureDefinition>();

        public IReadOnlyList<ProcedureDefinition> Procedures => procedures;

        public void Validate(List<string> problems)
        {
            var ids = new HashSet<string>();
            foreach (var procedure in procedures)
            {
                if (procedure == null)
                {
                    problems.Add($"{name}: contains an empty entry.");
                    continue;
                }

                if (!ids.Add(procedure.Id))
                {
                    problems.Add($"{name}: duplicate procedure id '{procedure.Id}'.");
                }

                procedure.Validate(problems);
            }
        }
    }
}
