using UnityEngine;

namespace Untitled
{
    [DefaultExecutionOrder(100)]
    public class SpriteBillboard : MonoBehaviour
    {
        [SerializeField] private Camera targetCamera;
        [SerializeField] private bool keepUpright;

        private void Awake()
        {
            if (targetCamera == null) targetCamera = Camera.main;
        }

        private void LateUpdate()
        {
            if (targetCamera == null) return;

            transform.rotation = keepUpright
                ? Quaternion.Euler(0f, targetCamera.transform.eulerAngles.y, 0f)
                : targetCamera.transform.rotation;
        }
    }
}
