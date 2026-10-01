using System.Collections.Generic;
using System.IO;
using Hindsight.Editor.Validation;
using Hindsight.Procedures;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace Hindsight.Editor.Scaffolding
{
    /// <summary>
    /// Bootstraps the PoC's scenes, prefabs and procedure data from code.
    ///
    /// This is a one-time generator, not part of the normal workflow: once the generated assets
    /// are committed, edit the scenes, prefabs and data assets directly in the Unity Editor.
    /// Re-running it OVERWRITES everything it generates (listed in <see cref="GeneratedFolders"/>).
    ///
    /// Headless: Unity -batchmode -quit -projectPath . -executeMethod Hindsight.Editor.Scaffolding.PocContentScaffolder.Run
    /// </summary>
    public static partial class PocContentScaffolder
    {
        private const string Root = "Assets/_Project/";
        private const string ScenesFolder = Root + "Scenes/";
        private const string PrefabsFolder = Root + "Prefabs/";
        private const string StepPrefabsFolder = PrefabsFolder + "Steps/";
        private const string SharedPrefabsFolder = PrefabsFolder + "Shared/";
        private const string DataFolder = Root + "Data/Procedures/";
        private const string ResourcesFolder = Root + "Resources/";
        private const string FontAssetPath = Root + "Fonts/Fredoka SDF.asset";
        private const string FontSourcePath = "Assets/ThirdParty/Fonts/Fredoka/Fredoka-Variable.ttf";

        private static readonly string[] GeneratedFolders =
        {
            ScenesFolder, StepPrefabsFolder, SharedPrefabsFolder, DataFolder, ResourcesFolder,
        };

        [MenuItem("Hindsight/Setup/2. Scaffold PoC Content (overwrites)", priority = 2)]
        public static void Run()
        {
            if (!Application.isBatchMode && !EditorUtility.DisplayDialog(
                    "Scaffold PoC content",
                    "This regenerates the PoC scenes, prefabs and procedure data, overwriting any edits to them. Continue?",
                    "Regenerate",
                    "Cancel"))
            {
                return;
            }

            if (TMP_Settings.instance == null)
            {
                Debug.LogError("[Hindsight] TextMeshPro essentials are missing. Run 'Window > TextMeshPro > Import TMP Essential Resources' first.");
                return;
            }

            ResetGeneratedFolders();

            // Work in a throwaway scene so in-memory prefab hierarchies never touch a real scene.
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            UiBuilder.Font = CreateFontAsset();
            var shared = BuildSharedPrefabs();
            var steps = BuildStepPrefabs(shared);
            var catalog = BuildProcedureData(steps);
            BuildBootstrapper(catalog);
            BuildScenes();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // Refresh can unload in-memory objects; validate what is actually on disk.
            catalog = AssetDatabase.LoadAssetAtPath<ProcedureCatalog>(DataFolder + "ProcedureCatalog.asset");
            var problems = ProcedureValidator.Collect(catalog);
            foreach (var problem in problems)
            {
                Debug.LogError("[Hindsight] Content problem: " + problem);
            }

            Debug.Log($"[Hindsight] Scaffold complete with {problems.Count} content problem(s).");
        }

        /// <summary>Regenerates only the title screen, leaving all other generated assets untouched.</summary>
        [MenuItem("Hindsight/Setup/3. Rebuild Main Menu Scene Only", priority = 3)]
        public static void RebuildMainMenu()
        {
            UiBuilder.Font = CreateFontAsset();
            BuildMainMenuScene();
            AssetDatabase.SaveAssets();
            Debug.Log("[Hindsight] Main menu scene rebuilt.");
        }

        private static void ResetGeneratedFolders()
        {
            foreach (var folder in GeneratedFolders)
            {
                var trimmed = folder.TrimEnd('/');
                if (AssetDatabase.IsValidFolder(trimmed))
                {
                    AssetDatabase.DeleteAsset(trimmed);
                }

                EnsureFolder(trimmed);
            }

            EnsureFolder(Root + "Fonts");
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            var parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
            if (!string.IsNullOrEmpty(parent))
            {
                EnsureFolder(parent);
            }

            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }

        private static TMP_FontAsset CreateFontAsset()
        {
            var existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath);
            if (existing != null)
            {
                return existing;
            }

            var source = AssetDatabase.LoadAssetAtPath<Font>(FontSourcePath);
            if (source == null)
            {
                throw new FileNotFoundException($"Font source missing at {FontSourcePath}.");
            }

            // Dynamic atlas so any character (e.g. future translations) renders without re-baking.
            var fontAsset = TMP_FontAsset.CreateFontAsset(source, 90, 9, GlyphRenderMode.SDFAA, 1024, 1024, AtlasPopulationMode.Dynamic, true);
            fontAsset.name = "Fredoka SDF";
            AssetDatabase.CreateAsset(fontAsset, FontAssetPath);

            fontAsset.atlasTextures[0].name = "Fredoka SDF Atlas";
            AssetDatabase.AddObjectToAsset(fontAsset.atlasTextures[0], fontAsset);
            fontAsset.material.name = "Fredoka SDF Material";
            AssetDatabase.AddObjectToAsset(fontAsset.material, fontAsset);

            fontAsset.TryAddCharacters(" !\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~’—…");
            EditorUtility.SetDirty(fontAsset);

            TMP_Settings.defaultFontAsset = fontAsset;
            EditorUtility.SetDirty(TMP_Settings.instance);
            AssetDatabase.SaveAssets();
            return fontAsset;
        }

        private static T CreateAsset<T>(string path)
            where T : ScriptableObject
        {
            var asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        private static List<T> ListOf<T>(params T[] items)
        {
            return new List<T>(items);
        }
    }
}
