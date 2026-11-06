using System;

namespace EyeCanDo.Core.Avatar
{
    /// <summary>
    /// What a character looks like: one choice per <see cref="Slot"/>. Fields are public for JSON
    /// serialisation. The player starts with <see cref="Default"/> and can change it any time.
    /// </summary>
    [Serializable]
    public sealed class AvatarLook
    {
        public string skin = "#F2C69B";
        public string ears = "round";
        public string eyeStyle = "round";
        public string eyeColor = "#4A90E2";
        public string brows = "soft";
        public string nose = "button";
        public string mouth = "smile";
        public string hairStyle = "tuft";
        public string hairColor = "#7A4A2A";
        public string shirtColor = "#FFC845";
        public string emblem = AvatarCatalog.None;
        public string shortsColor = "#2F6DB5";
        public string shoesColor = "#FF6B6B";
        public string hat = AvatarCatalog.None;
        public string glasses = AvatarCatalog.None;
        public string held = AvatarCatalog.None;

        public static AvatarLook Default => new AvatarLook();

        /// <summary>Mia, the friend who gets her drops first in the story.</summary>
        public static AvatarLook Mia => new AvatarLook
        {
            skin = "#C68A5E",
            eyeColor = "#6B4A2B",
            hairStyle = "puffs",
            hairColor = "#2B1B17",
            mouth = "big",
            brows = "arched",
            eyeStyle = "lashes",
            shirtColor = "#9B6BE0",
            shortsColor = "#2BB3A3",
            shoesColor = "#FF9BC2",
        };

        public string Get(Slot slot)
        {
            switch (slot)
            {
                case Slot.Skin: return skin;
                case Slot.Ears: return ears;
                case Slot.EyeStyle: return eyeStyle;
                case Slot.EyeColor: return eyeColor;
                case Slot.Brows: return brows;
                case Slot.Nose: return nose;
                case Slot.Mouth: return mouth;
                case Slot.HairStyle: return hairStyle;
                case Slot.HairColor: return hairColor;
                case Slot.ShirtColor: return shirtColor;
                case Slot.Emblem: return emblem;
                case Slot.ShortsColor: return shortsColor;
                case Slot.ShoesColor: return shoesColor;
                case Slot.Hat: return hat;
                case Slot.Glasses: return glasses;
                default: return held;
            }
        }

        public void Set(Slot slot, string id)
        {
            switch (slot)
            {
                case Slot.Skin: skin = id; break;
                case Slot.Ears: ears = id; break;
                case Slot.EyeStyle: eyeStyle = id; break;
                case Slot.EyeColor: eyeColor = id; break;
                case Slot.Brows: brows = id; break;
                case Slot.Nose: nose = id; break;
                case Slot.Mouth: mouth = id; break;
                case Slot.HairStyle: hairStyle = id; break;
                case Slot.HairColor: hairColor = id; break;
                case Slot.ShirtColor: shirtColor = id; break;
                case Slot.Emblem: emblem = id; break;
                case Slot.ShortsColor: shortsColor = id; break;
                case Slot.ShoesColor: shoesColor = id; break;
                case Slot.Hat: hat = id; break;
                case Slot.Glasses: glasses = id; break;
                default: held = id; break;
            }
        }

        public AvatarLook Copy()
        {
            return (AvatarLook)MemberwiseClone();
        }

        /// <summary>True when every choice exists in the catalog.</summary>
        public bool IsValid()
        {
            foreach (Slot slot in Enum.GetValues(typeof(Slot)))
            {
                if (AvatarCatalog.Find(slot, Get(slot)) == null)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
