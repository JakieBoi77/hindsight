using System.Collections;
using System.Linq;
using EyeCanDo.App;
using EyeCanDo.Content;
using EyeCanDo.Core.Avatar;
using EyeCanDo.Core.Content;
using EyeCanDo.Core.Progress;
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
    /// <summary>End-to-end runs of the real scenes, driven through the same handlers touches use.</summary>
    [Timeout(300000)]
    public sealed class GameFlowTests
    {
        [SetUp]
        public void SetUp()
        {
            AutoPlayer.FreshSave();
            Time.timeScale = 1f;
        }

        [TearDown]
        public void TearDown()
        {
            Time.timeScale = 1f;
        }

        [UnityTest]
        public IEnumerator DressUp_FreeChangesAreWorn_AndShopItemsAreTriedOnUntilBought()
        {
            yield return AutoPlayer.Load(SceneNames.ClinicMap);
            AutoPlayer.Find<ClinicMap>().DressUpButton.Click();
            yield return AutoPlayer.WaitForScene(SceneNames.DressUp);
            var screen = AutoPlayer.Find<EyeCanDo.DressUp.DressUpScreen>();

            screen.ShowTab(3);
            screen.Choose(AvatarCatalog.Find(Slot.HairStyle, "curly"));
            Assert.AreEqual("curly", GameServices.Progress.look.hairStyle, "Free choices are worn at once.");

            var crown = AvatarCatalog.Find(Slot.Hat, "crown");
            screen.ShowTab(5);
            screen.Choose(crown);
            Assert.AreEqual(crown, screen.Pending);
            Assert.AreEqual(AvatarCatalog.None, GameServices.Progress.look.hat, "Trying on is not buying.");
            screen.Buy();
            Assert.IsFalse(GameServices.Progress.Owns(crown), "Not enough coins yet.");

            GameServices.Progress.AddCoins(100);
            screen.Buy();
            Assert.IsTrue(GameServices.Progress.Owns(crown));
            Assert.AreEqual("crown", GameServices.Progress.look.hat);
            Assert.AreEqual(100 - crown.Price, GameServices.Progress.coins);

            AutoPlayer.Find<Stage>().Chrome.Home.Click();
            yield return AutoPlayer.WaitForScene(SceneNames.ClinicMap);
        }

        [UnityTest]
        public IEnumerator Saves_PlayLeadsToTheSaveFiles_AndEachSaveKeepsItsOwnPlayer()
        {
            AutoPlayer.FreshData();
            yield return AutoPlayer.Load(SceneNames.Title);
            AutoPlayer.Find<TitleScreen>().PlayButton.Click();
            yield return AutoPlayer.WaitForScene(SceneNames.Saves);
            var screen = AutoPlayer.Find<SaveSelectScreen>();
            Assert.IsTrue(screen.DeleteButtons.All(b => b == null), "A new install has only empty saves.");

            screen.Cards[1].Click();
            yield return AutoPlayer.WaitForScene(SceneNames.ClinicMap);
            Assert.AreEqual(1, GameServices.CurrentSave);
            GameServices.Progress.AddCoins(40);
            GameServices.Save();
            Assert.IsTrue(GameServices.Saves.IsUsed(1));
            Assert.IsFalse(GameServices.Saves.IsUsed(0), "Only the picked save is written.");

            AutoPlayer.Find<ClinicMap>().Stage.Chrome.Home.Click();
            yield return AutoPlayer.WaitForScene(SceneNames.Saves);
            screen = AutoPlayer.Find<SaveSelectScreen>();
            Assert.IsNotNull(screen.DeleteButtons[1]);
            screen.Cards[0].Click();
            yield return AutoPlayer.WaitForScene(SceneNames.ClinicMap);
            Assert.AreEqual(0, GameServices.Progress.coins, "Save 1 is a new player.");
            Assert.AreEqual(40, GameServices.Saves.Peek(1).coins, "Save 2 keeps its coins.");
        }

        [UnityTest]
        public IEnumerator Saves_DeleteAsksFirst_AndOnlyDeletesAfterConfirm()
        {
            GameServices.Progress.AddCoins(25);
            GameServices.Save();
            yield return AutoPlayer.Load(SceneNames.Saves);
            var screen = AutoPlayer.Find<SaveSelectScreen>();
            screen.DeleteButtons[0].Click();
            Assert.IsNotNull(ConfirmDialog.Current, "Deleting asks first.");
            StringAssert.Contains("25 coins", ConfirmDialog.Current.Message, "It says what will be lost.");
            ConfirmDialog.Current.Cancel();
            yield return AutoPlayer.Settle(0.3f);
            Assert.IsTrue(GameServices.Saves.IsUsed(0), "Keep it changes nothing.");

            screen.DeleteButtons[0].Click();
            ConfirmDialog.Current.Confirm();
            yield return AutoPlayer.Settle(0.3f);
            Assert.IsFalse(GameServices.Saves.IsUsed(0));
            Assert.IsNull(screen.DeleteButtons[0], "The card shows a new game.");
            Assert.AreEqual(SceneNames.Saves, UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }

        [UnityTest]
        public IEnumerator Settings_ApplyAtOnce_AndAreKeptForEverySave()
        {
            yield return AutoPlayer.Load(SceneNames.Title);
            AutoPlayer.Find<TitleScreen>().SettingsButton.Click();
            var settings = SettingsPanel.Current;
            Assert.IsNotNull(settings);
            settings.Volume.Slide(0.3f);
            Assert.AreEqual(0.3f, GameServices.Settings.soundVolume, 0.001f);
            settings.Options["Text size"][1].Click();
            Assert.AreEqual(TextSize.Big, GameServices.Settings.textSize);
            Assert.AreEqual(CaptionBar.BigHeight, ((RectTransform)AutoPlayer.Find<CaptionBar>().transform).sizeDelta.y, 0.1f, "Captions grow while settings are open.");
            settings.Options["Reduce motion"][1].Click();
            Assert.IsTrue(GameServices.Settings.reduceMotion);
            settings.Volume.Slide(0f);
            Assert.IsFalse(Sfx.Enabled);

            GameServices.DeleteSave(0);
            Assert.AreEqual(TextSize.Big, GameServices.Settings.textSize, "Deleting a save keeps the settings.");
            settings.Close();
            yield return AutoPlayer.Settle(0.3f);
        }

        [UnityTest]
        public IEnumerator Settings_ResetAsksTwice_ThenDeletesEverything()
        {
            GameServices.Progress.AddCoins(30);
            GameServices.Save();
            GameServices.ChooseSave(2);
            GameServices.Save();
            GameServices.Settings.reduceMotion = true;
            GameServices.SaveSettings();

            yield return AutoPlayer.Load(SceneNames.Title);
            AutoPlayer.Find<TitleScreen>().SettingsButton.Click();
            SettingsPanel.Current.ResetButton.Click();
            var first = ConfirmDialog.Current;
            Assert.IsNotNull(first);
            first.Cancel();
            yield return AutoPlayer.Settle(0.3f);
            Assert.IsTrue(GameServices.Saves.IsUsed(0), "No keeps everything.");

            SettingsPanel.Current.ResetButton.Click();
            ConfirmDialog.Current.Confirm();
            var second = ConfirmDialog.Current;
            Assert.IsNotNull(second, "Yes asks a second time.");
            Assert.AreNotSame(first, second);
            StringAssert.Contains("cannot be undone", second.Message);
            Assert.IsTrue(GameServices.Saves.IsUsed(0), "Nothing is deleted after only one yes.");
            second.Cancel();
            yield return AutoPlayer.Settle(0.3f);
            Assert.IsTrue(GameServices.Saves.IsUsed(0));

            SettingsPanel.Current.ResetButton.Click();
            ConfirmDialog.Current.Confirm();
            ConfirmDialog.Current.Confirm();
            yield return AutoPlayer.WaitForScene(SceneNames.Title);
            for (var slot = 0; slot < SaveSlots.Count; slot++)
            {
                Assert.IsFalse(GameServices.Saves.IsUsed(slot), $"Save {slot + 1} is deleted.");
            }

            Assert.IsFalse(GameServices.Settings.reduceMotion, "Settings are back to new.");
            Assert.IsFalse(System.IO.File.Exists(System.IO.Path.Combine(GameServices.DataFolder, "settings.json")));
            Assert.AreEqual(0, GameServices.Progress.coins);
        }

        [UnityTest]
        public IEnumerator Map_ComingSoonStops_DoNothingHarmful()
        {
            yield return AutoPlayer.Load(SceneNames.ClinicMap);
            var map = AutoPlayer.Find<ClinicMap>();
            map.OpenStop("microscope");
            yield return AutoPlayer.Settle(0.5f);
            Assert.IsFalse(map.Panel.IsOpen);
            Assert.AreEqual(SceneNames.ClinicMap, UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
            map.OpenStop(StopCatalog.EyeDropsId);
            yield return null;
            Assert.IsTrue(map.Panel.IsOpen);
            map.Panel.DropCatchCard.Click();
            yield return AutoPlayer.Settle(0.5f);
            Assert.AreEqual(SceneNames.ClinicMap, UnityEngine.SceneManagement.SceneManager.GetActiveScene().name, "Games stay locked until the story is done.");
        }

        [UnityTest]
        public IEnumerator Story_PlaysToTheEnd_SensationCannotBeSkipped_AndTheStickerIsSaved()
        {
            yield return AutoPlayer.Load(SceneNames.EyeDropsStory);
            var story = AutoPlayer.Find<EyeDropsStory>();
            var stage = story.Stage;
            Time.timeScale = 4f;
            yield return AutoPlayer.PlayStory(story, null, () => story.CurrentKind == StoryStepKind.Sensation);
            Time.timeScale = 1f;
            Assert.IsFalse(stage.Chrome.SkipAllowed);
            stage.Chrome.TriggerSkip();
            Assert.AreEqual(1f, Time.timeScale, "Skip does nothing on the sensation step.");

            Time.timeScale = 4f;
            yield return AutoPlayer.PlayStory(story);
            Time.timeScale = 1f;
            Assert.IsTrue(GameServices.Progress.eyeDropsSticker);
            Assert.AreEqual(StopStatus.Completed, GameServices.Progress.eyeDropsStory);
            Assert.AreEqual(CoinRules.StoryFirstTime, GameServices.Progress.coins);
            Assert.IsTrue(GameServices.Progress.GamesUnlocked);
            Assert.IsNotNull(story.GamesButton);
        }

        [UnityTest]
        public IEnumerator Story_Skip_FastForwardsTheStep()
        {
            yield return AutoPlayer.Load(SceneNames.EyeDropsStory);
            var story = AutoPlayer.Find<EyeDropsStory>();
            yield return AutoPlayer.WaitFor(() => story.Stage.Captions.IsWaitingForTap, 20f, "first line");
            Assert.AreEqual(StoryStepKind.Hello, story.CurrentKind);
            story.Stage.Chrome.TriggerSkip();
            yield return AutoPlayer.WaitFor(() => story.CurrentKind != StoryStepKind.Hello, 10f, "next step");
            Assert.AreEqual(StoryStepKind.MeetMia, story.CurrentKind);
        }

        [UnityTest]
        public IEnumerator Story_ATapShowsTheWholeLine_AndTheNextTapContinues()
        {
            yield return AutoPlayer.Load(SceneNames.EyeDropsStory);
            var captions = AutoPlayer.Find<EyeDropsStory>().Stage.Captions;
            yield return AutoPlayer.WaitFor(() => captions.IsReading, 20f, "first line");
            var line = captions.CurrentText;
            captions.SimulateTap();
            yield return AutoPlayer.WaitFor(() => captions.IsWaitingForTap, 0.5f, "tap to finish the line");
            Assert.AreEqual(line, captions.CurrentText, "The first tap only finishes the line.");
            captions.SimulateTap();
            yield return AutoPlayer.WaitFor(() => captions.CurrentText != line, 10f, "next line");
        }

        [UnityTest]
        public IEnumerator Games_OneTapPastTheInstruction_ThenCountDownAndStart()
        {
            AutoPlayer.StoryDone();
            GameServices.PendingLevel = 1;
            yield return AutoPlayer.Load(SceneNames.DropCatch);
            var game = AutoPlayer.Find<DropCatchGame>();
            var captions = AutoPlayer.Find<Stage>().Captions;
            yield return AutoPlayer.WaitFor(() => captions.IsWaitingForTap, 1f, "the instruction, at once");
            Assert.IsFalse(game.Live);
            captions.SimulateTap();
            yield return AutoPlayer.WaitFor(() => game.CountingDown == 3, 0.5f, "3");
            yield return AutoPlayer.WaitFor(() => game.CountingDown == 1, 2f, "1");
            Assert.AreEqual(0f, game.Round.Elapsed, "The clock waits for Go.");
            yield return AutoPlayer.WaitFor(() => game.Live, 2f, "go");
        }

        [UnityTest]
        public IEnumerator DropCatch_PerfectPlay_EarnsThreeStars_AndCoins()
        {
            AutoPlayer.StoryDone();
            var coinsBefore = GameServices.Progress.coins;
            GameServices.PendingLevel = 1;
            yield return AutoPlayer.Load(SceneNames.DropCatch);
            var game = AutoPlayer.Find<DropCatchGame>();
            yield return AutoPlayer.WaitFor(() => game.IsPlaying, 5f, "start");
            Time.timeScale = 3f;
            yield return AutoPlayer.PlayDropCatch(game);
            Time.timeScale = 1f;
            yield return AutoPlayer.WaitFor(() => game.ShowingResults, 5f, "results");
            Assert.AreEqual(0, game.Round.Missed, "Steering under every drop catches them all.");
            Assert.AreEqual(3, game.LastStars, "A perfect round earns three stars.");
            Assert.AreEqual(coinsBefore + game.LastCoins, GameServices.Progress.coins);
            Assert.Greater(game.LastCoins, 0);
            Assert.AreEqual(game.LastScore, GameServices.Progress.dropCatch.ScoreAt(1));
            Assert.AreEqual(1, GameServices.Progress.dropCatch.plays);
        }

        [UnityTest]
        public IEnumerator BlurrySpy_AWrongTapCostsTheThirdStar_AndTheBlurClears()
        {
            AutoPlayer.StoryDone();
            GameServices.PendingLevel = 1;
            yield return AutoPlayer.Load(SceneNames.BlurrySpy);
            var game = AutoPlayer.Find<BlurrySpyGame>();
            yield return AutoPlayer.WaitFor(() => game.IsPlaying, 5f, "start");
            yield return AutoPlayer.PlayBlurrySpy(game);
            yield return AutoPlayer.WaitFor(() => game.ShowingResults, 5f, "results");
            Assert.AreEqual(0f, game.Round.Blur, 0.001f);
            Assert.AreEqual(1, game.Round.WrongTaps);
            Assert.LessOrEqual(game.LastStars, 2, "Three stars need a perfect round.");
            Assert.GreaterOrEqual(GameServices.Progress.blurrySpy.StarsAt(1), 1);
        }

        [UnityTest]
        public IEnumerator Games_OpenedBeforeTheStory_ShowTheLock()
        {
            yield return AutoPlayer.Load(SceneNames.DropCatch);
            var game = AutoPlayer.Find<DropCatchGame>();
            yield return AutoPlayer.WaitFor(() => game.ShowingLocked, 5f, "lock");
            game.StartLevel(1);
            Assert.IsFalse(game.IsPlaying);
        }

        [UnityTest]
        public IEnumerator Home_FromAGame_GoesBackToTheMap()
        {
            AutoPlayer.StoryDone();
            yield return AutoPlayer.Load(SceneNames.DropCatch);
            var stage = AutoPlayer.Find<Stage>();
            stage.Chrome.Home.Click();
            yield return AutoPlayer.WaitForScene(SceneNames.ClinicMap);
            var map = AutoPlayer.Find<ClinicMap>();
            yield return null;
            Assert.IsTrue(map.Panel.IsOpen, "Home from a game reopens the Special Drops panel.");
        }
    }
}
