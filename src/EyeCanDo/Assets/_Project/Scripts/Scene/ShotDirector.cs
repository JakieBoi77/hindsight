using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

namespace Hindsight.Ophthalmology.SceneObjects
{
    /// <summary>
    /// Switches between named Cinemachine cameras ("shots"). Every child CinemachineCamera is a
    /// shot whose id is its GameObject name. The brain's default blend does the camera move.
    /// </summary>
    public sealed class ShotDirector : MonoBehaviour
    {
        const int LivePriority = 100;
        const int IdlePriority = 0;

        [SerializeField] CinemachineBrain brain;
        [SerializeField] string initialShot = "RoomWide";

        readonly Dictionary<string, CinemachineCamera> shots = new();
        public string CurrentShot { get; private set; }

        void Awake()
        {
            foreach (var cam in GetComponentsInChildren<CinemachineCamera>(true))
                shots[cam.name] = cam;
            if (!string.IsNullOrEmpty(initialShot)) Cut(initialShot, instant: true);
        }

        public CinemachineCamera GetShot(string id) => shots.TryGetValue(id, out var cam) ? cam : null;

        /// <summary>Makes <paramref name="id"/> the live camera. Blends unless <paramref name="instant"/>.</summary>
        public void Cut(string id, bool instant = false)
        {
            if (string.IsNullOrEmpty(id) || id == CurrentShot) return;
            if (!shots.TryGetValue(id, out var target))
            {
                Debug.LogWarning($"ShotDirector: no shot named '{id}'.", this);
                return;
            }
            foreach (var cam in shots.Values) cam.Priority.Value = IdlePriority;
            target.Priority.Value = LivePriority;
            CurrentShot = id;
            if (instant && brain != null) StartCoroutine(SuppressBlendOneFrame());
        }

        /// <summary>Cuts and waits until the camera has finished moving.</summary>
        public IEnumerator CutAndWait(string id, bool instant = false)
        {
            var changed = !string.IsNullOrEmpty(id) && id != CurrentShot;
            Cut(id, instant);
            if (!changed || brain == null) yield break;

            // The brain switches cameras in its own update, so wait until the new shot is live...
            var target = GetShot(id);
            for (var t = 0f; !ReferenceEquals(brain.ActiveVirtualCamera, target) && t < 3f; t += Time.deltaTime)
                yield return null;
            // ...then until the blend into it has finished.
            yield return null;
            while (brain.IsBlending) yield return null;
        }

        IEnumerator SuppressBlendOneFrame()
        {
            var saved = brain.DefaultBlend;
            brain.DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.Cut, 0f);
            yield return null;
            yield return null;
            brain.DefaultBlend = saved;
        }

#if UNITY_EDITOR
        public void EditorInit(CinemachineBrain value, string firstShot)
        {
            brain = value;
            initialShot = firstShot;
        }
#endif
    }
}
