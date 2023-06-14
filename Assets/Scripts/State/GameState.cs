using System.Collections.Generic;
using Models;
using UnityEngine;

namespace State
{
    [CreateAssetMenu(fileName = nameof(GameState), menuName = "State/Game")]
    public class GameState : ScriptableObject
    {
        public int totalMines;
        public int totalMinesFlagged;
        public int currentTime;
        public bool isInitialized;
        public List<GameModels.TileState> GetTileStates { get; private set; }

        private const int maxRows = 8;
        private const int maxColumns = 8;
        
        public void Initialize()
        {
            if (isInitialized)
            {
                return;
            }

            currentTime = 0;
            totalMinesFlagged = 0;
            LoadTileStates();
        }

        private void LoadTileStates()
        {
            GetTileStates = new List<GameModels.TileState>();
            for (int i = 0; i < maxRows; i++)
            {
                int rowPos = i;
                for (int j = 0; j < maxColumns; j++)
                {
                    var tileState = new GameModels.TileState()
                    {
                        tilePosition = new GameModels.TilePosition()
                        {
                            row = rowPos,
                            column = j,
                        }
                    };
                    GetTileStates.Add(tileState);
                }
            }

            isInitialized = true;
        }
    }
}
