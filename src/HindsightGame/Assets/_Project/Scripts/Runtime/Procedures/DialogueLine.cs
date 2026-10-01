using System;
using UnityEngine;

namespace Hindsight.Procedures
{
    /// <summary>Expression the guide doctor shows while speaking a line.</summary>
    public enum DoctorPose
    {
        Smile,
        Wave,
        Point,
        Cheer,
    }

    /// <summary>
    /// One thing the doctor says. Narration is optional today (text-only PoC) but every line
    /// carries a slot so recorded voice-over can be dropped in without code changes.
    /// </summary>
    [Serializable]
    public struct DialogueLine
    {
        [SerializeField, TextArea(2, 4)] private string text;
        [SerializeField] private DoctorPose pose;
        [SerializeField] private AudioClip narration;

        public DialogueLine(string text, DoctorPose pose = DoctorPose.Smile, AudioClip narration = null)
        {
            this.text = text;
            this.pose = pose;
            this.narration = narration;
        }

        public string Text => text;

        public DoctorPose Pose => pose;

        public AudioClip Narration => narration;

        public bool IsEmpty => string.IsNullOrWhiteSpace(text);
    }
}
