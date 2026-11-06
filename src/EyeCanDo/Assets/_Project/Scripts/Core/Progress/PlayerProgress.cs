using System;
using System.Collections.Generic;
using EyeCanDo.Core.Avatar;
using EyeCanDo.Core.Games;

namespace EyeCanDo.Core.Progress
{
    public enum StopStatus
    {
        NotStarted,
        InProgress,
        Completed,
    }

    /// <summary>Best results for one game, per level. Bests only ever go up (thm_NoLoss).</summary>
    [Serializable]
    public sealed class GameRecord
    {
        /// <summary>Best stars at each level. Index 0 is level 1.</summary>
        public int[] bestStars = new int[Levels.Max];

        public int[] bestScore = new int[Levels.Max];

        public int plays;

        public int TotalStars
        {
            get
            {
                var total = 0;
                foreach (var stars in bestStars)
                {
                    total += stars;
                }

                return total;
            }
        }

        public int StarsAt(int level)
        {
            return bestStars[Levels.Clamp(level) - 1];
        }

        public int ScoreAt(int level)
        {
            return bestScore[Levels.Clamp(level) - 1];
        }

        /// <summary>The level to suggest next: the first one without two stars yet, otherwise the top level.</summary>
        public int SuggestedLevel
        {
            get
            {
                for (var level = Levels.Min; level <= Levels.Max; level++)
                {
                    if (StarsAt(level) < 2)
                    {
                        return level;
                    }
                }

                return Levels.Max;
            }
        }

        /// <summary>Records a finished round and says which bests it beat.</summary>
        public RoundRecord Record(int level, int stars, int score)
        {
            var index = Levels.Clamp(level) - 1;
            stars = Math.Max(0, Math.Min(StarRules.MaxStars, stars));
            score = Math.Max(0, score);
            plays++;
            var result = new RoundRecord(stars > bestStars[index], score > bestScore[index], Math.Max(0, stars - bestStars[index]));
            bestStars[index] = Math.Max(bestStars[index], stars);
            bestScore[index] = Math.Max(bestScore[index], score);
            return result;
        }

        public bool IsValid()
        {
            if (bestStars == null || bestScore == null || bestStars.Length != Levels.Max || bestScore.Length != Levels.Max || plays < 0)
            {
                return false;
            }

            for (var i = 0; i < Levels.Max; i++)
            {
                if (bestStars[i] < 0 || bestStars[i] > StarRules.MaxStars || bestScore[i] < 0)
                {
                    return false;
                }
            }

            return true;
        }
    }

    public readonly struct RoundRecord
    {
        public RoundRecord(bool newBestStars, bool newBestScore, int newStars)
        {
            NewBestStars = newBestStars;
            NewBestScore = newBestScore;
            NewStars = newStars;
        }

        /// <summary>Stars earned at this level for the first time.</summary>
        public int NewStars { get; }

        public bool NewBestStars { get; }

        public bool NewBestScore { get; }
    }

    /// <summary>
    /// One player's save file. Only gameplay data: no names or other identifying information
    /// (C_PatientPrivacy, NFR_Minimization). Device settings such as the volume are kept apart in
    /// <see cref="GameSettings"/>. Fields are public for JSON serialisation.
    /// </summary>
    [Serializable]
    public sealed class PlayerProgress
    {
        public const int CurrentVersion = 3;

        public int version = CurrentVersion;

        public StopStatus eyeDropsStory = StopStatus.NotStarted;

        public bool eyeDropsSticker;

        public GameRecord dropCatch = new GameRecord();

        public GameRecord blurrySpy = new GameRecord();

        public int coins;

        /// <summary>Shop items bought, by key (such as "Hat:crown").</summary>
        public List<string> owned = new List<string>();

        /// <summary>The player's character. Starts as the default kid.</summary>
        public AvatarLook look = AvatarLook.Default;

        /// <summary>The games open once the story has been finished.</summary>
        public bool GamesUnlocked => eyeDropsStory == StopStatus.Completed;

        public GameRecord For(GameId game)
        {
            return game == GameId.DropCatch ? dropCatch : blurrySpy;
        }

        /// <summary>Opening the story marks it In Progress unless it was already completed (FR_StartLevel).</summary>
        public void StartStory()
        {
            if (eyeDropsStory == StopStatus.NotStarted)
            {
                eyeDropsStory = StopStatus.InProgress;
            }
        }

        /// <summary>
        /// Marks the story Completed and pays its coins. Returns true only the first time, when the
        /// sticker is given (FR_FirstReward).
        /// </summary>
        public bool FinishStory()
        {
            eyeDropsStory = StopStatus.Completed;
            var firstTime = !eyeDropsSticker;
            eyeDropsSticker = true;
            AddCoins(CoinRules.ForStory(firstTime));
            return firstTime;
        }

        /// <summary>Records a finished game round, pays its coins and returns what changed.</summary>
        public RoundResult FinishRound(GameId game, int level, int stars, int score)
        {
            var record = For(game).Record(level, stars, score);
            var coinsEarned = CoinRules.ForRound(stars, record.NewStars);
            AddCoins(coinsEarned);
            return new RoundResult(record, coinsEarned);
        }

        public void AddCoins(int amount)
        {
            coins = Math.Max(0, coins + Math.Max(0, amount));
        }

        public bool Owns(AvatarItem item)
        {
            return item != null && (item.IsFree || owned.Contains(item.Key));
        }

        public bool CanAfford(AvatarItem item)
        {
            return item != null && coins >= item.Price;
        }

        /// <summary>Buys an item if it is not owned and the child has enough coins.</summary>
        public bool TryBuy(AvatarItem item)
        {
            if (item == null || Owns(item) || !CanAfford(item))
            {
                return false;
            }

            coins -= item.Price;
            owned.Add(item.Key);
            return true;
        }

        /// <summary>Wears an item. Only free or bought items can be worn.</summary>
        public bool Equip(Slot slot, string id)
        {
            var item = AvatarCatalog.Find(slot, id);
            if (!Owns(item))
            {
                return false;
            }

            look.Set(slot, id);
            return true;
        }

        public bool IsValid()
        {
            if (version != CurrentVersion
                || !Enum.IsDefined(typeof(StopStatus), eyeDropsStory)
                || (eyeDropsSticker && eyeDropsStory != StopStatus.Completed)
                || dropCatch == null || !dropCatch.IsValid()
                || blurrySpy == null || !blurrySpy.IsValid()
                || coins < 0 || owned == null || look == null || !look.IsValid())
            {
                return false;
            }

            foreach (var key in owned)
            {
                if (AvatarCatalog.FindKey(key) == null)
                {
                    return false;
                }
            }

            foreach (Slot slot in Enum.GetValues(typeof(Slot)))
            {
                if (!Owns(AvatarCatalog.Find(slot, look.Get(slot))))
                {
                    return false;
                }
            }

            return true;
        }
    }

    public readonly struct RoundResult
    {
        public RoundResult(RoundRecord record, int coins)
        {
            Record = record;
            Coins = coins;
        }

        public RoundRecord Record { get; }

        public int Coins { get; }
    }
}
