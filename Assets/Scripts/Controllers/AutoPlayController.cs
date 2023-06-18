using System.Collections.Generic;
using Enums;
using Models;
using State;
using Utils;
using Random = UnityEngine.Random;

namespace Controllers
{
    public class AutoPlayController
    {
        public List<AutoPlayData> _multiMoves = new();
        public HashSet<TileState> _mines = new();
        public HashSet<TileState> _processedTiles = new();

        private List<TilePosition> _randomCornerTilePositions = new()
        {
            new() { row = 0, column = 0 },
            new() { row = 0, column = GameState.MaxColumns - 1 },
            new() { row = GameState.MaxRows - 1, column = 0 },
            new() { row = GameState.MaxRows - 1, column = GameState.MaxColumns - 1 },
        };
        
        public AutoPlayData GetAIMove(GameState gameState)  
        {
            if (_multiMoves.Count == 0)
            {
                GetMoves(gameState);
            }

            if (_multiMoves.Count == 0)
            {
                return null;
            }
            
            var move = _multiMoves[0];
            _multiMoves.Remove(move);
            return move;
        }

        private void GetMoves(GameState gameState)
        {
            var openTiles = gameState.GetTileStates.FindAll(x => x.isOpen);
            if (openTiles.Count == 0)
            {
                GetRandomCornerTileState(gameState);
            }
            else
            {
                var openNumberedTiles = gameState.GetTileStates.FindAll(x => x.isOpen && x.tileType != TileTypes.Empty);

                foreach (var tileState in openNumberedTiles)
                {
                    CheckToMarkMine(tileState, TilesHelper.GetTilesAround(tileState, gameState));
                    CheckToFindMine(tileState, TilesHelper.GetTilesAround(tileState, gameState), gameState);
                }

                if (_multiMoves.Count == 0)
                {
                    GetRandomCornerTileState(gameState);
                }
            }
        }

        private void GetRandomCornerTileState(GameState gameState)
        {
            if (_randomCornerTilePositions.Count == 0)
            {
                return;
            }
            
            var chosenCornerPosition = GetRandomTilePosition();
            _randomCornerTilePositions.Remove(chosenCornerPosition);

            AddToMultiMove(TilesHelper.GetTileState(gameState, chosenCornerPosition), InputAction.FindMine);
        }

        private TilePosition GetRandomTilePosition()
        {
            return _randomCornerTilePositions[Random.Range(0, _randomCornerTilePositions.Count)];
        }

        private void CheckToMarkMine(TileState centerTile, List<TileState> tileStates)
        {
            tileStates.Remove(centerTile);
            var notOpenTiles = tileStates.FindAll(x => !x.isOpen);
            if (TilesHelper.GetTileTypeCount(centerTile.tileType) == notOpenTiles.Count)
            {
                foreach (var notOpenTile in notOpenTiles)
                {
                    if (!_mines.Contains(notOpenTile))
                    {
                        _mines.Add(notOpenTile);
                        AddToMultiMove(notOpenTile, InputAction.MarkMine);
                    }
                }
            }
            
        }

        private void CheckToFindMine(TileState centerTile, List<TileState> tileStates, GameState gameState)
        {
            tileStates.Remove(centerTile);
            var flaggedTiles = tileStates.FindAll(x => x.isFlagged);
            var notOpenTiles = tileStates.FindAll(x => !x.isOpen && !x.isFlagged);
            var openTiles = tileStates.FindAll(x => x.isOpen);

            MinesCountEqualToFlagTilesPatternCheck(centerTile, flaggedTiles, notOpenTiles);
            SameColumnOrRowsPatternCheck(gameState, tileStates, notOpenTiles, centerTile);
        }

        private void MinesCountEqualToFlagTilesPatternCheck(TileState centerTile, List<TileState> flaggedTiles, List<TileState> notOpenTiles)
        {
            if (TilesHelper.GetTileTypeCount(centerTile.tileType) == flaggedTiles.Count)
            {
                foreach (var tileState in notOpenTiles)
                {
                    if (!_processedTiles.Contains(tileState))
                    {
                        _processedTiles.Add(tileState);
                        AddToMultiMove(tileState, InputAction.FindMine);
                    }
                }
            }
        }

        private void SameColumnOrRowsPatternCheck(GameState gameState, List<TileState> tileStates, List<TileState> notOpenTiles, TileState centerTile)
        {
            for (int i = -1; i <= 1; i+=2)
            {
                var sameRowTiles = TilesHelper.GetSameRowOrColumnTiles(notOpenTiles, centerTile.tilePosition.row + i, false);
                if (sameRowTiles.Count == 3)
                {
                    CheckSidewaysPattern(gameState, sameRowTiles, tileStates, notOpenTiles, centerTile, false);
                }
            }

            for (int i = -1; i <= 1; i+=2)
            {
                var sameColumnTiles = TilesHelper.GetSameRowOrColumnTiles(notOpenTiles, centerTile.tilePosition.column + i, true);
                if (sameColumnTiles.Count == 3)
                {
                    CheckSidewaysPattern(gameState, sameColumnTiles, tileStates, notOpenTiles, centerTile, true);
                }
            }
        }

