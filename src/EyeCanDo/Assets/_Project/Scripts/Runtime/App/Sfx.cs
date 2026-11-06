using UnityEngine;

namespace EyeCanDo.App
{
    /// <summary>The game's sound effects (CC0 clips fetched by Tools/SoundFetch). There is no music yet.</summary>
    public enum Sound
    {
        Tap,
        Pop,
        Whoosh,
        Hop,
        Step,
        DropFall,
        Catch,
        Golden,
        Plip,
        Shiver,
        Blink,
        Wipe,
        Correct,
        TryAgain,
        Wiggle,
        Star,
        Sparkle,
        Tick,
        RoundEnd,
        Reward,
        Cheer,
        Coin,
        Purchase,
        Locked,
        Count,
        Go,
    }

    /// <summary>
    /// Plays sound effects at the volume in the settings. Each sound gets its own voice from a
    /// small pool, so changing the pitch of one (a rising run of finds) never bends another that
    /// is still playing (the end-of-round jingle).
    /// </summary>
    public static class Sfx
    {
        private const int Voices = 8;

        private static SoundLibrary library;
        private static AudioSource[] sources;
        private static int next;

        private static readonly int[] MajorScale = { 0, 2, 4, 5, 7, 9, 11 };

        public static bool Enabled => GameServices.Settings.SoundOn;

        public static void Play(Sound sound, float pitch = 1f)
        {
            if (!Enabled)
            {
                return;
            }

            if (library == null)
            {
                library = Resources.Load<SoundLibrary>(SoundLibrary.ResourcePath);
                if (library == null)
                {
                    return;
                }
            }

            var entry = library.Find(sound);
            if (entry == null || entry.clips == null || entry.clips.Length == 0)
            {
                return;
            }

            var clip = entry.clips[UnityEngine.Random.Range(0, entry.clips.Length)];
            if (clip == null)
            {
                return;
            }

            var output = FreeVoice();
            if (output == null)
            {
                return;
            }

            output.pitch = pitch;
            output.clip = clip;
            output.volume = entry.volume * GameServices.Settings.soundVolume;
            output.Play();
        }

        /// <summary>
        /// The pitch for the nth note up a major scale from the clip's own note (0 plays it as
        /// recorded), so a run of sounds climbs like a little tune.
        /// </summary>
        public static float ScaleNote(int n)
        {
            n = Mathf.Max(0, n);
            return Mathf.Pow(2f, (MajorScale[n % 7] + 12 * (n / 7)) / 12f);
        }

        /// <summary>A voice that is not playing, or else the one that started longest ago.</summary>
        private static AudioSource FreeVoice()
        {
            if ((sources == null || sources[0] == null) && GameServices.Root != null)
            {
                sources = new AudioSource[Voices];
                for (var i = 0; i < Voices; i++)
                {
                    sources[i] = GameServices.Root.gameObject.AddComponent<AudioSource>();
                    sources[i].playOnAwake = false;
                }
            }

            if (sources == null || sources[0] == null)
            {
                return null;
            }

            for (var i = 0; i < Voices; i++)
            {
                var voice = sources[(next + i) % Voices];
                if (!voice.isPlaying)
                {
                    next = (next + i + 1) % Voices;
                    return voice;
                }
            }

            var oldest = sources[next];
            next = (next + 1) % Voices;
            return oldest;
        }
    }
}
