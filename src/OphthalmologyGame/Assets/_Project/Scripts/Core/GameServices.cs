using System;
using System.IO;
using Hindsight.Ophthalmology.Core.Progress;
using UnityEngine;

namespace Hindsight.Ophthalmology.Core
{
    /// <summary>
    /// App-wide data, ready before the first scene loads so Play works from any scene: the save
    /// files and the one being played, and the device settings (adapted from the 2D PoC).
    /// Saves live in persistentDataPath/saves/save_N.json, settings in settings.json beside them.
    /// </summary>
    public static class GameServices
    {
        static string dataFolderOverride;
        static DataFile<GameSettings> settingsFile;
        static bool saveChosen;

        /// <summary>The progress of the save being played (or of the last one, before one is picked).</summary>
        public static PlayerProgress Progress { get; private set; } = new();

        public static GameSettings Settings { get; private set; } = new();

        public static SaveSlots Saves { get; private set; }

        /// <summary>The slot <see cref="Progress"/> belongs to.</summary>
        public static int CurrentSave { get; private set; }

        /// <summary>Raised when a setting changes, so open screens can apply it at once.</summary>
        public static event Action SettingsChanged;

        /// <summary>Raised when coins, purchases or the look change.</summary>
        public static event Action ProgressChanged;

        /// <summary>Set after a full reset, so the title screen can say everything was deleted.</summary>
        public static bool JustReset { get; set; }

        public static string DataFolder => dataFolderOverride ?? Application.persistentDataPath;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Initialise() => LoadData();

        static void LoadData()
        {
            var folder = DataFolder;
            settingsFile = new DataFile<GameSettings>(Path.Combine(folder, "settings.json"),
                s => JsonUtility.ToJson(s, true), JsonUtility.FromJson<GameSettings>, s => s.IsValid());
            Saves = new SaveSlots(Path.Combine(folder, "saves"), p => JsonUtility.ToJson(p, true), JsonUtility.FromJson<PlayerProgress>);

            Settings = settingsFile.Load();
            if (settingsFile.RecoveredFromBadData)
            {
                Debug.LogWarning("[Hindsight] Saved settings were damaged and have been reset.");
                SaveSettings();
            }

            CurrentSave = Settings.lastSave;
            Progress = Saves.Load(CurrentSave);
            saveChosen = false;
            if (Saves.RecoveredFromBadData(CurrentSave))
                Debug.LogWarning("[Hindsight] A save file was damaged; it shows as a new game.");
        }

        /// <summary>
        /// Saves the current player. Nothing is written until a save file has been picked, so
        /// opening the title screen never creates an empty save.
        /// </summary>
        public static void Save()
        {
            if (Saves == null || (!saveChosen && !Saves.IsUsed(CurrentSave))) return;
            try
            {
                Saves.Save(CurrentSave, Progress);
            }
            catch (IOException exception)
            {
                // The previous save is left untouched; play continues.
                Debug.LogWarning($"[Hindsight] Could not save progress: {exception.Message}");
            }
            ProgressChanged?.Invoke();
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
                Debug.LogWarning($"[Hindsight] Could not save settings: {exception.Message}");
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

        /// <summary>Deletes one save file. The other saves and the settings are kept.</summary>
        public static void DeleteSave(int slot)
        {
            try
            {
                Saves.Delete(slot);
            }
            catch (IOException exception)
            {
                Debug.LogWarning($"[Hindsight] Could not delete Save {slot + 1}: {exception.Message}");
            }

            if (slot == CurrentSave)
            {
                Progress = new PlayerProgress();
                saveChosen = false;
            }
            ProgressChanged?.Invoke();
        }

        /// <summary>Puts the game back the way it was when first installed: every save and setting is deleted.</summary>
        public static void DeleteEverything()
        {
            try
            {
                Saves.DeleteAll();
                settingsFile.Delete();
            }
            catch (IOException exception)
            {
                Debug.LogWarning($"[Hindsight] Could not delete all data: {exception.Message}");
            }

            Settings = new GameSettings();
            CurrentSave = 0;
            Progress = new PlayerProgress();
            saveChosen = false;
            SettingsChanged?.Invoke();
            ProgressChanged?.Invoke();
        }

        /// <summary>Records a finished level for the current player and returns the coins paid.</summary>
        public static int FinishLevel(LevelDefinition level)
        {
            if (level == null) return 0;
            var paid = Progress.FinishLevel(level.LevelId, level.UnlocksOnComplete != null ? level.UnlocksOnComplete.LevelId : null);
            Save();
            return paid;
        }

        public static bool IsUnlocked(LevelDefinition level) => level != null && Progress.IsUnlocked(level.LevelId, level.UnlockedByDefault);

        public static bool IsCompleted(LevelDefinition level) => level != null && Progress.IsCompleted(level.LevelId);

        /// <summary>Test hook: keeps all saves and settings in a separate folder, starting from nothing.</summary>
        public static void UseTestData(string folder)
        {
            dataFolderOverride = folder;
            LoadData();
            DeleteEverything();
        }
    }
}
