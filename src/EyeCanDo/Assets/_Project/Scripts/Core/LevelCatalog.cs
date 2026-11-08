using System.Collections.Generic;
using UnityEngine;

namespace Hindsight.Ophthalmology.Core
{
    /// <summary>Ordered list of every level shown on the level select screen.</summary>
    [CreateAssetMenu(menuName = "Ophthalmology/Level Catalog", fileName = "LevelCatalog")]
    public sealed class LevelCatalog : ScriptableObject
    {
        [SerializeField] List<LevelDefinition> levels = new();

        public IReadOnlyList<LevelDefinition> Levels => levels;

#if UNITY_EDITOR
        public void EditorSetLevels(List<LevelDefinition> value) => levels = value;
#endif
    }
}
