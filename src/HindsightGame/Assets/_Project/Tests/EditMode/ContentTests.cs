using System.Linq;
using Hindsight.Editor.Validation;
using Hindsight.Procedures;
using Hindsight.Procedures.Steps;
using NUnit.Framework;
using UnityEditor;

namespace Hindsight.Tests.EditMode
{
    /// <summary>Guards the authored content so data mistakes fail CI instead of a child's session.</summary>
    public sealed class ContentTests
    {
        [Test]
        public void AllCatalogs_PassValidation()
        {
            var catalogs = ProcedureValidator.FindAllCatalogs();
            Assert.That(catalogs, Is.Not.Empty, "No ProcedureCatalog asset found.");

            foreach (var catalog in catalogs)
            {
                Assert.That(ProcedureValidator.Collect(catalog), Is.Empty, $"{catalog.name} has content problems.");
            }
        }

        [Test]
        public void EyeDrops_IsTheOnlyPlayableLevelAndEndsWithAReward()
        {
            var catalog = ProcedureValidator.FindAllCatalogs().Single();
            var playable = catalog.Procedures.Where(procedure => procedure.IsPlayable).ToList();

            Assert.That(playable.Select(procedure => procedure.Id), Is.EqualTo(new[] { "eye_drops" }));
            Assert.That(catalog.Procedures[0], Is.SameAs(playable[0]), "Eye Drops should be the first level shown.");
            Assert.That(playable[0].Steps.Last(), Is.InstanceOf<RewardStepDefinition>());
            Assert.That(playable[0].Steps.Count(step => step.MarksProcedureComplete), Is.EqualTo(1));
        }

        [Test]
        public void EyeDrops_WarnsAboutColdAndStingBeforeTheDropGoesIn()
        {
            // Development Plan PoC risk 1: children must hear about sensations before they happen.
            var eyeDrops = ProcedureValidator.FindAllCatalogs().Single().Procedures.First(procedure => procedure.Id == "eye_drops");
            var dropIndex = eyeDrops.Steps.ToList().FindIndex(step => step is HoldStillStepDefinition);
            var warnings = eyeDrops.Steps.Take(dropIndex)
                .OfType<DialogueStepDefinition>()
                .SelectMany(step => step.Lines)
                .Select(line => line.Text.ToLowerInvariant())
                .ToList();

            Assert.That(dropIndex, Is.GreaterThan(0));
            Assert.That(warnings.Any(text => text.Contains("cold")), Is.True, "Missing 'cold' warning before the drop.");
            Assert.That(warnings.Any(text => text.Contains("sting")), Is.True, "Missing 'sting' warning before the drop.");
        }

        [Test]
        public void EveryStepViewPrefab_MatchesItsDefinitionType()
        {
            foreach (var guid in AssetDatabase.FindAssets("t:" + nameof(StepDefinition)))
            {
                var step = AssetDatabase.LoadAssetAtPath<StepDefinition>(AssetDatabase.GUIDToAssetPath(guid));
                var viewType = step.ViewPrefab.GetType();
                var expected = viewType.BaseType?.GetGenericArguments().FirstOrDefault();

                Assert.That(expected, Is.Not.Null, $"{viewType.Name} should derive from StepView<T>.");
                Assert.That(expected.IsInstanceOfType(step), Is.True, $"{step.name} uses {viewType.Name}, which expects {expected.Name}.");
            }
        }
    }
}
