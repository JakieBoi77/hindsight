using UnityEngine;

namespace Hindsight.Ophthalmology.Core
{
    /// <summary>One clinic visit the player can pick from the level select screen.</summary>
    [CreateAssetMenu(menuName = "Ophthalmology/Level Definition", fileName = "Level")]
    public sealed class LevelDefinition : ScriptableObject
    {
        [SerializeField] string levelId = "eye-drops";
        [SerializeField] string displayName = "Eye Drops";
        [SerializeField] Sprite icon;
        [SerializeField, TextArea] string blurb = "";
        [Tooltip("Scene to load. Empty means the level is not built yet.")]
        [SerializeField] string sceneName = "";
        [SerializeField] bool unlockedByDefault;
        [Tooltip("Completing this level unlocks the level listed here.")]
        [SerializeField] LevelDefinition unlocksOnComplete;

        public string LevelId => levelId;
        public string DisplayName => displayName;
        public Sprite Icon => icon;
        public string Blurb => blurb;
        public string SceneName => sceneName;
        public bool UnlockedByDefault => unlockedByDefault;
        public LevelDefinition UnlocksOnComplete => unlocksOnComplete;
        public bool IsPlayable => !string.IsNullOrEmpty(sceneName);

#if UNITY_EDITOR
        public void EditorInit(string id, string name, string text, string scene, bool unlocked, LevelDefinition next, Sprite iconSprite)
        {
            icon = iconSprite;
            levelId = id;
            displayName = name;
            blurb = text;
            sceneName = scene;
            unlockedByDefault = unlocked;
            unlocksOnComplete = next;
        }
#endif
    }
}
