using System;
using Hindsight.Procedures;
using UnityEngine;

namespace Hindsight.App
{
    /// <summary>High-level navigation between the app's screens.</summary>
    public sealed class GameNavigator
    {
        private readonly SceneLoader sceneLoader;

        public GameNavigator(SceneLoader sceneLoader)
        {
            this.sceneLoader = sceneLoader ?? throw new ArgumentNullException(nameof(sceneLoader));
        }

        /// <summary>The procedure the Procedure scene should run next.</summary>
        public ProcedureDefinition ActiveProcedure { get; private set; }

        public Awaitable GoToMainMenuAsync()
        {
            return sceneLoader.LoadAsync(SceneIds.MainMenu);
        }

        public Awaitable GoToLevelSelectAsync()
        {
            return sceneLoader.LoadAsync(SceneIds.LevelSelect);
        }

        public Awaitable StartProcedureAsync(ProcedureDefinition procedure)
        {
            if (procedure == null)
            {
                throw new ArgumentNullException(nameof(procedure));
            }

            ActiveProcedure = procedure;
            return sceneLoader.LoadAsync(SceneIds.Procedure);
        }
    }
}
