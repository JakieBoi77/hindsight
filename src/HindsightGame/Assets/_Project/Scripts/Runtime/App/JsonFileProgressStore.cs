using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Hindsight.Core.Progress;
using UnityEngine;

namespace Hindsight.App
{
    /// <summary>Saves progress as JSON in persistentDataPath so the app works fully offline.</summary>
    public sealed class JsonFileProgressStore : IProgressStore
    {
        private const int CurrentVersion = 1;
        private const string DefaultFileName = "progress.json";

        private readonly string filePath;

        public JsonFileProgressStore()
            : this(Path.Combine(Application.persistentDataPath, DefaultFileName))
        {
        }

        public JsonFileProgressStore(string filePath)
        {
            this.filePath = filePath;
        }

        public PlayerProgress Load()
        {
            if (!File.Exists(filePath))
            {
                return new PlayerProgress();
            }

            try
            {
                var data = JsonUtility.FromJson<SaveData>(File.ReadAllText(filePath));
                if (data == null)
                {
                    Debug.LogWarning($"Progress file '{filePath}' was empty; starting fresh.");
                    return new PlayerProgress();
                }

                return new PlayerProgress(data.completedProcedureIds, data.earnedStickerIds);
            }
            catch (Exception exception) when (exception is IOException || exception is ArgumentException || exception is UnauthorizedAccessException)
            {
                // A corrupt or unreadable save must never block a child from playing.
                Debug.LogWarning($"Could not read progress file '{filePath}'; starting fresh. {exception.Message}");
                return new PlayerProgress();
            }
        }

        public void Save(PlayerProgress progress)
        {
            var data = new SaveData
            {
                version = CurrentVersion,
                completedProcedureIds = progress.CompletedProcedureIds.ToList(),
                earnedStickerIds = progress.EarnedStickerIds.ToList(),
            };

            // Write to a temp file first so an interrupted save cannot corrupt existing progress.
            var tempPath = filePath + ".tmp";
            try
            {
                File.WriteAllText(tempPath, JsonUtility.ToJson(data, true));
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                File.Move(tempPath, filePath);
            }
            catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException)
            {
                Debug.LogError($"Could not save progress to '{filePath}'. {exception.Message}");
            }
        }

        [Serializable]
        private sealed class SaveData
        {
            public int version;
            public List<string> completedProcedureIds = new List<string>();
            public List<string> earnedStickerIds = new List<string>();
        }
    }
}
