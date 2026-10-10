using UnityEngine;

namespace Untitled.Core
{
    public abstract class CharacterBase : EntityBase
    {
        private CharacterController controller;

        public CharacterController Controller
        {
            get
            {
                if (controller == null) controller = GetComponent<CharacterController>();
                return controller;
            }
        }

        protected virtual void Move(Vector2 moveDelta)
        {
            Controller.Move(new Vector3(moveDelta.x, 0f, moveDelta.y));
        }
    }
}
