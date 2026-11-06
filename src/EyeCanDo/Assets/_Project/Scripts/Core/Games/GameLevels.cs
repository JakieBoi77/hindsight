using System;

namespace EyeCanDo.Core.Games
{
    public enum GameId
    {
        DropCatch,
        BlurrySpy,
    }

    /// <summary>Each game has three levels. Every level can be played at any time; nothing is locked.</summary>
    public static class Levels
    {
        public const int Min = 1;
        public const int Max = 3;

        public static int Clamp(int level)
        {
            return Math.Max(Min, Math.Min(Max, level));
        }
    }

    /// <summary>
    /// Stars for one finished round: three only for a perfect round, and always at least one for
    /// finishing, so a round can never be failed (FR_NoPenalty).
    /// </summary>
    public static class StarRules
    {
        public const int MaxStars = 3;
    }
}
