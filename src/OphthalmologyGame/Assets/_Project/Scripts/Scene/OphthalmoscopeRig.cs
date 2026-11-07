using System.Collections;
using UnityEngine;

namespace Hindsight.Ophthalmology.SceneObjects
{
    /// <summary>
    /// The doctor's light, shown in front of the first-person camera. It hops between a few
    /// points and the player taps it each time ("follow the light").
    /// </summary>
    public sealed class OphthalmoscopeRig : MonoBehaviour
    {
        [SerializeField] Transform tool;
        [SerializeField] Light beam;
        [SerializeField] Transform[] lookPoints = new Transform[0];
        [SerializeField] float moveSeconds = 0.5f;

        public Interactable TapTarget => tool != null ? tool.GetComponent<Interactable>() : null;
        public int PointCount => lookPoints.Length;

        void Awake() => Show(false);

        public void Show(bool visible)
        {
            if (tool != null) tool.gameObject.SetActive(visible);
            if (beam != null) beam.enabled = visible;
        }

        public IEnumerator MoveToPoint(int index)
        {
            if (tool == null || lookPoints.Length == 0) yield break;
            var target = lookPoints[Mathf.Clamp(index, 0, lookPoints.Length - 1)];
            var from = tool.position;
            for (var t = 0f; t < moveSeconds; t += Time.deltaTime)
            {
                tool.position = Vector3.Lerp(from, target.position, Mathf.SmoothStep(0f, 1f, t / moveSeconds));
                yield return null;
            }
            tool.SetPositionAndRotation(target.position, target.rotation);
        }

        public IEnumerator Flash(float seconds)
        {
            if (beam == null) yield break;
            var baseIntensity = beam.intensity;
            for (var t = 0f; t < seconds; t += Time.deltaTime)
            {
                beam.intensity = baseIntensity * (1f + 3f * Mathf.Sin(Mathf.PI * t / seconds));
                yield return null;
            }
            beam.intensity = baseIntensity;
        }

#if UNITY_EDITOR
        public void EditorInit(Transform toolT, Light light, Transform[] points)
        {
            tool = toolT;
            beam = light;
            lookPoints = points;
        }
#endif
    }
}
