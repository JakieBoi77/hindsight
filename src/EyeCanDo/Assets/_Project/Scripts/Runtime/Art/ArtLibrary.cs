using System;
using System.Collections.Generic;
using UnityEngine;

namespace EyeCanDo.Art
{
    /// <summary>
    /// Every SVG sprite in Assets/_Project/Art, by name ("Characters/leo_head", "Icons/home").
    /// The editor builder fills it (EyeCanDo → Build All); it is loaded from Resources so every
    /// scene uses the same art. Replace an SVG file in place to change the art.
    /// </summary>
    [CreateAssetMenu(menuName = "EyeCanDo/Art Library", fileName = "ArtLibrary")]
    public sealed class ArtLibrary : ScriptableObject
    {
        public const string ResourcePath = "ArtLibrary";

        [Serializable]
        public sealed class Entry
        {
            public string name;
            public Sprite sprite;
        }

        public List<Entry> sprites = new List<Entry>();

        private static ArtLibrary instance;
        private Dictionary<string, Sprite> lookup;
        private readonly HashSet<string> reported = new HashSet<string>();

        public static ArtLibrary Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = Resources.Load<ArtLibrary>(ResourcePath);
                    if (instance == null)
                    {
                        Debug.LogWarning("[EyeCanDo] ArtLibrary missing from Resources. Run EyeCanDo → Build All.");
                        instance = CreateInstance<ArtLibrary>();
                    }
                }

                return instance;
            }
        }

        public static Sprite Get(string name)
        {
            return Instance.Find(name);
        }

        /// <summary>True if the library has art with this name (for optional parts).</summary>
        public bool Has(string name)
        {
            Find(null);
            return name != null && lookup.ContainsKey(name);
        }

        public Sprite Find(string name)
        {
            if (lookup == null)
            {
                lookup = new Dictionary<string, Sprite>();
                foreach (var entry in sprites)
                {
                    if (entry != null && entry.sprite != null)
                    {
                        lookup[entry.name] = entry.sprite;
                    }
                }
            }

            if (name == null)
            {
                return null;
            }

            if (lookup.TryGetValue(name, out var sprite))
            {
                return sprite;
            }

            if (reported.Add(name))
            {
                Debug.LogWarning($"[EyeCanDo] No art named '{name}'.");
            }

            return null;
        }
    }
}
