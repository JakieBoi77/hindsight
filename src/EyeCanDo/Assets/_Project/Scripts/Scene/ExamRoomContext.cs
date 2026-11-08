using System.Collections;
using System.Collections.Generic;
using Hindsight.Ophthalmology.Core;
using Hindsight.Ophthalmology.Core.Avatar;
using Hindsight.Ophthalmology.Procedure;
using Hindsight.Ophthalmology.UI;
using UnityEngine;

namespace Hindsight.Ophthalmology.SceneObjects
{
    /// <summary>
    /// Scene-side implementation of <see cref="IProcedureContext"/>. Anchors are the children of
    /// <see cref="anchorsRoot"/> looked up by name; interactables are found anywhere in the scene.
    /// Also plays the stage beats (Sam's turn, the player's kid) and routes narration to whoever speaks.
    /// </summary>
    public sealed class ExamRoomContext : MonoBehaviour, IProcedureContext
    {
        [SerializeField] LevelDefinition level;
        [SerializeField] Transform anchorsRoot;

        [Header("World")]
        [SerializeField] ShotDirector shots;
        [SerializeField] DoctorActor doctor;
        [SerializeField] KidActor sam;
        [SerializeField] KidActor playerKid;
        [SerializeField] EyeCloseupRig eyeCloseup;
        [SerializeField] EyeDropRig eyeDrop;
        [SerializeField] OphthalmoscopeRig ophthalmoscope;
        [SerializeField] TeachingChart chart;
        [Tooltip("The little light that shines in Sam's eye when the doctor looks in.")]
        [SerializeField] Light examGlint;

        [Header("UI")]
        [SerializeField] CaptionView captions;
        [SerializeField] ActionButtonsView buttons;
        [SerializeField] ScreenEffectsView effects;
        [SerializeField] RewardView reward;

        readonly Dictionary<string, Transform> anchors = new();
        readonly Dictionary<string, Interactable> interactables = new();

        public MonoBehaviour CoroutineHost => this;
        public LevelDefinition Level => GameFlow.CurrentLevel != null ? GameFlow.CurrentLevel : level;
        public ShotDirector Shots => shots;
        public DoctorActor Doctor => doctor;
        public KidActor Sam => sam;
        public KidActor PlayerKid => playerKid;
        public EyeCloseupRig EyeCloseup => eyeCloseup;
        public EyeDropRig EyeDrop => eyeDrop;
        public OphthalmoscopeRig Ophthalmoscope => ophthalmoscope;
        public TeachingChart Chart => chart;
        public CaptionView Captions => captions;
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
            if (captions != null && captions.Narrator != null)
            {
                var narrator = captions.Narrator;
                narrator.SpeakingChanged += OnSpeaking;
                narrator.LineStarted += OnLine;
                // Mouths open as wide as the recording is loud.
                if (doctor != null) doctor.SetVoice(() => narrator.VoiceLevel);
                if (sam != null) sam.SetVoice(() => narrator.VoiceLevel);
            }

            if (sam != null) sam.Apply(AvatarLook.Sam);
            if (examGlint != null) examGlint.enabled = false;
            if (playerKid != null)
            {
                playerKid.Apply(GameServices.Progress.look);
                playerKid.Show(false);
            }
        }

        /// <summary>Whenever the eye-drop bottle is held up over an eye, the doctor leans over and holds it.</summary>
        void Update()
        {
            if (doctor == null || eyeDrop == null) return;
            if (eyeDrop.IsHeldUp && !doctor.IsHoldingBottle) doctor.HoldBottle(eyeDrop.Bottle, eyeDrop.HeldOverEye);
            else if (!eyeDrop.IsHeldUp && doctor.IsHoldingBottle) doctor.FinishTask();
        }

        /// <summary>The speaker's brows show how they feel about the line.</summary>
        void OnLine(StoryLine line)
        {
            if (line.speaker == Speaker.Doctor && doctor != null) doctor.SetMood(line.mood);
            if (line.speaker == Speaker.Sam && sam != null) sam.SetMood(line.mood);
        }

