using UnityEngine;
using UnityEngine.EventSystems;

namespace EyeCanDo.UI
{
    /// <summary>Forwards press and release on the skip button to the hold-to-skip rule.</summary>
    public sealed class SkipPressRelay : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        public LevelChrome Owner;

        public void OnPointerDown(PointerEventData eventData)
        {
            Owner.PressSkip();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            Owner.ReleaseSkip();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            Owner.ReleaseSkip();
        }
    }
}
