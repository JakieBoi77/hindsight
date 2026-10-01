using System.IO;
using Hindsight.App;
using Hindsight.Core.Progress;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Hindsight.Tests.EditMode
{
    public sealed class JsonFileProgressStoreTests
    {
        private string filePath;

        [SetUp]
        public void SetUp()
        {
            filePath = Path.Combine(Path.GetTempPath(), $"hindsight-progress-{System.Guid.NewGuid():N}.json");
        }

        [TearDown]
        public void TearDown()
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        [Test]
        public void Load_WithoutFile_ReturnsFreshProgress()
        {
            var progress = new JsonFileProgressStore(filePath).Load();

            Assert.That(progress.CompletedProcedureIds, Is.Empty);
        }

        [Test]
        public void SaveThenLoad_RoundTrips()
        {
            var store = new JsonFileProgressStore(filePath);
            var progress = new PlayerProgress();
            progress.MarkCompleted("eye_drops");
            progress.AddSticker("sticker_brave_eyes");

            store.Save(progress);
            var loaded = new JsonFileProgressStore(filePath).Load();

            Assert.That(loaded.IsCompleted("eye_drops"), Is.True);
            Assert.That(loaded.EarnedStickerIds, Is.EqualTo(new[] { "sticker_brave_eyes" }));
        }

        [Test]
        public void CorruptFile_FallsBackToFreshProgressWithWarning()
        {
            File.WriteAllText(filePath, "{ this is not json");

            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("Could not read progress file"));
            var progress = new JsonFileProgressStore(filePath).Load();

            Assert.That(progress.CompletedProcedureIds, Is.Empty);
        }
    }
}
