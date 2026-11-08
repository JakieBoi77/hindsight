using System;
using Hindsight.Ophthalmology.Core.Avatar;
using UnityEngine;

namespace Hindsight.Ophthalmology.SceneObjects
{
    /// <summary>
    /// Layout of the face atlases (painted by the editor's FaceArtPainter) and where the face patches
    /// sit on the head. Three atlases, one per face layer:
    /// eyes (every style open and shut, cells 2:1), mouths (every style in four shapes from closed to
    /// wide open, square cells) and accessories (brows, hair, glasses, scars..., cells 5:4 covering
    /// the upper face). A fourth atlas holds the brows: every brow style in every mood, cells 3:1 in a
    /// band above the eyes. A patch shows one cell at a time through the material's tiling and offset.
    /// </summary>
    public static class FaceArt
    {
        public const int Size = 2048;

        public enum Layer
        {
            Eyes,
            Mouth,
            Brows,
        }

        /// <summary>Eye styles in atlas order (ids match the catalog).</summary>
        public static readonly string[] EyeStyles =
        {
            "basic", "lashes", "half", "shine", "squint", "angry", "eyeliner", "sad", "cry", "tired", "almond", "big", "small",
            "concern", "anime", "real", "fierce", "inverted",
        };

        /// <summary>Mouth styles in atlas order.</summary>
        public static readonly string[] MouthStyles =
        {
            "smiley", "bean", "cat", "cheek", "drool", "lips", "frown", "kissy", "nonplussed", "o", "squiggle", "triangle", "smirk",
            "cringe", "teeth", "vamp", "chomp", "wacky", "toothy",
        };

        /// <summary>Brow styles in atlas order, thin to thick ("none" has no cell). Each has a cell per mood.</summary>
        public static readonly string[] BrowStyles =
        {
            "thin", "soft", "arched", "straight", "short", "angled", "tapered", "thick", "bushy", "dot", "unibrow",
        };

        /// <summary>Moods drawn for every brow style (Neutral first).</summary>
        public const int MoodCount = 6;

        /// <summary>Mouth shapes per style: 0 closed (the style's resting look), then more and more open.</summary>
        public const int MouthFrames = 4;

        // ---- Where the patches sit on a kid's head (metres, from the head's centre; the doctor is scaled up).
        public const float EyesY = 0.02f;
        public const float EyesHeight = 0.14f;
        public const float EyesAspect = 2f;
        public const float MouthY = -0.074f;
        public const float MouthSize = 0.125f;
        public const float BrowsY = 0.092f;
        public const float BrowsHeight = 0.09f;
        public const float BrowsAspect = 3f;

        /// <summary>The left eye's centre x in a brows cell (x from -3 to 3, y from -1 to 1).</summary>
        public const float EyeXInBrows = EyeCellX * EyesHeight / BrowsHeight;

        /// <summary>Where a resting brow sits in a brows cell: just above the top of the eyes.</summary>
        public const float BrowRestY = -0.3f;

        /// <summary>The left eye's centre in an eyes cell (x from -2 to 2, y from -1 to 1); the right one mirrors it.</summary>
        public const float EyeCellX = 0.95f;

        static (int cols, int rows) Grid(Layer layer) => layer switch
        {
            Layer.Eyes => (5, 10), // 409 x 204 px
            Layer.Mouth => (9, 9), // 227 px
            _ => (6, 18), // brows: 341 x 113 px
        };

        public static float Aspect(Layer layer) => layer switch
        {
            Layer.Eyes => EyesAspect,
            Layer.Mouth => 1f,
            _ => BrowsAspect,
        };

        public static int CellCount(Layer layer) => layer switch
        {
            Layer.Eyes => EyeStyles.Length * 2 + 1, // + the rosy cheeks
            Layer.Mouth => MouthStyles.Length * MouthFrames,
            _ => BrowStyles.Length * MoodCount,
        };

        /// <summary>Cell <paramref name="index"/> of an atlas (UV, origin bottom-left), filling rows from the top.</summary>
        public static Rect Cell(Layer layer, int index)
        {
            var (cols, rows) = Grid(layer);
            int col = index % cols, row = index / cols;
            float w = 1f / cols, h = 1f / rows;
            return new Rect(col * w, 1f - (row + 1) * h, w, h);
        }

        public static int EyeIndex(string id) => Mathf.Max(0, Array.IndexOf(EyeStyles, id));

        public static int MouthIndex(string id) => Mathf.Max(0, Array.IndexOf(MouthStyles, id));


        public static Rect Eyes(int style, bool shut) => Cell(Layer.Eyes, style * 2 + (shut ? 1 : 0));

        /// <summary>Rosy cheeks, shown under every face (a cell of the eyes atlas, the same size as an eyes cell).</summary>
        public static Rect Cheeks => Cell(Layer.Eyes, EyeStyles.Length * 2);

        /// <summary>Where the cheeks patch sits: below the eyes.</summary>
        public const float CheeksY = -0.04f;

        public static Rect Mouth(int style, int frame) => Cell(Layer.Mouth, style * MouthFrames + Mathf.Clamp(frame, 0, MouthFrames - 1));


        /// <summary>-1 for "none" (the brows patch is hidden).</summary>
        public static int BrowIndex(string id) => Array.IndexOf(BrowStyles, id);

        /// <summary>A brow style in a mood (<see cref="Mood.Keep"/> shows Neutral).</summary>
        public static Rect Brows(int style, Mood mood) =>
            Cell(Layer.Brows, Mathf.Max(0, style) * MoodCount + Mathf.Clamp((int)mood, 0, MoodCount - 1));

        /// <summary>A cell as a material tiling/offset (_BaseMap_ST).</summary>
        public static Vector4 ST(Rect r) => new(r.width, r.height, r.x, r.y);
    }
}
