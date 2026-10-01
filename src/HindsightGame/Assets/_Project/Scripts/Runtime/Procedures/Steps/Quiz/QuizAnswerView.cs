using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Hindsight.Procedures.Steps
{
    /// <summary>One answer card in a quiz.</summary>
    public sealed class QuizAnswerView : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private Image background;
        [SerializeField] private Image icon;
        [SerializeField] private TMP_Text label;
        [SerializeField] private GameObject correctBadge;
        [SerializeField] private Color correctColor = new Color(0.55f, 0.85f, 0.55f);
        [SerializeField] private Color dimmedColor = new Color(0.8f, 0.8f, 0.8f);

        public Button Button => button;

        public void Bind(QuizAnswer answer)
        {
            icon.sprite = answer.Icon;
            icon.enabled = answer.Icon != null;
            label.text = answer.Text;
            correctBadge.SetActive(false);
        }

        public void ShowCorrect()
        {
            background.color = correctColor;
            correctBadge.SetActive(true);
        }

        public void ShowTriedAlready()
        {
            button.interactable = false;
            background.color = dimmedColor;
        }
    }
}
