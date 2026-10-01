using System;
using System.Collections.Generic;

namespace Hindsight.Core.Progress
{
    /// <summary>
    /// What the child has achieved so far. Identifiers are stable string ids taken from
    /// procedure data, so renaming a level's display name never loses progress.
    /// </summary>
    public sealed class PlayerProgress
    {
        private readonly HashSet<string> completedProcedureIds;
        private readonly List<string> earnedStickerIds;

        public PlayerProgress()
            : this(Array.Empty<string>(), Array.Empty<string>())
        {
        }

        public PlayerProgress(IEnumerable<string> completedProcedureIds, IEnumerable<string> earnedStickerIds)
        {
            this.completedProcedureIds = new HashSet<string>(completedProcedureIds ?? Array.Empty<string>(), StringComparer.Ordinal);
            this.earnedStickerIds = new List<string>();
            foreach (var stickerId in earnedStickerIds ?? Array.Empty<string>())
            {
                AddSticker(stickerId);
            }
        }

        public IReadOnlyCollection<string> CompletedProcedureIds => completedProcedureIds;

        /// <summary>Stickers in the order they were earned.</summary>
        public IReadOnlyList<string> EarnedStickerIds => earnedStickerIds;

        public bool IsCompleted(string procedureId)
        {
            return !string.IsNullOrEmpty(procedureId) && completedProcedureIds.Contains(procedureId);
        }

        public bool HasSticker(string stickerId)
        {
            return !string.IsNullOrEmpty(stickerId) && earnedStickerIds.Contains(stickerId);
        }

        /// <returns>True if this was the first completion of the procedure.</returns>
        public bool MarkCompleted(string procedureId)
        {
            if (string.IsNullOrEmpty(procedureId))
            {
                throw new ArgumentException("Procedure id is required.", nameof(procedureId));
            }

            return completedProcedureIds.Add(procedureId);
        }

        /// <returns>True if the sticker is new.</returns>
        public bool AddSticker(string stickerId)
        {
            if (string.IsNullOrEmpty(stickerId) || earnedStickerIds.Contains(stickerId))
            {
                return false;
            }

            earnedStickerIds.Add(stickerId);
            return true;
        }
    }
}
