using System;
using System.Threading;
using Hindsight.Core.Interactions;
using Hindsight.UI;
using UnityEngine;

namespace Hindsight.Procedures.Steps
{
    /// <summary>
    /// Drives a <see cref="HoldStillChallenge"/> from a <see cref="HoldButton"/>. Shared by every
    /// "keep still" moment so they all behave the same way for the child.
    /// </summary>
    public static class HoldInteraction
    {
        /// <param name="onProgress">Called every frame with 0..1 hold progress.</param>
        /// <param name="onInterruptedAsync">Plays the "oops, you blinked/moved" moment before the child retries.</param>
        public static async Awaitable RunAsync(
            HoldButton button,
            float requiredSeconds,
            Action<float> onProgress,
            Func<CancellationToken, Awaitable> onInterruptedAsync,
            CancellationToken cancellationToken)
        {
            var challenge = new HoldStillChallenge(requiredSeconds);
            void OnPressed() => challenge.BeginHold();
            void OnReleased() => challenge.ReleaseHold();

            button.Pressed += OnPressed;
            button.Released += OnReleased;
            button.Interactable = true;
            var handledInterruptions = 0;

            try
            {
                while (true)
                {
                    await Awaitable.NextFrameAsync(cancellationToken);
                    var state = challenge.Tick(Time.deltaTime);
                    onProgress(challenge.Progress);

                    if (state == HoldStillState.Succeeded)
                    {
                        return;
                    }

                    if (challenge.InterruptionCount > handledInterruptions)
                    {
                        handledInterruptions = challenge.InterruptionCount;
                        button.Interactable = false;
                        onProgress(0f);
                        await onInterruptedAsync(cancellationToken);
                        button.Interactable = true;
                    }
                }
            }
            finally
            {
                button.Pressed -= OnPressed;
                button.Released -= OnReleased;
                button.Interactable = false;
            }
        }
    }
}
