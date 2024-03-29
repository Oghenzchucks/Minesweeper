using System;
using System.Collections.Generic;
using Enums;
using Models;

namespace Events
{
    public class GameEventSystem
    {
        public static GameEventSystem Instance = new();
        
        //From Controller To UI
        public Action<List<TileState>, int> OnInitializeUI;
        public Action<InputAction> InputActionUpdate;
        public Action<List<TileState>> OnTilesToOpen;
        public Action<List<TileState>> OpenAllTiles;
        public Action<TileState> OnMarkMine;
        public Action<string> OnMineCountUpdate;
        public Action<int> OnTimeCountUpdate;
        public Action<bool> OnShowResult;
        public Action<bool> OnWarningAction;

        //From UI to Controller
        public Action StartGame;
        public Action ExitGame;
        public Action InputActionChange;
        public Action<TileState> OnTileClick;
    }
}
