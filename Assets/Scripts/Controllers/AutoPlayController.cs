using System.Collections.Generic;
using System.Linq;
using Enums;
using Models;
using State;
using UnityEngine;
using Utils;

namespace Controllers
{
    public class AutoPlayController
    {
        private List<AutoPlayData> _multiMoves = new();

        private List<TilePosition> _cornerTilePositions = new()
        {
            new() { row = 0, column = 0 },
            new() { row = 0, column = GameState.MaxColumns - 1 },
            new() { row = GameState.MaxRows - 1, column = 0 },
            new() { row = GameState.MaxRows - 1, column = GameState.MaxColumns - 1 }
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
                GetRandomCorner(gameState);
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
                    GetRandomCorner(gameState);
                }
            }
        }

        private void GetRandomCorner(GameState gameState)
        {
            if (_cornerTilePositions.Count == 0)
            {
                return;
            }
            
            var chosenCornerPosition = GetRandomCornerTilePosition();
            _cornerTilePositions.Remove(chosenCornerPosition);

            AddToMultiMove(TilesHelper.GetTileState(gameState, chosenCornerPosition), InputAction.FindMine);
        }

        private TilePosition GetRandomCornerTilePosition()
        {
            return _cornerTilePositions[Random.Range(0, _cornerTilePositions.Count)];
        }

        private void CheckToMarkMine(TileState centerTile, List<TileState> tileStates)
        {
            tileStates.Remove(centerTile);
            var notOpenTiles = tileStates.FindAll(x => !x.isOpen);
            if (TilesHelper.GetTileTypeCount(centerTile.tileType) == notOpenTiles.Count)
            {
                foreach (var notOpenTile in notOpenTiles)
                {
                    AddToMultiMove(notOpenTile, InputAction.MarkMine);
                }
            }
        }

        private void CheckToFindMine(TileState centerTile, List<TileState> tileStates, GameState gameState)
        {
            tileStates.Remove(centerTile);
            var flaggedTiles = tileStates.FindAll(x => x.isFlagged);
            var notOpenTiles = tileStates.FindAll(x => !x.isOpen);
            var openTiles = tileStates.FindAll(x => x.isOpen);

            MinesCountEqualToFlagTilesPatternCheck(centerTile, flaggedTiles, notOpenTiles);
            SameColumnOrRowsPatternCheck(gameState, tileStates, notOpenTiles, centerTile);
            //CheckNonOpenTilesPattern(openTiles, notOpenTiles, gameState);
        }

        private void CheckNonOpenTilesPattern(List<TileState> openTiles, List<TileState> centerNotOpenTiles, GameState gameState)
        {
            foreach (var tileState in openTiles)
            {
                if (tileState.tileType == TileTypes.One)
                {
                    var tilesAround = TilesHelper.GetTilesAround(tileState, gameState);
                    tilesAround.Remove(tileState);
                    var notOpenTiles = tilesAround.FindAll(x => !x.isOpen);
                    foreach (var nonIntersectingTile in GetNonIntersectingTiles(centerNotOpenTiles, notOpenTiles))
                    {
                        AddToMultiMove(nonIntersectingTile, InputAction.FindMine);
                    }
                }
            }
        }

        private void MinesCountEqualToFlagTilesPatternCheck(TileState centerTile, List<TileState> flaggedTiles, List<TileState> notOpenTiles)
        {
            if (TilesHelper.GetTileTypeCount(centerTile.tileType) == flaggedTiles.Count)
            {
                foreach (var tileState in notOpenTiles)
                {
                    AddToMultiMove(tileState, InputAction.FindMine);
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
                CheckSideTileForMatch(gameState, tileStates, notOpenTiles, centerTile, sideTileState, centerTileDifference, isColumn, i);
            }
        }

        private void CheckSideTileForMatch(GameState gameState, List<TileState> tileStates, 
            List<TileState> notOpenTiles, TileState centerTile, TileState sideTileState, int centerTileDifference, bool isColumn, int sideIndex)
        {
            if (sideTileState.tileType == centerTile.tileType)
            {
                var leftTiles = TilesHelper.GetTilesAround(sideTileState, gameState);
                leftTiles.Remove(sideTileState);
                var leftNotOpenTiles = leftTiles.FindAll(x => !x.isOpen);
                    
                if (leftNotOpenTiles.Count == 2 
                    && TilesHelper.DoesTilesShareRowOrColumn(leftNotOpenTiles, isColumn) 
                    && DoesTilesIntersect(notOpenTiles, leftNotOpenTiles)
                    && DifferenceBetweenTileCheck(leftNotOpenTiles, !isColumn) == 1)
                {
                    var tileDifference = isColumn ? leftNotOpenTiles[0].tilePosition.column - sideTileState.tilePosition.column 
                        : leftNotOpenTiles[0].tilePosition.row - sideTileState.tilePosition.row;
                    
                    if (centerTileDifference == tileDifference)
                    {
                        var tilePosition = GetMineTilePosition(centerTile, tileDifference, sideIndex, isColumn);
                        AddToMultiMove(tileStates.Find(x => x.tilePosition.IsEqual(tilePosition)), InputAction.FindMine);
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
                int intersectCount = centerNotOpenTiles.Count(tileState2 => tileState.tilePosition.IsEqual(tileState2.tilePosition));

                if (intersectCount == 0)
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