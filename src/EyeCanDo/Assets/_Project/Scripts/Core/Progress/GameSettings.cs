using System;

namespace EyeCanDo.Core.Progress
{
    public enum TextSize
    {
        Normal,
        Big,
    }

    public enum TextSpeed
    {
        Slow,
        Normal,
        Fast,
    }

    /// <summary>
    /// Settings for the whole device, shared by every save file (FR_Preferences). Kept apart from
    /// the save files so deleting a player never changes the volume. Fields are public for JSON
    /// serialisation.
    /// </summary>
    [Serializable]
    public sealed class GameSettings
    {
        public const int CurrentVersion = 1;

        public int version = CurrentVersion;

        /// <summary>Sound effect volume, 0 (off) to 1.</summary>
        public float soundVolume = 1f;

        public TextSize textSize = TextSize.Normal;

        public TextSpeed textSpeed = TextSpeed.Normal;

        /// <summary>Captions move on by themselves after a short pause, without a tap.</summary>
        public bool autoContinue;

        /// <summary>Less bobbing, drifting and confetti.</summary>
        public bool reduceMotion;

        /// <summary>The save file played last, shown on the title screen.</summary>
        public int lastSave;

        public bool SoundOn => soundVolume > 0.001f;

        /// <summary>How much bigger caption text is than normal.</summary>
        public float TextScale => textSize == TextSize.Big ? 1.25f : 1f;

        /// <summary>How fast captions type out, compared to normal.</summary>
        public float RevealSpeed
        {
            get
            {
                switch (textSpeed)
                {
                    case TextSpeed.Slow: return 0.5f;
                    case TextSpeed.Fast: return 2.5f;
                    default: return 1f;
                }
            }
        }

        public bool IsValid()
        {
            return version == CurrentVersion
                   && soundVolume >= 0f && soundVolume <= 1f
                   && Enum.IsDefined(typeof(TextSize), textSize)
                   && Enum.IsDefined(typeof(TextSpeed), textSpeed)
                   && lastSave >= 0 && lastSave < SaveSlots.Count;
        }
    }

    public sealed class SettingsFile : DataFile<GameSettings>
    {
        public SettingsFile(string path, Func<GameSettings, string> toJson, Func<string, GameSettings> fromJson)
            : base(path, toJson, fromJson, s => s.IsValid())
        {
        }
    }
}
