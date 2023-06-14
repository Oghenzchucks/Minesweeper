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
        public List<TileState> GetTileStates { get; private set; }

        private const int maxRows = 8;
        private const int maxColumns = 8;
        
        public void Initialize()
        {
            currentTime = 0;
            totalMinesFlagged = 0;
            LoadTileStates();
            isInitialized = true;
        }

        private void LoadTileStates()
        {
            GetTileStates = new List<TileState>();
            for (int i = 0; i < maxRows; i++)
            {
                int rowPos = i;
                for (int j = 0; j < maxColumns; j++)
                {
                    var tileState = new TileState()
                    {
                        tilePosition = new TilePosition()
                        {
                            row = rowPos,
                            column = j,
                        }
                    };
                    GetTileStates.Add(tileState);
                }
            }
        }
    }
}
