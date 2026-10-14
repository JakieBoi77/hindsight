using Hindsight.Ophthalmology.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Hindsight.Ophthalmology.UI
{
    /// <summary>One tile on the level select screen: icon, name, and a lock or "done" badge.</summary>
    public sealed class LevelCardView : MonoBehaviour
    {
        [SerializeField] Button button;
        [SerializeField] Image face;
        [SerializeField] Image icon;
        [SerializeField] TMP_Text title;
        [SerializeField] TMP_Text blurb;
        [SerializeField] GameObject lockBadge;
        [SerializeField] GameObject doneBadge;
        [SerializeField] Color lockedFace = new(0.88f, 0.9f, 0.94f);
        [SerializeField] Color lockedIconTint = new(1f, 1f, 1f, 0.35f);

        public Button Button => button;

        public void Bind(LevelDefinition level)
        {
            var unlocked = LevelProgress.IsUnlocked(level) && level.IsPlayable;
            title.text = level.DisplayName;
            blurb.text = unlocked ? level.Blurb : "Coming soon!";
            icon.sprite = level.Icon;
            icon.enabled = level.Icon != null;
            icon.color = unlocked ? Color.white : lockedIconTint;
            face.color = unlocked ? Color.white : lockedFace;
            lockBadge.SetActive(!unlocked);
            doneBadge.SetActive(unlocked && LevelProgress.IsCompleted(level));
            button.interactable = unlocked;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => GameFlow.LoadLevel(level));
        }

#if UNITY_EDITOR
        public void EditorInit(Button b, Image faceImage, Image iconImage, TMP_Text titleText, TMP_Text blurbText, GameObject lockGo, GameObject doneGo)
        {
            button = b;
            face = faceImage;
            icon = iconImage;
            title = titleText;
            blurb = blurbText;
            lockBadge = lockGo;
            doneBadge = doneGo;
        }
#endif
    }
}
