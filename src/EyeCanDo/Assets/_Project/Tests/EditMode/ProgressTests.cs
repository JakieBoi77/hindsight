using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            var colour = AvatarCatalog.For(Slot.Skin).First(i => i.IsFree && i.Id != p.look.skin);
            Assert.IsTrue(p.Equip(Slot.Skin, colour.Id));
            Assert.AreEqual(colour.Id, p.look.skin);
        }

        [Test]
        public void OldSaveIsUpgradedToScouts()
        {
            // A version 3 save: a realistic skin tone and hairstyle, a crown (still sold), a viking
            // outfit and a cape (retired), worn.
            const string json = "{\"version\":3,\"completedLevels\":[\"eye-drops\"],\"unlockedLevels\":[],\"stickers\":[],\"coins\":10," +
                                "\"owned\":[\"Hat:crown\",\"Outfit:viking\",\"Cape:hero\"]," +
                                "\"look\":{\"skin\":\"#C68A5E\",\"hairStyle\":\"long\",\"hair\":\"#2B1B17\",\"outfit\":\"viking\",\"shirt\":\"#5CBF73\",\"shoes\":\"#FFC845\",\"hat\":\"crown\",\"cape\":\"hero\"}}";
            var slots = Slots();
            File.WriteAllText(slots.PathOf(0), json);
            var loaded = slots.Load(0);
            Assert.IsFalse(slots.RecoveredFromBadData(0), "an old save is upgraded, not thrown away");
            Assert.IsTrue(loaded.IsValid());
            Assert.AreEqual(PlayerProgress.CurrentVersion, loaded.version);
            Assert.AreEqual(10 + 50 + 55, loaded.coins, "retired items are refunded");
            CollectionAssert.AreEqual(new[] { "Hat:crown" }, loaded.owned);
            Assert.AreEqual("crown", loaded.look.hat, "items still sold stay on");
            Assert.AreEqual(AvatarLook.Default.skin, loaded.look.skin, "old skin tones become the default bean colour");
            Assert.AreEqual(AvatarLook.Default.outfit, loaded.look.outfit);
            Assert.AreEqual(1, loaded.completedLevels.Count);
        }

        [Test]
        public void SashesAreRefundedWhenUpgrading()
        {
            // A version 4 save with a bought sash (sashes were removed in version 5).
            var p = new PlayerProgress { version = 4, coins = 5 };
            p.owned.Add("Sash:owl");
            p.owned.Add("Hat:crown");
            p.Upgrade();
            Assert.AreEqual(PlayerProgress.CurrentVersion, p.version);
            Assert.AreEqual(5 + 35, p.coins);
            CollectionAssert.AreEqual(new[] { "Hat:crown" }, p.owned);
            Assert.IsTrue(p.IsValid());
        }

        [Test]
        public void FaceAccessoriesAreRefundedAndOldFitsBecomeTheDefault()
        {
            // A version 6 save with bought glasses, wearing the skirt (both gone in version 7).
            var p = new PlayerProgress { version = 6, coins = 3 };
            p.owned.Add("Accessory:aviators");
            p.look.outfit = "skirt";
            p.Upgrade();
            Assert.AreEqual(3 + 20, p.coins, "bought accessories are refunded");
            CollectionAssert.IsEmpty(p.owned);
            Assert.AreEqual(AvatarCatalog.DefaultOutfit, p.look.outfit);
            Assert.IsTrue(p.IsValid());
        }

        [Test]
        public void EveryBrowStyleHasACellForEveryMood()
        {
            var cells = new HashSet<Rect>();
            for (var s = 0; s < SceneObjects.FaceArt.BrowStyles.Length; s++)
            foreach (Mood mood in System.Enum.GetValues(typeof(Mood)))
                if (mood != Mood.Keep) cells.Add(SceneObjects.FaceArt.Brows(s, mood));
            Assert.AreEqual(SceneObjects.FaceArt.CellCount(SceneObjects.FaceArt.Layer.Brows), cells.Count, "each style and mood has its own cell");
            Assert.AreEqual(SceneObjects.FaceArt.MoodCount, System.Enum.GetValues(typeof(Mood)).Length - 1);
        }

        [Test]
        public void EveryFaceChoiceHasArt()
        {
            CollectionAssert.AreEquivalent(SceneObjects.FaceArt.EyeStyles, AvatarCatalog.For(Slot.Eyes).Select(i => i.Id).ToArray());
            CollectionAssert.AreEquivalent(SceneObjects.FaceArt.MouthStyles, AvatarCatalog.For(Slot.Mouth).Select(i => i.Id).ToArray());
            CollectionAssert.AreEquivalent(SceneObjects.FaceArt.BrowStyles,
                AvatarCatalog.For(Slot.Brows).Select(i => i.Id).Where(id => id != AvatarCatalog.None).ToArray());
            // Every cell fits in its atlas and no two overlap.
            foreach (SceneObjects.FaceArt.Layer layer in System.Enum.GetValues(typeof(SceneObjects.FaceArt.Layer)))
            {
                var count = SceneObjects.FaceArt.CellCount(layer);
                // Shrunk by a hair: neighbouring cells share an edge, which rounding can make look like an overlap.
                var cells = Enumerable.Range(0, count).Select(i => SceneObjects.FaceArt.Cell(layer, i))
                    .Select(r => new Rect(r.x + 1e-4f, r.y + 1e-4f, r.width - 2e-4f, r.height - 2e-4f)).ToList();
                for (var i = 0; i < cells.Count; i++)
                {
                    Assert.IsTrue(cells[i].xMin >= 0f && cells[i].yMin >= -1e-5f && cells[i].xMax <= 1f + 1e-5f && cells[i].yMax <= 1f + 1e-5f,
                        $"{layer} cell {i} is outside the atlas");
                    for (var j = i + 1; j < cells.Count; j++)
                        Assert.IsFalse(cells[i].Overlaps(cells[j]), $"{layer} cells {i} and {j} overlap");
                }
            }
        }

        [Test]
        public void EveryFaceSlotHasPlentyOfFreeChoices()
        {
            foreach (var slot in new[] { Slot.Skin, Slot.Eyes, Slot.Mouth, Slot.Brows })
                Assert.GreaterOrEqual(AvatarCatalog.For(slot).Count(i => i.IsFree), 9, $"{slot}: too few free choices");
        }

        [Test]
        public void DevBuildsTopUpCoinsAndReleaseBuildsStartAtZero()
        {
            Assert.AreEqual(0, new PlayerProgress().coins, "a new player starts with no coins");
            Assert.AreEqual(CoinRules.DevCoins, CoinRules.WithDevTopUp(0, devBuild: true));
            Assert.AreEqual(CoinRules.DevCoins + 5, CoinRules.WithDevTopUp(CoinRules.DevCoins + 5, devBuild: true), "never takes coins away");
            Assert.AreEqual(12, CoinRules.WithDevTopUp(12, devBuild: false));
            var dearest = AvatarCatalog.All.Max(i => i.Price);
            Assert.GreaterOrEqual(CoinRules.DevCoins, dearest, "dev coins buy anything");
        }

        [Test]
        public void ThereIsSomethingFreeAndSomethingToBuyForHeadAndBody()
        {
            foreach (var slot in new[] { Slot.Hat, Slot.Outfit })
            {
                Assert.IsTrue(AvatarCatalog.For(slot).Any(i => i.IsFree && i.Id != AvatarCatalog.None), $"{slot}: nothing free");
                Assert.IsTrue(AvatarCatalog.For(slot).Any(i => !i.IsFree), $"{slot}: nothing to buy");
            }
        }

        [Test]
        public void CatalogKeysAreUnique()
        {
            var keys = new System.Collections.Generic.HashSet<string>();
            foreach (var item in AvatarCatalog.All) Assert.IsTrue(keys.Add(item.Key), "duplicate " + item.Key);
        }
    }

}
