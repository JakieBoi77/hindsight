using System;
using System.IO;
using EyeCanDo.Core.Interaction;
using EyeCanDo.Core.Progress;
using EyeCanDo.UI;
using UnityEngine;
using Object = UnityEngine.Object;

namespace EyeCanDo.App
{
    public static class SceneNames
    {
        public const string Title = "Title";
        public const string Saves = "Saves";
        public const string ClinicMap = "ClinicMap";
        public const string EyeDropsStory = "EyeDropsStory";
        public const string DropCatch = "DropCatch";
        public const string BlurrySpy = "BlurrySpy";
        public const string DressUp = "DressUp";
    }

    /// <summary>
    /// App-wide services, created before the first scene loads so Play works from any scene:
    /// the save files and the one being played, the device settings, the scene fader and the
    /// navigation input gate.
    /// </summary>
    public static class GameServices
    {
        private static string dataFolderOverride;
        private static SettingsFile settingsFile;
        private static bool saveChosen;

        /// <summary>The progress of the save being played (or of the last one, before one is picked).</summary>
        public static PlayerProgress Progress { get; private set; }

        public static GameSettings Settings { get; private set; } = new GameSettings();

        public static SaveSlots Saves { get; private set; }

        /// <summary>The slot <see cref="Progress"/> belongs to.</summary>
        public static int CurrentSave { get; private set; }

        /// <summary>Raised when a setting changes, so open screens can apply it at once (FR_Preferences).</summary>
        public static event Action SettingsChanged;

        public static InputGate NavigationGate { get; } = new InputGate();

        public static AppRoot Root { get; private set; }

        /// <summary>When set, the clinic map opens this stop's panel on arrival (returning from a level).</summary>
        public static string OpenStopOnMap { get; set; }

        /// <summary>Where Dress Up returns to.</summary>
        public static string DressUpReturn { get; set; }

        /// <summary>Set after a full reset, so the title screen can say everything was deleted.</summary>
        public static bool JustReset { get; set; }

        /// <summary>Opens Dress Up, remembering where to come back to.</summary>
        public static void GoToDressUp(string returnTo)
        {
            DressUpReturn = returnTo;
            GoTo(SceneNames.DressUp);
        }

        /// <summary>The level a game scene starts at, chosen on the stop panel. 0 means "suggest one".</summary>
        public static int PendingLevel { get; set; }

        public static string ProgressPath => Saves?.PathOf(CurrentSave);

        public static string DataFolder => dataFolderOverride ?? Application.persistentDataPath;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialise()
        {
            LoadData();
            if (Root == null)
            {
                var go = new GameObject("EyeCanDo App");
                Object.DontDestroyOnLoad(go);
                Root = go.AddComponent<AppRoot>();
            }

            NavigationGate.Open();
        }

        private static void LoadData()
        {
            var folder = DataFolder;
            settingsFile = new SettingsFile(Path.Combine(folder, "settings.json"), s => JsonUtility.ToJson(s, true), JsonUtility.FromJson<GameSettings>);
            Saves = new SaveSlots(Path.Combine(folder, "saves"), p => JsonUtility.ToJson(p, true), JsonUtility.FromJson<PlayerProgress>);
            try
            {
                if (Saves.AdoptLegacy(Path.Combine(folder, "progress.json")))
                {
                    Debug.Log("[EyeCanDo] Moved the earlier single save into Save 1.");
                }
            }
            catch (IOException exception)
            {
                Debug.LogWarning($"[EyeCanDo] Could not move the earlier save: {exception.Message}");
            }

            Settings = settingsFile.Load();
            if (settingsFile.RecoveredFromBadData)
            {
                Debug.LogWarning("[EyeCanDo] Saved settings were damaged and have been reset (FR_CorruptData).");
                SaveSettings();
            }

            CurrentSave = Settings.lastSave;
            Progress = Saves.Load(CurrentSave);
            saveChosen = false;
            if (Saves.RecoveredFromBadData(CurrentSave))
            {
                Debug.LogWarning("[EyeCanDo] A save file was damaged; it shows as a new game (FR_CorruptData).");
            }
        }

        /// <summary>
        /// Saves the current player. Nothing is written until a save file has been picked, so
        /// opening the title screen never creates an empty save.
        /// </summary>
        public static void Save()
        {
            if (Saves == null || (!saveChosen && !Saves.IsUsed(CurrentSave)))
            {
                return;
            }

            try
            {
                Saves.Save(CurrentSave, Progress);
            }
            catch (IOException exception)
            {
                // The previous save is left untouched (FR_PersistState); play continues.
                Debug.LogWarning($"[EyeCanDo] Could not save progress: {exception.Message}");
            }
        }

        /// <summary>Stores the settings and tells open screens to apply them.</summary>
        public static void SaveSettings()
        {
            try
            {
                settingsFile?.Save(Settings);
            }
            catch (IOException exception)
            {
                Debug.LogWarning($"[EyeCanDo] Could not save settings: {exception.Message}");
            }

            SettingsChanged?.Invoke();
        }

        /// <summary>Makes a slot the one being played: its saved progress, or a new player when empty.</summary>
        public static void ChooseSave(int slot)
        {
            CurrentSave = slot;
            Progress = Saves.Load(slot);
            saveChosen = true;
            Settings.lastSave = slot;
            SaveSettings();
            Save();
        }

        /// <summary>Plays a save file: picks it and opens the clinic map.</summary>
        public static void PlaySave(int slot)
        {
            // A second tap on another card while the map loads must not switch players.
            if (Root == null || Root.IsLoading || !NavigationGate.IsOpen)
            {
                return;
            }

            ChooseSave(slot);
            GoToMap();
        }

        /// <summary>Deletes one save file. The other saves and the settings are kept.</summary>
        public static void DeleteSave(int slot)
        {
            try
            {
                Saves.Delete(slot);
            }
            catch (IOException exception)
            {
                Debug.LogWarning($"[EyeCanDo] Could not delete Save {slot + 1}: {exception.Message}");
            }

            if (slot == CurrentSave)
            {
                Progress = new PlayerProgress();
                saveChosen = false;
            }
        }

        /// <summary>
        /// Puts the game back the way it was when first installed: every save file and every
        /// setting is deleted (FR_ResetJourney, FR_ClearAllData).
        /// </summary>
        public static void DeleteEverything()
        {
            try
            {
                Saves.DeleteAll();
                settingsFile.Delete();
            }
            catch (IOException exception)
            {
                Debug.LogWarning($"[EyeCanDo] Could not delete all data: {exception.Message}");
            }

            Settings = new GameSettings();
            CurrentSave = 0;
            Progress = new PlayerProgress();
            saveChosen = false;
            OpenStopOnMap = null;
            PendingLevel = 0;
            DressUpReturn = null;
            SettingsChanged?.Invoke();
        }

        /// <summary>Fades to another scene. Ignores repeated requests until the scene is shown.</summary>
        public static void GoTo(string sceneName)
        {
            if (Root == null || !NavigationGate.TryPass())
            {
                return;
            }

            Root.LoadScene(sceneName);
        }

        public static void GoToMap(string openStop = null)
        {
            OpenStopOnMap = openStop;
            GoTo(SceneNames.ClinicMap);
        }

        /// <summary>Test hook: keeps all saves and settings in a separate folder, starting from nothing.</summary>
        public static void UseTestData(string folder)
        {
            dataFolderOverride = folder;
            LoadData();
            DeleteEverything();
            NavigationGate.Open();
        }
    }
}
