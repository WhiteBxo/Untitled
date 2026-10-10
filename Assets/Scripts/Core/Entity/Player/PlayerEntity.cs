using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Untitled.Core
{
    public class PlayerEntity : EntityBase
    {
        public float Speed = 5;

        protected override void Start()
        {

        }

        protected override void Update()
        {
            Vector2 moveInput = InputManager.Instance.MoveInput;
            MoveCoord(Speed * Time.deltaTime * moveInput); 
        }
    }
}
