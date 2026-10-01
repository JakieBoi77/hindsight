using System.Collections.Generic;
using System.Threading;
using Hindsight.App;
using UnityEngine;

namespace Hindsight.Procedures
{
    /// <summary>
    /// Shared presentation services handed to every step: the guide doctor, the speech
    /// bubble, and sounds. Steps talk through this instead of reaching into the scene.
    /// </summary>
    public sealed class StepContext
    {
        private readonly DoctorCharacterView doctor;
        private readonly DoctorDialogueView dialogue;
        private readonly UiAudio audio;

        public StepContext(ProcedureDefinition procedure, DoctorCharacterView doctor, DoctorDialogueView dialogue, UiAudio audio)
        {
            Procedure = procedure;
            this.doctor = doctor;
            this.dialogue = dialogue;
            this.audio = audio;
        }

        public ProcedureDefinition Procedure { get; }

        /// <summary>Doctor speaks a line; completes when the child taps to continue.</summary>
        public async Awaitable SayAsync(DialogueLine line, CancellationToken cancellationToken)
        {
            if (line.IsEmpty)
            {
                return;
            }

            Present(line);
            await dialogue.SayAsync(line.Text, cancellationToken);
        }

        public async Awaitable SayAllAsync(IReadOnlyList<DialogueLine> lines, CancellationToken cancellationToken)
        {
            foreach (var line in lines)
            {
                await SayAsync(line, cancellationToken);
            }
        }

        /// <summary>Shows an instruction that stays up while the child does something on screen.</summary>
        public void Prompt(DialogueLine line)
        {
            if (line.IsEmpty)
            {
                return;
            }

            Present(line);
            dialogue.ShowPrompt(line.Text);
        }

        public void SetDoctorPose(DoctorPose pose)
        {
            doctor.SetPose(pose);
        }

        public void PlaySuccess()
        {
            if (audio != null)
            {
                audio.PlaySuccess();
            }
        }

        public void PlayGentleError()
        {
            if (audio != null)
            {
                audio.PlayGentleError();
            }
        }

        /// <summary>Returns shared presentation to neutral between steps.</summary>
        public void Reset()
        {
            dialogue.Hide();
            doctor.SetPose(DoctorPose.Smile);
            if (audio != null)
            {
                audio.PlayNarration(null);
            }
        }

        private void Present(DialogueLine line)
        {
            doctor.SetPose(line.Pose);
            if (audio != null)
            {
                audio.PlayNarration(line.Narration);
            }
        }
    }
}
