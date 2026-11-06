using System.Collections.Generic;
using System.IO;
using System.Linq;
using EyeCanDo.App;
using EyeCanDo.Art;
using EyeCanDo.Content;
using EyeCanDo.Games;
using EyeCanDo.Map;
using EyeCanDo.Story;
using EyeCanDo.Title;
using EyeCanDo.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.LowLevel;

namespace EyeCanDo.EditorTools
{
    /// <summary>
    /// Generates the PoC's settings, font, theme, art and sound libraries, content and scenes, so
    /// the project can be rebuilt from code. Content assets are only created when missing, so
    /// edits made in the Inspector are kept; use Reset Content to restore the defaults.
    /// Headless: Unity -batchmode -quit -projectPath . -executeMethod EyeCanDo.EditorTools.PocBuilder.BuildAll
    /// </summary>
    public static class PocBuilder
    {
        public const string ProjectRoot = "Assets/_Project/";
        public const string ArtFolder = ProjectRoot + "Art";
        public const string DataFolder = ProjectRoot + "Data/";
        public const string ScenesFolder = ProjectRoot + "Scenes/";
        public const string ResourcesFolder = ProjectRoot + "Resources/";
        public const string FontPath = ProjectRoot + "Fonts/Fredoka SDF.asset";
        public const string StoryPath = DataFolder + "StoryScript.asset";
        public const string CaptionsPath = DataFolder + "CaptionBank.asset";
        public const string CatalogPath = DataFolder + "StopCatalog.asset";
        public const string BlurShader = "EyeCanDo/UI Blur";
        public const string RecolorShader = "EyeCanDo/UI Recolor";

        private const string Sounds = ProjectRoot + "Audio/";
        private const string FredokaTtf = "Assets/ThirdParty/Fonts/Fredoka/Fredoka-Variable.ttf";

        [MenuItem("EyeCanDo/Build All", priority = 1)]
        public static void BuildAll()
        {
            ProjectSetup.Configure();
            AssetDatabase.ImportAsset(ArtFolder, ImportAssetOptions.ImportRecursive | ImportAssetOptions.ForceUpdate);
            var font = EnsureFont();
            EnsureTheme(font);
            EnsureArtLibrary();
            EnsureSoundLibrary();
            EnsureBlurMaterial();
            EnsureRecolorMaterial();
            AddMissingCaptions(EnsureContent(false).Captions);
            AssetDatabase.SaveAssets();
            BuildScenes();
            AssetDatabase.SaveAssets();

            var problems = ContentValidator.Problems(Load<StoryScript>(StoryPath), Load<CaptionBank>(CaptionsPath));
            foreach (var problem in problems)
            {
                Debug.LogError("[EyeCanDo] Content: " + problem);
            }

            Debug.Log("[EyeCanDo] Build All finished.");
        }

        [MenuItem("EyeCanDo/Reset Content To Defaults", priority = 20)]
        public static void ResetContent()
        {
            EnsureContent(true);
            AssetDatabase.SaveAssets();
            Debug.Log("[EyeCanDo] Story, captions and stops reset to the defaults.");
        }

        public struct ContentAssets
        {
            public StoryScript Story;
            public CaptionBank Captions;
            public StopCatalog Catalog;
        }

        public static ContentAssets EnsureContent(bool overwrite)
        {
            EnsureFolder(DataFolder);
            return new ContentAssets
            {
                Story = EnsureAsset<StoryScript>(StoryPath, overwrite, DefaultContent.Fill),
                Captions = EnsureAsset<CaptionBank>(CaptionsPath, overwrite, DefaultContent.Fill),
                Catalog = EnsureAsset<StopCatalog>(CatalogPath, overwrite, DefaultContent.Fill),
            };
        }

        /// <summary>Gives captions added since the bank was made their default text, keeping edited ones.</summary>
        private static void AddMissingCaptions(CaptionBank bank)
        {
            var defaults = DefaultContent.CreateCaptionBank();
            foreach (var field in typeof(CaptionBank).GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
            {
                if (field.FieldType == typeof(CaptionLine) && (!(field.GetValue(bank) is CaptionLine line) || string.IsNullOrEmpty(line.text)))
                {
                    field.SetValue(bank, field.GetValue(defaults));
                    EditorUtility.SetDirty(bank);
                }
            }

            Object.DestroyImmediate(defaults);
        }

        private static T EnsureAsset<T>(string path, bool overwrite, System.Action<T> fill)
            where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<T>();
                fill(asset);
                AssetDatabase.CreateAsset(asset, path);
            }
            else if (overwrite)
            {
                fill(asset);
                EditorUtility.SetDirty(asset);
            }

            return asset;
        }

