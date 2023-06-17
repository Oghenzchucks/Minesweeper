using System;
using System.Collections.Generic;
using System.Linq;
using Enums;
using Models;
using State;
using Utils;

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
            foreach (var aroundTileState in TilesHelper.GetTilesAround(tileState, _gameState))
            {
                if (aroundTileState.tileType == TileTypes.Empty && !_clearedTileStates.Contains(aroundTileState))
                {
                    _checkTileStates.Add(aroundTileState);
                }
                aroundTileState.isOpen = true;
                _openTileStates.Add(aroundTileState);
            }

            OnFinishedCheck();
        }
    }
}
