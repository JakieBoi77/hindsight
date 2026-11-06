using UnityEngine;
using UnityEngine.EventSystems;

namespace EyeCanDo.UI
{
    /// <summary>Forwards taps anywhere on the screen to the caption bar while a line is waiting.</summary>
    public sealed class CaptionTapRelay : MonoBehaviour, IPointerClickHandler
    {
        public CaptionBar Owner;

        public void OnPointerClick(PointerEventData eventData)
        {
            Owner.SimulateTap();
        }
    }
}
