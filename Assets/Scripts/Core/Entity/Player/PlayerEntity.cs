using UnityEngine;

namespace Untitled
{
    public class PlayerEntity : CharacterBase
    {
        public float Speed = 5;

        protected override void Update()
        {
            Vector2 moveInput = InputManager.Instance.MoveInput;
            Move(Speed * Time.deltaTime * moveInput);
        }
    }
}
