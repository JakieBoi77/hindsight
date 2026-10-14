using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Hindsight.Ophthalmology.UI
{
    /// <summary>Big gameplay buttons: "hold to keep your eye open" (with a progress ring) and "blink".</summary>
    public sealed class ActionButtonsView : MonoBehaviour
    {
        [SerializeField] HoldButton holdButton;
        [SerializeField] TMP_Text holdLabel;
        [SerializeField] Image holdRing;
        [SerializeField] Button tapButton;
        [SerializeField] TMP_Text tapLabel;

        int taps;

        public HoldButton Hold => holdButton;
        public int Taps => taps;

        void Awake()
        {
            tapButton.onClick.AddListener(() => taps++);
            HideAll();
        }

        public void ShowHold(string label)
        {
            holdLabel.text = label;
            SetHoldProgress(0f);
            holdButton.transform.parent.gameObject.SetActive(true);
        }

        /// <summary>Fills the ring around the hold button (0..1) as the drop falls.</summary>
        public void SetHoldProgress(float t)
        {
            if (holdRing != null) holdRing.fillAmount = Mathf.Clamp01(t);
        }

        public void ShowTap(string label)
        {
            taps = 0;
            tapLabel.text = label;
            tapButton.transform.parent.gameObject.SetActive(true);
        }

        public void SetTapLabel(string label) => tapLabel.text = label;

        public void HideAll()
        {
            holdButton.transform.parent.gameObject.SetActive(false);
            tapButton.transform.parent.gameObject.SetActive(false);
        }

#if UNITY_EDITOR
        public void EditorInit(HoldButton hold, TMP_Text holdText, Image ring, Button tap, TMP_Text tapText)
        {
            holdButton = hold;
            holdLabel = holdText;
            holdRing = ring;
            tapButton = tap;
            tapLabel = tapText;
        }
#endif
    }
}
