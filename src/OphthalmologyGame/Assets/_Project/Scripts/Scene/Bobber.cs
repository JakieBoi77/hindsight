using UnityEngine;

namespace Hindsight.Ophthalmology.SceneObjects
{
    /// <summary>Bobs and spins an object. Used for "tap me" arrows and rewards.</summary>
    public sealed class Bobber : MonoBehaviour
    {
        [SerializeField] float height = 0.08f;
        [SerializeField] float speed = 3f;
        [SerializeField] float spinDegreesPerSecond = 90f;

        Vector3 origin;

        void OnEnable() => origin = transform.localPosition;

        void Update()
        {
            transform.localPosition = origin + Vector3.up * (Mathf.Sin(Time.time * speed) * height);
            transform.Rotate(Vector3.up, spinDegreesPerSecond * Time.deltaTime, Space.World);
        }
    }
}
