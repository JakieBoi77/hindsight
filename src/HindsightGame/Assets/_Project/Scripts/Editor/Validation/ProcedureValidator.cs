using System.Collections.Generic;
using Hindsight.Procedures;
using UnityEditor;
using UnityEngine;

namespace Hindsight.Editor.Validation
{
    /// <summary>
    /// Checks authored procedure data for missing prefabs, empty dialogue and broken quizzes.
    /// Also exercised by an EditMode test so CI catches content mistakes.
    /// </summary>
    public static class ProcedureValidator
    {
        [MenuItem("Hindsight/Validate Procedure Content", priority = 20)]
        public static void ValidateAllFromMenu()
        {
            var problems = new List<string>();
            foreach (var catalog in FindAllCatalogs())
            {
                problems.AddRange(Collect(catalog));
            }

            if (problems.Count == 0)
            {
                Debug.Log("[Hindsight] Procedure content is valid.");
                return;
            }

            foreach (var problem in problems)
            {
                Debug.LogError("[Hindsight] " + problem);
            }
        }

        public static List<string> Collect(ProcedureCatalog catalog)
        {
            var problems = new List<string>();
            catalog.Validate(problems);
            return problems;
        }

        public static List<ProcedureCatalog> FindAllCatalogs()
        {
            var catalogs = new List<ProcedureCatalog>();
            foreach (var guid in AssetDatabase.FindAssets("t:" + nameof(ProcedureCatalog)))
            {
                catalogs.Add(AssetDatabase.LoadAssetAtPath<ProcedureCatalog>(AssetDatabase.GUIDToAssetPath(guid)));
            }

            return catalogs;
        }
    }
}
