using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Hindsight.Ophthalmology.UI
{
    /// <summary>Multiple-choice question with picture answers. Wrong answers get gentle feedback and another try.</summary>
    public sealed class QuizView : MonoBehaviour
    {
        [SerializeField] GameObject panel;
        [SerializeField] TMP_Text questionLabel;
        [SerializeField] TMP_Text feedbackLabel;
        [SerializeField] Button[] optionButtons = new Button[0];
        [SerializeField] Image[] optionIcons = new Image[0];
        [SerializeField] TMP_Text[] optionLabels = new TMP_Text[0];
        [SerializeField] Color normalColor = Color.white;
        [SerializeField] Color correctColor = new(0.78f, 0.95f, 0.8f);
        [SerializeField] Color wrongColor = new(1f, 0.85f, 0.85f);
        [SerializeField] Color correctTextColor = new(0.23f, 0.58f, 0.32f);
        [SerializeField] Color wrongTextColor = new(0.85f, 0.31f, 0.31f);

        int picked = -1;

        void Awake()
        {
            for (var i = 0; i < optionButtons.Length; i++)
            {
                var index = i;
                optionButtons[i].onClick.AddListener(() => picked = index);
            }
            panel.SetActive(false);
        }

        public IEnumerator Ask(string question, IReadOnlyList<string> options, IReadOnlyList<Sprite> icons, int correctIndex,
            string correctFeedback, string wrongFeedback, float celebrateSeconds = 1.8f)
        {
            panel.SetActive(true);
            questionLabel.text = question;
            feedbackLabel.text = "";
            for (var i = 0; i < optionButtons.Length; i++)
            {
                var used = i < options.Count;
                optionButtons[i].gameObject.SetActive(used);
                if (!used) continue;
                optionButtons[i].interactable = true;
                optionButtons[i].image.color = normalColor;
                optionLabels[i].text = options[i];
                var icon = icons != null && i < icons.Count ? icons[i] : null;
                optionIcons[i].sprite = icon;
                optionIcons[i].enabled = icon != null;
            }

            while (true)
            {
                picked = -1;
                while (picked < 0) yield return null;
                if (picked == correctIndex) break;
                optionButtons[picked].image.color = wrongColor;
                optionButtons[picked].interactable = false;
                feedbackLabel.color = wrongTextColor;
                feedbackLabel.text = wrongFeedback;
            }

            optionButtons[correctIndex].image.color = correctColor;
            feedbackLabel.color = correctTextColor;
            feedbackLabel.text = correctFeedback;
            foreach (var b in optionButtons) b.interactable = false;
            yield return new WaitForSeconds(celebrateSeconds);
            panel.SetActive(false);
        }

#if UNITY_EDITOR
        public void EditorInit(GameObject panelObject, TMP_Text question, TMP_Text feedback, Button[] buttons, Image[] icons, TMP_Text[] labels)
        {
            panel = panelObject;
            questionLabel = question;
            feedbackLabel = feedback;
            optionButtons = buttons;
            optionIcons = icons;
            optionLabels = labels;
        }
#endif
    }
}
