using System.Linq;

namespace Hindsight.Core.Progress
{
    /// <summary>Non-persistent store for tests and editor previews.</summary>
    public sealed class InMemoryProgressStore : IProgressStore
    {
        private PlayerProgress saved = new PlayerProgress();

        public int SaveCount { get; private set; }

        public PlayerProgress Load()
        {
            // Return a copy so callers cannot mutate the "saved" state without calling Save.
            return new PlayerProgress(saved.CompletedProcedureIds.ToArray(), saved.EarnedStickerIds.ToArray());
        }

        public void Save(PlayerProgress progress)
        {
            saved = new PlayerProgress(progress.CompletedProcedureIds.ToArray(), progress.EarnedStickerIds.ToArray());
            SaveCount++;
        }
    }
}
