using System;
using System.IO;

namespace EyeCanDo.Core.Progress
{
    /// <summary>
    /// Loads and saves one piece of data as a JSON file. Saving writes a temporary file and then
    /// replaces the real one, so an interrupted save leaves the previous save unchanged
    /// (FR_PersistState). Missing, unreadable or invalid data is replaced with new data and the
    /// game continues (FR_CorruptData). The JSON conversion is passed in so this class has no
    /// engine dependency.
    /// </summary>
    public class DataFile<T>
        where T : class, new()
    {
        private readonly string path;
        private readonly Func<T, string> toJson;
        private readonly Func<string, T> fromJson;
        private readonly Func<T, bool> isValid;

        public DataFile(string path, Func<T, string> toJson, Func<string, T> fromJson, Func<T, bool> isValid)
        {
            this.path = path ?? throw new ArgumentNullException(nameof(path));
            this.toJson = toJson ?? throw new ArgumentNullException(nameof(toJson));
            this.fromJson = fromJson ?? throw new ArgumentNullException(nameof(fromJson));
            this.isValid = isValid ?? throw new ArgumentNullException(nameof(isValid));
        }

        public string Path => path;

        public bool Exists => File.Exists(path);

        /// <summary>True when the last <see cref="Load"/> found damaged data and started over.</summary>
        public bool RecoveredFromBadData { get; private set; }

        public T Load()
        {
            RecoveredFromBadData = false;
            if (!File.Exists(path))
            {
                return new T();
            }

            try
            {
                var data = fromJson(File.ReadAllText(path));
                if (data != null && isValid(data))
                {
                    return data;
                }
            }
            catch (Exception)
            {
                // Fall through: damaged data is replaced below.
            }

            RecoveredFromBadData = true;
            return new T();
        }

        public void Save(T data)
        {
            var directory = System.IO.Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var temp = path + ".tmp";
            File.WriteAllText(temp, toJson(data));
            if (File.Exists(path))
            {
                File.Replace(temp, path, null);
            }
            else
            {
                File.Move(temp, path);
            }
        }

        public void Delete()
        {
            foreach (var file in new[] { path, path + ".tmp" })
            {
                if (File.Exists(file))
                {
                    File.Delete(file);
                }
            }
        }
    }

    /// <summary>One player's saved progress.</summary>
    public sealed class ProgressFile : DataFile<PlayerProgress>
    {
        public ProgressFile(string path, Func<PlayerProgress, string> toJson, Func<string, PlayerProgress> fromJson)
            : base(path, toJson, fromJson, p => p.IsValid())
        {
        }
    }
}