        private static TMP_FontAsset EnsureFont()
        {
            var existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
            if (existing != null)
            {
                return existing;
            }

            var source = AssetDatabase.LoadAssetAtPath<Font>(FredokaTtf);
            if (source == null)
            {
                Debug.LogError($"[EyeCanDo] Font missing at {FredokaTtf}.");
                return null;
            }

            EnsureFolder(Path.GetDirectoryName(FontPath).Replace('\\', '/') + "/");
            var font = TMP_FontAsset.CreateFontAsset(source, 90, 9, GlyphRenderMode.SDFAA, 1024, 1024, AtlasPopulationMode.Dynamic, true);
            font.name = "Fredoka SDF";
            AssetDatabase.CreateAsset(font, FontPath);
            font.atlasTextures[0].name = "Fredoka SDF Atlas";
            AssetDatabase.AddObjectToAsset(font.atlasTextures[0], font);
            font.material.name = "Fredoka SDF Material";
            AssetDatabase.AddObjectToAsset(font.material, font);
            font.TryAddCharacters("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789 .,!?'&-:/+");
            EditorUtility.SetDirty(font);
            return font;
        }

        /// <summary>The theme is generated: it is reset to the code defaults on every build.</summary>
        private static void EnsureTheme(TMP_FontAsset font)
        {
            EnsureFolder(ResourcesFolder);
            var path = ResourcesFolder + UiTheme.ResourcePath + ".asset";
            var theme = AssetDatabase.LoadAssetAtPath<UiTheme>(path);
            if (theme == null)
            {
                theme = ScriptableObject.CreateInstance<UiTheme>();
                AssetDatabase.CreateAsset(theme, path);
            }
            else
            {
                var defaults = ScriptableObject.CreateInstance<UiTheme>();
                EditorUtility.CopySerialized(defaults, theme);
                Object.DestroyImmediate(defaults);
            }

            theme.font = font;
            EditorUtility.SetDirty(theme);
        }

        /// <summary>Indexes every SVG sprite under Art by its path, such as "Characters/leo_head".</summary>
        private static void EnsureArtLibrary()
        {
            var path = ResourcesFolder + ArtLibrary.ResourcePath + ".asset";
            var art = AssetDatabase.LoadAssetAtPath<ArtLibrary>(path);
            if (art == null)
            {
                art = ScriptableObject.CreateInstance<ArtLibrary>();
                AssetDatabase.CreateAsset(art, path);
            }

            art.sprites = new List<ArtLibrary.Entry>();
            foreach (var guid in AssetDatabase.FindAssets("t:Sprite", new[] { ArtFolder }).OrderBy(g => AssetDatabase.GUIDToAssetPath(g)))
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                var sprite = AssetDatabase.LoadAllAssetsAtPath(assetPath).OfType<Sprite>().FirstOrDefault();
                if (sprite == null)
                {
                    continue;
                }

                var name = assetPath.Substring(ArtFolder.Length + 1);
                name = name.Substring(0, name.Length - Path.GetExtension(name).Length);
                art.sprites.Add(new ArtLibrary.Entry { name = name, sprite = sprite });
            }

