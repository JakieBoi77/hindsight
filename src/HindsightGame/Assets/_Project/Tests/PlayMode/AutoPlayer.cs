using System;
using System.Collections;
using System.Linq;
using Hindsight.Procedures;
using Hindsight.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Hindsight.Tests.PlayMode
{
    /// <summary>
    /// Plays a procedure through its real UI the way a child would: tapping the continue arrow,
    /// pressing whatever button is on screen and holding hold-buttons. Quiz answers are tried in
    /// order, so wrong answers (and the encouraging retry flow) are exercised too.
    /// </summary>
    internal sealed class AutoPlayer
    {
        private const float ActionIntervalSeconds = 0.25f;

        private const float EarlyReleaseSeconds = 1f;

        private readonly ProcedureController controller;
        private HoldButton heldButton;
        private float holdStartedAt;
        private bool hasReleasedEarly;

        public AutoPlayer(ProcedureController controller)
        {
            this.controller = controller;
        }

        /// <summary>When true, taps the speech bubble to finish typing instantly.</summary>
        public bool FastForwardDialogue { get; set; } = true;

        /// <summary>When true, lets go of the first hold early to exercise the "oops, you blinked" retry.</summary>
        public bool ReleaseFirstHoldEarly { get; set; }

        /// <summary>Number of times a hold was let go before it succeeded.</summary>
        public int EarlyReleaseCount { get; private set; }

        /// <summary>Called after each action with a short description, e.g. to take screenshots.</summary>
        public Func<string, IEnumerator> AfterAction { get; set; }

        public int ActionCount { get; private set; }

        public IEnumerator PlayToEnd(float timeoutSeconds)
        {
            var deadline = Time.realtimeSinceStartup + timeoutSeconds;
            while (controller != null && (controller.Session == null || !controller.Session.IsComplete))
            {
                if (Time.realtimeSinceStartup > deadline)
                {
                    Assert.Fail($"Procedure did not finish within {timeoutSeconds}s (step {controller.Session?.CurrentIndex}).");
                }

                yield return new WaitForSecondsRealtime(ActionIntervalSeconds);
                if (controller == null)
                {
                    yield break;
                }

                var action = TryAct();
                if (action == null)
                {
                    continue;
                }

                ActionCount++;
                if (AfterAction != null)
                {
                    yield return AfterAction($"step{controller.Session.CurrentIndex + 1:00}_{ActionCount:000}_{action}");
                }
            }
        }

        private string TryAct()
        {
            if (heldButton != null)
            {
                if (ReleaseFirstHoldEarly && !hasReleasedEarly && Time.realtimeSinceStartup - holdStartedAt > EarlyReleaseSeconds)
                {
                    heldButton.Release();
                    heldButton = null;
                    hasReleasedEarly = true;
                    EarlyReleaseCount++;
                    return "release_early";
                }

                // Keep holding until the game itself disables the button (success).
                if (heldButton.isActiveAndEnabled && heldButton.Interactable)
                {
                    return null;
                }

                heldButton = null;
            }

            var dialogue = Object.FindAnyObjectByType<DoctorDialogueView>();
            if (dialogue != null && dialogue.IsWaitingForContinue)
            {
                Click(dialogue, "ContinueButton");
                return "continue";
            }

            var view = controller.CurrentView;
            if (view == null)
            {
                return null;
            }

            var hold = view.GetComponentsInChildren<HoldButton>().FirstOrDefault(button => button.isActiveAndEnabled && button.Interactable);
            if (hold != null)
            {
                hold.Press();
                heldButton = hold;
                holdStartedAt = Time.realtimeSinceStartup;
                return "hold";
            }

            var tap = view.GetComponentsInChildren<Button>().FirstOrDefault(button => button.isActiveAndEnabled && button.IsInteractable());
            if (tap != null)
            {
                tap.onClick.Invoke();
                return "tap_" + tap.name;
            }

            if (FastForwardDialogue && dialogue != null && !string.IsNullOrEmpty(dialogue.CurrentText))
            {
                Click(dialogue, "Body");
            }

            return null;
        }

        private static void Click(Component root, string name)
        {
            var button = root.GetComponentsInChildren<Button>().FirstOrDefault(candidate => candidate.name == name);
            Assert.That(button, Is.Not.Null, $"No button named {name} under {root.name}.");
            button.onClick.Invoke();
        }
    }
}
