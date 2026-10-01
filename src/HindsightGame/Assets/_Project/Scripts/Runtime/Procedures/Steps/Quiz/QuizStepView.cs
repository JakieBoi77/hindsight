using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Hindsight.Core.Interactions;
using Hindsight.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Hindsight.Procedures.Steps
{
    public sealed class QuizStepView : StepView<QuizStepDefinition>
    {
        [SerializeField] private QuizAnswerView answerTemplate;
        [SerializeField] private RectTransform answerContainer;

        private readonly List<QuizAnswerView> answerViews = new List<QuizAnswerView>();

        protected override async Awaitable RunStepAsync(QuizStepDefinition definition, StepContext context, CancellationToken cancellationToken)
        {
            BuildAnswers(definition.Answers);
            var evaluator = new QuizEvaluator(definition.Answers.Count, definition.CorrectIndex);
            var buttons = answerViews.Select(view => view.Button).ToArray();

            context.Prompt(definition.Question);
            while (!evaluator.IsSolved)
            {
                var chosen = await AwaitableExtensions.WaitForAnyClickAsync(buttons, cancellationToken);
                var view = answerViews[chosen];

                if (evaluator.Submit(chosen) == QuizAnswerResult.TryAgain)
                {
                    context.PlayGentleError();
                    view.ShowTriedAlready();
                    context.Prompt(definition.TryAgainLine);
                    await Tween.ShakeAsync((RectTransform)view.transform, 18f, 0.4f, cancellationToken);
                    continue;
                }

                foreach (var button in buttons)
                {
                    button.interactable = false;
                }

                view.ShowCorrect();
                context.PlaySuccess();
                await Tween.PunchScaleAsync(view.transform, 0.12f, 0.35f, cancellationToken);
            }

            await context.SayAsync(definition.CorrectLine, cancellationToken);
        }

        private void BuildAnswers(IReadOnlyList<QuizAnswer> answers)
        {
            answerTemplate.gameObject.SetActive(false);
            foreach (var answer in answers)
            {
                var view = Instantiate(answerTemplate, answerContainer);
                view.gameObject.SetActive(true);
                view.Bind(answer);
                answerViews.Add(view);
            }
        }
    }
}
