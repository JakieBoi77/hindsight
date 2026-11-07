namespace Hindsight.Ophthalmology.Core.Progress
{
    /// <summary>
    /// How many coins the child earns. A level pays most the first time and a little on replays. Coins are only spent in
    /// the dress-up shop and are never taken away.
    /// </summary>
    public static class CoinRules
    {
        public const int LevelFirstTime = 40;
        public const int LevelAgain = 5;

        public static int ForLevel(bool firstTime) => firstTime ? LevelFirstTime : LevelAgain;
    }
}
