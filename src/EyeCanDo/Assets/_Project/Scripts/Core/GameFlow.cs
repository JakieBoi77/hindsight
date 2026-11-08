using UnityEngine;
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
            ResumeTime();
            CurrentLevel = level;
            SceneManager.LoadScene(level.SceneName);
        }

        /// <summary>Plays the current level again from the start.</summary>
        public static void ReplayLevel()
        {
            ResumeTime();
            if (CurrentLevel != null) LoadLevel(CurrentLevel);
            else SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public static void ReturnToLevelSelect()
        {
            ResumeTime();
            CurrentLevel = null;
            OpenLevelSelectOnMenuLoad = true;
            SceneManager.LoadScene(MenuScene);
        }

        /// <summary>Back to the title screen (after a full reset).</summary>
        public static void ReturnToTitle()
        {
            ResumeTime();
            CurrentLevel = null;
            OpenLevelSelectOnMenuLoad = false;
            SceneManager.LoadScene(MenuScene);
        }

        public static void ConsumeMenuRequest() => OpenLevelSelectOnMenuLoad = false;

        /// <summary>Undoes a pause from the settings panel before changing scene.</summary>
        static void ResumeTime()
        {
            Time.timeScale = 1f;
            AudioListener.pause = false;
        }
    }
}
