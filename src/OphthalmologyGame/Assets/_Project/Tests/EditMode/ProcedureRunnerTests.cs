using System.Collections;
using System.Collections.Generic;
using Hindsight.Ophthalmology.Procedure;
using NUnit.Framework;
using UnityEngine;

namespace Hindsight.Ophthalmology.Tests
{
    public sealed class ProcedureRunnerTests
    {
        sealed class RecordingStep : ProcedureStep
        {
            public List<string> Log;
            public int Frames;

            public override IEnumerator Run(IProcedureContext context)
            {
                Log.Add(name + ":start");
                for (var i = 0; i < Frames; i++) yield return null;
                Log.Add(name + ":end");
            }
        }

        static RecordingStep Step(string name, List<string> log, int frames)
        {
            var step = ScriptableObject.CreateInstance<RecordingStep>();
            step.name = name;
            step.Log = log;
            step.Frames = frames;
            return step;
        }

        /// <summary>Drives nested coroutines to completion the way Unity's scheduler would.</summary>
        static void Drain(IEnumerator routine)
        {
            var stack = new Stack<IEnumerator>();
            stack.Push(routine);
            var guard = 0;
            while (stack.Count > 0 && guard++ < 10000)
            {
                var top = stack.Peek();
                if (!top.MoveNext()) { stack.Pop(); continue; }
                if (top.Current is IEnumerator nested) stack.Push(nested);
            }
            Assert.Less(guard, 10000, "procedure never finished");
        }

        [Test]
        public void RunsStepsInOrderAndWaitsForEach()
        {
            var log = new List<string>();
            var steps = new List<ProcedureStep> { Step("a", log, 3), Step("b", log, 0), Step("c", log, 1) };

            Drain(ProcedureRunner.RunSteps(steps, null));

            CollectionAssert.AreEqual(new[] { "a:start", "a:end", "b:start", "b:end", "c:start", "c:end" }, log);
        }

        [Test]
        public void SkipsNullStepsAndReportsProgress()
        {
            var log = new List<string>();
            var steps = new List<ProcedureStep> { Step("a", log, 0), null, Step("c", log, 0) };
            var started = new List<int>();

            Drain(ProcedureRunner.RunSteps(steps, null, (i, n, _) => started.Add(i)));

            CollectionAssert.AreEqual(new[] { 0, 2 }, started);
            CollectionAssert.AreEqual(new[] { "a:start", "a:end", "c:start", "c:end" }, log);
        }
    }
}
