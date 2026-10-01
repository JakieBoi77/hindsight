namespace Hindsight.Core.Progress
{
    public enum LevelAvailability
    {
        /// <summary>Content exists but its prerequisite has not been completed.</summary>
        Locked,

        /// <summary>Content has not been built yet.</summary>
        ComingSoon,

        Unlocked,
        Completed,
    }

    public static class UnlockRules
    {
        /// <param name="procedureId">Stable id of the level being checked.</param>
        /// <param name="isPlayable">False for levels whose content does not exist yet.</param>
        /// <param name="requiredProcedureId">Level that must be completed first, or null/empty for none.</param>
        public static LevelAvailability Evaluate(string procedureId, bool isPlayable, string requiredProcedureId, PlayerProgress progress)
        {
            if (!isPlayable)
            {
                return LevelAvailability.ComingSoon;
            }

            if (progress.IsCompleted(procedureId))
            {
                return LevelAvailability.Completed;
            }

            if (!string.IsNullOrEmpty(requiredProcedureId) && !progress.IsCompleted(requiredProcedureId))
            {
                return LevelAvailability.Locked;
            }

            return LevelAvailability.Unlocked;
        }

        public static bool CanPlay(LevelAvailability availability)
        {
            return availability == LevelAvailability.Unlocked || availability == LevelAvailability.Completed;
        }
    }
}
