using System.Linq;
using Hindsight.EyeCanDo.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using static Hindsight.EyeCanDo.EditorTools.BuilderUtil;

namespace Hindsight.EyeCanDo.EditorTools
{
    /// <summary>The Hindsight studio splash: the Splash scene the game starts in (see <see cref="StudioSplash"/>).</summary>
    public static partial class PocBuilder
    {
        public const string SplashScenePath = ScenesDir + "/Splash.unity";
        const string SplashSoundPath = Root + "/Audio/Splash/hindsight_splash.wav"; // made by Tools/SplashGen/make_splash_audio.py
        const string ShadersDir = Root + "/Shaders";
        const float SplashFontSize = 190f;

        /// <summary>The scenes in the build, in order: the game starts in the first.</summary>
        static string[] BuildScenePaths => new[] { SplashScenePath, MenuScenePath, EyeDropsScenePath };

        [MenuItem("POC/Rebuild Splash")]
        public static void BuildSplash()
        {
            BuildSplashScene();
            EditorBuildSettings.scenes = BuildScenePaths.Select(path => new EditorBuildSettingsScene(path, true)).ToArray();
            AssetDatabase.SaveAssets();
            Debug.Log("PocBuilder: splash built. Press Play in the Splash scene to see it.");
        }

        static Material ShaderMaterial(string name)
        {
            var shader = AssetDatabase.LoadAssetAtPath<Shader>($"{ShadersDir}/{name}.shader");
            var path = $"{MaterialsDir}/{name}.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                EnsureFolder(MaterialsDir);
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }
            material.shader = shader;
            EditorUtility.SetDirty(material);
            return material;
        }

        static void BuildSplashScene()
        {
            var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(SplashSoundPath);
            if (clip == null) Debug.LogError($"PocBuilder: missing splash sound {SplashSoundPath}");
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Nothing 3D here: the camera only clears the screen and listens.
            var camGo = new GameObject("Main Camera") { tag = "MainCamera" };
            var cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Color.black;
            cam.cullingMask = 0;
            camGo.AddComponent<AudioListener>();

            var canvas = (RectTransform)MakeCanvas().transform;
            var backdrop = Stretch("Backdrop", canvas).gameObject.AddComponent<RawImage>();
            backdrop.material = ShaderMaterial("SplashBackdrop");
            backdrop.raycastTarget = false;

            // The mark and INDSIGHT; StudioSplash sizes and places them from the text's own metrics.
            var lockup = Rect("Lockup", canvas, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            var wordRt = Rect("Word", lockup, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            wordRt.pivot = new Vector2(0f, 0.5f);
            wordRt.sizeDelta = new Vector2(1600f, 400f);
            var word = wordRt.gameObject.AddComponent<TextMeshProUGUI>();
            word.font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(UiKit.FredokaPath);
            word.fontStyle = FontStyles.Bold; // the variable font's default instance is light
            word.text = "INDSIGHT";
            word.fontSize = SplashFontSize;
            word.characterSpacing = 3f;
            word.alignment = TextAlignmentOptions.MidlineLeft;
            word.textWrappingMode = TextWrappingModes.NoWrap;
            word.overflowMode = TextOverflowModes.Overflow;
            word.raycastTarget = false;
            var mark = Rect("Mark", lockup, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f)).gameObject.AddComponent<RawImage>();
            mark.material = ShaderMaterial("SplashMark");
            mark.raycastTarget = false;

            var fader = Img(Stretch("Fader", canvas), Color.black, raycast: false);
            // An invisible sheet that catches the tap that skips the splash.
            Img(Stretch("TapCatcher", canvas), Color.clear);

            var sting = new GameObject("Sting").AddComponent<AudioSource>();
            sting.playOnAwake = false;
            sting.spatialBlend = 0f;
            sting.clip = clip;

            canvas.gameObject.AddComponent<StudioSplash>().EditorInit(lockup, backdrop, mark, word, fader, sting);
            SaveScene(SplashScenePath);
        }
    }
}
