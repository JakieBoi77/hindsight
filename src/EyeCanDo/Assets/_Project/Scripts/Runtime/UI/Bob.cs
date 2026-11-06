using UnityEngine;

namespace EyeCanDo.UI
{
    /// <summary>A gentle up-and-down bob, to show something can be tapped or is alive. Stays still with Reduce motion.</summary>
    public sealed class Bob : MonoBehaviour
    {
        public float Height = 10f;
        public float Speed = 2.2f;

        private RectTransform rect;
        private Vector2 start;
        private float time;

        private void Start()
        {
            rect = (RectTransform)transform;
            start = rect.anchoredPosition;
            time = Random.Range(0f, 3f);
        }

        private void Update()
        {
            if (App.GameServices.Settings.reduceMotion)
            {
                rect.anchoredPosition = start;
                return;
            }

            time += Time.deltaTime;
            rect.anchoredPosition = start + Vector2.up * (Mathf.Sin(time * Speed) * Height);
        }

        /// <summary>Call after moving the object yourself, so it bobs around the new spot.</summary>
        public void Rebase()
        {
            if (rect != null)
            {
                start = rect.anchoredPosition;
            }
        }
    }
}
