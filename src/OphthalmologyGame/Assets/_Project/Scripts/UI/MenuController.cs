using System.Collections.Generic;
using Hindsight.Ophthalmology.Core;
using Hindsight.Ophthalmology.SceneObjects;
using UnityEngine;
using UnityEngine.UI;

namespace Hindsight.Ophthalmology.UI
{
    /// <summary>Title screen ("Play") and the level select list, both in the Menu scene.</summary>
    public sealed class MenuController : MonoBehaviour
    {
        [SerializeField] LevelCatalog catalog;
        [SerializeField] GameObject titlePanel;
        [SerializeField] GameObject levelSelectPanel;
        [SerializeField] Button playButton;
        [SerializeField] Button backButton;
        [SerializeField] RectTransform levelListRoot;
        [SerializeField] LevelCardView cardTemplate;
        [SerializeField] DoctorActor doctor;

        readonly List<LevelCardView> spawned = new();

        void Awake()
        {
            playButton.onClick.AddListener(ShowLevelSelect);
            backButton.onClick.AddListener(ShowTitle);
            cardTemplate.gameObject.SetActive(false);
        }

        void Start()
        {
            if (GameFlow.OpenLevelSelectOnMenuLoad)
            {
                GameFlow.ConsumeMenuRequest();
                ShowLevelSelect();
            }
            else
            {
                ShowTitle();
            }
        }

        void ShowTitle()
        {
            titlePanel.SetActive(true);
            levelSelectPanel.SetActive(false);
            if (doctor != null) doctor.Play(DoctorPose.Wave);
        }

        void ShowLevelSelect()
        {
            titlePanel.SetActive(false);
            levelSelectPanel.SetActive(true);
            BuildLevelList();
        }

        void BuildLevelList()
        {
            foreach (var b in spawned) Destroy(b.gameObject);
            spawned.Clear();

            foreach (var level in catalog.Levels)
            {
                var card = Instantiate(cardTemplate, levelListRoot);
                card.gameObject.SetActive(true);
                card.Bind(level);
                spawned.Add(card);
            }
        }

#if UNITY_EDITOR
        public void EditorInit(LevelCatalog levelCatalog, GameObject title, GameObject levelSelect, Button play,
            Button back, RectTransform listRoot, LevelCardView template, DoctorActor doc)
        {
            catalog = levelCatalog;
            titlePanel = title;
            levelSelectPanel = levelSelect;
            playButton = play;
            backButton = back;
            levelListRoot = listRoot;
            cardTemplate = template;
            doctor = doc;
        }
#endif
    }
}
