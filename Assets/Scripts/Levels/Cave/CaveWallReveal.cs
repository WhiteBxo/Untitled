using UnityEngine;

namespace Untitled
{
    // Raycasts activate the reveal; the shader clips only foreground wall pixels near the player.
    [DefaultExecutionOrder(200), DisallowMultipleComponent]
    public sealed class CaveWallReveal : MonoBehaviour
    {
        [SerializeField] private Camera viewCamera;
        [SerializeField] private Renderer targetVisual;
        [SerializeField, Min(0.1f)] private float revealRadius = 0.85f;
        [SerializeField, Min(0.1f)] private float fadeSpeed = 8f;

        private readonly RaycastHit[] hits = new RaycastHit[16];
        private float revealAmount;

        private static readonly int TargetId = Shader.PropertyToID("_CaveRevealTarget");
        private static readonly int CameraId = Shader.PropertyToID("_CaveRevealCamera");

        private void LateUpdate()
        {
            if (viewCamera == null || targetVisual == null || !targetVisual.enabled || !targetVisual.gameObject.activeInHierarchy)
            {
                Clear();
                return;
            }
            Vector3 center = targetVisual.bounds.center;
            Vector3 cameraPosition = viewCamera.transform.position;
            float height = targetVisual.bounds.extents.y * 0.75f;
            bool blocked = IsOccluded(cameraPosition, center) ||
                IsOccluded(cameraPosition, center + Vector3.up * height) ||
                IsOccluded(cameraPosition, center - Vector3.up * height);
            revealAmount = Mathf.MoveTowards(revealAmount, blocked ? 1f : 0f, fadeSpeed * Time.deltaTime);
            Shader.SetGlobalVector(TargetId, new Vector4(center.x, center.y, center.z, revealRadius));
            Shader.SetGlobalVector(CameraId, new Vector4(cameraPosition.x, cameraPosition.y, cameraPosition.z, revealAmount));
        }

        private bool IsOccluded(Vector3 origin, Vector3 point)
        {
            Vector3 direction = point - origin;
            float distance = direction.magnitude;
            if (distance < 0.001f) return false;
            int count = Physics.RaycastNonAlloc(origin, direction / distance, hits, distance,
                Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < count; i++)
                if (hits[i].collider.transform.IsChildOf(transform)) return true;
            return false;
        }

        private void OnDisable()
        {
            revealAmount = 0f;
            Clear();
        }

        private static void Clear()
        {
            Shader.SetGlobalVector(TargetId, Vector4.zero);
            Shader.SetGlobalVector(CameraId, Vector4.zero);
        }
    }
}
