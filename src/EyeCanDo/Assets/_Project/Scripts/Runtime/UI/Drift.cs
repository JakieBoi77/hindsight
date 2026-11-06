using UnityEngine;

namespace EyeCanDo.UI
{
    /// <summary>Slowly drifts a decoration sideways and wraps it around, for clouds. Stops with Reduce motion.</summary>
    public sealed class Drift : MonoBehaviour
    {
        public float Speed = 15f;
        public float Range = 1200f;

        private RectTransform rect;

        private void Awake()
        {
            rect = (RectTransform)transform;
        }

        private void Update()
        {
            if (App.GameServices.Settings.reduceMotion)
            {
                return;
            }

            var position = rect.anchoredPosition;
            position.x += Speed * Time.deltaTime;
            if (position.x > Range)
            {
                position.x = -Range;
            }

            rect.anchoredPosition = position;
        }
    }
}
