using System.Collections.Generic;
using Hindsight.Ophthalmology.Core;
using Hindsight.Ophthalmology.Procedure;
using UnityEngine;
using UnityEngine.UI;

namespace Hindsight.Ophthalmology.UI
{
    /// <summary>Top bar during a level: home, a dot per storyboard panel (done / now / to do), and settings. No words.</summary>
    public sealed class LevelHud : MonoBehaviour
    {
        [SerializeField] ProcedureRunner runner;
        [SerializeField] RectTransform dotsRoot;
        [SerializeField] Image dotTemplate;
        [SerializeField] Button homeButton;
        [SerializeField] Button settingsButton;
        [SerializeField] SettingsView settings;
        [SerializeField] Color doneColor = new(0.3f, 0.72f, 0.4f);
        [SerializeField] Color currentColor = new(1f, 0.78f, 0.27f);
        [SerializeField] Color todoColor = new(1f, 1f, 1f, 0.75f);

        readonly List<Image> dots = new();

        void Awake()
        {
            homeButton.onClick.AddListener(GameFlow.ReturnToLevelSelect);
            if (settingsButton != null && settings != null) settingsButton.onClick.AddListener(() => settings.Open(pause: true));
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
            Paint(index);
        }

        void OnCompleted() => Paint(dots.Count);

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
        public void EditorInit(ProcedureRunner procedureRunner, RectTransform dotsParent, Image dot, Button home, Button gear, SettingsView panel)
        {
            runner = procedureRunner;
            dotsRoot = dotsParent;
            dotTemplate = dot;
            homeButton = home;
            settingsButton = gear;
            settings = panel;
        }
#endif
    }
}
