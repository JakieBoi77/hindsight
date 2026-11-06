namespace EyeCanDo.Core.Progress
{
    /// <summary>
    /// How many coins the child earns. The story pays most the first time; every finished game
    /// round pays a little more for more stars, and each star earned for the first time pays a
    /// bonus. Coins are only spent in the shop, and are never taken away for playing badly.
    /// </summary>
    public static class CoinRules
    {
        public const int StoryFirstTime = 40;
        public const int StoryAgain = 5;
        public const int RoundBase = 5;
        public const int PerStar = 2;
        public const int PerNewStar = 10;

        public static int ForStory(bool firstTime)
        {
            return firstTime ? StoryFirstTime : StoryAgain;
        }

        /// <param name="stars">Stars this round (1 to 3).</param>
        /// <param name="newStars">How many more stars than the previous best at this level.</param>
        public static int ForRound(int stars, int newStars)
        {
            return RoundBase + PerStar * System.Math.Max(0, stars) + PerNewStar * System.Math.Max(0, newStars);
        }
    }
}