        void OnSpeaking(Speaker speaker, bool on)
        {
            if (speaker == Speaker.Doctor && doctor != null) doctor.SetTalking(on);
            if (speaker == Speaker.Sam && sam != null) sam.SetTalking(on);
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

        public IEnumerator PlayBeat(StoryBeat beat)
        {
            switch (beat)
            {
                case StoryBeat.DoctorWave:
                    doctor.Play(DoctorPose.Wave);
                    break;
                case StoryBeat.DoctorCheer:
                    doctor.Play(DoctorPose.Cheer);
                    break;
                case StoryBeat.SamWave:
                    sam.Play(KidPose.Wave);
                    Sfx.Play(SfxId.Pop);
                    yield return Wait(1.1f);
                    break;
                case StoryBeat.SamSit:
                    // Off camera: a quick fade hides Sam hopping in.
                    yield return effects.FadeOut(0.25f);
                    sam.SitInSeat(GetAnchor("SamSeat"));
                    doctor.PlaceAt(GetAnchor("DoctorSamPoint"));
                    doctor.SetProp(DoctorProp.None);
                    shots.Cut("SamChair", instant: true);
                    yield return null;
                    yield return effects.FadeIn(0.35f);
                    break;
                case StoryBeat.SamWalkToChair:
                    shots.Cut("ChairApproach");
                    yield return sam.WalkTo(GetAnchor("KidStandPoint"));
                    break;
                case StoryBeat.DoctorExamSam:
                    yield return ExamSam();
                    break;
                case StoryBeat.SamStandUp:
                    // Off camera again: Sam hops down and stands by the chair; Dr. Iris stays beside it.
                    yield return effects.FadeOut(0.25f);
                    sam.PlaceAt(GetAnchor("KidStandPoint"));
                    doctor.PlaceAt(GetAnchor("DoctorSamPoint"));
                    shots.Cut("ChairApproach", instant: true);
                    yield return null;
                    yield return effects.FadeIn(0.35f);
                    break;
                case StoryBeat.SamDance:
                    Sfx.Play(SfxId.Cheer);
                    doctor.Play(DoctorPose.Cheer);
                    yield return sam.Dance(2.6f);
                    break;
                case StoryBeat.SamLeave:
                    // Dr. Iris steps back to her spot, then Sam goes round the chair and out of the door.
                    yield return doctor.MoveTo(GetAnchor("DoctorStandPoint"));
                    shots.Cut("DoorView");
                    yield return sam.WalkTo(GetAnchor("SamExitWay"));
                    yield return sam.WalkTo(GetAnchor("SamExitPoint"));
                    yield return Wait(0.3f);
                    sam.Show(false);
                    break;
                case StoryBeat.SamTiltBack:
                    sam.SetTiltBack(1f);
                    yield return Wait(0.6f);
                    break;
                case StoryBeat.DropOnSam:
                    sam.SetTiltBack(1f);
                    Sfx.Play(SfxId.DropFall);
                    yield return eyeDrop.FallOnto(sam.EyePosition, doctor.transform.position);
                    Sfx.Play(SfxId.Shiver);
                    sam.Shiver();
                    yield return Wait(0.5f);
                    break;
                case StoryBeat.SamBlink:
                    sam.SetTiltBack(0f);
                    Sfx.Play(SfxId.Blink);
                    yield return sam.Blink();
                    yield return Wait(0.3f);
                    Sfx.Play(SfxId.Blink);
                    yield return sam.Blink();
                    break;
                case StoryBeat.SamCheer:
                    sam.Play(KidPose.Cheer);
                    break;
                case StoryBeat.SamHide:
                    sam.Show(false);
                    break;
                case StoryBeat.KidAppear:
                    // Sam has gone home; after a quick fade it is the player's turn, Dr. Iris by the chair.
                    yield return effects.FadeOut(0.3f);
                    sam.Show(false);
                    playerKid.PlaceAt(GetAnchor("KidStandPoint"));
                    doctor.PlaceAt(GetAnchor("DoctorSamPoint"));
                    shots.Cut("ChairApproach", instant: true);
                    yield return null;
                    playerKid.Play(KidPose.Wave);
                    yield return effects.FadeIn(0.4f);
                    break;
                case StoryBeat.KidHide:
                    playerKid.Show(false);
                    break;
                case StoryBeat.DoctorToDropPoint:
                    doctor.PlaceAt(GetAnchor("DoctorDropPoint"));
                    break;
                case StoryBeat.DoctorToExamPoint:
                    doctor.PlaceAt(GetAnchor("DoctorExamPoint"));
                    break;
            }
        }

        /// <summary>
        /// The doctor leans in to Sam, holds the light up to her own eye and looks into his; the light
        /// glints in his eye. Then she straightens up.
        /// </summary>
        IEnumerator ExamSam()
        {
            var eye = sam.EyePosition;
            doctor.Examine(eye, 0.72f);
            yield return Wait(1.1f); // leaning in
            if (examGlint != null)
            {
                examGlint.transform.position = sam.EyePosition + (doctor.transform.position - sam.EyePosition).normalized * 0.06f;
                examGlint.enabled = true;
                Sfx.Play(SfxId.Sparkle);
                for (var t = 0f; t < 2.2f; t += Time.deltaTime)
                {
                    examGlint.intensity = 1.2f + Mathf.Sin(t * 9f) * 0.5f;
                    yield return null;
                }
                examGlint.enabled = false;
            }
            else
            {
                yield return Wait(2.2f);
            }
            doctor.FinishTask();
            yield return Wait(0.6f);
            doctor.SetProp(DoctorProp.None);
            doctor.Play(DoctorPose.Idle);
        }

        static IEnumerator Wait(float seconds)
        {
            for (var t = 0f; t < seconds; t += Time.deltaTime) yield return null;
        }

#if UNITY_EDITOR
        public void EditorInit(LevelDefinition lvl, Transform anchorRoot, ShotDirector shotDirector, DoctorActor doc, KidActor samActor,
            KidActor kid, EyeCloseupRig closeup, EyeDropRig drop, OphthalmoscopeRig scope, TeachingChart wallChart, Light glint, CaptionView caption,
            ActionButtonsView btns, ScreenEffectsView fx, RewardView rwd)
        {
            level = lvl;
            anchorsRoot = anchorRoot;
            shots = shotDirector;
            doctor = doc;
            sam = samActor;
            playerKid = kid;
            eyeCloseup = closeup;
            eyeDrop = drop;
            ophthalmoscope = scope;
            chart = wallChart;
            examGlint = glint;
            captions = caption;
            buttons = btns;
            effects = fx;
            reward = rwd;
        }
#endif
    }
}
