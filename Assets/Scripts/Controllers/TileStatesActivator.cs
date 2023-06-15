using System;
using System.Collections.Generic;
using System.Linq;
using Enums;
using Models;
using State;

namespace Controllers
{
    public class TileStatesActivator
    {
        private GameState _gameState;
        
        private List<TileState> _checkTileStates = new();
        private List<TileState> _clearedTileStates = new();
        private HashSet<TileState> _openTileStates = new();
        private bool _isChecking;

        public Action<List<TileState>> OnOpenTileStates;

        public void Initialize(GameState gameState)
        {
            _gameState = gameState;
        }

        public void AddToCheckTilesQueue(TileState tileState)
        {
            _checkTileStates.Add(tileState);
            if (!_isChecking)
            {
                _isChecking = true;
                GetTilesToOpen(tileState);
            }
        }

        private void OnFinishedCheck()
        {
            _clearedTileStates.Add(_checkTileStates[0]);
            _checkTileStates.RemoveAt(0);
            
            if (_checkTileStates.Count > 0)
            {
                GetTilesToOpen(_checkTileStates[0]);
            }
            else
            {
                OnOpenTileStates?.Invoke(_openTileStates.ToList());
                _openTileStates.Clear();
                _clearedTileStates.Clear();
                _isChecking = false;
            }
        }
        
        private void GetTilesToOpen(TileState tileState)
        {
            var tilePosition = tileState.tilePosition;
            
            int minRow = tilePosition.row - 1;
            int maxRow = tilePosition.row + 1;
            int minColumn = tilePosition.column - 1;
            int maxColumn = tilePosition.column + 1;
            
            for (int i = minRow; i <= maxRow; i++)
            {
                for (int j = minColumn; j <= maxColumn; j++)
                {
                    var newTilePosition = new TilePosition() { row = i, column = j};
                    if (IsInRange(newTilePosition))
                    {
                        var addedTileState = _gameState.GetTileStates.Find(x => x.tilePosition.IsEqual(newTilePosition));
                        if (addedTileState.tileType == TileTypes.Empty && !_clearedTileStates.Contains(addedTileState))
                        {
                            _checkTileStates.Add(addedTileState);
                        }
                        addedTileState.isOpen = true;
                        _openTileStates.Add(addedTileState);
                    }
                }
            }

            OnFinishedCheck();
        }

        private bool IsInRange(TilePosition tilePosition)
        {
            return tilePosition.row is >= 0 and < GameState.MaxRows &&
                   tilePosition.column is >= 0 and < GameState.MaxColumns;
        }
    }
}
