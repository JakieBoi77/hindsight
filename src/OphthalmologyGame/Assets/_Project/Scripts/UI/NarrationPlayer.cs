using System;
using System.Collections;
using Hindsight.Ophthalmology.Core;
using Hindsight.Ophthalmology.Procedure;
using UnityEngine;

namespace Hindsight.Ophthalmology.UI
{
    /// <summary>
    /// Plays narration. A line with a recorded clip plays it and waits for it to end; a line
    /// without one waits the time it would take to say it, so the storyboard keeps its pace before
    /// the recordings exist. The speaker's mouth moves while a line is spoken.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public sealed class NarrationPlayer : MonoBehaviour
    {
        AudioSource source;
        int version;

        /// <summary>Raised with (speaker, true) when a line starts and (speaker, false) when it stops.</summary>
        public event Action<Speaker, bool> SpeakingChanged;

        public bool IsSpeaking { get; private set; }

        void Awake()
        {
            source = GetComponent<AudioSource>();
            source.playOnAwake = false;
            ApplyVolume();
            GameServices.SettingsChanged += ApplyVolume;
        }

        void OnDestroy() => GameServices.SettingsChanged -= ApplyVolume;

        void ApplyVolume()
        {
            if (source != null) source.volume = GameServices.Settings.voiceVolume;
        }

        /// <summary>Speaks a line to the end. A newer Play or Stop cuts it short.</summary>
        public IEnumerator Play(StoryLine line)
        {
            if (line == null || line.IsEmpty) yield break;
            Stop();
            var mine = ++version;
            IsSpeaking = true;
            SpeakingChanged?.Invoke(line.speaker, true);

            if (line.clip != null)
            {
                source.clip = line.clip;
                source.Play();
                // isPlaying stays true while the listener is paused, so a paused game waits here.
                while (mine == version && (source.isPlaying || AudioListener.pause)) yield return null;
            }
            else
            {
                for (var t = 0f; t < line.EstimatedSeconds && mine == version; t += Time.deltaTime)
                    yield return null;
            }

            if (mine != version) yield break; // a newer line owns the speaking state now
            // The mouth stops with the voice, but the line isn't over until the child has had a
            // moment to take it in: nothing moves on (Next, the next line, a cut) before then.
            SpeakingChanged?.Invoke(line.speaker, false);
            var settle = PauseAfter(line);
            for (var t = 0f; t < settle && mine == version; t += Time.deltaTime) yield return null;
            if (mine != version) yield break;
            IsSpeaking = false;
        }

        /// <summary>Quiet time after a line: about a second, a little more for longer lines.</summary>
        public static float PauseAfter(StoryLine line) => 0.9f + Mathf.Min(0.6f, StoryLine.WordCount(line.narration) * 0.06f);

        public void Stop()
        {
            version++;
            if (source != null && source.isPlaying) source.Stop();
            if (!IsSpeaking) return;
            IsSpeaking = false;
            foreach (Speaker s in Enum.GetValues(typeof(Speaker))) SpeakingChanged?.Invoke(s, false);
        }
    }
}
