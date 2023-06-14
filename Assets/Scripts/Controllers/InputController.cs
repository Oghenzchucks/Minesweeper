using System;
using Enums;

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
    }
}
