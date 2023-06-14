using System;
using System.Collections.Generic;
using Models;

namespace Events
{
    public class GameEventSystem
    {
        public static GameEventSystem Instance = new();
        
        //Sent To UI
        public Action<List<GameModels.TileState>, int> OnInitializeUI;
    }
}