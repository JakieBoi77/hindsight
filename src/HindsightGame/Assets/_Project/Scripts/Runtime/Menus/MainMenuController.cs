using Hindsight.App;
using Hindsight.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Hindsight.Menus
{
    /// <summary>Title screen. Only "Play" for the PoC; settings and parent area come later.</summary>
    public sealed class MainMenuController : MonoBehaviour
    {
        [SerializeField] private Button playButton;

        private void OnEnable()
        {
            playButton.onClick.AddListener(OnPlayClicked);
        }

        private void OnDisable()
        {
            playButton.onClick.RemoveListener(OnPlayClicked);
        }

        private void OnPlayClicked()
        {
            if (GameServices.Navigator == null)
            {
                Debug.LogError("Navigation services are not initialised.");
                return;
            }

            GameServices.Navigator.GoToLevelSelectAsync().Forget();
        }
    }
}
