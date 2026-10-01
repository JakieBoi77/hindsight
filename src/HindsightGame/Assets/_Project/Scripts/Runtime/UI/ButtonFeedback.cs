using Hindsight.App;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Hindsight.UI
{
    /// <summary>Squish-on-press and a tap sound, so every button feels responsive to small children.</summary>
    [RequireComponent(typeof(Button))]
    public sealed class ButtonFeedback : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private float pressedScale = 0.92f;

        private Button button;
        private Vector3 baseScale;

        private void Awake()
        {
            button = GetComponent<Button>();
            baseScale = transform.localScale;
        }

        private void OnEnable()
        {
            button.onClick.AddListener(PlayTapSound);
        }

        private void OnDisable()
        {
            button.onClick.RemoveListener(PlayTapSound);
            transform.localScale = baseScale;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (button.IsInteractable())
            {
                transform.localScale = baseScale * pressedScale;
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            transform.localScale = baseScale;
        }

        private static void PlayTapSound()
        {
            if (GameServices.Audio != null)
            {
                GameServices.Audio.PlayTap();
            }
        }
    }
}
