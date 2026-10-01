using System;
using System.Threading;
using Hindsight.App;
using Hindsight.Core.Animation;
using Hindsight.Core.Procedures;
using Hindsight.Tweening;
using Hindsight.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Hindsight.Procedures
{
    /// <summary>
    /// Runs a <see cref="ProcedureDefinition"/> step by step in the Procedure scene. Flow rules
    /// live in <see cref="ProcedureSession"/>; this class only spawns views and wires services.
    /// </summary>
    public sealed class ProcedureController : MonoBehaviour
    {
        [Tooltip("Used when the scene is opened directly in the editor instead of from Level Select.")]
        [SerializeField] private ProcedureDefinition fallbackProcedure;
        [SerializeField] private RectTransform stepStage;
        [SerializeField] private CanvasGroup stepStageGroup;
        [SerializeField] private DoctorCharacterView doctor;
        [SerializeField] private DoctorDialogueView dialogue;
        [SerializeField] private ProgressDotsView progressDots;
        [SerializeField] private Button homeButton;
        [SerializeField] private ConfirmDialog exitDialog;

        [Tooltip("Development builds only: lets testers and demos jump past a step.")]
        [SerializeField] private Button developerSkipButton;
        [SerializeField] private float stepTransitionSeconds = 0.25f;

        private CancellationTokenSource stepCancellation;

        /// <summary>Raised when a step's view has been created, with the step index.</summary>
        public event Action<int> StepStarted;

        public ProcedureDefinition Procedure { get; private set; }

        public ProcedureSession Session { get; private set; }

        public StepView CurrentView { get; private set; }

        private void Awake()
        {
            homeButton.onClick.AddListener(OnHomeClicked);

            var allowSkip = Debug.isDebugBuild;
            developerSkipButton.gameObject.SetActive(allowSkip);
            if (allowSkip)
            {
                developerSkipButton.onClick.AddListener(SkipCurrentStep);
            }
        }

        private void Start()
        {
            RunProcedureAsync(destroyCancellationToken).Forget();
        }

        /// <summary>Ends the current step early. Used by the developer skip button and automated tests.</summary>
        public void SkipCurrentStep()
        {
            stepCancellation?.Cancel();
        }

        private async Awaitable RunProcedureAsync(CancellationToken cancellationToken)
        {
            Procedure = ResolveProcedure();
            if (Procedure == null || Procedure.Steps.Count == 0)
            {
                Debug.LogError("No playable procedure was selected; returning to level select.");
                await NavigateToLevelSelectAsync();
                return;
            }

            Session = new ProcedureSession(Procedure.Steps.Count);
            progressDots.Build(Session.StepCount);
            var context = new StepContext(Procedure, doctor, dialogue, GameServices.Audio);

            while (!Session.IsComplete)
            {
                var step = Procedure.Steps[Session.CurrentIndex];
                progressDots.SetCurrent(Session.CurrentIndex);

                // Record before the reward plays so closing the app on the reward screen keeps progress.
                if (step.MarksProcedureComplete && GameServices.Progress != null)
                {
                    GameServices.Progress.RecordCompletion(Procedure);
                }

                await RunStepAsync(step, context, cancellationToken);
                Session.CompleteCurrentStep();
            }

            await NavigateToLevelSelectAsync();
        }

        private async Awaitable RunStepAsync(StepDefinition step, StepContext context, CancellationToken cancellationToken)
        {
            stepCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            CurrentView = Instantiate(step.ViewPrefab, stepStage);
            CurrentView.name = step.ViewPrefab.name;
            stepStageGroup.alpha = 0f;
            StepStarted?.Invoke(Session.CurrentIndex);

            try
            {
                await Tween.FadeAsync(stepStageGroup, 1f, stepTransitionSeconds, EaseType.OutQuad, stepCancellation.Token);
                await CurrentView.RunAsync(step, context, stepCancellation.Token);
                await Tween.FadeAsync(stepStageGroup, 0f, stepTransitionSeconds, EaseType.InQuad, stepCancellation.Token);
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                // Step was skipped on purpose; continue with the next one.
            }
            finally
            {
                stepCancellation.Dispose();
                stepCancellation = null;
                if (CurrentView != null)
                {
                    Destroy(CurrentView.gameObject);
                }

                CurrentView = null;
                if (!cancellationToken.IsCancellationRequested)
                {
                    context.Reset();
                }
            }
        }

        private ProcedureDefinition ResolveProcedure()
        {
            var selected = GameServices.Navigator != null ? GameServices.Navigator.ActiveProcedure : null;
            return selected != null ? selected : fallbackProcedure;
        }

        private void OnHomeClicked()
        {
            ConfirmExitAsync().Forget();
        }

        private async Awaitable ConfirmExitAsync()
        {
            if (exitDialog.IsOpen)
            {
                return;
            }

            if (await exitDialog.AskAsync(destroyCancellationToken))
            {
                await NavigateToLevelSelectAsync();
            }
        }

        private static Awaitable NavigateToLevelSelectAsync()
        {
            if (GameServices.Navigator == null)
            {
                Debug.LogWarning("Navigation services are not initialised; staying in the Procedure scene.");
                return Awaitable.NextFrameAsync();
            }

            return GameServices.Navigator.GoToLevelSelectAsync();
        }
    }
}
