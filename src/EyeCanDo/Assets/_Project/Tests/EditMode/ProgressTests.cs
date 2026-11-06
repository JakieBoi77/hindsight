using System.IO;
using EyeCanDo.Core.Avatar;
using EyeCanDo.Core.Games;
using EyeCanDo.Core.Progress;
using NUnit.Framework;
using UnityEngine;

namespace EyeCanDo.Tests.EditMode
{
    public sealed class GameRecordTests
    {
        [Test]
        public void Bests_OnlyEverGoUp()
        {
            var record = new GameRecord();
            var first = record.Record(1, 2, 30);
            Assert.IsTrue(first.NewBestStars);
            Assert.IsTrue(first.NewBestScore);

            var worse = record.Record(1, 1, 10);
            Assert.IsFalse(worse.NewBestStars);
            Assert.IsFalse(worse.NewBestScore);
            Assert.AreEqual(2, record.StarsAt(1), "Stars are never taken away (thm_NoLoss).");
            Assert.AreEqual(30, record.ScoreAt(1));
            Assert.AreEqual(2, record.plays);
        }

        [Test]
        public void SuggestedLevel_IsTheFirstWithoutTwoStars()
        {
            var record = new GameRecord();
            Assert.AreEqual(1, record.SuggestedLevel);
            record.Record(1, 2, 10);
            Assert.AreEqual(2, record.SuggestedLevel);
            record.Record(2, 3, 10);
            record.Record(3, 2, 10);
            Assert.AreEqual(3, record.SuggestedLevel);
            Assert.AreEqual(7, record.TotalStars);
        }

        [Test]
        public void OutOfRangeInput_IsClamped()
        {
            var record = new GameRecord();
            record.Record(9, 7, -5);
            Assert.AreEqual(3, record.StarsAt(3));
            Assert.AreEqual(0, record.ScoreAt(3));
            Assert.IsTrue(record.IsValid());
        }
    }

    public sealed class PlayerProgressTests
    {
        private string folder;

