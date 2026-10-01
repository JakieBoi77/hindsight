using System.IO;
using UnityEditor;
using UnityEngine;

namespace Hindsight.Editor.Scaffolding
{
    /// <summary>Typed access to the sprites the scaffolder wires into prefabs. Missing art fails loudly.</summary>
    internal static class ArtLibrary
    {
        private const string ProjectArt = "Assets/_Project/Art/";
        private const string Kenney = "Assets/ThirdParty/Kenney/UIPack/";

        public static Sprite PanelSprite => Art("UI/panel_rounded_9s.png");

        public static Sprite Circle => Art("UI/circle.png");

        public static Sprite Ring => Art("UI/ring.png");

        public static Sprite Art(string relativePath)
        {
            return Load<Sprite>(ProjectArt + relativePath);
        }

        public static Sprite KenneySprite(string fileName)
        {
            return Load<Sprite>(Kenney + "Sprites/" + fileName);
        }

        public static AudioClip KenneySound(string fileName)
        {
            return Load<AudioClip>(Kenney + "Sounds/" + fileName);
        }

        private static T Load<T>(string path)
            where T : Object
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null)
            {
                throw new FileNotFoundException($"Expected {typeof(T).Name} at '{path}'. Did the art import correctly?");
            }

            return asset;
        }
    }
}
