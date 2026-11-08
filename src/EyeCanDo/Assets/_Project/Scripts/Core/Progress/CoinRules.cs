using System;

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

        /// <summary>
        /// In the editor and development builds every save is topped up to this many coins, so the
        /// whole shop can be tried. Release builds start at zero and only earn coins from levels.
        /// </summary>
        public const int DevCoins = 1000;

        /// <summary>The coins a save should have when loaded: topped up in dev builds, unchanged otherwise.</summary>
        public static int WithDevTopUp(int coins, bool devBuild) => devBuild ? Math.Max(coins, DevCoins) : coins;
    }
}