            Debug.Log($"[EyeCanDo] Art library: {art.sprites.Count} sprites.");
            EditorUtility.SetDirty(art);
        }

        private static void EnsureSoundLibrary()
        {
            var path = ResourcesFolder + SoundLibrary.ResourcePath + ".asset";
            var library = AssetDatabase.LoadAssetAtPath<SoundLibrary>(path);
            if (library == null)
            {
                library = ScriptableObject.CreateInstance<SoundLibrary>();
                AssetDatabase.CreateAsset(library, path);
            }

            // The clips are levelled by Tools/SoundFetch; these volumes set the mix. Sounds that
            // happen often (caption pops, footsteps, coins) are kept quiet so they never nag.
            library.entries = new[]
            {
                Entry(Sound.Tap, 0.5f, "tap"),
                Entry(Sound.Pop, 0.3f, "pop"),
                Entry(Sound.Whoosh, 0.4f, "whoosh"),
                Entry(Sound.Hop, 0.45f, "hop"),
                Entry(Sound.Step, 0.2f, "step"),
                Entry(Sound.DropFall, 0.35f, "drop_fall"),
                Entry(Sound.Catch, 0.6f, "catch"),
                Entry(Sound.Golden, 0.6f, "golden"),
                Entry(Sound.Plip, 0.35f, "plip"),
                Entry(Sound.Shiver, 0.4f, "shiver"),
                Entry(Sound.Blink, 0.4f, "blink"),
                Entry(Sound.Wipe, 0.4f, "wipe"),
                Entry(Sound.Correct, 0.7f, "correct"),
                Entry(Sound.TryAgain, 0.5f, "try_again"),
                Entry(Sound.Wiggle, 0.5f, "wiggle"),
                Entry(Sound.Star, 0.6f, "star"),
                Entry(Sound.Sparkle, 0.5f, "sparkle"),
                Entry(Sound.Tick, 0.4f, "tick"),
                Entry(Sound.Count, 0.5f, "count"),
                Entry(Sound.Go, 0.7f, "go"),
                Entry(Sound.RoundEnd, 0.7f, "round_end"),
                Entry(Sound.Reward, 0.7f, "reward"),
                Entry(Sound.Cheer, 0.7f, "cheer"),
                Entry(Sound.Coin, 0.35f, "coin"),
                Entry(Sound.Purchase, 0.7f, "purchase"),
                Entry(Sound.Locked, 0.5f, "locked"),
            };
            EditorUtility.SetDirty(library);
        }

        /// <summary>A sound's clips: name.wav, or name_1.wav, name_2.wav... picked at random.</summary>
        private static SoundLibrary.Entry Entry(Sound sound, float volume, string name)
        {
            var clips = new List<AudioClip>();
            var single = AssetDatabase.LoadAssetAtPath<AudioClip>(Sounds + name + ".wav");
            if (single != null)
            {
                clips.Add(single);
            }

            for (var i = 1; ; i++)
            {
                var clip = AssetDatabase.LoadAssetAtPath<AudioClip>($"{Sounds}{name}_{i}.wav");
                if (clip == null)
                {
                    break;
                }

                clips.Add(clip);
            }

            if (clips.Count == 0)
            {
                Debug.LogError($"[EyeCanDo] Sound missing: {name}");
            }

            return new SoundLibrary.Entry { sound = sound, clips = clips.ToArray(), volume = volume };
        }

        private static void EnsureBlurMaterial()
        {
            var path = ResourcesFolder + BlurrySpyGame.BlurMaterialPath + ".mat";
            var shader = Shader.Find(BlurShader);
            if (shader == null)
            {
                Debug.LogError($"[EyeCanDo] Shader missing: {BlurShader}");
                return;
            }

            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }

            material.shader = shader;
            EditorUtility.SetDirty(material);
        }

        /// <summary>The avatar recolour material: white parts take the Image colour, black parts the ink.</summary>
        private static void EnsureRecolorMaterial()
        {
            var path = ResourcesFolder + "Recolor.mat";
            var shader = Shader.Find(RecolorShader);
            if (shader == null)
            {
                Debug.LogError($"[EyeCanDo] Shader missing: {RecolorShader}");
                return;
            }

            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }

            material.shader = shader;
            material.SetColor("_Ink", UiTheme.Instance.ink);
            EditorUtility.SetDirty(material);
        }

        private static T Load<T>(string path)
            where T : Object
        {
            return AssetDatabase.LoadAssetAtPath<T>(path);
        }

        private static void BuildScenes()
        {
            EnsureFolder(ScenesFolder);

            // Assets are loaded by path for each scene: opening a new scene unloads earlier references.
            var title = BuildScene<TitleScreen>(SceneNames.Title, so => so.FindProperty("captions").objectReferenceValue = Load<CaptionBank>(CaptionsPath));
            var saves = BuildScene<Menus.SaveSelectScreen>(SceneNames.Saves, so => so.FindProperty("captions").objectReferenceValue = Load<CaptionBank>(CaptionsPath));
            var map = BuildScene<ClinicMap>(SceneNames.ClinicMap, so =>
            {
                so.FindProperty("catalog").objectReferenceValue = Load<StopCatalog>(CatalogPath);
                so.FindProperty("captions").objectReferenceValue = Load<CaptionBank>(CaptionsPath);
            });
            var story = BuildScene<EyeDropsStory>(SceneNames.EyeDropsStory, so =>
            {
                so.FindProperty("script").objectReferenceValue = Load<StoryScript>(StoryPath);
                so.FindProperty("captions").objectReferenceValue = Load<CaptionBank>(CaptionsPath);
            });
            var dropCatch = BuildScene<DropCatchGame>(SceneNames.DropCatch, so => so.FindProperty("captions").objectReferenceValue = Load<CaptionBank>(CaptionsPath));
            var spy = BuildScene<BlurrySpyGame>(SceneNames.BlurrySpy, so => so.FindProperty("captions").objectReferenceValue = Load<CaptionBank>(CaptionsPath));
            var dressUp = BuildScene<DressUp.DressUpScreen>(SceneNames.DressUp, so => so.FindProperty("captions").objectReferenceValue = Load<CaptionBank>(CaptionsPath));

            foreach (var old in new[] { "LookUpBlink", "BalloonBreath" })
            {
                AssetDatabase.DeleteAsset(ScenesFolder + old + ".unity");
            }

            EditorBuildSettings.scenes = new[] { title, saves, map, story, dropCatch, spy, dressUp }
                .Select(p => new EditorBuildSettingsScene(p, true))
                .ToArray();
        }

        private static string BuildScene<T>(string name, System.Action<SerializedObject> assign)
            where T : MonoBehaviour
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            cameraObject.tag = "MainCamera";
            var camera = cameraObject.GetComponent<Camera>();
            camera.orthographic = true;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = UiTheme.Instance.background;
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);

            var eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            eventSystem.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();

            var controller = new GameObject(name, typeof(T));
            var serialized = new SerializedObject(controller.GetComponent<T>());
            assign(serialized);
            serialized.ApplyModifiedPropertiesWithoutUndo();

            var path = ScenesFolder + name + ".unity";
            EditorSceneManager.SaveScene(scene, path);
            return path;
        }

        private static void EnsureFolder(string folder)
        {
            var trimmed = folder.TrimEnd('/');
            if (AssetDatabase.IsValidFolder(trimmed))
            {
                return;
            }

            var parent = Path.GetDirectoryName(trimmed).Replace('\\', '/');
            EnsureFolder(parent + "/");
            AssetDatabase.CreateFolder(parent, Path.GetFileName(trimmed));
        }
    }
}
