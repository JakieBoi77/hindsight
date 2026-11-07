using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Hindsight.Ophthalmology.UI
{
    /// <summary>A chunky switch: green with a tick and "On", or grey and "Off" (the state never depends on colour alone).</summary>
    public sealed class OnOffButton : MonoBehaviour
    {
        [SerializeField] Button button;
        [SerializeField] Image face;
        [SerializeField] Image depth;
        [SerializeField] TMP_Text label;
        [SerializeField] GameObject tick;
        [SerializeField] Color onFace = new(0.3f, 0.72f, 0.4f);
        [SerializeField] Color onDepth = new(0.23f, 0.58f, 0.32f);
        [SerializeField] Color offFace = new(0.6f, 0.64f, 0.72f);
        [SerializeField] Color offDepth = new(0.45f, 0.49f, 0.57f);

        public bool Value { get; private set; }
        public event Action<bool> Changed;

        void Awake() => button.onClick.AddListener(() =>
        {
            Set(!Value);
            Changed?.Invoke(Value);
        });

        public void Set(bool value)
        {
            Value = value;
            face.color = value ? onFace : offFace;
            depth.color = value ? onDepth : offDepth;
            label.text = value ? "On" : "Off";
            if (tick != null) tick.SetActive(value);
        }

#if UNITY_EDITOR
        public void EditorInit(Button b, Image faceImage, Image depthImage, TMP_Text text, GameObject tickMark)
        {
            button = b;
            face = faceImage;
            depth = depthImage;
            label = text;
            tick = tickMark;
        }
#endif
    }
}
