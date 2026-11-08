using System;

namespace Hindsight.EyeCanDo.Core.Avatar
{
    /// <summary>
    /// How a scout feels, shown by their brows (every brow style is drawn in each mood). Story lines
    /// and actions set it; <see cref="Keep"/> leaves it as it is.
    /// </summary>
    public enum Mood
    {
        Keep = -1,
        Neutral,
        Happy,
        Worried,
        Angry,
        Surprised,
        Focused,
    }

    /// <summary>
    /// What a scout looks like: one choice per <see cref="Slot"/>. Fields are public for JSON
    /// serialisation. The player starts with <see cref="Default"/> and can change it any time.
    /// </summary>
    [Serializable]
    public sealed class AvatarLook
    {
        public string skin = "#FFD43B";
        public string eyes = "basic";
        public string mouth = "smiley";
        public string brows = "soft";
        public string hat = AvatarCatalog.None;
        public string outfit = AvatarCatalog.DefaultOutfit;

        public static AvatarLook Default => new();

        /// <summary>Sam, the boy who gets drops first in the story (the peer model).</summary>
        public static AvatarLook Sam => new()
        {
            skin = "#4C8DF6",
            eyes = "basic",
            mouth = "bean",
            brows = "thick",
            hat = "cap",
            outfit = AvatarCatalog.DefaultOutfit,
        };

        /// <summary>
        /// Dr. Iris: a purple scout in a white coat and head mirror. Her coat and mirror are not sold
        /// in the shop, so this look is not <see cref="IsValid"/> for a player.
        /// </summary>
        public static AvatarLook Doctor => new()
        {
            skin = "#A06CF0",
            eyes = "lashes",
            mouth = "smiley",
            brows = "thin",
            hat = DoctorHat,
            outfit = DoctorOutfit,
        };

        public const string DoctorHat = "headmirror";
        public const string DoctorOutfit = "labcoat";

        public string Get(Slot slot) => slot switch
        {
            Slot.Skin => skin,
            Slot.Eyes => eyes,
            Slot.Mouth => mouth,
            Slot.Brows => brows,
            Slot.Hat => hat,
            _ => outfit,
        };

        public void Set(Slot slot, string id)
        {
            switch (slot)
            {
                case Slot.Skin: skin = id; break;
                case Slot.Eyes: eyes = id; break;
                case Slot.Mouth: mouth = id; break;
                case Slot.Brows: brows = id; break;
                case Slot.Hat: hat = id; break;
                default: outfit = id; break;
            }
        }

        public AvatarLook Copy() => (AvatarLook)MemberwiseClone();

        /// <summary>True when every choice exists in the catalog.</summary>
        public bool IsValid()
        {
            foreach (Slot slot in Enum.GetValues(typeof(Slot)))
                if (AvatarCatalog.Find(slot, Get(slot)) == null) return false;
            return true;
        }
    }
}
