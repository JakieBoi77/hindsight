using System.Collections.Generic;
using Hindsight.App;
using Hindsight.Core.Progress;
using Hindsight.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Hindsight.Menus
{
    /// <summary>Builds a card per procedure in the catalog and starts the chosen one.</summary>
    public sealed class LevelSelectController : MonoBehaviour
    {
        [SerializeField] private LevelCardView cardPrefab;
        [SerializeField] private RectTransform cardContainer;
        [SerializeField] private Button backButton;

        private readonly List<LevelCardView> cards = new List<LevelCardView>();

        public IReadOnlyList<LevelCardView> Cards => cards;

        private void Awake()
        {
            backButton.onClick.AddListener(OnBackClicked);
        }

        private void Start()
        {
            if (!GameServices.IsInitialized)
            {
                Debug.LogError("Game services are not initialised; cannot build the level list.");
                return;
            }

            BuildCards();
        }

        private void BuildCards()
        {
            foreach (var procedure in GameServices.Catalog.Procedures)
            {
                var card = Instantiate(cardPrefab, cardContainer);
                card.Bind(procedure, GameServices.Progress.GetAvailability(procedure));
                card.Clicked += OnCardClicked;
                cards.Add(card);
            }
        }

        private void OnCardClicked(LevelCardView card)
        {
            if (!UnlockRules.CanPlay(card.Availability))
            {
                card.PlayLockedFeedback(destroyCancellationToken);
                return;
            }

            GameServices.Navigator.StartProcedureAsync(card.Procedure).Forget();
        }

        private void OnBackClicked()
        {
            GameServices.Navigator.GoToMainMenuAsync().Forget();
        }
    }
}
