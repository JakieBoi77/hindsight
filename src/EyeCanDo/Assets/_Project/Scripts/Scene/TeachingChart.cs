using UnityEngine;

namespace Hindsight.Ophthalmology.SceneObjects
{
    /// <summary>
    /// The wall chart Dr. Iris explains the drops with: two picture panels (a drop goes in, the pupil
    /// gets big) that light up one after another as she talks about each one.
    /// The panel being talked about glows and pulses; the ones already explained keep a soft glow.
    /// </summary>
    public sealed class TeachingChart : MonoBehaviour
    {
        [SerializeField] Transform[] panels = new Transform[0];
        [Tooltip("A glowing frame behind each panel, same order as Panels.")]
        [SerializeField] Renderer[] glows = new Renderer[0];
        [SerializeField] Color glowColor = new(1f, 0.8f, 0.2f);
        [SerializeField] float pulse = 0.03f;

        Vector3[] baseScales;
        MaterialPropertyBlock block;
        int current = -1;
        float since;

        public int PanelCount => panels.Length;

        void Awake()
        {
            baseScales = new Vector3[panels.Length];
            for (var i = 0; i < panels.Length; i++) baseScales[i] = panels[i].localScale;
            block = new MaterialPropertyBlock();
            ResetChart();
        }

        /// <summary>Turns every panel off.</summary>
        public void ResetChart()
        {
            current = -1;
            foreach (var g in glows)
                if (g != null) g.enabled = false;
        }

        /// <summary>Lights panel <paramref name="index"/> (and keeps the earlier ones softly lit).</summary>
        public void Light(int index)
        {
            if (index < 0 || index >= panels.Length || index <= current) return;
            current = index;
            since = 0f;
            for (var i = 0; i < glows.Length; i++)
                if (glows[i] != null) glows[i].enabled = i <= index;
        }

        void Update()
        {
            if (baseScales == null) return;
            since += Time.deltaTime;
            for (var i = 0; i < panels.Length; i++)
            {
                var active = i == current;
                // A pop when it lights, then a gentle pulse while it is the one being talked about.
                var pop = active ? Mathf.Max(0f, 1f - since * 3f) * 0.12f : 0f;
                var wobble = active ? Mathf.Sin(Time.time * 5f) * pulse : 0f;
                panels[i].localScale = baseScales[i] * (1f + pop + wobble);
                if (i < glows.Length && glows[i] != null && glows[i].enabled)
                {
                    var strength = active ? 2.2f + Mathf.Sin(Time.time * 5f) * 0.6f : 0.8f;
                    glows[i].GetPropertyBlock(block);
                    block.SetColor("_BaseColor", glowColor);
                    block.SetColor("_EmissionColor", glowColor * strength);
                    glows[i].SetPropertyBlock(block);
                }
            }
        }

#if UNITY_EDITOR
        public void EditorInit(Transform[] panelTransforms, Renderer[] glowRenderers)
        {
            panels = panelTransforms;
            glows = glowRenderers;
        }
#endif
    }
}
