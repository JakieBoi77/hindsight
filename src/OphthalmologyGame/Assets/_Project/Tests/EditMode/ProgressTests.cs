using System.IO;
using Hindsight.Ophthalmology.Core.Avatar;
using Hindsight.Ophthalmology.Core.Progress;
using NUnit.Framework;
using UnityEngine;

namespace Hindsight.Ophthalmology.Tests
{
    /// <summary>Save files, settings, coins and the dress-up shop (ported and trimmed from the 2D PoC).</summary>
    public sealed class ProgressTests
    {
        string folder;

        [SetUp]
        public void SetUp()
        {
            folder = Path.Combine(Path.GetTempPath(), "hindsight-progress-tests-" + System.Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(folder);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(folder)) Directory.Delete(folder, true);
        }

        SaveSlots Slots() => new(folder, p => JsonUtility.ToJson(p, true), JsonUtility.FromJson<PlayerProgress>);

        static AvatarItem Shop(Slot slot) => AvatarCatalog.For(slot).First(i => !i.IsFree);

        [Test]
        public void NewPlayerIsValidAndStartsWithNothing()
        {
            var p = new PlayerProgress();
            Assert.IsTrue(p.IsValid());
            Assert.AreEqual(0, p.coins);
            Assert.IsEmpty(p.completedLevels);
            Assert.IsTrue(AvatarLook.Default.IsValid());
            Assert.IsTrue(AvatarLook.Sam.IsValid());
        }

        [Test]
        public void SaveAndLoadRoundTrips()
        {
            var slots = Slots();
            var p = new PlayerProgress();
            p.FinishLevel("eye-drops", "eye-chart");
            p.AddCoins(100);
            Assert.IsTrue(p.TryBuy(Shop(Slot.Hat)));
            p.Equip(Slot.Hat, Shop(Slot.Hat).Id);
            slots.Save(1, p);

            var loaded = slots.Load(1);
            Assert.IsFalse(slots.RecoveredFromBadData(1));
            Assert.AreEqual(p.coins, loaded.coins);
            CollectionAssert.AreEqual(p.completedLevels, loaded.completedLevels);
            CollectionAssert.AreEqual(p.owned, loaded.owned);
            Assert.AreEqual(Shop(Slot.Hat).Id, loaded.look.hat);
            Assert.IsTrue(loaded.IsUnlocked("eye-chart", false));
        }

        [Test]
        public void SlotsAreIndependent()
        {
            var slots = Slots();
            var p = new PlayerProgress();
            p.AddCoins(7);
            slots.Save(0, p);
            Assert.IsTrue(slots.IsUsed(0));
            Assert.IsFalse(slots.IsUsed(1));
            Assert.IsNull(slots.Peek(2));
            Assert.AreEqual(0, slots.Load(2).coins);
        }

        [Test]
        public void DamagedSaveIsTreatedAsEmpty()
        {
            var slots = Slots();
            File.WriteAllText(slots.PathOf(0), "{ this is not json");
            Assert.IsNull(slots.Peek(0));
            var loaded = slots.Load(0);
            Assert.IsTrue(slots.RecoveredFromBadData(0));
            Assert.IsTrue(loaded.IsValid());
        }

        [Test]
        public void InvalidDataIsRejected()
        {
            var slots = Slots();
            var p = new PlayerProgress();
            p.owned.Add("Hat:not-a-hat");
            File.WriteAllText(slots.PathOf(0), JsonUtility.ToJson(p));
            Assert.IsNull(slots.Peek(0));
        }

        [Test]
        public void DeleteOneAndDeleteAll()
        {
            var slots = Slots();
            for (var i = 0; i < SaveSlots.Count; i++) slots.Save(i, new PlayerProgress());
            slots.Delete(1);
            Assert.IsTrue(slots.IsUsed(0));
            Assert.IsFalse(slots.IsUsed(1));
            slots.DeleteAll();
            for (var i = 0; i < SaveSlots.Count; i++) Assert.IsFalse(slots.IsUsed(i));
        }

        [Test]
        public void SettingsRoundTripAndValidate()
        {
            var file = new DataFile<GameSettings>(Path.Combine(folder, "settings.json"), s => JsonUtility.ToJson(s), JsonUtility.FromJson<GameSettings>, s => s.IsValid());
            var s = new GameSettings { soundVolume = 0.3f, voiceVolume = 0.6f, captionsOn = false, bigText = true, lastSave = 2 };
            file.Save(s);
            var loaded = file.Load();
            Assert.AreEqual(0.3f, loaded.soundVolume, 1e-4f);
            Assert.AreEqual(0.6f, loaded.voiceVolume, 1e-4f);
            Assert.IsFalse(loaded.captionsOn);
            Assert.IsTrue(loaded.bigText);
            Assert.AreEqual(2, loaded.lastSave);

            Assert.IsFalse(new GameSettings { soundVolume = 2f }.IsValid());
            Assert.IsFalse(new GameSettings { lastSave = SaveSlots.Count }.IsValid());
        }

        [Test]
        public void LevelPaysMoreTheFirstTimeAndGivesTheSticker()
        {
            var p = new PlayerProgress();
            Assert.AreEqual(CoinRules.LevelFirstTime, p.FinishLevel("eye-drops", "eye-chart"));
            Assert.AreEqual(CoinRules.LevelAgain, p.FinishLevel("eye-drops", "eye-chart"));
            Assert.AreEqual(CoinRules.LevelFirstTime + CoinRules.LevelAgain, p.coins);
            Assert.IsTrue(p.HasSticker("eye-drops"));
            Assert.AreEqual(1, p.completedLevels.Count);
            Assert.AreEqual(1, p.unlockedLevels.Count);
        }

        [Test]
        public void CoinsAreNeverTakenAway()
        {
            var p = new PlayerProgress();
            p.AddCoins(10);
            p.AddCoins(-50);
            Assert.AreEqual(10, p.coins);
        }

        [Test]
        public void ShopNeedsEnoughCoinsAndOnlySellsOnce()
        {
            var p = new PlayerProgress();
            var hat = Shop(Slot.Hat);
            Assert.IsFalse(p.TryBuy(hat), "no coins yet");
            Assert.IsFalse(p.Equip(Slot.Hat, hat.Id), "cannot wear what is not bought");

            p.AddCoins(hat.Price);
            Assert.IsTrue(p.TryBuy(hat));
            Assert.AreEqual(0, p.coins);
            Assert.IsFalse(p.TryBuy(hat), "already owned");
            Assert.IsTrue(p.Equip(Slot.Hat, hat.Id));
            Assert.IsTrue(p.IsValid());
        }

        [Test]
        public void FreeItemsCanBeWornWithoutBuying()
        {
            var p = new PlayerProgress();
            var colour = AvatarCatalog.For(Slot.Shirt).First(i => i.IsFree && i.Id != p.look.shirt);
            Assert.IsTrue(p.Equip(Slot.Shirt, colour.Id));
            Assert.AreEqual(colour.Id, p.look.shirt);
        }

        [Test]
        public void CatalogKeysAreUnique()
        {
            var keys = new System.Collections.Generic.HashSet<string>();
            foreach (var item in AvatarCatalog.All) Assert.IsTrue(keys.Add(item.Key), "duplicate " + item.Key);
        }
    }

    static class EnumerableExtensions
    {
        public static T First<T>(this System.Collections.Generic.IEnumerable<T> items, System.Func<T, bool> match)
        {
            foreach (var item in items)
                if (match(item)) return item;
            throw new System.InvalidOperationException("no match");
        }
    }
}
