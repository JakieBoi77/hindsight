using Hindsight.Core.Animation;
using Hindsight.Tweening;
using UnityEngine;

namespace Hindsight.UI
{
    /// <summary>Full-screen overlay used to hide scene changes. Blocks input while visible.</summary>
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class ScreenFader : MonoBehaviour
    {
        [SerializeField] private float fadeSeconds = 0.3f;

        private CanvasGroup canvasGroup;

        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            SetVisible(false);
        }

        public async Awaitable FadeOutAsync()
        {
            canvasGroup.blocksRaycasts = true;
            await Tween.FadeAsync(canvasGroup, 1f, fadeSeconds, EaseType.InOutQuad, destroyCancellationToken);
        }

        public async Awaitable FadeInAsync()
        {
            await Tween.FadeAsync(canvasGroup, 0f, fadeSeconds, EaseType.InOutQuad, destroyCancellationToken);
            canvasGroup.blocksRaycasts = false;
        }

        private void SetVisible(bool visible)
        {
            canvasGroup.alpha = visible ? 1f : 0f;
            canvasGroup.blocksRaycasts = visible;
        }
    }
}
