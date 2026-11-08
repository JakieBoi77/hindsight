using System;
using System.Collections;
using Hindsight.Ophthalmology.Core.Avatar;
using UnityEngine;

namespace Hindsight.Ophthalmology.SceneObjects
{
    public enum KidPose
    {
        Idle,
        Wave,
        Cheer,
        Sit,
        Walk,
    }

    /// <summary>
    /// A child character (Sam, or the player's own kid): a bean (<see cref="BeanRig"/>) dressed by an
    /// <see cref="AvatarLook"/>. Steps and stage beats ask for poses and moves here; the rig does the
    /// animation.
    /// </summary>
    [RequireComponent(typeof(BeanRig))]
    public sealed class KidActor : MonoBehaviour
    {
        [SerializeField] BeanRig rig;

        [Header("Moving")]
        [SerializeField] float walkSpeed = 2.4f;
        [SerializeField] float turnSpeed = 800f;

        public BeanRig Rig => rig != null ? rig : rig = GetComponent<BeanRig>();

        /// <summary>Where the kid's eyes are (the drop's target).</summary>
        public Vector3 EyePosition => Rig.Eyes != null ? Rig.Eyes.position : transform.position + Vector3.up;

        /// <summary>Dresses the kid. Safe to call any time.</summary>
        public void Apply(AvatarLook look) => Rig.ApplyLook(look);

        public void Play(KidPose pose)
        {
            switch (pose)
            {
                case KidPose.Idle: Rig.SetMode(BeanMode.Idle); break;
                case KidPose.Walk: Rig.SetMode(BeanMode.Walk); break;
                case KidPose.Sit: Rig.SetMode(BeanMode.Sit); break;
                case KidPose.Wave: Rig.Wave(1.5f); break;
                case KidPose.Cheer: Rig.Cheer(); break;
            }
        }

        public void SetTalking(bool value) => Rig.SetTalking(value);

        /// <summary>Shows a feeling with the brows (<see cref="Mood.Keep"/> changes nothing).</summary>
        public void SetMood(Mood mood) => Rig.SetMood(mood);

        /// <summary>Where the mouth gets its loudness from while talking (the narration).</summary>
        public void SetVoice(Func<float> level) => Rig.Voice = level;

        public void Show(bool visible) => gameObject.SetActive(visible);

        /// <summary>Stands the kid at an anchor, facing its forward.</summary>
        public void PlaceAt(Transform target)
        {
            if (target == null) return;
            Show(true);
            transform.SetPositionAndRotation(target.position, target.rotation);
            Rig.SetMode(BeanMode.Idle);
            Rig.ResetLayers();
        }

        /// <summary>Walks to <paramref name="target"/> and turns to face its forward.</summary>
        public IEnumerator WalkTo(Transform target)
        {
            if (target == null) yield break;
            Show(true);
            if (Rig.Mode == BeanMode.Sit) Rig.SetMode(BeanMode.Idle);
            Rig.SetTilt(0f);
            var flat = new Vector3(target.position.x, transform.position.y, target.position.z);
            if (Vector3.Distance(transform.position, flat) > 0.05f)
            {
                Rig.SetMoveSpeed(walkSpeed);
                Rig.SetMode(BeanMode.Walk);
                while (Vector3.Distance(transform.position, flat) > 0.02f)
                {
                    var dir = flat - transform.position;
                    if (dir.sqrMagnitude > 0.0001f)
                        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(dir), turnSpeed * Time.deltaTime);
                    transform.position = Vector3.MoveTowards(transform.position, flat, walkSpeed * Time.deltaTime);
                    yield return null;
                }
                Rig.SetMode(BeanMode.Idle);
            }
            while (Quaternion.Angle(transform.rotation, target.rotation) > 1f)
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, target.rotation, turnSpeed * Time.deltaTime);
                yield return null;
            }
            transform.rotation = target.rotation;
        }

        /// <summary>A happy dance: bouncing, swaying and pumping the arms, then back to standing.</summary>
        public IEnumerator Dance(float seconds)
        {
            Rig.Dance(seconds);
            for (var t = 0f; t < seconds; t += Time.deltaTime) yield return null;
        }

        /// <summary>
        /// Sits the kid in the chair: the seat anchor's rotation is the body's (facing the footrest)
        /// and its position is where the bottom of the torso rests. The body leans back against the backrest.
        /// </summary>
        public void SitInSeat(Transform seat)
        {
            if (seat == null) return;
            Show(true);
            transform.rotation = seat.rotation;
            Rig.SetMode(BeanMode.Sit);
            Rig.ResetLayers(); // also poses it outside Play mode
            transform.position += seat.position - Rig.SeatPoint.position;
        }

        /// <summary>Head back (1) or upright (0), eased.</summary>
        public void SetTiltBack(float amount) => Rig.SetTilt(amount);

        public void Shiver(float seconds = 0.8f) => Rig.Shiver(seconds);

        /// <summary>Two big squeezed blinks.</summary>
        public IEnumerator Blink()
        {
            for (var i = 0; i < 2; i++)
            {
                Rig.SetEyesShut(true);
                for (var t = 0f; t < 0.16f; t += Time.deltaTime)
                {
                    Rig.SetSquash(Mathf.Sin(Mathf.PI * t / 0.16f));
                    yield return null;
                }
                Rig.SetSquash(0f);
                Rig.SetEyesShut(false);
                for (var t = 0f; t < 0.15f; t += Time.deltaTime) yield return null;
            }
        }

#if UNITY_EDITOR
        public void EditorInit(BeanRig beanRig) => rig = beanRig;
#endif
    }
}
