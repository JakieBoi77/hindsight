using System;
using UnityEngine;

namespace EyeCanDo.App
{
    /// <summary>
    /// Clips for each <see cref="Sound"/>, assigned by the editor builder from the sounds in
    /// Assets/_Project/Audio. A sound can have several clips, picked at random.
    /// </summary>
    [CreateAssetMenu(menuName = "EyeCanDo/Sound Library", fileName = "SoundLibrary")]
    public sealed class SoundLibrary : ScriptableObject
    {
        public const string ResourcePath = "SoundLibrary";

        [Serializable]
        public sealed class Entry
        {
            public Sound sound;
            public AudioClip[] clips;

            [Range(0f, 1f)]
            public float volume = 0.8f;
        }

        public Entry[] entries = Array.Empty<Entry>();

        public Entry Find(Sound sound)
        {
            foreach (var entry in entries)
            {
                if (entry != null && entry.sound == sound)
                {
                    return entry;
                }
            }

            return null;
        }
    }
}
