using System;
using System.Collections.Generic;
using Hindsight.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Hindsight.Procedures
{
    /// <summary>The guide doctor. Swaps artwork per pose; idle motion lives on a separate component.</summary>
    public sealed class DoctorCharacterView : MonoBehaviour
    {
        [SerializeField] private Image image;
        [SerializeField] private List<PoseSprite> poses = new List<PoseSprite>();

        private DoctorPose currentPose = DoctorPose.Smile;

        public DoctorPose CurrentPose => currentPose;

        private void Awake()
        {
            ApplySprite(currentPose);
        }

        public void SetPose(DoctorPose pose)
        {
            if (pose == currentPose)
            {
                return;
            }

            currentPose = pose;
            ApplySprite(pose);
            Tween.PunchScaleAsync(image.transform, 0.04f, 0.25f, destroyCancellationToken).Forget();
        }

        private void ApplySprite(DoctorPose pose)
        {
            foreach (var entry in poses)
            {
                if (entry.Pose == pose)
                {
                    image.sprite = entry.Sprite;
                    return;
                }
            }

            // Fall back to the first pose rather than showing nothing if art for a pose is missing.
            if (poses.Count > 0)
            {
                image.sprite = poses[0].Sprite;
            }
        }

        [Serializable]
        private struct PoseSprite
        {
            [SerializeField] private DoctorPose pose;
            [SerializeField] private Sprite sprite;

            public DoctorPose Pose => pose;

            public Sprite Sprite => sprite;
        }
    }
}
