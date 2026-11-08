using Hindsight.EyeCanDo.Core;
using UnityEngine;

namespace Hindsight.EyeCanDo.UI
{
    public enum SfxId
    {
        Tap,
        Coin,
        Cheer,
        Blink,
        Shiver,
        Correct,
        TryAgain,
        Reward,
        Purchase,
        DropFall,
        Pop,
        Sparkle,
    }

    /// <summary>
    /// Plays short CC0 sound effects (from the 2D PoC) at the sound volume from settings.
    /// Created on first use and kept across scenes.
    /// </summary>
    public static class Sfx
    {
        static AudioSource source;
        static SoundLibrary library;

        public static void Play(SfxId id)
        {
            if (!Application.isPlaying) return;
            if (source == null)
            {
                var go = new GameObject("Sfx");
                Object.DontDestroyOnLoad(go);
                source = go.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.ignoreListenerPause = false;
                library = Resources.Load<SoundLibrary>("SoundLibrary");
            }
            var clip = library != null ? library.Get(id) : null;
            if (clip == null) return;
            source.PlayOneShot(clip, library.VolumeOf(id) * GameServices.Settings.soundVolume);
        }
    }
}
