using System;
using System.IO;

namespace EyeCanDo.Core.Progress
{
    /// <summary>
    /// The save files: one per player, each with its own character, coins and progress. Slots
    /// are numbered from 0; the game shows them as Save 1 to Save 3. A slot whose file is missing
    /// or damaged counts as empty, so it is offered as a new game (FR_CorruptData).
    /// </summary>
    public sealed class SaveSlots
    {
        public const int Count = 3;

        private readonly ProgressFile[] files = new ProgressFile[Count];

        public SaveSlots(string folder, Func<PlayerProgress, string> toJson, Func<string, PlayerProgress> fromJson)
        {
            Folder = folder ?? throw new ArgumentNullException(nameof(folder));
            for (var slot = 0; slot < Count; slot++)
            {
                files[slot] = new ProgressFile(Path.Combine(folder, $"save_{slot + 1}.json"), toJson, fromJson);
            }
        }

        public string Folder { get; }

        public static bool IsSlot(int slot)
        {
            return slot >= 0 && slot < Count;
        }

        public string PathOf(int slot)
        {
            return files[Check(slot)].Path;
        }

        /// <summary>The saved progress in a slot, or null when the slot is empty or damaged.</summary>
        public PlayerProgress Peek(int slot)
        {
            var file = files[Check(slot)];
            if (!file.Exists)
            {
                return null;
            }

            var progress = file.Load();
            return file.RecoveredFromBadData ? null : progress;
        }

        public bool IsUsed(int slot)
        {
            return Peek(slot) != null;
        }

        /// <summary>The progress in a slot, or a new player when the slot is empty or damaged.</summary>
        public PlayerProgress Load(int slot)
        {
            return files[Check(slot)].Load();
        }

        public bool RecoveredFromBadData(int slot)
        {
            return files[Check(slot)].RecoveredFromBadData;
        }

        public void Save(int slot, PlayerProgress progress)
        {
            files[Check(slot)].Save(progress ?? throw new ArgumentNullException(nameof(progress)));
        }

        public void Delete(int slot)
        {
            files[Check(slot)].Delete();
        }

        public void DeleteAll()
        {
            foreach (var file in files)
            {
                file.Delete();
            }
        }

        /// <summary>
        /// Moves the single save of earlier builds into the first slot, once, when no slot is
        /// used yet. Returns true when a save was moved.
        /// </summary>
        public bool AdoptLegacy(string legacyPath)
        {
            if (string.IsNullOrEmpty(legacyPath) || !File.Exists(legacyPath))
            {
                return false;
            }

            for (var slot = 0; slot < Count; slot++)
            {
                if (files[slot].Exists)
                {
                    return false;
                }
            }

            Directory.CreateDirectory(Folder);
            File.Move(legacyPath, files[0].Path);
            return true;
        }

        private static int Check(int slot)
        {
            if (!IsSlot(slot))
            {
                throw new ArgumentOutOfRangeException(nameof(slot), slot, $"Save slots are 0 to {Count - 1}.");
            }

            return slot;
        }
    }
}
