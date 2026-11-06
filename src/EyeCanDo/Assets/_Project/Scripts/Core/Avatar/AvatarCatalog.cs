using System;
using System.Collections.Generic;

namespace EyeCanDo.Core.Avatar
{
    /// <summary>Each part of the character the player can change.</summary>
    public enum Slot
    {
        Skin,
        Ears,
        EyeStyle,
        EyeColor,
        Brows,
        Nose,
        Mouth,
        HairStyle,
        HairColor,
        ShirtColor,
        Emblem,
        ShortsColor,
        ShoesColor,
        Hat,
        Glasses,
        Held,
    }

    /// <summary>One choice for a slot: a style id or a colour (#RRGGBB). Price 0 is free.</summary>
    public sealed class AvatarItem
    {
        public AvatarItem(Slot slot, string id, int price = 0)
        {
            Slot = slot;
            Id = id;
            Price = price;
        }

        public Slot Slot { get; }

        public string Id { get; }

        public int Price { get; }

        public bool IsFree => Price == 0;

        /// <summary>Key used to remember ownership, such as "Hat:crown".</summary>
        public string Key => Slot + ":" + Id;

        public bool IsColor => Id.StartsWith("#", StringComparison.Ordinal);
    }

    /// <summary>
    /// Everything a character can wear or look like. Faces, hair and most colours are free; the
    /// shop sells special colours, shirt pictures, hats, glasses and things to hold for coins.
    /// </summary>
    public static class AvatarCatalog
    {
        public const string None = "none";

        private static readonly List<AvatarItem> Items = new List<AvatarItem>();

        static AvatarCatalog()
        {
            Free(Slot.Skin, "#FBE0CF", "#F5CBA7", "#F2C69B", "#E8B98A", "#D99B6C", "#C68A5E", "#A86B45", "#8D5A3B", "#6B4128");
            Free(Slot.Ears, "round", "small", "big", "pointy");
            Free(Slot.EyeStyle, "round", "big", "lashes", "dots");
            Free(Slot.EyeColor, "#6B4A2B", "#4A3426", "#4A90E2", "#3E9E6E", "#8FA9B8", "#9B6BE0", "#2BB3A3");
            Free(Slot.Brows, "soft", "thick", "straight", "arched", None);
            Free(Slot.Nose, "button", "dot", "curve", "round");
            Free(Slot.Mouth, "smile", "big", "cheeky", "tongue");
            Free(Slot.HairStyle, "tuft", "curly", "bowl", "pigtails", "puffs", "long", "spiky", "bun", "buzz", None);
            Free(Slot.HairColor, "#2B1B17", "#4A2C1D", "#7A4A2A", "#A0522D", "#D8692E", "#E8C26A", "#F1E0B0", "#8C8C8C", "#5B6CC8", "#E77BB0");
            Free(Slot.ShirtColor, "#FFC845", "#FF6B6B", "#5CBF73", "#4A90E2", "#9B6BE0", "#FF9BC2", "#2BB3A3", "#FFFFFF", "#3D4A6B");
            Free(Slot.ShortsColor, "#2F6DB5", "#3D4A6B", "#9B6BE0", "#2BB3A3", "#FF9BC2", "#D9A066", "#5CBF73");
            Free(Slot.ShoesColor, "#FF6B6B", "#FFC845", "#4A90E2", "#9B6BE0", "#FF9BC2", "#FFFFFF", "#3D4A6B");

            // Special colours cost a few coins; they sit after the free ones in the same row.
            Paid(Slot.HairColor, ("#7EE0C3", 15), ("#C9A0FF", 15), ("#3FA9F5", 15), ("#FF4F79", 20), ("#FFD34D", 20));
            Paid(Slot.EyeColor, ("#E0A526", 15), ("#E85A9A", 15));
            Paid(Slot.ShirtColor, ("#FFD34D", 15), ("#7EE0C3", 15), ("#FF9F43", 15));
            Paid(Slot.ShoesColor, ("#FFD34D", 15), ("#2BB3A3", 15));

            Shop(Slot.Emblem, ("star", 15), ("heart", 15), ("smile", 15), ("flower", 20), ("paw", 20), ("eye", 20), ("fish", 20), ("cat", 25),
                ("sun", 25), ("moon", 25), ("music", 25), ("rainbow", 25), ("bolt", 30), ("butterfly", 30), ("dino", 30), ("rocket", 30), ("planet", 35));
            Shop(Slot.Hat, ("cap", 30), ("beanie", 30), ("bow", 30), ("bucket", 35), ("flowers", 40), ("party", 40), ("catears", 45), ("bunny", 45),
                ("headphones", 50), ("chef", 50), ("cowboy", 55), ("pirate", 55), ("propeller", 60), ("tiara", 65), ("viking", 65), ("crown", 70),
                ("wizard", 70), ("unicorn", 80));
            Shop(Slot.Glasses, ("round", 25), ("square", 25), ("heart", 35), ("star", 35), ("cateye", 35), ("threed", 40), ("goggles", 40),
                ("sparkle", 45), ("shades", 45), ("aviator", 50), ("mask", 55));
            Shop(Slot.Held, ("balloon", 25), ("flag", 25), ("lollipop", 25), ("sunflower", 30), ("book", 30), ("kite", 35), ("icecream", 35),
                ("bubbles", 35), ("magnifier", 40), ("teddy", 40), ("umbrella", 40), ("dino", 50), ("wand", 50), ("rocket", 55), ("guitar", 60),
                ("trophy", 75));
        }

        public static IReadOnlyList<AvatarItem> All => Items;

        public static IEnumerable<AvatarItem> For(Slot slot)
        {
            foreach (var item in Items)
            {
                if (item.Slot == slot)
                {
                    yield return item;
                }
            }
        }

        public static AvatarItem Find(Slot slot, string id)
        {
            foreach (var item in Items)
            {
                if (item.Slot == slot && item.Id == id)
                {
                    return item;
                }
            }

            return null;
        }

        public static AvatarItem FindKey(string key)
        {
            foreach (var item in Items)
            {
                if (item.Key == key)
                {
                    return item;
                }
            }

            return null;
        }

        /// <summary>Slots whose "none" choice means nothing is worn.</summary>
        public static bool IsOptional(Slot slot)
        {
            return slot == Slot.Emblem || slot == Slot.Hat || slot == Slot.Glasses || slot == Slot.Held;
        }

        private static void Free(Slot slot, params string[] ids)
        {
            foreach (var id in ids)
            {
                Items.Add(new AvatarItem(slot, id));
            }
        }

        private static void Paid(Slot slot, params (string id, int price)[] items)
        {
            foreach (var (id, price) in items)
            {
                Items.Add(new AvatarItem(slot, id, price));
            }
        }

        private static void Shop(Slot slot, params (string id, int price)[] items)
        {
            Items.Add(new AvatarItem(slot, None));
            foreach (var (id, price) in items)
            {
                Items.Add(new AvatarItem(slot, id, price));
            }
        }
    }
}
