using System;

namespace Hindsight.Ophthalmology.Core.Avatar
{
    /// <summary>
    /// What a kid looks like: one choice per <see cref="Slot"/>. Fields are public for JSON
    /// serialisation. The player starts with <see cref="Default"/> and can change it any time.
    /// </summary>
    [Serializable]
    public sealed class AvatarLook
    {
        public string skin = "#F2C69B";
        public string hairStyle = "short";
        public string hair = "#4A2C1D";
        public string outfit = "tee";
        public string shirt = "#FFC845";
        public string shoes = "#FF6B6B";
        public string hat = AvatarCatalog.None;
        public string cape = AvatarCatalog.None;

        public static AvatarLook Default => new();

        /// <summary>Sam, the boy who gets drops first in the story (the peer model).</summary>
        public static AvatarLook Sam => new()
        {
            skin = "#C68A5E",
            hairStyle = "short",
            hair = "#2B1B17",
            outfit = "jacket",
            shirt = "#5CBF73",
            shoes = "#FFC845",
            hat = "cap",
        };

        public string Get(Slot slot) => slot switch
        {
            Slot.Skin => skin,
            Slot.HairStyle => hairStyle,
            Slot.Hair => hair,
            Slot.Outfit => outfit,
            Slot.Shirt => shirt,
            Slot.Shoes => shoes,
            Slot.Hat => hat,
            _ => cape,
        };

        public void Set(Slot slot, string id)
        {
            switch (slot)
            {
                case Slot.Skin: skin = id; break;
                case Slot.HairStyle: hairStyle = id; break;
                case Slot.Hair: hair = id; break;
                case Slot.Outfit: outfit = id; break;
                case Slot.Shirt: shirt = id; break;
                case Slot.Shoes: shoes = id; break;
                case Slot.Hat: hat = id; break;
                default: cape = id; break;
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
