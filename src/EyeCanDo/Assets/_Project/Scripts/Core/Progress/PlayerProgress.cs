using System;
using System.Collections.Generic;
using Hindsight.EyeCanDo.Core.Avatar;

namespace Hindsight.EyeCanDo.Core.Progress
{
    /// <summary>
    /// One player's save file. Only gameplay data: no names or other identifying information.
    /// Device settings such as the volume are kept apart in <see cref="GameSettings"/>.
    /// Fields are public for JSON serialisation (ported and trimmed from the 2D PoC).
    /// </summary>
    [Serializable]
    public sealed class PlayerProgress
    {
        // 2: outfits, hairstyles and capes; 3: everyday outfits, no pants colour; 4: scout characters (faces, hats, fits, sashes); 5: no sashes
        public const int CurrentVersion = 7;

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

        /// <summary>Shop items that no longer exist (old outfits, hats, capes, sashes and face accessories), and what they cost.</summary>
        static readonly Dictionary<string, int> RetiredItems = new()
        {
            ["Outfit:explorer"] = 25,
            ["Outfit:adventurer"] = 35,
            ["Outfit:viking"] = 50,
            ["Hat:bow"] = 20,
            ["Hat:party"] = 30,
            ["Hat:bear"] = 50,
            ["Cape:travel"] = 30,
            ["Cape:fur"] = 40,
            ["Cape:wizard"] = 45,
            ["Cape:hero"] = 55,
            ["Sash:tadpole"] = 10,
            ["Sash:rabbit"] = 15,
            ["Sash:raccoon"] = 20,
            ["Sash:mule"] = 25,
            ["Sash:kangaroo"] = 30,
            ["Sash:owl"] = 35,
            ["Sash:wolf"] = 40,
            ["Sash:goat"] = 45,
            ["Sash:scoutmaster"] = 100,
            ["Accessory:roundglasses"] = 15,
            ["Accessory:aviators"] = 20,
            ["Accessory:facepaint"] = 15,
            ["Accessory:scar"] = 15,
            ["Accessory:bow"] = 15,
            ["Accessory:eyepatch"] = 20,
            ["Accessory:brokenglasses"] = 20,
            ["Accessory:disguise"] = 30,
            ["Accessory:shades"] = 25,
            ["Accessory:scar2"] = 15,
            ["Accessory:thirdeye"] = 40,
        };

        /// <summary>
        /// Brings a save from an earlier version up to date. Version 4 replaced the people with scout
        /// characters: bought items that still exist (the wizard hat, knight helmet, crown, knight
        /// outfit) are kept, retired ones are refunded in coins, and anything worn that no longer
        /// exists or was never bought (old skin tones, hairstyles) goes back to the default.
        /// </summary>
        public void Upgrade()
        {
            if (version < 2 || version >= CurrentVersion) return;
            owned ??= new List<string>();
            look ??= AvatarLook.Default;
            foreach (var key in owned.ToArray())
            {
                if (AvatarCatalog.FindKey(key) != null) continue;
                if (RetiredItems.TryGetValue(key, out var refund)) AddCoins(refund);
                owned.Remove(key);
            }

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
