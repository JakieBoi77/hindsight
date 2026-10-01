using Hindsight.Procedures;
using Hindsight.UI;
using UnityEngine;

namespace Hindsight.App
{
    /// <summary>
    /// Creates the persistent service object before the first scene loads. Because it is
    /// spawned from Resources, pressing Play in any scene works without a dedicated boot scene.
    /// </summary>
    public sealed class GameBootstrapper : MonoBehaviour
    {
        internal const string ResourcePath = "GameBootstrapper";

        [SerializeField] private ProcedureCatalog catalog;
        [SerializeField] private ScreenFader screenFader;
        [SerializeField] private UiAudio uiAudio;
        [SerializeField] private int targetFrameRate = 60;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void CreateBeforeFirstScene()
        {
            if (GameServices.IsInitialized)
            {
                return;
            }

            var prefab = Resources.Load<GameBootstrapper>(ResourcePath);
            if (prefab == null)
            {
                Debug.LogError($"Missing Resources/{ResourcePath} prefab; app services were not created.");
                return;
            }

            var instance = Instantiate(prefab);
            instance.name = prefab.name;
        }

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            Application.targetFrameRate = targetFrameRate;

            var navigator = new GameNavigator(new SceneLoader(screenFader));
            GameServices.Initialize(catalog, new JsonFileProgressStore(), navigator, uiAudio);
        }
    }
}
