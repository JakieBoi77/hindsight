using System;
using Hindsight.Core.Progress;
using Hindsight.Procedures;
using UnityEngine;

namespace Hindsight.App
{
    /// <summary>
    /// Composition root for app-wide services, populated by <see cref="GameBootstrapper"/>.
    /// A small static registry keeps scene scripts free of singletons while still letting
    /// tests swap implementations (see <see cref="UseProgressStore"/>).
    /// </summary>
    public static class GameServices
    {
        public static bool IsInitialized { get; private set; }

        public static ProcedureCatalog Catalog { get; private set; }

        public static ProgressService Progress { get; private set; }

        public static GameNavigator Navigator { get; private set; }

        /// <summary>May be null in tests that run without audio.</summary>
        public static UiAudio Audio { get; private set; }

        public static void Initialize(ProcedureCatalog catalog, IProgressStore progressStore, GameNavigator navigator, UiAudio audio)
        {
            if (catalog == null)
            {
                throw new ArgumentNullException(nameof(catalog));
            }

            Catalog = catalog;
            Progress = new ProgressService(progressStore ?? throw new ArgumentNullException(nameof(progressStore)));
            Navigator = navigator ?? throw new ArgumentNullException(nameof(navigator));
            Audio = audio;
            IsInitialized = true;
        }

        /// <summary>Replaces the persistence backend, e.g. with an in-memory store during tests.</summary>
        public static void UseProgressStore(IProgressStore progressStore)
        {
            Progress = new ProgressService(progressStore ?? throw new ArgumentNullException(nameof(progressStore)));
        }

        // Static state survives play sessions when domain reload is disabled, so clear it explicitly.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        internal static void Reset()
        {
            Catalog = null;
            Progress = null;
            Navigator = null;
            Audio = null;
            IsInitialized = false;
        }
    }
}
