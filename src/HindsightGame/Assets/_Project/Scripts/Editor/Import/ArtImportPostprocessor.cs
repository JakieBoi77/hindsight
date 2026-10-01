using Unity.VectorGraphics;
using Unity.VectorGraphics.Editor;
using UnityEditor;
using UnityEngine;

namespace Hindsight.Editor.Import
{
    /// <summary>
    /// Applies consistent import settings so artists can drop files in and get working UI sprites:
    /// SVG illustrations become textured sprites; PNG UI shapes become sprites, with 9-slice
    /// borders for files named "*_9s".
    /// </summary>
    public sealed class ArtImportPostprocessor : AssetPostprocessor
    {
        private const string ProjectArtRoot = "Assets/_Project/Art/";
        private const string ThirdPartySpritesRoot = "Assets/ThirdParty/Kenney/UIPack/Sprites/";
        private const int NineSliceBorder = 48;

        // Bump to force re-import of existing art after changing these rules.
        public override uint GetVersion()
        {
            return 2;
        }

        private void OnPreprocessAsset()
        {
            if (!assetPath.StartsWith(ProjectArtRoot) || !(assetImporter is SVGImporter svgImporter))
            {
                return;
            }

            svgImporter.SvgType = SVGType.TexturedSprite;
            svgImporter.KeepTextureAspectRatio = true;
            svgImporter.TextureSize = assetPath.Contains("/Backgrounds/") ? 2048 : 1024;
            svgImporter.SampleCount = 4;
            svgImporter.ViewportOptions = ViewportOptions.PreserveViewport;
            svgImporter.GradientResolution = 128;
            svgImporter.FilterMode = FilterMode.Bilinear;
            svgImporter.WrapMode = TextureWrapMode.Clamp;
        }

        private void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith(ProjectArtRoot) && !assetPath.StartsWith(ThirdPartySpritesRoot))
            {
                return;
            }

            var importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.filterMode = FilterMode.Bilinear;

            if (assetPath.EndsWith("_9s.png"))
            {
                importer.spriteBorder = new Vector4(NineSliceBorder, NineSliceBorder, NineSliceBorder, NineSliceBorder);
            }
        }
    }
}
