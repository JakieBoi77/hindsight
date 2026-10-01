using System;

namespace Hindsight.Core.Interactions
{
    public enum QuizAnswerResult
    {
        Correct,
        TryAgain,
    }

    /// <summary>
    /// Grades a single multiple-choice question. Wrong answers never end the quiz;
    /// they prompt another try so the activity stays encouraging.
    /// </summary>
    public sealed class QuizEvaluator
    {
        public QuizEvaluator(int answerCount, int correctIndex)
        {
            if (answerCount < 2)
            {
                throw new ArgumentOutOfRangeException(nameof(answerCount), answerCount, "A question needs at least two answers.");
            }

            if (correctIndex < 0 || correctIndex >= answerCount)
            {
                throw new ArgumentOutOfRangeException(nameof(correctIndex), correctIndex, "Correct answer index is outside the answer list.");
            }

            AnswerCount = answerCount;
            CorrectIndex = correctIndex;
        }

        public int AnswerCount { get; }

        public int CorrectIndex { get; }

        public int WrongAttempts { get; private set; }

        public bool IsSolved { get; private set; }

        public QuizAnswerResult Submit(int answerIndex)
        {
            if (answerIndex < 0 || answerIndex >= AnswerCount)
            {
                throw new ArgumentOutOfRangeException(nameof(answerIndex), answerIndex, "Answer index is outside the answer list.");
            }

            if (answerIndex == CorrectIndex)
            {
                IsSolved = true;
                return QuizAnswerResult.Correct;
            }

            WrongAttempts++;
            return QuizAnswerResult.TryAgain;
        }
    }
}
