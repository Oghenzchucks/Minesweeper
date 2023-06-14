using System;
using System.Collections.Generic;
using Models;

namespace Events
{
    public class GameEventSystem
    {
        public static GameEventSystem Instance = new();
        
        //Sent To UI
        public Action<List<TileState>, int> OnInitializeUI;
        
        //From UI
        public Action InputActionChange;
    }
}
