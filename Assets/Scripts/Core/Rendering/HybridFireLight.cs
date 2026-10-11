using UnityEngine;

namespace Untitled
{
    [RequireComponent(typeof(Light))]
    public sealed class HybridFireLight : MonoBehaviour
    {
        [SerializeField] private float baseIntensity = 0.65f;
        [SerializeField] private float baseRange = 2.2f;
        private Light lamp;

        private void Awake() => lamp = GetComponent<Light>();

        private void Update()
        {
            if (lamp == null) return;
            lamp.intensity = baseIntensity * (0.94f + 0.07f * Mathf.Sin(Time.time * 8.1f)
                + 0.035f * Mathf.Sin(Time.time * 13.7f));
            lamp.range = baseRange * Mathf.Max(0.01f, transform.lossyScale.x);
        }
    }
}
