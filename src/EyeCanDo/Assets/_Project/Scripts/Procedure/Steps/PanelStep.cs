using System.Collections;
using System.Collections.Generic;
using Hindsight.EyeCanDo.SceneObjects;
using UnityEngine;

namespace Hindsight.EyeCanDo.Procedure.Steps
{
    /// <summary>
    /// One storyboard panel: a framed shot, one narrated line, an optional stage beat, an optional
    /// follow-up line (often another speaker) and an optional thing to discover (a glowing object that plays a short fact
    /// when tapped). The panel ends on Next, or when the hold button is pressed; with a follow-up line,
    /// the hold button only appears once that line starts.
    /// </summary>
    [CreateAssetMenu(menuName = "EyeCanDo/Steps/Panel", fileName = "PanelStep")]
    public sealed class PanelStep : ProcedureStep
    {
        public enum EndOn
        {
            Next,
            HoldPressed,
        }

        [SerializeField] string shotId = "";
        [Tooltip("Instant beat played before the camera cuts (e.g. seat Sam off camera).")]
        [SerializeField] StoryBeat setupBeat = StoryBeat.None;
        [Tooltip("Anchor the doctor walks to (or is placed at) first. Empty = stay put.")]
        [SerializeField] string doctorAnchorId = "";
        [SerializeField] bool teleportDoctor;
        [SerializeField] DoctorPose doctorPose = DoctorPose.Idle;
        [SerializeField] DoctorProp doctorProp = DoctorProp.None;
        [Tooltip("Beat played on camera.")]
        [SerializeField] StoryBeat beat = StoryBeat.None;
        [Tooltip("Finish the beat before the line starts (e.g. the drop lands, then 'Cold!').")]
        [SerializeField] bool beatBeforeLine;
        [SerializeField] StoryLine line = new();
        [Tooltip("Optional second line, often another speaker (Sam says \"Ooh, cold!\", then Dr. Iris explains).")]
        [SerializeField] StoryLine followUpLine = new();
        [Tooltip("Optional interactable that glows during the panel.")]
        [SerializeField] string discoverId = "";
        [SerializeField] StoryLine discoverLine = new();
        [SerializeField] EndOn endOn = EndOn.Next;
        [Tooltip("With EndOn = HoldPressed: the eye-drop bottle appears over the eye when the button does.")]
        [SerializeField] bool dropperWithButton;
        [Tooltip("Beat played after the lines, before the next panel (e.g. Sam walks to the chair).")]
        [SerializeField] StoryBeat afterBeat = StoryBeat.None;

        public override IEnumerable<StoryLine> Lines => NonEmpty(line, followUpLine, discoverLine);

        public override IEnumerator Run(IProcedureContext ctx)
        {
            if (setupBeat != StoryBeat.None) yield return ctx.PlayBeat(setupBeat);

            var anchor = ctx.GetAnchor(doctorAnchorId);
            if (anchor != null && teleportDoctor) ctx.Doctor.PlaceAt(anchor);
            var shot = ctx.Shots.CutAndWait(shotId);
            if (anchor != null && !teleportDoctor)
            {
                ctx.CoroutineHost.StartCoroutine(shot);
                yield return ctx.Doctor.MoveTo(anchor);
            }
            else
            {
                yield return shot;
            }
            ctx.Doctor.SetProp(doctorProp);
            ctx.Doctor.Play(doctorPose);

            // The thing to discover: optional, so the panel never waits for it.
            var discover = string.IsNullOrEmpty(discoverId) ? null : ctx.GetInteractable(discoverId);
            void OnDiscover()
            {
                discover.SetAccepting(false);
                ctx.CoroutineHost.StartCoroutine(ctx.Captions.Aside(discoverLine));
            }
            if (discover != null)
            {
                discover.Tapped += OnDiscover;
                discover.SetAccepting(true);
            }

            if (beat != StoryBeat.None)
            {
                if (beatBeforeLine) yield return ctx.PlayBeat(beat);
                else ctx.CoroutineHost.StartCoroutine(ctx.PlayBeat(beat));
            }

            // With a follow-up line ("Hold the button..."), the button waits for it: nothing to press
            // while the instructions are still being explained.
            var hasFollowUp = !followUpLine.IsEmpty;
            if (endOn == EndOn.HoldPressed && !hasFollowUp) ShowHoldButton(ctx);
            yield return SayIfAny(ctx, line, waitForNext: endOn == EndOn.Next && !hasFollowUp);
            if (endOn == EndOn.HoldPressed && hasFollowUp) ShowHoldButton(ctx);
            yield return SayIfAny(ctx, followUpLine, waitForNext: endOn == EndOn.Next);
            if (endOn == EndOn.HoldPressed)
                while (!ctx.Buttons.Hold.IsHeld) yield return null;

            if (discover != null)
            {
                discover.Tapped -= OnDiscover;
                discover.SetAccepting(false);
            }
            if (endOn == EndOn.Next) ctx.Captions.Hide();
            if (afterBeat != StoryBeat.None) yield return ctx.PlayBeat(afterBeat);
        }

        void ShowHoldButton(IProcedureContext ctx)
        {
            if (dropperWithButton) ctx.EyeDrop.ShowHolding(); // the bottle comes up over the eye with the button
            ctx.Buttons.ShowHold("");
        }

#if UNITY_EDITOR
        public PanelStep EditorInit(string shot, StoryLine text, StoryBeat onCamera = StoryBeat.None, StoryBeat setup = StoryBeat.None)
        {
            shotId = shot;
            line = text;
            beat = onCamera;
            setupBeat = setup;
            return this;
        }

        public PanelStep EditorDoctor(string anchor, DoctorPose pose, DoctorProp prop = DoctorProp.None, bool teleport = false)
        {
            doctorAnchorId = anchor;
            doctorPose = pose;
            doctorProp = prop;
            teleportDoctor = teleport;
            return this;
        }

        public PanelStep EditorBeatFirst()
        {
            beatBeforeLine = true;
            return this;
        }

        public PanelStep EditorFollowUp(StoryLine text)
        {
            followUpLine = text;
            return this;
        }

        public PanelStep EditorDiscover(string id, StoryLine text)
        {
            discoverId = id;
            discoverLine = text;
            return this;
        }

        public PanelStep EditorAfter(StoryBeat after)
        {
            afterBeat = after;
            return this;
        }

        public PanelStep EditorEndOnHold(bool showDropper = false)
        {
            endOn = EndOn.HoldPressed;
            dropperWithButton = showDropper;
            return this;
        }
#endif
    }
}
