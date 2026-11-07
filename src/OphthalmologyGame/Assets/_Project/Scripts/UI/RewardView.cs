using System.Collections;
using Hindsight.Ophthalmology.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Hindsight.Ophthalmology.UI
{
    /// <summary>End-of-level sticker, confetti and the button back to level select.</summary>
    public sealed class RewardView : MonoBehaviour
    {
        [SerializeField] GameObject panel;
        [SerializeField] RectTransform sticker;
        [SerializeField] TMP_Text titleLabel;
        [SerializeField] TMP_Text bodyLabel;
        [SerializeField] Button continueButton;
        [SerializeField] ParticleSystem confetti;

        void Awake()
        {
            continueButton.onClick.AddListener(GameFlow.ReturnToLevelSelect);
            panel.SetActive(false);
        }

        public IEnumerator Show(string title, string body)
        {
            panel.SetActive(true);
            titleLabel.text = title;
            bodyLabel.text = body;
            if (confetti != null) confetti.Play();

            // Pop the sticker in with a little overshoot.
            for (var t = 0f; t < 0.6f; t += Time.deltaTime)
            {
                var k = t / 0.6f;
                var s = 1f + 0.25f * Mathf.Sin(k * Mathf.PI) * (1f - k);
                sticker.localScale = Vector3.one * (k * s);
                sticker.localRotation = Quaternion.Euler(0, 0, Mathf.Lerp(-25f, 8f, k));
                yield return null;
            }
            sticker.localScale = Vector3.one;
        }

#if UNITY_EDITOR
        public void EditorInit(GameObject panelObject, RectTransform stickerRect, TMP_Text title, TMP_Text body,
            Button button, ParticleSystem particles)
        {
            panel = panelObject;
            sticker = stickerRect;
            titleLabel = title;
            bodyLabel = body;
            continueButton = button;
            confetti = particles;
        }
#endif
    }
}
