using System.Threading;
using Hindsight.Core.Animation;
using Hindsight.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Hindsight.Procedures
{
    /// <summary>
    /// Speech bubble with a typewriter reveal. Tapping the bubble while text is typing shows it
    /// all at once; tapping again (or the arrow) continues. Prompts stay up with no arrow.
    /// </summary>
    public sealed class DoctorDialogueView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup bubbleGroup;
        [SerializeField] private RectTransform bubble;
        [SerializeField] private TMP_Text label;
        [SerializeField] private Button bubbleButton;
        [SerializeField] private Button continueButton;
        [SerializeField] private float charactersPerSecond = 40f;

        private CancellationTokenSource typingCancellation;
        private bool skipTypingRequested;
        private bool isShowing;

        public string CurrentText => label.text;

        public bool IsWaitingForContinue => continueButton.gameObject.activeSelf;

        private void Awake()
        {
            bubbleButton.onClick.AddListener(RequestSkipTyping);
            HideImmediate();
        }

        private void OnDestroy()
        {
            CancelTyping();
        }

        public async Awaitable SayAsync(string text, CancellationToken cancellationToken)
        {
            CancelTyping();
            continueButton.gameObject.SetActive(false);
            await ShowBubbleAsync(cancellationToken);
            await TypeAsync(text, cancellationToken);

            continueButton.gameObject.SetActive(true);
            try
            {
                await AwaitableExtensions.WaitForAnyClickAsync(new[] { continueButton, bubbleButton }, cancellationToken);
            }
            finally
            {
                if (continueButton != null)
                {
                    continueButton.gameObject.SetActive(false);
                }
            }
        }

        public void ShowPrompt(string text)
        {
            CancelTyping();
            continueButton.gameObject.SetActive(false);
            typingCancellation = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken);
            ShowPromptAsync(text, typingCancellation.Token).Forget();
        }

        public void Hide()
        {
            CancelTyping();
            HideImmediate();
        }

        private async Awaitable ShowPromptAsync(string text, CancellationToken cancellationToken)
        {
            await ShowBubbleAsync(cancellationToken);
            await TypeAsync(text, cancellationToken);
        }

        private async Awaitable ShowBubbleAsync(CancellationToken cancellationToken)
        {
            if (isShowing)
            {
                return;
            }

            isShowing = true;
            bubbleGroup.alpha = 1f;
            bubbleGroup.blocksRaycasts = true;
            bubble.localScale = Vector3.one * 0.85f;
            await Tween.ScaleAsync(bubble, Vector3.one, 0.2f, EaseType.OutBack, cancellationToken);
        }

        private async Awaitable TypeAsync(string text, CancellationToken cancellationToken)
        {
            skipTypingRequested = false;
            label.text = text;
            label.maxVisibleCharacters = 0;
            label.ForceMeshUpdate();
            var total = label.textInfo.characterCount;

            var visible = 0f;
            while (visible < total && !skipTypingRequested)
            {
                await Awaitable.NextFrameAsync(cancellationToken);
                visible += charactersPerSecond * Time.unscaledDeltaTime;
                label.maxVisibleCharacters = Mathf.Min(Mathf.FloorToInt(visible), total);
            }

            label.maxVisibleCharacters = int.MaxValue;
            skipTypingRequested = false;
        }

        private void RequestSkipTyping()
        {
            skipTypingRequested = true;
        }

        private void CancelTyping()
        {
            if (typingCancellation == null)
            {
                return;
            }

            typingCancellation.Cancel();
            typingCancellation.Dispose();
            typingCancellation = null;
        }

        private void HideImmediate()
        {
            isShowing = false;
            bubbleGroup.alpha = 0f;
            bubbleGroup.blocksRaycasts = false;
            continueButton.gameObject.SetActive(false);
            label.text = string.Empty;
        }
    }
}
