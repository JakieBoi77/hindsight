using Hindsight.Ophthalmology.Core.Avatar;
using Hindsight.Ophthalmology.Procedure;
using Hindsight.Ophthalmology.UI;
using UnityEngine;

namespace Hindsight.Ophthalmology.SceneObjects
{
    /// <summary>
    /// A small studio far from the room with a copy of each speaking character, filmed head-and-shoulders
    /// into render textures for the caption bubble. The copies talk whenever their character speaks, so
    /// the portrait is the real 3D character, mouth moving.
    /// </summary>
    public sealed class SpeakerPortraits : MonoBehaviour
    {
        [SerializeField] NarrationPlayer narrator;
        [SerializeField] DoctorActor doctor;
        [SerializeField] KidActor sam;

        void Awake()
        {
            if (sam != null) sam.Apply(AvatarLook.Sam);
            if (narrator != null) narrator.SpeakingChanged += OnSpeaking;
        }

        void OnDestroy()
        {
            if (narrator != null) narrator.SpeakingChanged -= OnSpeaking;
        }

        void OnSpeaking(Speaker speaker, bool on)
        {
            if (speaker == Speaker.Doctor && doctor != null) doctor.SetTalking(on);
            if (speaker == Speaker.Sam && sam != null) sam.SetTalking(on);
        }

#if UNITY_EDITOR
        public void EditorInit(NarrationPlayer player, DoctorActor doctorCopy, KidActor samCopy)
        {
            narrator = player;
            doctor = doctorCopy;
            sam = samCopy;
        }
#endif
    }
}
