using System;
using System.Collections;
using System.Collections.Generic;
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
        /// <summary>Loudness samples per second of recording.</summary>
        const int EnvelopeRate = 60;

        static readonly Dictionary<AudioClip, float[]> Envelopes = new();

        AudioSource source;
        int version;
        bool voicing;
        bool estimating;

        /// <summary>Raised with (speaker, true) when a line starts and (speaker, false) when it stops.</summary>
        public event Action<Speaker, bool> SpeakingChanged;

        /// <summary>A line starts (before <see cref="SpeakingChanged"/>): who says it and how they feel.</summary>
        public event Action<StoryLine> LineStarted;

        public bool IsSpeaking { get; private set; }

        /// <summary>
        /// How loud the line is right now, 0 (silent) to 1 (its loudest), read from the recording
        /// itself so it doesn't depend on the volume setting. The speaker's mouth opens this wide.
        /// Lines without a recording babble instead.
        /// </summary>
        public float VoiceLevel
        {
            get
            {
                if (!voicing) return 0f;
                if (estimating) return SceneObjects.BeanRig.Babble(Time.time);
                if (source == null || source.clip == null) return 0f;
                var envelope = Envelope(source.clip);
                if (envelope == null) return SceneObjects.BeanRig.Babble(Time.time);
                var i = Mathf.FloorToInt(source.time * EnvelopeRate);
                return i >= 0 && i < envelope.Length ? envelope[i] : 0f;
            }
        }

        /// <summary>
        /// The clip's loudness over time: RMS per 1/60 s, scaled so the line's loud parts reach 1.
        /// Needs the clip's sample data (narration imports as Decompress On Load); null if unavailable.
        /// </summary>
        static float[] Envelope(AudioClip clip)
        {
            if (Envelopes.TryGetValue(clip, out var cached)) return cached;
            float[] envelope = null;
            try
            {
                var channels = Mathf.Max(1, clip.channels);
                var data = new float[clip.samples * channels];
                if (clip.GetData(data, 0))
                {
                    var window = Mathf.Max(1, clip.frequency / EnvelopeRate);
                    envelope = new float[Mathf.CeilToInt((float)clip.samples / window)];
                    for (var w = 0; w < envelope.Length; w++)
                    {
                        double sum = 0;
                        var start = w * window * channels;
                        var end = Mathf.Min(data.Length, start + window * channels);
                        for (var i = start; i < end; i++) sum += data[i] * data[i];
                        envelope[w] = end > start ? Mathf.Sqrt((float)(sum / (end - start))) : 0f;
                    }
                    // Normalise to the loud parts of this line (90th percentile), not its single loudest peak.
                    var sorted = (float[])envelope.Clone();
                    Array.Sort(sorted);
                    var loud = Mathf.Max(1e-4f, sorted[Mathf.Clamp((int)(sorted.Length * 0.9f), 0, sorted.Length - 1)]);
                    var floor = loud * 0.12f; // breaths and room noise stay closed
                    for (var w = 0; w < envelope.Length; w++)
                        envelope[w] = Mathf.Clamp01((envelope[w] - floor) / (loud - floor));
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"NarrationPlayer: can't read {clip.name} for lip sync ({e.Message}); the mouth will babble.");
            }
            Envelopes[clip] = envelope;
            return envelope;
        }

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
            voicing = true;
            estimating = line.clip == null;
            LineStarted?.Invoke(line);
            SpeakingChanged?.Invoke(line.speaker, true);

            if (line.clip != null)
            {
                Envelope(line.clip); // read the loudness before the first frame needs it
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
            voicing = false;
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
            voicing = false;
            if (source != null && source.isPlaying) source.Stop();
            if (!IsSpeaking) return;
            IsSpeaking = false;
            foreach (Speaker s in Enum.GetValues(typeof(Speaker))) SpeakingChanged?.Invoke(s, false);
        }
    }
}
