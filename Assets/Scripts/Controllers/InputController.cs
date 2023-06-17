using System;
using Enums;
using UnityEngine;

namespace Controllers
{
    [Serializable]
    public class InputController
    {
        public bool handleInput;
        public InputAction GetInputAction { get; private set; }

        public void ChangeInputAction()
        {
            switch (GetInputAction)
            {
                case InputAction.FindMine:
                    GetInputAction = InputAction.MarkMine;
                    break;
                case InputAction.MarkMine:
                    GetInputAction = InputAction.FindMine;
                    break;
            }
        }
        
        public void UpdateInputAction(InputAction inputAction)
        {
            GetInputAction = inputAction;
        }

        public void OnUpdate()
        {
            if (!handleInput)
            {
                return;
            }
            
            if (Input.GetMouseButtonDown(0))
            {
                UpdateInputAction (InputAction.FindMine);
            }
            
            if (Input.GetMouseButtonDown(1))
            {
                UpdateInputAction (InputAction.MarkMine);
            }
        }
    }
}
