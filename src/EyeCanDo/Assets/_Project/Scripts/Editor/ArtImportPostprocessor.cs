using Unity.VectorGraphics;
using Unity.VectorGraphics.Editor;
using UnityEditor;
using UnityEngine;

namespace EyeCanDo.EditorTools
{
    /// <summary>
    /// Imports the game's SVG art as sprites rendered at a resolution comfortably above their
    /// largest size on a tablet, with 4x anti-aliasing, so edges stay sharp on every screen.
    /// </summary>
    public sealed class ArtImportPostprocessor : AssetPostprocessor
    {
        public const string ArtRoot = "Assets/_Project/Art/";

        // Bump to re-import existing art after changing these rules.
        public override uint GetVersion()
        {
            return 4;
        }

        private void OnPreprocessAsset()
        {
            if (!assetPath.StartsWith(ArtRoot) || !(assetImporter is SVGImporter svg))
            {
                return;
            }

            svg.SvgType = SVGType.TexturedSprite;
            svg.KeepTextureAspectRatio = true;
            svg.TextureSize = TextureSizeFor(assetPath);
            svg.SampleCount = 4;
            svg.ViewportOptions = ViewportOptions.PreserveViewport;
            svg.GradientResolution = 128;
            svg.FilterMode = FilterMode.Bilinear;
            svg.WrapMode = TextureWrapMode.Clamp;
            svg.SvgPixelsPerUnit = 100f;
        }

        /// <summary>Longest side of the rendered texture, by kind of art.</summary>
        public static int TextureSizeFor(string path)
        {
            if (path.Contains("/Avatar/"))
            {
                return path.EndsWith("_stand.svg") || path.EndsWith("_wave.svg") || path.EndsWith("_seated.svg") ? 1024 : 512;
            }

            if (path.Contains("/Characters/"))
            {
                return path.Contains("_head") || path.Contains("_eyes_") || path.Contains("_mouth_") || path.Contains("_glasses") ? 512 : 1024;
            }

            if (path.Contains("/Eye/") || path.Contains("exam_chair") || path.Contains("sticker") || path.Contains("/Room/") || path.Contains("Spy/sofa") || path.Contains("Spy/shelf"))
            {
                return 1024;
            }

            return 512;
        }
    }
}
