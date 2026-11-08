using System;
using UnityEngine;

namespace Hindsight.EyeCanDo.Procedure
{
    /// <summary>Who says a line. Their portrait is shown in the caption bubble and they talk on stage.</summary>
    public enum Speaker
    {
        Doctor,
        Sam,
        None,
    }

    /// <summary>
    /// One narrated line. <see cref="narration"/> is what is spoken; <see cref="caption"/> is the
    /// short optional text on screen (8 words or fewer). <see cref="clip"/> is the recorded voice:
    /// the builder fills it from Audio/Narration/&lt;id&gt;.wav when that file exists, so recordings
    /// can be dropped in later with no code change. Without a clip the line is timed silently.
    /// </summary>
    [Serializable]
    public sealed class StoryLine
    {
        public const int MaxCaptionWords = 8;

        [Tooltip("Stable key, also the narration file name (Audio/Narration/<id>.wav).")]
        public string id = "";

        public Speaker speaker = Speaker.Doctor;

        [TextArea(1, 3), Tooltip("The full sentence the narrator says.")]
        public string narration = "";

        [Tooltip("Short caption shown on screen. 8 words or fewer.")]
        public string caption = "";

        [Tooltip("Recorded narration. Empty until the recordings exist.")]
        public AudioClip clip;

        [Tooltip("How the speaker feels saying it (their brows show it). Keep leaves their mood as it is.")]
        public Core.Avatar.Mood mood = Core.Avatar.Mood.Keep;

        public StoryLine()
        {
        }

        public StoryLine(string id, Speaker speaker, string narration, string caption, AudioClip clip = null,
            Core.Avatar.Mood mood = Core.Avatar.Mood.Keep)
        {
            this.mood = mood;
            this.id = id;
            this.speaker = speaker;
            this.narration = narration;
            this.caption = caption;
            this.clip = clip;
        }

        public bool IsEmpty => string.IsNullOrEmpty(narration) && string.IsNullOrEmpty(caption);

        /// <summary>How long a line takes to say when there is no recording (about 2.5 words a second).</summary>
        public float EstimatedSeconds => Mathf.Max(1.5f, WordCount(narration) / 2.5f);

        public static int WordCount(string text) =>
            string.IsNullOrWhiteSpace(text) ? 0 : text.Split((char[])null, StringSplitOptions.RemoveEmptyEntries).Length;
    }

    /// <summary>
    /// Something that happens on stage with a panel, played by the scene context. "Setup" beats are
    /// instant and run before the camera cuts (so nobody pops in on camera); the rest play on camera.
    /// </summary>
    public enum StoryBeat
    {
        None,
        DoctorWave,
        DoctorCheer,
        SamWave,
        SamSit,
        SamTiltBack,
        DropOnSam,
        SamBlink,
        SamCheer,
        SamHide,
        SamWalkToChair,
        DoctorExamSam,
        SamStandUp,
        SamDance,
        SamLeave,
        KidAppear,
        KidHide,
        DoctorToDropPoint,
        DoctorToExamPoint,
    }
}
