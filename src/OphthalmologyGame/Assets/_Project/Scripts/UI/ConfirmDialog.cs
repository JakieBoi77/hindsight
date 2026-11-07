using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Hindsight.Ophthalmology.UI
{
    /// <summary>A yes/no question on a dimmed backdrop (delete a save, reset the game). Saying no changes nothing.</summary>
    public sealed class ConfirmDialog : MonoBehaviour
    {
        [SerializeField] GameObject panel;
        [SerializeField] TMP_Text titleLabel;
        [SerializeField] TMP_Text bodyLabel;
        [SerializeField] Button yesButton;
        [SerializeField] TMP_Text yesLabel;
        [SerializeField] Button noButton;
        [SerializeField] TMP_Text noLabel;

        Action onYes;

        public bool IsOpen => panel.activeSelf;

        void Awake()
        {
            yesButton.onClick.AddListener(() =>
            {
                var action = onYes;
                Close();
                action?.Invoke();
            });
            noButton.onClick.AddListener(Close);
            panel.SetActive(false);
        }

        public void Ask(string title, string body, string yes, string no, Action confirmed)
        {
            titleLabel.text = title;
            bodyLabel.text = body;
            yesLabel.text = yes;
            noLabel.text = no;
            onYes = confirmed;
            panel.SetActive(true);
            panel.transform.SetAsLastSibling();
        }

        public void Close()
        {
            onYes = null;
            panel.SetActive(false);
        }

#if UNITY_EDITOR
        public void EditorInit(GameObject panelObject, TMP_Text title, TMP_Text body, Button yes, TMP_Text yesText, Button no, TMP_Text noText)
        {
            panel = panelObject;
            titleLabel = title;
            bodyLabel = body;
            yesButton = yes;
            yesLabel = yesText;
            noButton = no;
            noLabel = noText;
        }
#endif
    }
}
