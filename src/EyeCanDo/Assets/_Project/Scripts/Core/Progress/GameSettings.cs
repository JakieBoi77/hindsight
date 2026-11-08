using System;

namespace Hindsight.EyeCanDo.Core.Progress
{
    /// <summary>
    /// Settings for the whole device, shared by every save file. Kept apart from the save files
    /// so deleting a player never changes the volume. Fields are public for JSON serialisation.
    /// </summary>
    [Serializable]
    public sealed class GameSettings
    {
        public const int CurrentVersion = 1;

        public int version = CurrentVersion;

        /// <summary>Sound effect volume, 0 (off) to 1.</summary>
        public float soundVolume = 1f;

        /// <summary>Narration volume, 0 (off) to 1.</summary>
        public float voiceVolume = 1f;

        /// <summary>Show the one-line caption under each narrated line.</summary>
        public bool captionsOn = true;

        /// <summary>Bigger caption text.</summary>
        public bool bigText;

        /// <summary>Lines move on by themselves after the narration ends, without a tap.</summary>
        public bool autoContinue;

        /// <summary>Less bobbing, pulsing and confetti.</summary>
        public bool reduceMotion;

        /// <summary>The save file played last, shown on the title screen.</summary>
        public int lastSave;

        public float CaptionScale => bigText ? 1.25f : 1f;

        public bool IsValid() =>
            version == CurrentVersion
            && soundVolume >= 0f && soundVolume <= 1f
            && voiceVolume >= 0f && voiceVolume <= 1f
            && SaveSlots.IsSlot(lastSave);
    }
}
