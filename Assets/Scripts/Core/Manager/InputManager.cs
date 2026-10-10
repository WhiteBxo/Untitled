using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Untitled.Core
{
    public class InputManager : ManagerBase<InputManager>
    {
        private InputActions inputActions;

        public Vector2 MoveInput;

        public override void Initialize()
        {
            base.Initialize();
            inputActions = new InputActions();
            EnableInput();
            inputActions.Player.Move.performed += OnPlayerMove;
            inputActions.Player.Move.canceled += OnPlayerMoveCanceled;
        }

        public void EnableInput()
        {
            inputActions.Enable();
        }   

        public void DisableInput()
        {
            inputActions.Disable();
        }

        private void OnPlayerMove(InputAction.CallbackContext ctx)
        {
            MoveInput = ctx.ReadValue<Vector2>();
        }

        private void OnPlayerMoveCanceled(InputAction.CallbackContext ctx)
        {
            MoveInput = Vector2.zero;
        }
    }
}