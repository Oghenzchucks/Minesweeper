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
        public bool IsMatched => totalMines == totalMinesFlagged;

        public const int MaxRows = 9;
        public const int MaxColumns = 9;
        
        public void Initialize(MinesData minesData)
        {
            currentTime = 0;
            totalMinesFlagged = 0;
            LoadTileStates();
            GenerateMinePlacements(minesData);
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

        private void GenerateMinePlacements(MinesData minesData)
        {
            if (minesData == null)
            {
                var tilePositions = GetTileStates.Select(x => x.tilePosition).ToList();
                RemoveCorners(ref tilePositions);
                RemoveCenter(ref tilePositions);
                AddMinesToTileState(SelectMinesPosition(tilePositions));
            }
            else
            {
                totalMines = minesData.minesPosition.Count;
                AddMinesToTileState(minesData.minesPosition);
            }
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

        public bool IsGameFinished()
        {
            var mines = GetTileStates.FindAll(x => x.tileType == TileTypes.Mine);
            var markedMines = GetTileStates.FindAll(x => x.isFlagged);
            var matchCount = 0;

            foreach (var mine in mines)
            {
                if (markedMines.Exists(x => x.tilePosition.IsEqual(mine.tilePosition)))
                {
                    matchCount++;
                }
            }
            return matchCount == mines.Count;
        }
    }
}
