using UnityEngine;

namespace Hindsight.Ophthalmology.Core
{
    /// <summary>Saves which levels are unlocked/completed. PlayerPrefs is enough for the POC.</summary>
    public static class LevelProgress
    {
        const string UnlockedPrefix = "level.unlocked.";
        const string CompletedPrefix = "level.completed.";

        public static bool IsUnlocked(LevelDefinition level) =>
            level.UnlockedByDefault || PlayerPrefs.GetInt(UnlockedPrefix + level.LevelId, 0) == 1;

        public static bool IsCompleted(LevelDefinition level) =>
            PlayerPrefs.GetInt(CompletedPrefix + level.LevelId, 0) == 1;

        public static void MarkCompleted(LevelDefinition level)
        {
            if (level == null) return;
            PlayerPrefs.SetInt(CompletedPrefix + level.LevelId, 1);
            if (level.UnlocksOnComplete != null)
                PlayerPrefs.SetInt(UnlockedPrefix + level.UnlocksOnComplete.LevelId, 1);
            PlayerPrefs.Save();
        }
    }
}
