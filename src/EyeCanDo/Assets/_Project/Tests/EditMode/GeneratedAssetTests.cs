using System;
using System.Linq;
using EyeCanDo.App;
using EyeCanDo.Art;
using EyeCanDo.EditorTools;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace EyeCanDo.Tests.EditMode
{
    /// <summary>The libraries Build All generates load and are complete (EyeCanDo → Build All).</summary>
    public sealed class GeneratedAssetTests
    {
        [Test]
        public void SoundLibrary_HasAClipForEverySound()
        {
            var library = Resources.Load<SoundLibrary>(SoundLibrary.ResourcePath);
            Assert.IsNotNull(library, "SoundLibrary must load from Resources.");
            foreach (Sound sound in Enum.GetValues(typeof(Sound)))
            {
                var entry = library.Find(sound);
                Assert.IsNotNull(entry, $"No entry for {sound}.");
                Assert.IsTrue(entry.clips.Length > 0 && entry.clips.All(c => c != null), $"Missing clip for {sound}.");
            }
        }

        [TestCase("Characters/doctor_head")]
        [TestCase("Avatar/eyes_round_up_white")]
        [TestCase("Avatar/hair_curly_back")]
        [TestCase("Avatar/hat_crown")]
        [TestCase("Eye/closeup_lid")]
        [TestCase("Icons/home")]
        [TestCase("Spy/duck")]
        [TestCase("Props/bottle_buddy")]
        public void ArtLibrary_FindsTheArt(string name)
        {
            var library = Resources.Load<ArtLibrary>(ArtLibrary.ResourcePath);
            Assert.IsNotNull(library);
            Assert.IsNotNull(library.Find(name));
        }

        [Test]
        public void EverySvg_IsInTheArtLibrary_AtASharpResolution()
        {
            var library = Resources.Load<ArtLibrary>(ArtLibrary.ResourcePath);
            var svgs = AssetDatabase.FindAssets("", new[] { PocBuilder.ArtFolder })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(p => p.EndsWith(".svg"))
                .ToList();
            Assert.Greater(svgs.Count, 100);
            Assert.AreEqual(svgs.Count, library.sprites.Count);
            foreach (var entry in library.sprites)
            {
                var texture = entry.sprite.texture;
                Assert.GreaterOrEqual(Mathf.Max(texture.width, texture.height), 512, $"{entry.name} is rendered too small to stay sharp.");
            }
        }

        [Test]
        public void BlurMaterial_UsesTheBlurShader()
        {
            var material = Resources.Load<Material>(EyeCanDo.Games.BlurrySpyGame.BlurMaterialPath);
            Assert.IsNotNull(material);
            Assert.AreEqual(PocBuilder.BlurShader, material.shader.name);
        }
    }
}
