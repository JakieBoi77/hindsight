using System;
using System.Collections.Generic;

namespace Hindsight.Ophthalmology.Core.Avatar
{
    /// <summary>
    /// Each part of the scout the player can change: colour, eyes, mouth and brows; then headwear and
    /// a fit (outfit).
    /// </summary>
    public enum Slot
    {
        Skin,
        Eyes,
        Mouth,
        Brows,
        Hat,
        Outfit,
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
    /// Everything the scout can look like or wear, all designed for this game. Colours, eyes, mouths,
    /// basic accessories and the everyday fits are free; the rest is bought with coins. Style ids match the editor's builders: face atlas cells
    /// (SceneObjects.FaceArt) and child objects ("Hat_crown", "Outfit_default_Body").
    /// </summary>
    public static class AvatarCatalog
    {
        public const string None = "none";

        /// <summary>The everyday fit every scout starts in: the camp shirt, navy shorts and socks.</summary>
        public const string DefaultOutfit = "default";

        static readonly List<AvatarItem> Items = new();

        static AvatarCatalog()
        {
            // Bright, friendly colours.
            Free(Slot.Skin, "#F2554B", "#FF9A3C", "#FFD43B", "#A6E23A", "#3FBF5A", "#2EC4B6", "#4C8DF6", "#A06CF0", "#FF7FBF",
                "#FFB59E", "#B9805A", "#F4EEDF");

            Styles(Slot.Eyes, ("basic", "Basic"), ("lashes", "Lashes"), ("half", "Half"), ("shine", "Shine"), ("squint", "Squint"),
                ("angry", "Angry"), ("eyeliner", "Eyeliner"), ("sad", "Sad"), ("cry", "Cry"), ("tired", "Tired"), ("almond", "Almond"),
                ("big", "Big"), ("small", "Small"), ("concern", "Concern"), ("anime", "Anime"), ("real", "Real"), ("fierce", "Fierce"),
                ("inverted", "Inverted"));

            Styles(Slot.Mouth, ("smiley", "Smiley"), ("bean", "Bean"), ("cat", "Cat"), ("cheek", "Cheek"), ("drool", "Drool"),
                ("lips", "Lips"), ("frown", "Frown"), ("kissy", "Kissy"), ("nonplussed", "Non-plussed"), ("o", "O"),
                ("squiggle", "Squiggle"), ("triangle", "Triangle"), ("smirk", "Smirk"), ("cringe", "Cringe"), ("teeth", "Teeth"),
                ("vamp", "Vamp"), ("chomp", "Chomp"), ("wacky", "Wacky"), ("toothy", "Toothy"));

            // Brows, thin to thick. Each is drawn in every mood, so brows show how a scout feels.
            Styles(Slot.Brows, (None, "None"), ("thin", "Thin"), ("soft", "Soft"), ("arched", "Arched"), ("straight", "Straight"),
                ("short", "Short"), ("angled", "Angled"), ("tapered", "Tapered"), ("thick", "Thick"), ("bushy", "Bushy"), ("dot", "Dots"),
                ("unibrow", "Unibrow"));

            Styles(Slot.Hat, (None, "None"), ("cap", "Cap"), ("beanie", "Beanie"), ("sprout", "Sprout"), ("bandana", "Bandana"));
            Shop(Slot.Hat, ("beret", 20, "Beret"), ("headband", 20, "Ninja headband"), ("visor", 20, "Visor"), ("sailor", 25, "Sailor hat"),
                ("medic", 25, "Medic hat"), ("straw", 30, "Straw hat"), ("sunhat", 30, "Sun hat"), ("pith", 35, "Pith helmet"),
                ("propeller", 35, "Propeller hat"), ("chef", 35, "Chef hat"), ("mushroom", 40, "Mushroom hat"), ("wolfears", 40, "Wolf ears"),
                ("frog", 45, "Frog hat"), ("cowboy", 45, "Cowboy hat"), ("gnome", 50, "Gnome hat"), ("wizard", 50, "Wizard hat"),
                ("jester", 55, "Jester hat"), ("antlers", 55, "Antlers"), ("laurel", 60, "Laurel"), ("knight", 70, "Knight helmet"),
                ("crown", 80, "Crown"));

            Styles(Slot.Outfit, (DefaultOutfit, "Default"));
            Shop(Slot.Outfit, ("sailor", 30, "Sailor"), ("tropical", 30, "Tropical"), ("climber", 40, "Climber"), ("ice", 40, "Ice puffer"),
                ("cowboy", 45, "Cowboy"), ("aviator", 50, "Aviator"), ("gnome", 50, "Gnome"), ("toga", 55, "Toga"), ("mummy", 60, "Mummy"),
                ("skele", 60, "Skele onesie"), ("jester", 65, "Jester"), ("fairy", 70, "Fairy"), ("knight", 80, "Knight"),
                ("astronaut", 90, "Astronaut"));

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

        /// <summary>Slots switched as 3D objects (the rest are colours or face drawings).</summary>
        public static bool IsStyle(Slot slot) => slot is Slot.Hat or Slot.Outfit;

        /// <summary>Slots drawn on the face (shown as their drawing on the shop cards).</summary>
        public static bool IsFace(Slot slot) => slot is Slot.Eyes or Slot.Mouth or Slot.Brows;

        static void Free(Slot slot, params string[] ids)
        {
            foreach (var id in ids) Items.Add(new AvatarItem(slot, id));
        }

        static void Styles(Slot slot, params (string id, string label)[] items)
        {
            foreach (var (id, label) in items) Items.Add(new AvatarItem(slot, id, 0, label));
        }

        static void Shop(Slot slot, params (string id, int price, string label)[] items)
        {
            foreach (var (id, price, label) in items) Items.Add(new AvatarItem(slot, id, price, label));
        }
    }
}
