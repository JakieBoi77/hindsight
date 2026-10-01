using System;
using System.Collections.Generic;
using UnityEngine;

namespace Hindsight.Procedures.Steps
{
    /// <summary>A picture-based multiple-choice check. Wrong answers prompt a retry, never a fail.</summary>
    [CreateAssetMenu(menuName = "Hindsight/Steps/Quiz", fileName = "QuizStep")]
    public sealed class QuizStepDefinition : StepDefinition
    {
        [SerializeField] private DialogueLine question;
        [SerializeField] private List<QuizAnswer> answers = new List<QuizAnswer>();
        [SerializeField] private int correctIndex;
        [SerializeField] private DialogueLine tryAgainLine;
        [SerializeField] private DialogueLine correctLine;

        public DialogueLine Question => question;

        public IReadOnlyList<QuizAnswer> Answers => answers;

        public int CorrectIndex => correctIndex;

        public DialogueLine TryAgainLine => tryAgainLine;

        public DialogueLine CorrectLine => correctLine;

        public override void Validate(List<string> problems)
        {
            base.Validate(problems);
            RequireLine(question, nameof(question), name, problems);
            RequireLine(tryAgainLine, nameof(tryAgainLine), name, problems);
            RequireLine(correctLine, nameof(correctLine), name, problems);
            if (answers.Count < 2)
            {
                problems.Add($"{name}: a quiz needs at least two answers.");
            }

            if (correctIndex < 0 || correctIndex >= answers.Count)
            {
                problems.Add($"{name}: correct answer index {correctIndex} is outside the answer list.");
            }

            for (var i = 0; i < answers.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(answers[i].Text))
                {
                    problems.Add($"{name}: answer {i} has no text.");
                }

                if (answers[i].Icon == null)
                {
                    problems.Add($"{name}: answer {i} has no picture (pre-readers rely on pictures).");
                }
            }
        }
    }

    [Serializable]
    public struct QuizAnswer
    {
        [SerializeField] private string text;
        [SerializeField] private Sprite icon;

        public QuizAnswer(string text, Sprite icon)
        {
            this.text = text;
            this.icon = icon;
        }

        public string Text => text;

        public Sprite Icon => icon;
    }
}
