using UnityEngine;

namespace Hindsight.UI
{
    /// <summary>Keeps interactive UI clear of notches and rounded corners on phones.</summary>
    [RequireComponent(typeof(RectTransform))]
    [ExecuteAlways]
    public sealed class SafeAreaFitter : MonoBehaviour
    {
        private RectTransform rectTransform;
        private Rect appliedSafeArea;
        private Vector2Int appliedScreenSize;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        private void OnEnable()
        {
            Apply();
        }

        // Called by Unity when the canvas resizes (rotation, window resize) — cheaper than polling in Update.
        private void OnRectTransformDimensionsChange()
        {
            Apply();
        }

        private void Apply()
        {
            if (rectTransform == null || Screen.width <= 0 || Screen.height <= 0)
            {
                return;
            }

            var safeArea = Screen.safeArea;
            var screenSize = new Vector2Int(Screen.width, Screen.height);
            if (safeArea == appliedSafeArea && screenSize == appliedScreenSize)
            {
                return;
            }

            appliedSafeArea = safeArea;
            appliedScreenSize = screenSize;

            var anchorMin = safeArea.position;
            var anchorMax = safeArea.position + safeArea.size;
            anchorMin.x /= screenSize.x;
            anchorMin.y /= screenSize.y;
            anchorMax.x /= screenSize.x;
            anchorMax.y /= screenSize.y;

            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
        }
    }
}
