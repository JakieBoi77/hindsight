using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Hindsight.Ophthalmology.UI
{
    /// <summary>Speech panel at the bottom of the screen with a typewriter effect and a Next button.</summary>
    public sealed class DialogueView : MonoBehaviour
    {
        [SerializeField] GameObject panel;
        [SerializeField] TMP_Text speakerLabel;
        [SerializeField] TMP_Text bodyLabel;
        [SerializeField] Button nextButton;
        [SerializeField] float charactersPerSecond = 45f;

        /// <summary>True while a line is being typed out. The doctor's mouth follows this.</summary>
        public event System.Action<bool> TypingChanged;

        bool nextPressed;
        int lineVersion; // a newer Say() cancels an older one still typing/waiting

        void Awake()
        {
            nextButton.onClick.AddListener(() => nextPressed = true);
            Hide();
        }

        /// <summary>Shows a line. If <paramref name="waitForNext"/>, waits for the player to tap Next.</summary>
        public IEnumerator Say(string speaker, string text, bool waitForNext = true)
        {
            var version = ++lineVersion;
            panel.SetActive(true);
            speakerLabel.text = speaker;
            speakerLabel.gameObject.SetActive(!string.IsNullOrEmpty(speaker));
            bodyLabel.text = text;
            nextButton.gameObject.SetActive(waitForNext);
            nextPressed = false;

            // Typewriter. Tapping Next during it skips to the full line.
            bodyLabel.maxVisibleCharacters = 0;
            var total = text.Length;
            TypingChanged?.Invoke(true);
            for (var shown = 0f; shown < total && !nextPressed; shown += charactersPerSecond * Time.deltaTime)
            {
                bodyLabel.maxVisibleCharacters = Mathf.FloorToInt(shown);
                yield return null;
                if (version != lineVersion) yield break; // the newer line owns TypingChanged now
            }
            bodyLabel.maxVisibleCharacters = int.MaxValue;
            TypingChanged?.Invoke(false);
            nextPressed = false;

            if (!waitForNext) yield break;
            while (!nextPressed && version == lineVersion) yield return null;
            nextPressed = false;
        }

        public void Hide()
        {
            lineVersion++;
            TypingChanged?.Invoke(false);
            panel.SetActive(false);
        }

#if UNITY_EDITOR
        public void EditorInit(GameObject panelObject, TMP_Text speaker, TMP_Text body, Button next)
        {
            panel = panelObject;
            speakerLabel = speaker;
            bodyLabel = body;
            nextButton = next;
        }
#endif
    }
}
