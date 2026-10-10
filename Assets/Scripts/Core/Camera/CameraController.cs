using UnityEngine;

namespace Untitled.Core
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private float viewAngle = 45f;
        [SerializeField] private float distance = 7.07f;
        [SerializeField] private float damping = 8f;

        public Transform Target => target;

        public float ViewAngle => viewAngle;

        public float Distance => distance;

        private Vector3 Offset
        {
            get
            {
                float pitch = viewAngle * Mathf.Deg2Rad;
                return new Vector3(0f, Mathf.Sin(pitch), -Mathf.Cos(pitch)) * distance;
            }
        }

        private void Awake()
        {
            if (target == null)
            {
                target = FindTarget();
            }

            if (target == null)
            {
                Debug.LogWarning("[CameraController] Camera target is null", this);
            }
        }

        private void LateUpdate()
        {
            if (target == null) return;

            Vector3 desiredPosition = target.position + Offset;
            float t = 1f - Mathf.Exp(-damping * Time.deltaTime);
            transform.SetPositionAndRotation(Vector3.Lerp(transform.position, desiredPosition, t), Quaternion.Euler(viewAngle, 0f, 0f));
        }

        /// <summary>设置跟随目标。</summary>
        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }

        /// <summary>设置俯视角。</summary>
        public void SetViewAngle(float angle)
        {
            viewAngle = Mathf.Clamp(angle, 0f, 89f);
        }

        /// <summary>设置相机到目标的距离。</summary>
        public void SetDistance(float value)
        {
            distance = Mathf.Max(0.1f, value);
        }

        private Transform FindTarget()
        {
            PlayerEntity player = FindObjectOfType<PlayerEntity>();
            return player != null ? player.transform : null;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (target == null) return;

            Vector3 desiredPosition = target.position + Offset;

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(desiredPosition, 0.3f);
            Gizmos.DrawLine(target.position, desiredPosition);
        }
#endif
    }
}
