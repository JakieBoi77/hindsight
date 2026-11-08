using System;
using System.IO;

namespace Hindsight.Ophthalmology.Core.Progress
{
    /// <summary>
    /// Loads and saves one piece of data as a JSON file. Saving writes a temporary file and then
    /// replaces the real one, so an interrupted save leaves the previous save unchanged.
    /// Missing, unreadable or invalid data is replaced with new data and the game continues.
    /// The JSON conversion is passed in so this class has no engine dependency (ported from the
    /// 2D PoC's ProgressFile).
    /// </summary>
    public class DataFile<T>
        where T : class, new()
    {
        readonly Func<T, string> toJson;
        readonly Func<string, T> fromJson;
        readonly Func<T, bool> isValid;

        public DataFile(string path, Func<T, string> toJson, Func<string, T> fromJson, Func<T, bool> isValid)
        {
            Path = path ?? throw new ArgumentNullException(nameof(path));
            this.toJson = toJson ?? throw new ArgumentNullException(nameof(toJson));
            this.fromJson = fromJson ?? throw new ArgumentNullException(nameof(fromJson));
            this.isValid = isValid ?? throw new ArgumentNullException(nameof(isValid));
        }

        public string Path { get; }

        public bool Exists => File.Exists(Path);

        /// <summary>True when the last <see cref="Load"/> found damaged data and started over.</summary>
        public bool RecoveredFromBadData { get; private set; }

        public T Load()
        {
            RecoveredFromBadData = false;
            if (!File.Exists(Path)) return new T();

            try
            {
                var data = fromJson(File.ReadAllText(Path));
                if (data != null && isValid(data)) return data;
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
            var directory = System.IO.Path.GetDirectoryName(Path);
            if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);

            var temp = Path + ".tmp";
            File.WriteAllText(temp, toJson(data));
            if (File.Exists(Path)) File.Replace(temp, Path, null);
            else File.Move(temp, Path);
        }

        public void Delete()
        {
            foreach (var file in new[] { Path, Path + ".tmp" })
                if (File.Exists(file)) File.Delete(file);
        }
    }
}
