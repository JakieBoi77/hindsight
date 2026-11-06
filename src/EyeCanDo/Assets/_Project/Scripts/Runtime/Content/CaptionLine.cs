using System;
using UnityEngine;

namespace EyeCanDo.Content
{
    /// <summary>A small picture shown beside a caption, so the meaning does not depend on reading.</summary>
    public enum CaptionIcon
    {
        None,
        Cold,
        Clock,
        Blurry,
        Light,
        Heart,
        Star,
        Speak,
        Drop,
        Eye,
        Sunglasses,
        Coin,
        Lock,
    }

    /// <summary>Who says a line. Their face is shown in the caption bar, and they talk on stage.</summary>
    public enum Speaker
    {
        Doctor,
        Mia,
        You,
        None,
    }

    /// <summary>
    /// What happens on stage with a story line. Plain beats play while the caption shows, and the
    /// child taps to continue. Beats starting with "You" (and TapBottle, Quiz) are things the
    /// child does: the line stays up as the prompt until the child has done it.
    /// </summary>
    public enum StoryBeat
    {
        None,
        DoctorWave,
        DoctorPoint,
        DoctorCheer,
        MiaWalksIn,
        MiaWaves,
        ShowBigEye,
        TapBottle,
        HideBigEye,
        Quiz,
        MiaSits,
        MiaTipsHead,
        DropOnMia,
        MiaShivers,
        MiaBlinks,
        MiaWipes,
        MiaWalksOut,
        YouWalkIn,
        YouHopUp,
        YouTipHead,
        YouLookUp,
        YouShiver,
        YouBlink,
        YouWipe,
        Sticker,
    }

    /// <summary>
    /// One line of child-facing text. It stands in for narration in the PoC: a grown-up can read
    /// it aloud, and a recorded voice can be dropped into <see cref="narration"/> later without
    /// code changes. Keep sentences to 10 words or fewer (NFR_Language).
    /// </summary>
    [Serializable]
    public sealed class CaptionLine
    {
        [TextArea(1, 3)]
        public string text;

        public CaptionIcon icon;

        public Speaker speaker;

        public StoryBeat beat;

        [Tooltip("Recorded narration for this line. Empty in the PoC.")]
        public AudioClip narration;

        public CaptionLine()
        {
        }

        public CaptionLine(string text, CaptionIcon icon = CaptionIcon.None, Speaker speaker = Speaker.Doctor, StoryBeat beat = StoryBeat.None)
        {
            this.text = text;
            this.icon = icon;
            this.speaker = speaker;
            this.beat = beat;
        }

        /// <summary>True when the child must do something before the story moves on.</summary>
        public bool IsAction => IsActionBeat(beat);

        public static bool IsActionBeat(StoryBeat beat)
        {
            switch (beat)
            {
                case StoryBeat.TapBottle:
                case StoryBeat.Quiz:
                case StoryBeat.YouHopUp:
                case StoryBeat.YouTipHead:
                case StoryBeat.YouLookUp:
                case StoryBeat.YouBlink:
                case StoryBeat.YouWipe:
                    return true;
                default:
                    return false;
            }
        }
    }
}
