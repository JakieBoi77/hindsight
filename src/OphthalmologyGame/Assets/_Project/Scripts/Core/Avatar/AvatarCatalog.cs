using System;
using System.Collections.Generic;

namespace Hindsight.Ophthalmology.Core.Avatar
{
    /// <summary>Each part of the 3D kid the player can change.</summary>
    public enum Slot
    {
        Skin,
        HairStyle,
        Hair,
        Outfit,
        Shirt,
        Shoes,
        Hat,
        Cape,
    }

    /// <summary>One choice for a slot: a style id or a colour (#RRGGBB). Price 0 is free.</summary>
    public sealed class AvatarItem
    {
        public AvatarItem(Slot slot, string id, int price = 0, string label = null)
        {
            Slot = slot;
            Id = id;
            Price = price;
            Label = label ?? id;
        }

        public Slot Slot { get; }
        public string Id { get; }
        public int Price { get; }

        /// <summary>Short name shown on the shop card.</summary>
        public string Label { get; }

        public bool IsFree => Price == 0;

        /// <summary>Key used to remember ownership, such as "Hat:crown".</summary>
        public string Key => Slot + ":" + Id;

        public bool IsColor => Id.StartsWith("#", StringComparison.Ordinal);
    }

    /// <summary>
    /// Everything the kid can look like or wear. Every colour, skin tone, hairstyle and everyday outfit is free;
    /// the dressing-up outfits, hats and capes are bought with coins. Style ids match the child objects the prefab builder
    /// makes: "Outfit_knight_Body", "HairStyle_long", "Hat_crown", "Cape_wizard".
    /// </summary>
    public static class AvatarCatalog
    {
        public const string None = "none";

        static readonly List<AvatarItem> Items = new();

        static AvatarCatalog()
        {
            Free(Slot.Skin, "#FBE0CF", "#F2C69B", "#E8B98A", "#C68A5E", "#A86B45", "#6B4128");
            Styles(Slot.HairStyle, ("short", "Short"), ("long", "Long"), ("tidy", "Tidy"));
            Free(Slot.Hair, "#2B1B17", "#4A2C1D", "#A0522D", "#D8692E", "#E8C26A", "#F1E0B0", "#8C8C8C",
                "#7EE0C3", "#C9A0FF", "#3FA9F5", "#FF4F79");
            Free(Slot.Shirt, "#FFC845", "#FF6B6B", "#5CBF73", "#4A90E2", "#9B6BE0", "#FF9BC2", "#2BB3A3", "#FF9F43",
                "#FFFFFF", "#3D4A6B");
            Free(Slot.Shoes, "#FF6B6B", "#FFC845", "#4A90E2", "#FFFFFF", "#3D4A6B", "#2BB3A3", "#9B6BE0");

            // Everyday clothes for a trip to the doctor are free; the dressing-up outfits are bought.
            Styles(Slot.Outfit, ("tee", "T-shirt"), ("raincoat", "Raincoat"), ("jacket", "Jacket"));
            Shop(Slot.Outfit, false, ("explorer", 25, "Explorer"), ("adventurer", 35, "Adventurer"), ("viking", 50, "Viking"),
                ("knight", 60, "Knight armour"));
            Shop(Slot.Hat, true, ("cap", 20, "Cap"), ("beanie", 20, "Beanie"), ("bow", 20, "Bow"), ("party", 30, "Party hat"),
                ("wizard", 45, "Wizard hat"), ("bear", 50, "Bear hat"), ("knight", 60, "Knight helmet"), ("crown", 70, "Crown"));
            // The travel and hero capes take the shirt colour; the wizard and fur capes keep their own.
            Shop(Slot.Cape, true, ("travel", 30, "Travel cape"), ("fur", 40, "Fur cape"), ("wizard", 45, "Wizard cape"),
                ("hero", 55, "Hero cape"));
        }

        public static IReadOnlyList<AvatarItem> All => Items;

        public static IEnumerable<AvatarItem> For(Slot slot)
        {
            foreach (var item in Items)
                if (item.Slot == slot) yield return item;
        }

        public static AvatarItem Find(Slot slot, string id)
        {
            foreach (var item in Items)
                if (item.Slot == slot && item.Id == id) return item;
            return null;
        }

        public static AvatarItem FindKey(string key)
        {
            foreach (var item in Items)
                if (item.Key == key) return item;
            return null;
        }

        /// <summary>Slots shown and switched as 3D objects (the rest are colours).</summary>
        public static bool IsStyle(Slot slot) => slot is Slot.HairStyle or Slot.Outfit or Slot.Hat or Slot.Cape;

        static void Free(Slot slot, params string[] ids)
        {
            foreach (var id in ids) Items.Add(new AvatarItem(slot, id));
        }

        static void Styles(Slot slot, params (string id, string label)[] items)
        {
            foreach (var (id, label) in items) Items.Add(new AvatarItem(slot, id, 0, label));
        }

        static void Shop(Slot slot, bool withNone, params (string id, int price, string label)[] items)
        {
            if (withNone) Items.Add(new AvatarItem(slot, None, 0, "None"));
            foreach (var (id, price, label) in items) Items.Add(new AvatarItem(slot, id, price, label));
        }
    }
}
