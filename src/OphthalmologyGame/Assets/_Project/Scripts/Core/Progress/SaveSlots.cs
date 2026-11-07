using System;
using System.IO;

namespace Hindsight.Ophthalmology.Core.Progress
{
    /// <summary>
    /// The save files: one per player, each with its own character, coins and progress. Slots are
    /// numbered from 0; the game shows them as Save 1 to Save 3. A slot whose file is missing or
    /// damaged counts as empty, so it is offered as a new game (ported from the 2D PoC).
    /// </summary>
    public sealed class SaveSlots
    {
        public const int Count = 3;

        readonly DataFile<PlayerProgress>[] files = new DataFile<PlayerProgress>[Count];

        public SaveSlots(string folder, Func<PlayerProgress, string> toJson, Func<string, PlayerProgress> fromJson)
        {
            Folder = folder ?? throw new ArgumentNullException(nameof(folder));
            for (var slot = 0; slot < Count; slot++)
                files[slot] = new DataFile<PlayerProgress>(Path.Combine(folder, $"save_{slot + 1}.json"), toJson, fromJson, p =>
                {
                    p.Upgrade();
                    return p.IsValid();
                });
        }

        public string Folder { get; }

        public static bool IsSlot(int slot) => slot >= 0 && slot < Count;

        public string PathOf(int slot) => files[Check(slot)].Path;

        /// <summary>The saved progress in a slot, or null when the slot is empty or damaged.</summary>
        public PlayerProgress Peek(int slot)
        {
            var file = files[Check(slot)];
            if (!file.Exists) return null;
            var progress = file.Load();
            return file.RecoveredFromBadData ? null : progress;
        }

        public bool IsUsed(int slot) => Peek(slot) != null;

        /// <summary>The progress in a slot, or a new player when the slot is empty or damaged.</summary>
        public PlayerProgress Load(int slot) => files[Check(slot)].Load();

        public bool RecoveredFromBadData(int slot) => files[Check(slot)].RecoveredFromBadData;

        public void Save(int slot, PlayerProgress progress) =>
            files[Check(slot)].Save(progress ?? throw new ArgumentNullException(nameof(progress)));

        public void Delete(int slot) => files[Check(slot)].Delete();

        public void DeleteAll()
        {
            foreach (var file in files) file.Delete();
        }

        static int Check(int slot)
        {
            if (!IsSlot(slot))
                throw new ArgumentOutOfRangeException(nameof(slot), slot, $"Save slots are 0 to {Count - 1}.");
            return slot;
        }
    }
}