        [SetUp]
        public void SetUp()
        {
            folder = Path.Combine(Path.GetTempPath(), "EyeCanDoTests_" + System.Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(folder);
        }

        [TearDown]
        public void TearDown()
        {
            Directory.Delete(folder, true);
        }

        private ProgressFile File(string name = "progress.json")
        {
            return new ProgressFile(Path.Combine(folder, name), p => JsonUtility.ToJson(p), JsonUtility.FromJson<PlayerProgress>);
        }

        [Test]
        public void Story_StatusAndSticker_AreGivenOnce()
        {
            var progress = new PlayerProgress();
            progress.StartStory();
            Assert.AreEqual(StopStatus.InProgress, progress.eyeDropsStory);
            Assert.IsTrue(progress.FinishStory());
            Assert.IsFalse(progress.FinishStory(), "No duplicate sticker on replay.");
            progress.StartStory();
            Assert.AreEqual(StopStatus.Completed, progress.eyeDropsStory, "Replaying keeps Completed.");
        }

        [Test]
        public void Story_PaysCoins_MostTheFirstTime()
        {
            var progress = new PlayerProgress();
            progress.FinishStory();
            Assert.AreEqual(CoinRules.StoryFirstTime, progress.coins);
            progress.FinishStory();
            Assert.AreEqual(CoinRules.StoryFirstTime + CoinRules.StoryAgain, progress.coins);
        }

        [Test]
        public void Games_UnlockOnlyAfterTheStory()
        {
            var progress = new PlayerProgress();
            Assert.IsFalse(progress.GamesUnlocked);
            progress.StartStory();
            Assert.IsFalse(progress.GamesUnlocked);
            progress.FinishStory();
            Assert.IsTrue(progress.GamesUnlocked);
        }

        [Test]
        public void Rounds_PayCoins_WithABonusForNewStars()
        {
            var progress = new PlayerProgress();
            var first = progress.FinishRound(GameId.DropCatch, 1, 2, 10);
            Assert.AreEqual(CoinRules.ForRound(2, 2), first.Coins);
            var same = progress.FinishRound(GameId.DropCatch, 1, 2, 10);
            Assert.AreEqual(CoinRules.ForRound(2, 0), same.Coins, "No bonus for stars already earned.");
            var better = progress.FinishRound(GameId.DropCatch, 1, 3, 12);
            Assert.AreEqual(1, better.Record.NewStars);
            Assert.AreEqual(first.Coins + same.Coins + better.Coins, progress.coins);
        }

        [Test]
        public void Shop_BuysOnlyWithEnoughCoins_AndOnlyOnce()
        {
            var progress = new PlayerProgress();
            var crown = AvatarCatalog.Find(Slot.Hat, "crown");
            Assert.IsFalse(progress.Equip(Slot.Hat, "crown"), "Shop items must be bought before wearing.");
            Assert.IsFalse(progress.TryBuy(crown));
            progress.AddCoins(crown.Price + 5);
            Assert.IsTrue(progress.TryBuy(crown));
            Assert.AreEqual(5, progress.coins);
            Assert.IsFalse(progress.TryBuy(crown), "Already owned.");
            Assert.IsTrue(progress.Equip(Slot.Hat, "crown"));
            Assert.AreEqual("crown", progress.look.hat);
            Assert.IsTrue(progress.IsValid());
        }

        [Test]
        public void FaceHairAndColours_AreFree()
        {
            var progress = new PlayerProgress();
            Assert.IsTrue(progress.Equip(Slot.HairStyle, "curly"));
            Assert.IsTrue(progress.Equip(Slot.HairColor, "#E77BB0"));
            Assert.IsTrue(progress.Equip(Slot.EyeStyle, "lashes"));
            Assert.IsTrue(progress.Equip(Slot.Skin, "#6B4128"));
            Assert.IsFalse(progress.Equip(Slot.HairStyle, "mohawk"), "Unknown styles are refused.");
            Assert.IsTrue(progress.IsValid());
        }

        [Test]
        public void SaveAndLoad_RoundTrips()
        {
            var file = File();
            var progress = new PlayerProgress();
            progress.FinishStory();
            progress.For(GameId.DropCatch).Record(2, 3, 42);
            progress.AddCoins(100);
            progress.TryBuy(AvatarCatalog.Find(Slot.Glasses, "star"));
            progress.Equip(Slot.Glasses, "star");
            progress.Equip(Slot.HairStyle, "bun");
            file.Save(progress);
            file.Save(progress);

            var loaded = file.Load();
            Assert.IsFalse(file.RecoveredFromBadData);
            Assert.IsTrue(loaded.eyeDropsSticker);
            Assert.AreEqual("star", loaded.look.glasses);
            Assert.AreEqual("bun", loaded.look.hairStyle);
            Assert.AreEqual(progress.coins, loaded.coins);
            CollectionAssert.Contains(loaded.owned, "Glasses:star");
            Assert.AreEqual(3, loaded.dropCatch.StarsAt(2));
            Assert.AreEqual(42, loaded.dropCatch.ScoreAt(2));
        }

        [Test]
        public void MissingFile_StartsFresh()
        {
            var loaded = File("none.json").Load();
            Assert.AreEqual(StopStatus.NotStarted, loaded.eyeDropsStory);
            Assert.AreEqual(0, loaded.coins);
            Assert.AreEqual(AvatarLook.Default.hairStyle, loaded.look.hairStyle);
        }

        [TestCase("not json at all")]
        [TestCase("{\"version\": 99}")]
        [TestCase("{\"version\": 1, \"eyeDropsSticker\": true, \"eyeDropsStory\": 2}")]
        [TestCase("{\"version\": 2, \"eyeDropsSticker\": true, \"eyeDropsStory\": 2}")]
        [TestCase("{\"version\": 3, \"eyeDropsSticker\": true, \"eyeDropsStory\": 0}")]
        [TestCase("{\"version\": 3, \"coins\": -4}")]
        [TestCase("{\"version\": 3, \"look\": {\"hat\": \"crown\"}}")]
        public void DamagedOrOldData_IsReplaced_WithoutThrowing(string contents)
        {
            var file = File();
            System.IO.File.WriteAllText(file.Path, contents);
            var loaded = file.Load();
            Assert.IsTrue(file.RecoveredFromBadData);
            Assert.IsTrue(loaded.IsValid());
            Assert.IsFalse(loaded.eyeDropsSticker);
        }

        [Test]
        public void LeftoverTempFile_DoesNotBreakSaving()
        {
            var file = File();
            System.IO.File.WriteAllText(file.Path + ".tmp", "half-written");
            file.Save(new PlayerProgress());
            Assert.IsTrue(file.Load().IsValid());
        }
    }

    public sealed class SaveSlotTests
    {
        private string folder;

