using System.Collections.Generic;
using System.Threading;
using Hindsight.Core.Interactions;
using Hindsight.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Hindsight.Procedures.Steps
{
    public sealed class TapCountStepView : StepView<TapCountStepDefinition>
    {
        [SerializeField] private EyeView eye;
        [SerializeField] private float dilatedPupilScale = 0.85f;
        [SerializeField] private Button tapTarget;
        [SerializeField] private IdleMotion tapTargetPulse;
        [SerializeField] private Image counterTemplate;
        [SerializeField] private Sprite counterEmpty;
        [SerializeField] private Sprite counterFilled;

        private readonly List<Image> counters = new List<Image>();

        protected override async Awaitable RunStepAsync(TapCountStepDefinition definition, StepContext context, CancellationToken cancellationToken)
        {
            eye.SetPupilScale(dilatedPupilScale);
            BuildCounters(definition.RequiredTaps);
            var goal = new TapGoal(definition.RequiredTaps);

            context.Prompt(definition.Prompt);
            tapTargetPulse.enabled = true;
            while (!goal.IsReached)
            {
                await tapTarget.WaitForClickAsync(cancellationToken);
                goal.RegisterTap();

                var counter = counters[goal.TapCount - 1];
                counter.sprite = counterFilled;
                Tween.PunchScaleAsync(counter.transform, 0.35f, 0.3f, cancellationToken).Forget();
                await eye.BlinkAsync(cancellationToken);
            }

            tapTargetPulse.enabled = false;
            tapTarget.interactable = false;
            context.PlaySuccess();
            await context.SayAllAsync(definition.SuccessLines, cancellationToken);
        }

        private void BuildCounters(int count)
        {
            counterTemplate.gameObject.SetActive(false);
            for (var i = 0; i < count; i++)
            {
                var counter = Instantiate(counterTemplate, counterTemplate.transform.parent);
                counter.gameObject.SetActive(true);
                counter.sprite = counterEmpty;
                counters.Add(counter);
            }
        }
    }
}
