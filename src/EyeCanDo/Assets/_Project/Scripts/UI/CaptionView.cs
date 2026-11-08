using System.Collections;
using Hindsight.EyeCanDo.Core;
using Hindsight.EyeCanDo.Procedure;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Hindsight.EyeCanDo.UI
{
    /// <summary>
    /// The narrated-line bubble at the bottom of the screen: the speaker's live 3D portrait, an optional
    /// one-line caption, a replay button and a Next arrow that appears once the line has been said.
    /// Captions can be turned off in settings; the portrait, replay and Next stay.
    /// </summary>
    public sealed class CaptionView : MonoBehaviour
    {
        const float AutoContinueSeconds = 1.6f;

        [SerializeField] GameObject panel;
        [SerializeField] Image bubble;
        [SerializeField] RawImage portrait;
        [SerializeField] TMP_Text captionLabel;
        [SerializeField] Button nextButton;
        [SerializeField] Button replayButton;
        [SerializeField] NarrationPlayer narrator;
        [Tooltip("Live render of each speaker's 3D model, per Speaker in enum order (see SpeakerPortraits).")]
        [SerializeField] Texture[] portraits = new Texture[0];
        [Tooltip("Bubble colour per Speaker, in enum order.")]
        [SerializeField] Color[] bubbleColors = new Color[0];

        StoryLine current;
        bool nextPressed;
        bool replayRequested;
        int lineVersion;
        float baseFontSize;

        public NarrationPlayer Narrator => narrator;

        void Awake()
        {
            nextButton.onClick.AddListener(() => nextPressed = true);
            replayButton.onClick.AddListener(() => replayRequested = true);
            baseFontSize = captionLabel.fontSize;
            GameServices.SettingsChanged += ApplySettings;
            panel.SetActive(false);
        }

        void OnDestroy() => GameServices.SettingsChanged -= ApplySettings;

        /// <summary>Shows and speaks a line. If <paramref name="waitForNext"/>, waits for Next (or auto-continue).</summary>
        public IEnumerator Say(StoryLine line, bool waitForNext = true)
        {
            var version = ++lineVersion;
            Show(line);
            nextButton.gameObject.SetActive(false);
            nextPressed = replayRequested = false;

            yield return narrator.Play(line);
            if (version != lineVersion) yield break;
            if (!waitForNext) yield break;

            nextButton.gameObject.SetActive(true);
            var waited = 0f;
            while (!nextPressed && version == lineVersion)
            {
                if (replayRequested)
                {
                    replayRequested = false;
                    yield return narrator.Play(current);
                    waited = 0f;
                    continue;
                }
                waited += Time.deltaTime;
                if (GameServices.Settings.autoContinue && waited >= AutoContinueSeconds) break;
                yield return null;
            }
            nextPressed = false;
            if (version == lineVersion) nextButton.gameObject.SetActive(false);
        }

        /// <summary>Speaks a line without the bubble (used under full-screen cards).</summary>
        public IEnumerator Speak(StoryLine line)
        {
            if (line == null || line.IsEmpty) yield break;
            yield return narrator.Play(line);
        }

        /// <summary>
        /// A short extra line (a discovered fact) said over the current one; the current caption
        /// comes back afterwards. Does not change what the panel is waiting for.
        /// </summary>
        public IEnumerator Aside(StoryLine line)
        {
            if (line == null || line.IsEmpty) yield break;
            var previous = current;
            var wasOpen = panel.activeSelf;
            Show(line);
            yield return narrator.Play(line);
            if (current != line) yield break; // the panel moved on
            if (wasOpen && previous != null) Show(previous);
            else panel.SetActive(false);
        }

        public void Hide()
        {
            lineVersion++;
            narrator.Stop();
            current = null;
            panel.SetActive(false);
        }

        void Update()
        {
            // Replay also works while the line is still being said, or when nothing waits for Next.
            if (!replayRequested || current == null || nextButton.gameObject.activeSelf) return;
            replayRequested = false;
            StartCoroutine(narrator.Play(current));
        }

        void Show(StoryLine line)
        {
            current = line;
            panel.SetActive(true);
            var index = (int)line.speaker;
            portrait.texture = index < portraits.Length ? portraits[index] : null;
            portrait.transform.parent.parent.gameObject.SetActive(portrait.texture != null);
            if (index < bubbleColors.Length) bubble.color = bubbleColors[index];
            captionLabel.text = line.caption;
            ApplySettings();
        }

        void ApplySettings()
        {
            var settings = GameServices.Settings;
            captionLabel.gameObject.SetActive(settings.captionsOn);
            captionLabel.fontSize = baseFontSize * settings.CaptionScale;
        }

#if UNITY_EDITOR
        public void EditorInit(GameObject panelObject, Image bubbleImage, RawImage portraitImage, TMP_Text caption, Button next,
            Button replay, NarrationPlayer player, Texture[] speakerPortraits, Color[] speakerColors)
        {
            panel = panelObject;
            bubble = bubbleImage;
            portrait = portraitImage;
            captionLabel = caption;
            nextButton = next;
            replayButton = replay;
            narrator = player;
            portraits = speakerPortraits;
            bubbleColors = speakerColors;
        }
#endif
    }
}
