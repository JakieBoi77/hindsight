using System.Collections;
using System.Collections.Generic;
using Hindsight.EyeCanDo.Core;
using Hindsight.EyeCanDo.SceneObjects;
using UnityEngine;
using UnityEngine.UI;

namespace Hindsight.EyeCanDo.UI
{
    /// <summary>
    /// The menu scene: title (Play, Settings) → save files → level select (coins, Dress Up) and
    /// the dress-up screen. The doctor greets on the title; on level select and dress-up the
    /// player's own kid stands in the room in their clothes.
    /// </summary>
    public sealed class MenuController : MonoBehaviour
    {
        [SerializeField] LevelCatalog catalog;
        [SerializeField] GameObject titlePanel;
        [SerializeField] GameObject savesPanel;
        [SerializeField] GameObject levelSelectPanel;
        [SerializeField] GameObject dressUpPanel;
        [SerializeField] GameObject resetNotice;
        [SerializeField] Button playButton;
        [SerializeField] Button settingsButton;
        [SerializeField] Button savesBackButton;
        [SerializeField] Button levelsBackButton;
        [SerializeField] Button dressUpButton;
        [SerializeField] RectTransform levelListRoot;
        [SerializeField] LevelCardView cardTemplate;
        [SerializeField] SaveSelectView saves;
        [SerializeField] DressUpView dressUp;
        [SerializeField] SettingsView settings;
        [SerializeField] DoctorActor doctor;
        [SerializeField] KidActor kid;
        [SerializeField] Transform cameraRig;
        [SerializeField] Transform roomView;
        [SerializeField] Transform mirrorView;
        [SerializeField] Transform kidView;

        readonly List<LevelCardView> spawned = new();
        Coroutine cameraMove;

        void Awake()
        {
            playButton.onClick.AddListener(ShowSaves);
            settingsButton.onClick.AddListener(() => settings.Open());
            savesBackButton.onClick.AddListener(ShowTitle);
            levelsBackButton.onClick.AddListener(ShowSaves);
            dressUpButton.onClick.AddListener(ShowDressUp);
            saves.Picked += OnSavePicked;
            dressUp.Closed += ShowLevelSelect;
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
            if (GameServices.JustReset)
            {
                GameServices.JustReset = false;
                StartCoroutine(Notice());
            }
        }

        IEnumerator Notice()
        {
            resetNotice.SetActive(true);
            for (var t = 0f; t < 3f; t += Time.unscaledDeltaTime) yield return null;
            resetNotice.SetActive(false);
        }

        void Show(GameObject panel, Transform view = null)
        {
            titlePanel.SetActive(panel == titlePanel);
            savesPanel.SetActive(panel == savesPanel);
            levelSelectPanel.SetActive(panel == levelSelectPanel);
            dressUpPanel.SetActive(panel == dressUpPanel);
            MoveCamera(view != null ? view : roomView);
        }

        void ShowTitle()
        {
            Show(titlePanel);
            ShowKid(false);
            if (doctor != null) doctor.Play(DoctorPose.Wave);
        }

        void ShowSaves()
        {
            Show(savesPanel);
            ShowKid(false);
        }

        void OnSavePicked(int slot)
        {
            GameServices.ChooseSave(slot);
            ShowLevelSelect();
        }

        void ShowLevelSelect()
        {
            Show(levelSelectPanel, kidView);
            ShowKid(true);
            if (kid != null) kid.Play(KidPose.Wave);
            BuildLevelList();
        }

        void ShowDressUp()
        {
            Show(dressUpPanel, mirrorView);
            ShowKid(true);
        }

        /// <summary>The player's kid and the doctor share one spot in the room: the doctor greets, the kid shows off their clothes.</summary>
        void ShowKid(bool visible)
        {
            if (doctor != null) doctor.gameObject.SetActive(!visible);
            if (kid == null) return;
            kid.Show(visible);
            if (visible) kid.Apply(GameServices.Progress.look);
        }

        void MoveCamera(Transform view)
        {
            if (cameraRig == null || view == null) return;
            if (cameraMove != null) StopCoroutine(cameraMove);
            cameraMove = StartCoroutine(MoveCameraRoutine(view));
        }

        IEnumerator MoveCameraRoutine(Transform view)
        {
            var fromPos = cameraRig.position;
            var fromRot = cameraRig.rotation;
            var seconds = GameServices.Settings.reduceMotion ? 0f : 0.6f;
            for (var t = 0f; t < seconds; t += Time.unscaledDeltaTime)
            {
                var k = Mathf.SmoothStep(0f, 1f, t / seconds);
                cameraRig.SetPositionAndRotation(Vector3.Lerp(fromPos, view.position, k), Quaternion.Slerp(fromRot, view.rotation, k));
                yield return null;
            }
            cameraRig.SetPositionAndRotation(view.position, view.rotation);
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
        public void EditorInit(LevelCatalog levelCatalog, GameObject title, GameObject savesGo, GameObject levelSelect, GameObject dressUpGo,
            GameObject notice, Button play, Button gear, Button savesBack, Button levelsBack, Button dressUpOpen, RectTransform listRoot,
            LevelCardView template, SaveSelectView saveView, DressUpView dressView, SettingsView settingsView, DoctorActor doc, KidActor kidActor,
            Transform camera, Transform room, Transform mirror, Transform kidFrame)
        {
            catalog = levelCatalog;
            titlePanel = title;
            savesPanel = savesGo;
            levelSelectPanel = levelSelect;
            dressUpPanel = dressUpGo;
            resetNotice = notice;
            playButton = play;
            settingsButton = gear;
            savesBackButton = savesBack;
            levelsBackButton = levelsBack;
            dressUpButton = dressUpOpen;
            levelListRoot = listRoot;
            cardTemplate = template;
            saves = saveView;
            dressUp = dressView;
            settings = settingsView;
            doctor = doc;
            kid = kidActor;
            cameraRig = camera;
            roomView = room;
            mirrorView = mirror;
            kidView = kidFrame;
        }
#endif
    }
}
