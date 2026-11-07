using System.Collections.Generic;
using Hindsight.Ophthalmology.Core;
using Hindsight.Ophthalmology.Procedure;
using Hindsight.Ophthalmology.UI;
using UnityEngine;

namespace Hindsight.Ophthalmology.SceneObjects
{
    /// <summary>
    /// Scene-side implementation of <see cref="IProcedureContext"/>. Anchors are the children of
    /// <see cref="anchorsRoot"/> looked up by name; interactables are found anywhere in the scene.
    /// </summary>
    public sealed class ExamRoomContext : MonoBehaviour, IProcedureContext
    {
        [SerializeField] LevelDefinition level;
        [SerializeField] Transform anchorsRoot;

        [Header("World")]
        [SerializeField] ShotDirector shots;
        [SerializeField] DoctorActor doctor;
        [SerializeField] EyeCloseupRig eyeCloseup;
        [SerializeField] EyeDropRig eyeDrop;
        [SerializeField] OphthalmoscopeRig ophthalmoscope;

        [Header("UI")]
        [SerializeField] DialogueView dialogue;
        [SerializeField] QuizView quiz;
        [SerializeField] ActionButtonsView buttons;
        [SerializeField] ScreenEffectsView effects;
        [SerializeField] RewardView reward;

        readonly Dictionary<string, Transform> anchors = new();
        readonly Dictionary<string, Interactable> interactables = new();

        public MonoBehaviour CoroutineHost => this;
        public LevelDefinition Level => GameFlow.CurrentLevel != null ? GameFlow.CurrentLevel : level;
        public ShotDirector Shots => shots;
        public DoctorActor Doctor => doctor;
        public EyeCloseupRig EyeCloseup => eyeCloseup;
        public EyeDropRig EyeDrop => eyeDrop;
        public OphthalmoscopeRig Ophthalmoscope => ophthalmoscope;
        public DialogueView Dialogue => dialogue;
        public QuizView Quiz => quiz;
        public ActionButtonsView Buttons => buttons;
        public ScreenEffectsView Effects => effects;
        public RewardView Reward => reward;

        void Awake()
        {
            if (anchorsRoot != null)
                foreach (Transform child in anchorsRoot)
                    anchors[child.name] = child;
            foreach (var item in FindObjectsByType<Interactable>(FindObjectsInactive.Include))
                interactables[item.Id] = item;
            if (dialogue != null && doctor != null) dialogue.TypingChanged += doctor.SetTalking;
        }

        public Transform GetAnchor(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            if (anchors.TryGetValue(id, out var t)) return t;
            Debug.LogWarning($"ExamRoomContext: no anchor named '{id}'.", this);
            return null;
        }

        public Interactable GetInteractable(string id)
        {
            if (interactables.TryGetValue(id, out var item)) return item;
            Debug.LogWarning($"ExamRoomContext: no interactable '{id}'.", this);
            return null;
        }

#if UNITY_EDITOR
        public void EditorInit(LevelDefinition lvl, Transform anchorRoot, ShotDirector shotDirector, DoctorActor doc,
            EyeCloseupRig closeup, EyeDropRig drop, OphthalmoscopeRig scope, DialogueView dlg, QuizView qz,
            ActionButtonsView btns, ScreenEffectsView fx, RewardView rwd)
        {
            level = lvl;
            anchorsRoot = anchorRoot;
            shots = shotDirector;
            doctor = doc;
            eyeCloseup = closeup;
            eyeDrop = drop;
            ophthalmoscope = scope;
            dialogue = dlg;
            quiz = qz;
            buttons = btns;
            effects = fx;
            reward = rwd;
        }
#endif
    }
}
