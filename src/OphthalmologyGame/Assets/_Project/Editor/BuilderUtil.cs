using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Hindsight.Ophthalmology.EditorTools
{
    /// <summary>Small helpers shared by the POC builders: folders, materials, model fitting, uGUI.</summary>
    internal static class BuilderUtil
    {
        public const string Root = "Assets/_Project";
        public const string MaterialsDir = Root + "/Materials";
        public const string PrefabsDir = Root + "/Prefabs";
        public const string DataDir = Root + "/Data";
        public const string ScenesDir = Root + "/Scenes";
        public const string FontPath = "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset";

        static TMP_FontAsset font;
        public static TMP_FontAsset TmpFont => font != null ? font : font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);

        public static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            var parent = Path.GetDirectoryName(path)!.Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }

        /// <summary>Loads or creates a ScriptableObject asset so GUIDs stay stable across rebuilds.</summary>
        public static T LoadOrCreate<T>(string path) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null) return asset;
            EnsureFolder(Path.GetDirectoryName(path)!.Replace('\\', '/'));
            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        // ---------- Materials ----------

        public static Material Lit(string name, Color color, float smoothness = 0.25f, Color? emission = null)
        {
            var path = $"{MaterialsDir}/{name}.mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                EnsureFolder(MaterialsDir);
                mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                AssetDatabase.CreateAsset(mat, path);
            }
            mat.SetColor("_BaseColor", color);
            mat.SetFloat("_Smoothness", smoothness);
            if (emission.HasValue)
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", emission.Value);
                mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            }
            EditorUtility.SetDirty(mat);
            return mat;
        }

        public static Material ParticleUnlit(string name)
        {
            var path = $"{MaterialsDir}/{name}.mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat != null) return mat;
            EnsureFolder(MaterialsDir);
            mat = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
            mat.SetColor("_BaseColor", Color.white);
            AssetDatabase.CreateAsset(mat, path);
            return mat;
        }

        /// <summary>Copies an imported (read-only) material so its colour can be changed.</summary>
        public static Material Recolor(Material source, string name, Color color)
        {
            var path = $"{MaterialsDir}/{name}.mat";
            AssetDatabase.DeleteAsset(path);
            var mat = new Material(source) { name = name };
            foreach (var prop in new[] { "baseColorFactor", "_BaseColor", "_Color" })
                if (mat.HasProperty(prop)) mat.SetColor(prop, color);
            AssetDatabase.CreateAsset(mat, path);
            return mat;
        }

        // ---------- Primitives & models ----------

        public static GameObject Primitive(PrimitiveType type, string name, Transform parent, Vector3 localPos,
            Vector3 localScale, Material mat, bool keepCollider = false)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = localScale;
            go.GetComponent<Renderer>().sharedMaterial = mat;
            if (!keepCollider) Object.DestroyImmediate(go.GetComponent<Collider>());
            return go;
        }

        public static GameObject InstantiateModel(string assetPath, Transform parent)
        {
            var src = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (src == null)
            {
                Debug.LogError($"PocBuilder: model not found at {assetPath}");
                return null;
            }
            var go = (GameObject)PrefabUtility.InstantiatePrefab(src, parent);
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            return go;
        }

        public static Bounds WorldBounds(GameObject go)
        {
            var renderers = go.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0) return new Bounds(go.transform.position, Vector3.zero);
            var b = renderers[0].bounds;
            foreach (var r in renderers) b.Encapsulate(r.bounds);
            return b;
        }

        /// <summary>Moves <paramref name="visual"/> so its bounds sit centred on, and on top of, its parent's origin.</summary>
        public static void GroundAndCenter(GameObject visual)
        {
            var b = WorldBounds(visual);
            var anchor = visual.transform.parent != null ? visual.transform.parent.position : Vector3.zero;
            visual.transform.position += anchor - new Vector3(b.center.x, b.min.y, b.center.z);
        }

        public static void ScaleUniform(GameObject visual, float factor) => visual.transform.localScale *= factor;

        // ---------- uGUI ----------

        public static Sprite RoundedSprite => AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        public static Sprite CircleSprite => AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");

        public static RectTransform Rect(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 offsetMin = default, Vector2 offsetMax = default)
        {
            var go = new GameObject(name, typeof(RectTransform)) { layer = 5 };
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = offsetMin;
            rt.offsetMax = offsetMax;
            return rt;
        }

        public static RectTransform Stretch(string name, Transform parent) => Rect(name, parent, Vector2.zero, Vector2.one);

        public static Image Img(RectTransform rt, Color color, Sprite sprite = null, bool raycast = true)
        {
            var img = rt.gameObject.AddComponent<Image>();
            img.color = color;
            img.raycastTarget = raycast;
            if (sprite != null)
            {
                img.sprite = sprite;
                img.type = Image.Type.Sliced;
            }
            return img;
        }

        public static TextMeshProUGUI Text(RectTransform parent, string name, string text, float size, Color color,
            TextAlignmentOptions align = TextAlignmentOptions.Center, bool bold = false)
        {
            var rt = Stretch(name, parent);
            var tmp = rt.gameObject.AddComponent<TextMeshProUGUI>();
            tmp.font = TmpFont;
            tmp.text = text;
            tmp.fontSize = size;
            tmp.color = color;
            tmp.alignment = align;
            tmp.textWrappingMode = TextWrappingModes.Normal;
            tmp.raycastTarget = false;
            if (bold) tmp.fontStyle = FontStyles.Bold;
            return tmp;
        }

        public static Button Button(RectTransform parent, string name, string label, Color color, float fontSize,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin = default, Vector2 offsetMax = default,
            Sprite sprite = null)
        {
            var rt = Rect(name, parent, anchorMin, anchorMax, offsetMin, offsetMax);
            var img = Img(rt, color, sprite != null ? sprite : RoundedSprite);
            var button = rt.gameObject.AddComponent<Button>();
            button.targetGraphic = img;
            var colors = button.colors;
            colors.pressedColor = new Color(0.8f, 0.8f, 0.8f);
            colors.disabledColor = new Color(0.85f, 0.85f, 0.85f, 0.8f);
            button.colors = colors;
            var text = Text(rt, "Label", label, fontSize, Color.white, TextAlignmentOptions.Center, bold: true);
            text.rectTransform.offsetMin = new Vector2(16, 8);
            text.rectTransform.offsetMax = new Vector2(-16, -8);
            return button;
        }
    }
}
