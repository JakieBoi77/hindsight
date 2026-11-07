using System.Collections;
using Hindsight.Ophthalmology.Core;
using Hindsight.Ophthalmology.SceneObjects;
using Hindsight.Ophthalmology.UI;
using UnityEngine;

namespace Hindsight.Ophthalmology.Procedure
{
    /// <summary>Everything a step is allowed to touch in the scene.</summary>
    public interface IProcedureContext
    {
        MonoBehaviour CoroutineHost { get; }
        LevelDefinition Level { get; }

        ShotDirector Shots { get; }
        DoctorActor Doctor { get; }
        KidActor Sam { get; }
        KidActor PlayerKid { get; }
        EyeCloseupRig EyeCloseup { get; }
        EyeDropRig EyeDrop { get; }
        OphthalmoscopeRig Ophthalmoscope { get; }
        TeachingChart Chart { get; }

        CaptionView Captions { get; }
        ActionButtonsView Buttons { get; }
        ScreenEffectsView Effects { get; }
        RewardView Reward { get; }

        Transform GetAnchor(string id);
        Interactable GetInteractable(string id);

        /// <summary>Plays a stage beat to completion (instant for setup beats).</summary>
        IEnumerator PlayBeat(StoryBeat beat);
    }
}
