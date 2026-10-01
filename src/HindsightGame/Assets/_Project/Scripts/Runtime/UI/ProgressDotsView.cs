using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Hindsight.UI
{
    /// <summary>Shows how far through a procedure the child is, without needing to read numbers.</summary>
    public sealed class ProgressDotsView : MonoBehaviour
    {
        [SerializeField] private Image dotTemplate;
        [SerializeField] private Color doneColor = new Color(0.36f, 0.75f, 0.45f);
        [SerializeField] private Color currentColor = new Color(1f, 0.78f, 0.25f);
        [SerializeField] private Color upcomingColor = new Color(1f, 1f, 1f, 0.55f);
        [SerializeField] private float currentScale = 1.35f;

        private readonly List<Image> dots = new List<Image>();

        public int DotCount => dots.Count;

        public void Build(int count)
        {
            foreach (var dot in dots)
            {
                Destroy(dot.gameObject);
            }

            dots.Clear();
            dotTemplate.gameObject.SetActive(false);
            for (var i = 0; i < count; i++)
            {
                var dot = Instantiate(dotTemplate, dotTemplate.transform.parent);
                dot.gameObject.SetActive(true);
                dot.name = $"Dot{i + 1}";
                dots.Add(dot);
            }
        }

        public void SetCurrent(int index)
        {
            for (var i = 0; i < dots.Count; i++)
            {
                var dot = dots[i];
                dot.color = i < index ? doneColor : i == index ? currentColor : upcomingColor;
                dot.transform.localScale = Vector3.one * (i == index ? currentScale : 1f);
            }
        }
    }
}