        private void CheckSidewaysPattern(GameState gameState, List<TileState> sameLineTiles, List<TileState> tileStates, 
            List<TileState> notOpenTiles, TileState centerTile, bool isColumn)
        {
            var centerTileDifference = isColumn ? sameLineTiles[0].tilePosition.column - centerTile.tilePosition.column 
                : sameLineTiles[0].tilePosition.row - centerTile.tilePosition.row;

            for (int i = -1; i <= 1; i+=2)
            {
                var tilePosition = new TilePosition()
                {
                    row = isColumn ? centerTile.tilePosition.row + i : centerTile.tilePosition.row,
                    column = isColumn ? centerTile.tilePosition.column : centerTile.tilePosition.column + i,
                };
                
                var sideTileState = tileStates.Find(x => x.tilePosition.IsEqual(tilePosition));

                if (sideTileState.tileType == TileTypes.One)
                {
                    CheckSideTileForMatch(gameState, tileStates, notOpenTiles, centerTile, sideTileState, centerTileDifference, isColumn, i);
                }
            }
        }

        private void CheckSideTileForMatch(GameState gameState, List<TileState> tileStates, 
            List<TileState> notOpenTiles, TileState centerTile, TileState sideTileState, int centerTileDifference, bool isColumn, int sideIndex)
        {
            var sideTiles = TilesHelper.GetTilesAround(sideTileState, gameState);
            sideTiles.Remove(sideTileState);
            var notOpenSideTiles = sideTiles.FindAll(x => !x.isOpen && !x.isFlagged);

            if (sideTileState.tileType == centerTile.tileType)
            {
                if (notOpenSideTiles.Count == 2 
                    && TilesHelper.DoesTilesShareRowOrColumn(notOpenSideTiles, isColumn) 
                    && DoesTilesIntersect(notOpenTiles, notOpenSideTiles)
                    && DifferenceBetweenTileCheck(notOpenSideTiles, !isColumn) == 1)
                {
                    var tileDifference = isColumn ? notOpenSideTiles[0].tilePosition.column - sideTileState.tilePosition.column 
                        : notOpenSideTiles[0].tilePosition.row - sideTileState.tilePosition.row;
                    
                    if (centerTileDifference == tileDifference)
                    {
                        var tilePosition = GetMineTilePosition(centerTile, tileDifference, sideIndex, isColumn);
                        var newTileState = tileStates.Find(x => x.tilePosition.IsEqual(tilePosition));
                        if (!_processedTiles.Contains(newTileState))
                        {
                            _processedTiles.Add(newTileState);
                            AddToMultiMove(newTileState, InputAction.FindMine);
                        }
                    }
                }
            }
        }

        private bool DoesTilesIntersect(List<TileState> compareTiles1, List<TileState> compareTiles2)
        {
            foreach (var tileState in compareTiles1)
            {
                foreach (var tileState2 in compareTiles2)
                {
                    if (tileState.tilePosition.IsEqual(tileState2.tilePosition))
                    {
                        return true;
                    }
                }
            }
            
            return false;
        }

        private int DifferenceBetweenTileCheck(List<TileState> tileStates, bool isColumn)
        {
            var diff = 0;
            for (int i = 1; i < tileStates.Count; i++)
            {
                var tempDiff = 0;
                tempDiff = isColumn
                    ? tileStates[i].tilePosition.column - tileStates[i - 1].tilePosition.column
                    : tileStates[i].tilePosition.row - tileStates[i - 1].tilePosition.row;

                if (tempDiff > diff)
                {
                    diff = tempDiff;
                }
            }

            return diff;
        }
        
        /// <summary>
        /// sideIndex refers to the direction either left or right or upwards or downwards
        /// stepDiff refers to the difference between centerTile and sameTiles chosen row or column
        /// </summary>
        /// <param name="centerTile"></param>
        /// <param name="stepDiff"></param>
        /// <param name="sideIndex"></param>
        /// <param name="isColumn"></param>
        /// <returns></returns>
        private TilePosition GetMineTilePosition(TileState centerTile, int stepDiff, int sideIndex, bool isColumn)
        {
            if (isColumn)
            {
                return new TilePosition()
                {
                    row = centerTile.tilePosition.row + (sideIndex * -1),
                    column = centerTile.tilePosition.column + stepDiff
                };
            }

            return new TilePosition()
            {
                row = centerTile.tilePosition.row + stepDiff,
                column = centerTile.tilePosition.column + (sideIndex * -1)
            };
        }
        
        private List<TileState> GetNonIntersectingTiles(List<TileState> centerNotOpenTiles, List<TileState> aroundNotOpenTiles)
        {
            var nonIntersectingTiles = new List<TileState>();
            foreach (var tileState in aroundNotOpenTiles)
            {
                var count = 0;
                foreach (var centerTileState in centerNotOpenTiles)
                {
                    if (centerTileState.tilePosition.IsEqual(tileState.tilePosition))
                    {
                        count++;
                    }
                }
                
                if (count == 0)
                {
                    nonIntersectingTiles.Add(tileState);
                }
            }
            
            return nonIntersectingTiles;
        }

        private void AddToMultiMove(TileState chosenState, InputAction inputAction)
        {
            var autoMove = new AutoPlayData
            {
                tileState = chosenState,
                inputAction = inputAction
            };
            _multiMoves.Add(autoMove);
        }
    }
}