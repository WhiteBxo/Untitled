using UnityEngine;

namespace Untitled
{
    public class PlayerEntity : CharacterBase
    {
        public float Speed = 5;

        [SerializeField] private bool useGravity;
        private float verticalSpeed;

        protected override void Update()
        {
            Vector2 moveInput = InputManager.Instance.MoveInput;
            float speed = Speed * DebugManager.Instance.MoveSpeedMultiplier;
            Move(speed * Time.deltaTime * moveInput);
        }

        protected override void Move(Vector2 moveDelta)
        {
            if (!useGravity)
            {
                base.Move(moveDelta);
                return;
            }

            // Move takes a displacement; gravity must be integrated explicitly.
            verticalSpeed = Controller.isGrounded && verticalSpeed < 0f
                ? -2f
                : verticalSpeed + Physics.gravity.y * Time.deltaTime;
            Controller.Move(new Vector3(moveDelta.x, verticalSpeed * Time.deltaTime, moveDelta.y));
        }
    }
}
