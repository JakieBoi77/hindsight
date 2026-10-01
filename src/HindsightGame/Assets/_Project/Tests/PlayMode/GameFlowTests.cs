using System.Collections;
using System.IO;
using System.Linq;
using Hindsight.App;
using Hindsight.Core.Progress;
using Hindsight.Menus;
using Hindsight.Procedures;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Hindsight.Tests.PlayMode
{
    /// <summary>End-to-end checks of the PoC slice: title → level select → Eye Drops → reward.</summary>
    public sealed class GameFlowTests
    {
        private const float SceneTimeoutSeconds = 10f;

        private InMemoryProgressStore progressStore;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            Assert.That(GameServices.IsInitialized, Is.True, "GameBootstrapper should initialise services before any scene loads.");

            // Never touch the developer's real save file from tests.
            progressStore = new InMemoryProgressStore();
            GameServices.UseProgressStore(progressStore);

            SceneManager.LoadScene(SceneIds.MainMenu);
            yield return WaitForScene(SceneIds.MainMenu);
        }

        [UnityTest]
        public IEnumerator Play_OpensLevelSelect_WithOnlyEyeDropsUnlocked()
        {
            ClickButton("PlayButton");
            yield return WaitForScene(SceneIds.LevelSelect);
            yield return null;

            var cards = Object.FindAnyObjectByType<LevelSelectController>().Cards;
            Assert.That(cards.Count, Is.EqualTo(GameServices.Catalog.Procedures.Count));
            Assert.That(cards[0].Procedure.Id, Is.EqualTo("eye_drops"));
            Assert.That(cards[0].Availability, Is.EqualTo(LevelAvailability.Unlocked));
            Assert.That(cards.Skip(1).All(card => !UnlockRules.CanPlay(card.Availability)), Is.True, "Only Eye Drops should be playable.");
        }

        [UnityTest]
        public IEnumerator TappingLockedLevel_StaysOnLevelSelect()
        {
            ClickButton("PlayButton");
            yield return WaitForScene(SceneIds.LevelSelect);
            yield return null;

            var locked = Object.FindAnyObjectByType<LevelSelectController>().Cards[1];
            locked.Button.onClick.Invoke();
            yield return new WaitForSecondsRealtime(1f);

            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo(SceneIds.LevelSelect));
        }

        [UnityTest]
        public IEnumerator EyeDrops_CanBeCompletedByAChild_AndRecordsProgress()
        {
            yield return OpenEyeDrops();
            var controller = Object.FindAnyObjectByType<ProcedureController>();

            var player = new AutoPlayer(controller) { ReleaseFirstHoldEarly = true };
            yield return player.PlayToEnd(timeoutSeconds: 240f);
            Assert.That(player.EarlyReleaseCount, Is.EqualTo(1), "The blink/retry path should have been exercised.");

            yield return WaitForScene(SceneIds.LevelSelect);
            yield return null;

            Assert.That(progressStore.SaveCount, Is.GreaterThan(0), "Completion should be saved.");
            Assert.That(GameServices.Progress.Current.IsCompleted("eye_drops"), Is.True);
            Assert.That(GameServices.Progress.Current.HasSticker("sticker_brave_eyes"), Is.True);
            var card = Object.FindAnyObjectByType<LevelSelectController>().Cards[0];
            Assert.That(card.Availability, Is.EqualTo(LevelAvailability.Completed));
        }

        [UnityTest]
        public IEnumerator DeveloperSkip_AdvancesThroughEveryStep()
        {
            yield return OpenEyeDrops();
            var controller = Object.FindAnyObjectByType<ProcedureController>();
            yield return new WaitUntil(() => controller.Session != null);
            var stepCount = controller.Session.StepCount;
            var started = 0;
            controller.StepStarted += _ => started++;

            while (!controller.Session.IsComplete)
            {
                yield return new WaitForSecondsRealtime(0.4f);
                controller.SkipCurrentStep();
            }

            Assert.That(started, Is.EqualTo(stepCount - 1), "Every step after the first should start.");
            yield return WaitForScene(SceneIds.LevelSelect);
        }

        /// <summary>
        /// Saves a screenshot storyboard of the whole slice for design review. Explicit so CI skips it.
        /// Output: &lt;project&gt;/TestResults/Screenshots (or HINDSIGHT_SCREENSHOT_DIR).
        /// </summary>
        [UnityTest]
        [Explicit("Generates screenshots for review; run on demand.")]
        [Category("Screenshots")]
        public IEnumerator CaptureStoryboard()
        {
            var folder = System.Environment.GetEnvironmentVariable("HINDSIGHT_SCREENSHOT_DIR");
            if (string.IsNullOrEmpty(folder))
            {
                folder = Path.Combine(Application.dataPath, "..", "TestResults", "Screenshots");
            }

            using (var rig = new ScreenCaptureRig(folder))
            {
                yield return new WaitForSecondsRealtime(1f);
                yield return rig.Capture("00_title");

                ClickButton("PlayButton");
                yield return WaitForScene(SceneIds.LevelSelect);
                yield return new WaitForSecondsRealtime(1f);
                yield return rig.Capture("01_level_select");

                Object.FindAnyObjectByType<LevelSelectController>().Cards[0].Button.onClick.Invoke();
                yield return WaitForScene(SceneIds.Procedure);
                var controller = Object.FindAnyObjectByType<ProcedureController>();
                yield return new WaitForSecondsRealtime(2.5f);
                yield return rig.Capture("02_procedure_start");

                var player = new AutoPlayer(controller)
                {
                    FastForwardDialogue = false,
                    ReleaseFirstHoldEarly = true,
                    AfterAction = action => CaptureAfterDelay(rig, "03_" + action),
                };
                yield return player.PlayToEnd(timeoutSeconds: 400f);

                yield return WaitForScene(SceneIds.LevelSelect);
                yield return new WaitForSecondsRealtime(1f);
                yield return rig.Capture("99_level_select_completed");
            }
        }

        private static IEnumerator CaptureAfterDelay(ScreenCaptureRig rig, string name)
        {
            yield return new WaitForSecondsRealtime(0.9f);
            yield return rig.Capture(name);
        }

        private static IEnumerator OpenEyeDrops()
        {
            ClickButton("PlayButton");
            yield return WaitForScene(SceneIds.LevelSelect);
            yield return null;
            Object.FindAnyObjectByType<LevelSelectController>().Cards[0].Button.onClick.Invoke();
            yield return WaitForScene(SceneIds.Procedure);
        }

        private static void ClickButton(string name)
        {
            var button = Object.FindObjectsByType<Button>().FirstOrDefault(candidate => candidate.name == name);
            Assert.That(button, Is.Not.Null, $"Button '{name}' not found.");
            button.onClick.Invoke();
        }

        private static IEnumerator WaitForScene(string sceneName)
        {
            var deadline = Time.realtimeSinceStartup + SceneTimeoutSeconds;
            while (SceneManager.GetActiveScene().name != sceneName || !SceneManager.GetActiveScene().isLoaded)
            {
                if (Time.realtimeSinceStartup > deadline)
                {
                    Assert.Fail($"Timed out waiting for scene '{sceneName}' (current: {SceneManager.GetActiveScene().name}).");
                }

                yield return null;
            }

            // Wait for the fade-in so the scene is interactive.
            yield return new WaitForSecondsRealtime(0.4f);
        }
    }
}
