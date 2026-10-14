using System.Collections;
using UnityEngine;

namespace Hindsight.Ophthalmology.SceneObjects
{
    /// <summary>Big model eye used to show what dilation does to the pupil.</summary>
    public sealed class EyeCloseupRig : MonoBehaviour
    {
        [SerializeField] Transform pupil;
        [SerializeField] float normalSize = 0.3f;
        [SerializeField] float dilatedSize = 0.75f;

        public void ResetPupil() => SetPupil(normalSize);

        public IEnumerator Dilate(float seconds)
        {
            ResetPupil();
            for (var t = 0f; t < seconds; t += Time.deltaTime)
            {
                SetPupil(Mathf.Lerp(normalSize, dilatedSize, Mathf.SmoothStep(0f, 1f, t / seconds)));
                yield return null;
            }
            SetPupil(dilatedSize);
        }

        void SetPupil(float size)
        {
            if (pupil == null) return;
            var s = pupil.localScale;
            pupil.localScale = new Vector3(size, s.y, size);
        }

#if UNITY_EDITOR
        public void EditorInit(Transform pupilTransform) => pupil = pupilTransform;
#endif
    }
}
