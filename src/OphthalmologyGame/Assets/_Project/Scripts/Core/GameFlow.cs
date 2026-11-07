using UnityEngine.SceneManagement;

namespace Hindsight.Ophthalmology.Core
{
    /// <summary>Scene navigation. Keeps scene names in one place.</summary>
    public static class GameFlow
    {
        public const string MenuScene = "Menu";

        /// <summary>Set when returning from a level so the menu opens on level select instead of the title.</summary>
        public static bool OpenLevelSelectOnMenuLoad { get; private set; }

        /// <summary>The level currently being played (null in the menu).</summary>
        public static LevelDefinition CurrentLevel { get; private set; }

        public static void LoadLevel(LevelDefinition level)
        {
            if (level == null || !level.IsPlayable) return;
            CurrentLevel = level;
            SceneManager.LoadScene(level.SceneName);
        }

        public static void ReturnToLevelSelect()
        {
            CurrentLevel = null;
            OpenLevelSelectOnMenuLoad = true;
            SceneManager.LoadScene(MenuScene);
        }

        public static void ConsumeMenuRequest() => OpenLevelSelectOnMenuLoad = false;
    }
}
