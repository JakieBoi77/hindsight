using UnityEngine;

namespace EyeCanDo.UI
{
    /// <summary>
    /// Keeps an interactive element at least <see cref="MinimumMillimetres"/> wide and high on the
    /// physical screen (TOUCH_TARGET_MIN, NFR_TouchTarget). On small phones the canvas scales
    /// down, so the element grows to compensate.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public sealed class TouchTargetScaler : MonoBehaviour
    {
        public const float MinimumMillimetres = 15f;
        private const float FallbackDpi = 160f;

        private Vector2 designSize;
        private Vector2Int lastScreen;

        public static float MinimumCanvasUnits(Canvas canvas)
        {
            var dpi = Screen.dpi > 0f ? Screen.dpi : FallbackDpi;
            var pixels = MinimumMillimetres / 25.4f * dpi;
            var scale = canvas != null ? canvas.rootCanvas.scaleFactor : 1f;
            return pixels / Mathf.Max(scale, 0.0001f);
        }

        private void Start()
        {
            designSize = ((RectTransform)transform).sizeDelta;
            Apply();
        }

        private void Update()
        {
            if (lastScreen.x != Screen.width || lastScreen.y != Screen.height)
            {
                Apply();
            }
        }

        private void Apply()
        {
            lastScreen = new Vector2Int(Screen.width, Screen.height);
            var canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                return;
            }

            var minimum = MinimumCanvasUnits(canvas);
            var rect = (RectTransform)transform;
            rect.sizeDelta = new Vector2(Mathf.Max(designSize.x, minimum), Mathf.Max(designSize.y, minimum));
        }
    }
}
