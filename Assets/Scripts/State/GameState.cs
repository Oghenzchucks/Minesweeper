using System.Collections.Generic;
using System.Linq;
using Enums;
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
        public List<TileState> GetTileStates { get; private set; }

        public const int MaxRows = 8;
        public const int MaxColumns = 8;
        
        public void Initialize()
        {
            currentTime = 0;
            totalMinesFlagged = 0;
            LoadTileStates();
            GenerateMinePlacements();
        }

        private void LoadTileStates()
        {
            GetTileStates = new List<TileState>();
            for (int i = 0; i < MaxRows; i++)
            {
                int rowPos = i;
                for (int j = 0; j < MaxColumns; j++)
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

        private void GenerateMinePlacements()
        {
            var tilePositions = GetTileStates.Select(x => x.tilePosition).ToList();
            RemoveCorners(ref tilePositions);
            RemoveCenter(ref tilePositions);
            AddMinesToTileState(SelectMinesPosition(tilePositions));
        }

        private void RemoveCorners(ref List<TilePosition> tilePositions)
        {
            tilePositions.Remove(new TilePosition() { row = 0, column = 0 });
            tilePositions.Remove(new TilePosition() { row = 0, column = 7 });
            tilePositions.Remove(new TilePosition() { row = 7, column = 0 });
            tilePositions.Remove(new TilePosition() { row = 7, column = 7 });
        }
        
        private void RemoveCenter(ref List<TilePosition> tilePositions)
        {
            tilePositions.Remove(new TilePosition() { row = 4, column = 4 });
        }
        
        private List<TilePosition> SelectMinesPosition(List<TilePosition> tilePositions)
        {
            var chosenSelections = new List<TilePosition>();
            
            for (int i = 0; i < totalMines; i++)
            {
                var chosenSelection = Random.Range(0, tilePositions.Count);
                chosenSelections.Add(tilePositions[chosenSelection]);
                tilePositions.Remove(tilePositions[chosenSelection]);
            }

            return chosenSelections;
        }
        
        private void AddMinesToTileState(List<TilePosition> selectMinesPosition)
        {
            foreach (var minePosition in selectMinesPosition)
            {
                GetTileStates.First(x => x.tilePosition.IsEqual(minePosition)).tileType = TileTypes.Mine;
            }
        }
    }
}
