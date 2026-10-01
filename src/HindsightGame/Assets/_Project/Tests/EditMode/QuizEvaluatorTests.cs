using System;
using Hindsight.Core.Interactions;
using NUnit.Framework;

namespace Hindsight.Tests.EditMode
{
    public sealed class QuizEvaluatorTests
    {
        [Test]
        public void WrongAnswer_AsksToTryAgainWithoutSolving()
        {
            var quiz = new QuizEvaluator(3, 1);

            Assert.That(quiz.Submit(0), Is.EqualTo(QuizAnswerResult.TryAgain));
            Assert.That(quiz.IsSolved, Is.False);
            Assert.That(quiz.WrongAttempts, Is.EqualTo(1));
        }

        [Test]
        public void CorrectAnswer_SolvesQuiz()
        {
            var quiz = new QuizEvaluator(3, 2);

            Assert.That(quiz.Submit(2), Is.EqualTo(QuizAnswerResult.Correct));
            Assert.That(quiz.IsSolved, Is.True);
        }

        [TestCase(1, 0)]
        [TestCase(3, 3)]
        [TestCase(3, -1)]
        public void InvalidConfiguration_IsRejected(int answerCount, int correctIndex)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new QuizEvaluator(answerCount, correctIndex));
        }

        [Test]
        public void OutOfRangeAnswer_IsRejected()
        {
            var quiz = new QuizEvaluator(2, 0);

            Assert.Throws<ArgumentOutOfRangeException>(() => quiz.Submit(5));
        }
    }
}
