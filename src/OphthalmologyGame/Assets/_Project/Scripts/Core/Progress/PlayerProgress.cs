using System;
using System.Collections.Generic;
using Hindsight.Ophthalmology.Core.Avatar;

namespace Hindsight.Ophthalmology.Core.Progress
{
    /// <summary>
    /// One player's save file. Only gameplay data: no names or other identifying information.
    /// Device settings such as the volume are kept apart in <see cref="GameSettings"/>.
    /// Fields are public for JSON serialisation (ported and trimmed from the 2D PoC).
    /// </summary>
    [Serializable]
    public sealed class PlayerProgress
    {
        public const int CurrentVersion = 3; // 2: outfits, hairstyles and capes; 3: everyday outfits, no pants colour

        public int version = CurrentVersion;

        /// <summary>Ids of levels finished at least once.</summary>
        public List<string> completedLevels = new();

        /// <summary>Ids of levels opened by finishing an earlier one.</summary>
        public List<string> unlockedLevels = new();

        /// <summary>Ids of stickers collected (one per level).</summary>
        public List<string> stickers = new();

        public int coins;

        /// <summary>Shop items bought, by key (such as "Hat:crown").</summary>
        public List<string> owned = new();

        /// <summary>The player's character. Starts as the default kid.</summary>
        public AvatarLook look = AvatarLook.Default;

        public bool IsCompleted(string levelId) => completedLevels.Contains(levelId);

        public bool IsUnlocked(string levelId, bool unlockedByDefault) => unlockedByDefault || unlockedLevels.Contains(levelId);

        public bool HasSticker(string levelId) => stickers.Contains(levelId);

        /// <summary>
        /// Marks a level finished, opens the next one, gives its sticker and pays its coins.
        /// Returns the coins paid (more the first time).
        /// </summary>
        public int FinishLevel(string levelId, string unlocksLevelId)
        {
            if (string.IsNullOrEmpty(levelId)) return 0;
            var firstTime = !completedLevels.Contains(levelId);
            if (firstTime) completedLevels.Add(levelId);
            if (!stickers.Contains(levelId)) stickers.Add(levelId);
            if (!string.IsNullOrEmpty(unlocksLevelId) && !unlockedLevels.Contains(unlocksLevelId)) unlockedLevels.Add(unlocksLevelId);
            var paid = CoinRules.ForLevel(firstTime);
            AddCoins(paid);
            return paid;
        }

        public void AddCoins(int amount) => coins = Math.Max(0, coins + Math.Max(0, amount));

        public bool Owns(AvatarItem item) => item != null && (item.IsFree || owned.Contains(item.Key));

        public bool CanAfford(AvatarItem item) => item != null && coins >= item.Price;

        /// <summary>Buys an item if it is not owned and the child has enough coins.</summary>
        public bool TryBuy(AvatarItem item)
        {
            if (item == null || Owns(item) || !CanAfford(item)) return false;
            coins -= item.Price;
            owned.Add(item.Key);
            return true;
        }

        /// <summary>Wears an item. Only free or bought items can be worn.</summary>
        public bool Equip(Slot slot, string id)
        {
            var item = AvatarCatalog.Find(slot, id);
            if (!Owns(item)) return false;
            look.Set(slot, id);
            return true;
        }

        /// <summary>
        /// Brings a save from the previous version up to date: anything worn that is no longer free
        /// (the explorer outfit used to be) and was never bought goes back to the default.
        /// </summary>
        public void Upgrade()
        {
            if (version != 2 || look == null) return;
            var fresh = AvatarLook.Default;
            foreach (Slot slot in Enum.GetValues(typeof(Slot)))
                if (!Owns(AvatarCatalog.Find(slot, look.Get(slot)))) look.Set(slot, fresh.Get(slot));
            version = CurrentVersion;
        }

        public bool IsValid()
        {
            if (version != CurrentVersion || completedLevels == null || unlockedLevels == null || stickers == null
                || coins < 0 || owned == null || look == null || !look.IsValid())
                return false;

            foreach (var key in owned)
                if (AvatarCatalog.FindKey(key) == null) return false;

            foreach (Slot slot in Enum.GetValues(typeof(Slot)))
                if (!Owns(AvatarCatalog.Find(slot, look.Get(slot)))) return false;

            return true;
        }
    }
}
