using System.Threading;
using Hindsight.Core.Animation;
using Hindsight.Tweening;
using Hindsight.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Hindsight.Procedures.Steps
{
    public sealed class ExamineStepView : StepView<ExamineStepDefinition>
    {
        [SerializeField] private EyeView eye;
        [SerializeField] private float dilatedPupilScale = 0.85f;
        [SerializeField] private Button toolButton;
        [SerializeField] private RectTransform tool;
        [SerializeField] private RectTransform toolTarget;
        [SerializeField] private IdleMotion toolPulse;
        [SerializeField] private CanvasGroup lightGlow;
        [SerializeField] private HoldButton holdButton;
        [SerializeField] private Image holdRing;

        protected override async Awaitable RunStepAsync(ExamineStepDefinition definition, StepContext context, CancellationToken cancellationToken)
        {
            // The drops have worked by now, so the pupil is shown already dilated.
            eye.SetPupilScale(dilatedPupilScale);
            lightGlow.alpha = 0f;
            holdButton.Interactable = false;
            holdButton.gameObject.SetActive(false);
            var toolHome = tool.anchoredPosition;
            toolButton.interactable = false;

            await context.SayAllAsync(definition.IntroLines, cancellationToken);

            context.Prompt(definition.TapToolPrompt);
            toolButton.interactable = true;
            toolPulse.enabled = true;
            await toolButton.WaitForClickAsync(cancellationToken);
            toolPulse.enabled = false;
            toolButton.interactable = false;

            await Tween.MoveAsync(tool, toolTarget.anchoredPosition, 0.6f, EaseType.OutCubic, cancellationToken);
            await Tween.FadeAsync(lightGlow, 1f, 0.3f, EaseType.OutQuad, cancellationToken);

            holdButton.gameObject.SetActive(true);
            context.Prompt(definition.HoldPrompt);
            await HoldInteraction.RunAsync(
                holdButton,
                definition.HoldSeconds,
                progress => holdRing.fillAmount = progress,
                async token =>
                {
                    context.PlayGentleError();
                    await context.SayAsync(definition.InterruptedLine, token);
                    context.Prompt(definition.HoldPrompt);
                },
                cancellationToken);

            holdButton.gameObject.SetActive(false);
            await Tween.FadeAsync(lightGlow, 0f, 0.3f, EaseType.OutQuad, cancellationToken);
            await Tween.MoveAsync(tool, toolHome, 0.5f, EaseType.OutCubic, cancellationToken);
            context.PlaySuccess();
            await context.SayAllAsync(definition.SuccessLines, cancellationToken);
        }
    }
}
