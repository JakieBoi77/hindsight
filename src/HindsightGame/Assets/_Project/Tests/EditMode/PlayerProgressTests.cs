using System.IO;
using Hindsight.App;
using Hindsight.Core.Progress;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Hindsight.Tests.EditMode
{
    public sealed class PlayerProgressTests
    {
        [Test]
        public void MarkCompleted_IsIdempotent()
        {
            var progress = new PlayerProgress();

            Assert.That(progress.MarkCompleted("eye_drops"), Is.True);
            Assert.That(progress.MarkCompleted("eye_drops"), Is.False);
            Assert.That(progress.IsCompleted("eye_drops"), Is.True);
        }

        [Test]
        public void Stickers_KeepEarnOrderAndIgnoreDuplicatesAndBlanks()
        {
            var progress = new PlayerProgress(new string[0], new[] { "a", "b", "a", "", null });

            Assert.That(progress.EarnedStickerIds, Is.EqualTo(new[] { "a", "b" }));
            Assert.That(progress.AddSticker("c"), Is.True);
            Assert.That(progress.HasSticker("c"), Is.True);
        }
    }
}