        [SetUp]
        public void SetUp()
        {
            folder = Path.Combine(Path.GetTempPath(), "EyeCanDoSlots_" + System.Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(folder);
        }

        [TearDown]
        public void TearDown()
        {
            Directory.Delete(folder, true);
        }

        private SaveSlots Slots()
        {
            return new SaveSlots(Path.Combine(folder, "saves"), p => JsonUtility.ToJson(p), JsonUtility.FromJson<PlayerProgress>);
        }

        [Test]
        public void NewInstall_HasOnlyEmptySlots()
        {
            var slots = Slots();
            for (var slot = 0; slot < SaveSlots.Count; slot++)
            {
                Assert.IsFalse(slots.IsUsed(slot));
                Assert.IsNull(slots.Peek(slot));
                Assert.AreEqual(0, slots.Load(slot).coins, "An empty slot loads as a new player.");
            }
        }

        [Test]
        public void EachSlot_KeepsItsOwnPlayer()
        {
            var slots = Slots();
            var first = new PlayerProgress();
            first.AddCoins(10);
            var third = new PlayerProgress();
            third.FinishStory();
            slots.Save(0, first);
            slots.Save(2, third);

            Assert.AreEqual(10, slots.Peek(0).coins);
            Assert.IsNull(slots.Peek(1));
            Assert.IsTrue(slots.Peek(2).eyeDropsSticker);
            Assert.IsFalse(slots.Peek(0).eyeDropsSticker);
        }

        [Test]
        public void Delete_RemovesOnlyThatSlot()
        {
            var slots = Slots();
            slots.Save(0, new PlayerProgress());
            slots.Save(1, new PlayerProgress());
            slots.Delete(0);
            Assert.IsFalse(slots.IsUsed(0));
            Assert.IsTrue(slots.IsUsed(1));
        }

        [Test]
        public void DeleteAll_EmptiesEverySlot()
        {
            var slots = Slots();
            for (var slot = 0; slot < SaveSlots.Count; slot++)
            {
                slots.Save(slot, new PlayerProgress());
            }

            slots.DeleteAll();
            for (var slot = 0; slot < SaveSlots.Count; slot++)
            {
                Assert.IsFalse(slots.IsUsed(slot));
            }

            Assert.IsEmpty(Directory.GetFiles(slots.Folder));
        }

        [Test]
        public void DamagedSlot_ShowsAsEmpty()
        {
            var slots = Slots();
            slots.Save(1, new PlayerProgress());
            File.WriteAllText(slots.PathOf(1), "{ broken");
            Assert.IsNull(slots.Peek(1));
            Assert.IsTrue(slots.Load(1).IsValid());
        }

        [Test]
        public void EarlierSingleSave_MovesIntoTheFirstSlotOnce()
        {
            var legacy = Path.Combine(folder, "progress.json");
            var old = new PlayerProgress();
            old.AddCoins(77);
            File.WriteAllText(legacy, JsonUtility.ToJson(old));
            var slots = Slots();

            Assert.IsTrue(slots.AdoptLegacy(legacy));
            Assert.AreEqual(77, slots.Peek(0).coins);
            Assert.IsFalse(File.Exists(legacy));
            Assert.IsFalse(slots.AdoptLegacy(legacy), "Only once.");
        }

        [Test]
        public void SlotsOutOfRange_AreRefused()
        {
            var slots = Slots();
            Assert.Throws<System.ArgumentOutOfRangeException>(() => slots.Peek(SaveSlots.Count));
            Assert.Throws<System.ArgumentOutOfRangeException>(() => slots.Delete(-1));
        }
    }

    public sealed class GameSettingsTests
    {
        [Test]
        public void Defaults_AreFullSoundNormalTextAndMotion()
        {
            var settings = new GameSettings();
            Assert.IsTrue(settings.IsValid());
            Assert.IsTrue(settings.SoundOn);
            Assert.AreEqual(1f, settings.TextScale);
            Assert.AreEqual(1f, settings.RevealSpeed);
            Assert.IsFalse(settings.autoContinue);
            Assert.IsFalse(settings.reduceMotion);
        }

        [Test]
        public void SaveAndLoad_RoundTrips_AndBadDataGoesBackToDefaults()
        {
            var path = Path.Combine(Path.GetTempPath(), "EyeCanDoSettings_" + System.Guid.NewGuid().ToString("N") + ".json");
            try
            {
                var file = new SettingsFile(path, s => JsonUtility.ToJson(s), JsonUtility.FromJson<GameSettings>);
                var settings = new GameSettings { soundVolume = 0.4f, textSize = TextSize.Big, textSpeed = TextSpeed.Fast, autoContinue = true, reduceMotion = true, lastSave = 2 };
                file.Save(settings);
                var loaded = file.Load();
                Assert.AreEqual(0.4f, loaded.soundVolume, 0.0001f);
                Assert.AreEqual(TextSize.Big, loaded.textSize);
                Assert.AreEqual(TextSpeed.Fast, loaded.textSpeed);
                Assert.IsTrue(loaded.autoContinue && loaded.reduceMotion);
                Assert.AreEqual(2, loaded.lastSave);
                Assert.Greater(loaded.TextScale, 1f);
                Assert.Greater(loaded.RevealSpeed, 1f);

                File.WriteAllText(path, "{\"version\": 1, \"soundVolume\": 3, \"lastSave\": 9}");
                var recovered = file.Load();
                Assert.IsTrue(file.RecoveredFromBadData);
                Assert.AreEqual(1f, recovered.soundVolume);
                Assert.AreEqual(0, recovered.lastSave);
            }
            finally
            {
                File.Delete(path);
            }
        }
    }
}
