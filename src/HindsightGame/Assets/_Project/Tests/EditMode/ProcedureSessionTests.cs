using System;
using Hindsight.Core.Procedures;
using NUnit.Framework;

namespace Hindsight.Tests.EditMode
{
    public sealed class ProcedureSessionTests
    {
        [Test]
        public void Constructor_RejectsEmptyProcedures()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new ProcedureSession(0));
        }

        [Test]
        public void CompleteCurrentStep_AdvancesAndRaisesStepChanged()
        {
            var session = new ProcedureSession(3);
            var raised = -1;
            session.StepChanged += index => raised = index;

            session.CompleteCurrentStep();

            Assert.That(session.CurrentIndex, Is.EqualTo(1));
            Assert.That(raised, Is.EqualTo(1));
            Assert.That(session.IsComplete, Is.False);
        }

        [Test]
        public void CompletingLastStep_CompletesSessionOnce()
        {
            var session = new ProcedureSession(2);
            var completedCount = 0;
            session.Completed += () => completedCount++;

            session.CompleteCurrentStep();
            session.CompleteCurrentStep();

            Assert.That(session.IsComplete, Is.True);
            Assert.That(session.Progress, Is.EqualTo(1f));
            Assert.That(completedCount, Is.EqualTo(1));
            Assert.Throws<InvalidOperationException>(session.CompleteCurrentStep);
        }

        [Test]
        public void Progress_ReflectsFinishedSteps()
        {
            var session = new ProcedureSession(4);
            session.CompleteCurrentStep();

            Assert.That(session.Progress, Is.EqualTo(0.25f).Within(0.0001f));
            Assert.That(session.IsLastStep, Is.False);
        }
    }
}
