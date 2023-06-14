using System;
using System.Collections.Generic;
using Enums;
using Models;

namespace Events
{
    public class GameEventSystem
    {
        public static GameEventSystem Instance = new();
        
        //Sent To UI
        public Action<List<TileState>, int> OnInitializeUI;
        public Action<InputAction> InputActionUpdate;

        //From UI
        public Action InputActionChange;
        public Action<TileState> OnTileClick;
    }
}
