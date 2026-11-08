using UnityEngine;

namespace Hindsight.EyeCanDo.SceneObjects
{
    /// <summary>Gently pulses an object's size, for 3D "tap me" glows.</summary>
    public sealed class PulseScale : MonoBehaviour
    {
        [SerializeField] float amount = 0.12f;
        [SerializeField] float cyclesPerSecond = 1.2f;

        Vector3 baseScale;

        void Awake() => baseScale = transform.localScale;

        void Update()
        {
            var s = 1f + amount * Mathf.Sin(Time.time * cyclesPerSecond * Mathf.PI * 2f);
            transform.localScale = new Vector3(baseScale.x * s, baseScale.y, baseScale.z * s);
        }
    }
}
