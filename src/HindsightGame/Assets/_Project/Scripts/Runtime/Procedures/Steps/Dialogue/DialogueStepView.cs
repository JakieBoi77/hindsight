using System.Threading;
using Hindsight.Core.Animation;
using Hindsight.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Hindsight.Procedures.Steps
{
    public sealed class DialogueStepView : StepView<DialogueStepDefinition>
    {
        [SerializeField] private Image illustration;

        protected override async Awaitable RunStepAsync(DialogueStepDefinition definition, StepContext context, CancellationToken cancellationToken)
        {
            var hasIllustration = definition.Illustration != null;
            illustration.gameObject.SetActive(hasIllustration);
            if (hasIllustration)
            {
                illustration.sprite = definition.Illustration;
                illustration.transform.localScale = Vector3.zero;
                Tween.ScaleAsync(illustration.transform, Vector3.one, 0.4f, EaseType.OutBack, cancellationToken).Forget();
            }

            await context.SayAllAsync(definition.Lines, cancellationToken);
        }
    }
}
