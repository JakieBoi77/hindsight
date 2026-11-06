using TMPro;
using UnityEngine;

namespace EyeCanDo.UI
{
    /// <summary>
    /// The one place for the game's look: palette, font and shape sizes. Every widget reads from
    /// it, so captions, buttons and panels all match (GameDesign §3, §8.5).
    /// </summary>
    [CreateAssetMenu(menuName = "EyeCanDo/UI Theme", fileName = "UiTheme")]
    public sealed class UiTheme : ScriptableObject
    {
        public const string ResourcePath = "UiTheme";

        [Header("Font")]
        public TMP_FontAsset font;

        [Header("Palette")]
        public Color ink = Hex("#26324A");
        public Color paper = Hex("#FFFFFF");
        public Color background = Hex("#FFF6E8");
        public Color wall = Hex("#E3F3FF");
        public Color floor = Hex("#FFD9A8");
        public Color primary = Hex("#FFC23D");
        public Color accent = Hex("#4DA8F0");
        public Color success = Hex("#5CC98A");
        public Color soft = Hex("#FFE9C7");
        public Color muted = Hex("#B9C2CF");
        public Color drop = Hex("#7CC8FF");
        public Color danger = Hex("#FF8E8E");

        [Header("Shapes")]
        public float cornerRadius = 28f;
        public float outlineWidth = 6f;
        public float shadowOffset = 10f;

        [Header("Text sizes (reference 1920x1080)")]
        public float captionSize = 50f;
        public float buttonSize = 46f;
        public float titleSize = 64f;
        public float labelSize = 34f;

        private static UiTheme instance;

        public static UiTheme Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = Resources.Load<UiTheme>(ResourcePath);
                    if (instance == null)
                    {
                        Debug.LogWarning("[EyeCanDo] UiTheme missing from Resources. Run EyeCanDo → Build All.");
                        instance = CreateInstance<UiTheme>();
                    }
                }

                return instance;
            }
        }

        public static Color Hex(string hex)
        {
            return ColorUtility.TryParseHtmlString(hex, out var color) ? color : Color.magenta;
        }

        /// <summary>A darker shade of a colour, for button depth and outlines.</summary>
        public static Color Darker(Color color, float amount = 0.2f)
        {
            return Color.Lerp(color, Color.black, amount);
        }
    }
}
