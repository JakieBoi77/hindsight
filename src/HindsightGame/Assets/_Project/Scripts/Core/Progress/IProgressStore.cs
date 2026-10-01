namespace Hindsight.Core.Progress
{
    /// <summary>Persists <see cref="PlayerProgress"/>. Implementations must work fully offline.</summary>
    public interface IProgressStore
    {
        /// <summary>Loads saved progress, or returns fresh progress when nothing usable is saved.</summary>
        PlayerProgress Load();

        void Save(PlayerProgress progress);
    }
}
