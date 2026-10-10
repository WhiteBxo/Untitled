using UnityEngine;

namespace Untitled.Core
{
    [DefaultExecutionOrder(100)]
    public class SpriteBillboard : MonoBehaviour
    {
        [SerializeField] private Camera targetCamera;

        private void Awake()
        {
            if (targetCamera == null) targetCamera = Camera.main;
        }

        private void LateUpdate()
        {
            if (targetCamera == null) return;

            transform.rotation = targetCamera.transform.rotation;
        }
    }
}
