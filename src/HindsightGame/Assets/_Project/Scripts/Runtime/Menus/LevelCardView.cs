using System;
using System.Threading;
using Hindsight.Core.Progress;
using Hindsight.Procedures;
using Hindsight.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Hindsight.Menus
{
    /// <summary>One level on the level select screen.</summary>
    public sealed class LevelCardView : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private Image background;
        [SerializeField] private Image icon;
        [SerializeField] private TMP_Text title;
        [SerializeField] private TMP_Text status;
        [SerializeField] private GameObject lockBadge;
        [SerializeField] private GameObject completedBadge;
        [SerializeField] private CanvasGroup contentGroup;
        [SerializeField] private Color lockedTint = new Color(0.82f, 0.84f, 0.88f);
        [SerializeField] private float lockedContentAlpha = 0.55f;

        private bool feedbackPlaying;

        public event Action<LevelCardView> Clicked;

        public ProcedureDefinition Procedure { get; private set; }

        public LevelAvailability Availability { get; private set; }

        public Button Button => button;

        private void Awake()
        {
            button.onClick.AddListener(() => Clicked?.Invoke(this));
        }

        public void Bind(ProcedureDefinition procedure, LevelAvailability availability)
        {
            Procedure = procedure;
            Availability = availability;
            name = $"LevelCard_{procedure.Id}";

            title.text = procedure.DisplayName;
            icon.sprite = procedure.Icon;
            icon.enabled = procedure.Icon != null;

            var playable = UnlockRules.CanPlay(availability);
            background.color = playable ? procedure.AccentColor : lockedTint;
            contentGroup.alpha = playable ? 1f : lockedContentAlpha;
            lockBadge.SetActive(!playable);
            completedBadge.SetActive(availability == LevelAvailability.Completed);
            status.text = StatusText(availability);
        }

        /// <summary>Gentle wiggle when a locked card is tapped, instead of doing nothing.</summary>
        public void PlayLockedFeedback(CancellationToken cancellationToken)
        {
            if (feedbackPlaying)
            {
                return;
            }

            PlayLockedFeedbackAsync(cancellationToken).Forget();
        }

        private async Awaitable PlayLockedFeedbackAsync(CancellationToken cancellationToken)
        {
            feedbackPlaying = true;
            try
            {
                await Tween.ShakeAsync((RectTransform)lockBadge.transform, 14f, 0.4f, cancellationToken);
            }
            finally
            {
                feedbackPlaying = false;
            }
        }

        private static string StatusText(LevelAvailability availability)
        {
            switch (availability)
            {
                case LevelAvailability.Completed:
                    return "Play again!";
                case LevelAvailability.Unlocked:
                    return "Play!";
                case LevelAvailability.ComingSoon:
                    return "Coming soon";
                default:
                    return "Locked";
            }
        }
    }
}
