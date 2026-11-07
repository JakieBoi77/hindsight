using System.Collections.Generic;
using Hindsight.Ophthalmology.Core;
using Hindsight.Ophthalmology.Procedure;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Hindsight.Ophthalmology.UI
{
    /// <summary>Top bar during a level: a dot per step (done / now / to do), the step name, and a home button.</summary>
    public sealed class LevelHud : MonoBehaviour
    {
        [SerializeField] ProcedureRunner runner;
        [SerializeField] TMP_Text stepLabel;
        [SerializeField] RectTransform dotsRoot;
        [SerializeField] Image dotTemplate;
        [SerializeField] Button homeButton;
        [SerializeField] Color doneColor = new(0.3f, 0.72f, 0.4f);
        [SerializeField] Color currentColor = new(1f, 0.78f, 0.27f);
        [SerializeField] Color todoColor = new(1f, 1f, 1f, 0.75f);

        readonly List<Image> dots = new();

        void Awake()
        {
            homeButton.onClick.AddListener(GameFlow.ReturnToLevelSelect);
            dotTemplate.gameObject.SetActive(false);
            runner.StepStarted += OnStepStarted;
            runner.Completed += OnCompleted;
        }

        void OnDestroy()
        {
            runner.StepStarted -= OnStepStarted;
            runner.Completed -= OnCompleted;
        }

        void OnStepStarted(int index, int count, ProcedureStep step)
        {
            EnsureDots(count);
            stepLabel.text = step.Title;
            Paint(index);
        }

        void OnCompleted()
        {
            stepLabel.text = "All done!";
            Paint(dots.Count);
        }

        void EnsureDots(int count)
        {
            while (dots.Count < count)
            {
                var dot = Instantiate(dotTemplate, dotsRoot);
                dot.gameObject.SetActive(true);
                dots.Add(dot);
            }
        }

        void Paint(int current)
        {
            for (var i = 0; i < dots.Count; i++)
            {
                dots[i].color = i < current ? doneColor : i == current ? currentColor : todoColor;
                dots[i].transform.localScale = Vector3.one * (i == current ? 1.35f : 1f);
            }
        }

#if UNITY_EDITOR
        public void EditorInit(ProcedureRunner procedureRunner, TMP_Text label, RectTransform dotsParent, Image dot, Button home)
        {
            runner = procedureRunner;
            stepLabel = label;
            dotsRoot = dotsParent;
            dotTemplate = dot;
            homeButton = home;
        }
#endif
    }
}
