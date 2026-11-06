using System.Collections;
using System.IO;
using EyeCanDo.App;
using EyeCanDo.Content;
using EyeCanDo.Core.Avatar;
using EyeCanDo.Games;
using EyeCanDo.Map;
using EyeCanDo.Menus;
using EyeCanDo.Story;
using EyeCanDo.Title;
using EyeCanDo.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace EyeCanDo.Tests.PlayMode
{
    /// <summary>
    /// Saves a screenshot of every screen and moment to TestResults/Storyboard/&lt;device&gt;/, for
    /// design, clinician and child-life review. Runs at a phone shape (19.5:9) and a tablet shape
    /// (4:3) to check that nothing is cut off or covered. Explicit: run it on purpose.
    /// </summary>
    [Explicit("Writes screenshots; run on purpose.")]
    [Timeout(900000)]
    public sealed class StoryboardCapture
    {
        private ScreenshotRig rig;
        private int shot;

        [UnityTest]
        [TestCase("phone", 2340, 1080, ExpectedResult = null)]
        [TestCase("tablet", 1440, 1080, ExpectedResult = null)]
        public IEnumerator Capture(string device, int width, int height)
        {
            var folder = Path.Combine(Path.GetDirectoryName(Application.dataPath), "TestResults", "Storyboard", device);
            if (Directory.Exists(folder))
            {
                Directory.Delete(folder, true);
            }

            AutoPlayer.FreshData();
            shot = 0;
            using (rig = new ScreenshotRig(folder, width, height))
            {
                // Title.
                yield return AutoPlayer.Load(SceneNames.Title);
                yield return AutoPlayer.Settle(1.2f);
                yield return Snap("title");

                // Settings, opened from the title.
                AutoPlayer.Find<TitleScreen>().SettingsButton.Click();
                yield return AutoPlayer.Settle(0.8f);
                yield return Snap("settings");
                SettingsPanel.Current.Close();
                yield return AutoPlayer.Settle(0.5f);

                // The save files, all empty on a new install.
                AutoPlayer.Find<TitleScreen>().PlayButton.Click();
                yield return AutoPlayer.WaitForScene(SceneNames.Saves);
                yield return AutoPlayer.Settle(1f);
                yield return Snap("saves_new");

                // Map, the locked games and the Special Drops panel.
                AutoPlayer.Find<SaveSelectScreen>().Cards[0].Click();
                yield return AutoPlayer.WaitForScene(SceneNames.ClinicMap);
                // Map and the Special Drops panel.
                yield return AutoPlayer.Settle(1f);
                yield return Snap("map");
                var map = AutoPlayer.Find<ClinicMap>();
                map.OpenStop("microscope");
                yield return AutoPlayer.Settle(0.6f);
                yield return Snap("map_coming_soon");
                map.OpenStop(StopCatalog.EyeDropsId);
                yield return AutoPlayer.Settle(0.8f);
                yield return Snap("map_panel_locked");
                map.Panel.DropCatchCard.Click();
                yield return AutoPlayer.Settle(0.6f);
                yield return Snap("map_panel_locked_tap");
                map.Panel.StoryCard.Click();
                yield return AutoPlayer.WaitForScene(SceneNames.EyeDropsStory);

                // The story.
                var story = AutoPlayer.Find<EyeDropsStory>();
                yield return AutoPlayer.PlayStory(story, Snap);
                yield return AutoPlayer.Settle(2.5f);
                yield return Snap("story_end_coins");

                // Drop Catch: level picker, play, results.
                story.GamesButton.Click();
                yield return AutoPlayer.WaitForScene(SceneNames.ClinicMap);
                yield return AutoPlayer.Settle(0.8f);
                yield return Snap("map_panel_after_story");
                AutoPlayer.Find<ClinicMap>().Panel.DropCatchCard.Click();
                yield return AutoPlayer.WaitForScene(SceneNames.DropCatch);
                yield return AutoPlayer.Settle(0.8f);
                yield return Snap("catch_levels");
                var dropCatch = AutoPlayer.Find<DropCatchGame>();
                dropCatch.StartLevel(3);
                yield return AutoPlayer.PlayDropCatch(dropCatch, Snap);
                yield return AutoPlayer.WaitFor(() => dropCatch.ShowingResults, 10f, "catch results");
                yield return AutoPlayer.Settle(2.5f);
                yield return Snap("catch_results");

                // Blurry Spy.
                GameServices.PendingLevel = 3;
                GameServices.GoTo(SceneNames.BlurrySpy);
                yield return AutoPlayer.WaitForScene(SceneNames.BlurrySpy);
                var spy = AutoPlayer.Find<BlurrySpyGame>();
                yield return AutoPlayer.WaitFor(() => spy.IsPlaying, 10f, "spy start");
                yield return AutoPlayer.PlayBlurrySpy(spy, Snap);
                yield return AutoPlayer.WaitFor(() => spy.ShowingResults, 10f, "spy results");
                yield return AutoPlayer.Settle(2.5f);
                yield return Snap("spy_results");

                // Dress up and the shop, with the coins earned.
                GameServices.Progress.AddCoins(150);
                GameServices.GoToDressUp(SceneNames.ClinicMap);
                yield return AutoPlayer.WaitForScene(SceneNames.DressUp);
                var dressUp = AutoPlayer.Find<EyeCanDo.DressUp.DressUpScreen>();
                yield return AutoPlayer.Settle(0.8f);
                for (var tab = 0; tab < EyeCanDo.DressUp.DressUpScreen.Tabs.Length; tab++)
                {
                    dressUp.ShowTab(tab);
                    yield return AutoPlayer.Settle(0.4f);
                    yield return Snap($"dressup_tab_{EyeCanDo.DressUp.DressUpScreen.Tabs[tab].Name}");
                }

                dressUp.ShowTab(3);
                dressUp.Choose(AvatarCatalog.Find(Slot.HairStyle, "long"));
                dressUp.Choose(AvatarCatalog.Find(Slot.HairColor, "#D8692E"));
                dressUp.ShowTab(1);
                dressUp.Choose(AvatarCatalog.Find(Slot.EyeStyle, "lashes"));
                dressUp.ShowTab(5);
                dressUp.Choose(AvatarCatalog.Find(Slot.Hat, "unicorn"));
                yield return AutoPlayer.Settle(0.6f);
                yield return Snap("dressup_try_on");
                dressUp.Buy();
                dressUp.ShowTab(7);
                dressUp.Choose(AvatarCatalog.Find(Slot.Held, "guitar"));
                dressUp.Buy();
                dressUp.ShowTab(4);
                dressUp.Choose(AvatarCatalog.Find(Slot.Emblem, "star"));
                dressUp.Buy();
                yield return AutoPlayer.Settle(1.2f);
                yield return Snap("dressup_bought");

                // Head back while wearing a hat and glasses: the hat lifts and the glasses fade.
                dressUp.ShowTab(6);
                dressUp.Choose(AvatarCatalog.Find(Slot.Glasses, "round"));
                dressUp.Buy();
                GameServices.PendingLevel = 1;
                GameServices.GoTo(SceneNames.DropCatch);
                yield return AutoPlayer.WaitForScene(SceneNames.DropCatch);
                yield return AutoPlayer.Settle(1f);
                yield return Snap("catch_dressed_head_back");

                GameServices.GoToMap();
                yield return AutoPlayer.WaitForScene(SceneNames.ClinicMap);
                yield return AutoPlayer.Settle(1f);
                yield return Snap("map_after");

                // Back to the save files: Save 1 shows the character and what it has earned.
                AutoPlayer.Find<Stage>().Chrome.Home.Click();
                yield return AutoPlayer.WaitForScene(SceneNames.Saves);
                yield return AutoPlayer.Settle(1f);
                yield return Snap("saves_used");
                AutoPlayer.Find<SaveSelectScreen>().DeleteButtons[0].Click();
                yield return AutoPlayer.Settle(0.6f);
                yield return Snap("saves_delete_ask");
                ConfirmDialog.Current.Cancel();
                yield return AutoPlayer.Settle(0.4f);

                // Big text, then the two questions before everything is reset.
                AutoPlayer.Find<Stage>().Chrome.Home.Click();
                yield return AutoPlayer.WaitForScene(SceneNames.Title);
                AutoPlayer.Find<TitleScreen>().SettingsButton.Click();
                yield return AutoPlayer.Settle(0.6f);
                SettingsPanel.Current.Options["Text size"][1].Click();
                SettingsPanel.Current.ResetButton.Click();
                yield return AutoPlayer.Settle(0.6f);
                yield return Snap("settings_reset_ask");
                ConfirmDialog.Current.Confirm();
                yield return AutoPlayer.Settle(0.6f);
                yield return Snap("settings_reset_ask_again");
                ConfirmDialog.Current.Confirm();
                yield return AutoPlayer.WaitForScene(SceneNames.Title);
                yield return AutoPlayer.Settle(1.4f);
                yield return Snap("title_after_reset");
            }
        }

        private IEnumerator Snap(string name)
        {
            shot++;
            yield return rig.Capture($"{shot:000}_{name}");
        }
    }
}
