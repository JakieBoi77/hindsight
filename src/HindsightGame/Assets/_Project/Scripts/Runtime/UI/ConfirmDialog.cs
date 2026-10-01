using System.Threading;
using Hindsight.Core.Animation;
using Hindsight.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Hindsight.UI
{
    /// <summary>Two-choice modal ("Keep going" / "Leave") that guards against accidental exits.</summary>
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class ConfirmDialog : MonoBehaviour
    {
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button cancelButton;
        [SerializeField] private RectTransform panel;

        private CanvasGroup canvasGroup;

        public bool IsOpen { get; private set; }

        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            Hide();
        }

        /// <returns>True if the user confirmed.</returns>
        public async Awaitable<bool> AskAsync(CancellationToken cancellationToken)
        {
            IsOpen = true;
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
            panel.localScale = Vector3.one * 0.8f;

            try
            {
                var fade = Tween.FadeAsync(canvasGroup, 1f, 0.2f, EaseType.OutQuad, cancellationToken);
                await Tween.ScaleAsync(panel, Vector3.one, 0.25f, EaseType.OutBack, cancellationToken);
                await fade;

                var choice = await AwaitableExtensions.WaitForAnyClickAsync(new[] { confirmButton, cancelButton }, cancellationToken);
                return choice == 0;
            }
            finally
            {
                Hide();
            }
        }

        private void Hide()
        {
            IsOpen = false;
            if (canvasGroup == null)
            {
                return;
            }

            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }
    }
}
