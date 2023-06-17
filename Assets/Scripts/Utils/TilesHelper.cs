using System.Collections.Generic;
using Enums;
using Models;
using State;

namespace Utils
{
    public static class TilesHelper 
    {
        private static bool IsInRange(TilePosition tilePosition)
        {
            return tilePosition.row is >= 0 and < GameState.MaxRows &&
                   tilePosition.column is >= 0 and < GameState.MaxColumns;
        }
        
        public static TileTypes GetTileType(int minesCount)
        {
            switch (minesCount)
            {
                case 1:
                    return TileTypes.One;
                case 2:
                    return TileTypes.Two;
                case 3:
                    return TileTypes.Three;
                case 4:
                    return TileTypes.Four;
                case 5:
                    return TileTypes.Five;
                case 6:
                    return TileTypes.Six;
                case 7:
                    return TileTypes.Seven;
                case 8:
                    return TileTypes.Eight;
                default:
                    return TileTypes.Empty;
            }
        }
        
        public static int GetTileTypeCount(TileTypes tileType)
        {
            switch (tileType)
            {
                case TileTypes.One:
                    return 1;
                case TileTypes.Two:
                    return 2;
                case TileTypes.Three:
                    return 3;
                case TileTypes.Four:
                    return 4;
                case TileTypes.Five:
                    return 5;
                case TileTypes.Six:
                    return 6;
                case TileTypes.Seven:
                    return 7;
                case TileTypes.Eight:
                    return 8;
                default:
                    return 0;
            }
        }
        
        public static List<TileState> GetTilesAround(TileState tileState, GameState gameState)
        {
            int minRow = tileState.tilePosition.row - 1;
            int maxRow = tileState.tilePosition.row + 1;
            int minColumn = tileState.tilePosition.column - 1;
            int maxColumn = tileState.tilePosition.column + 1;

            List<TileState> tilesStatesAround = new List<TileState>();
            for (int i = minRow; i <= maxRow; i++)
            {
                for (int j = minColumn; j <= maxColumn; j++)
                {
                    var newTilePosition = new TilePosition() { row = i, column = j};
                    
                    if (IsInRange(newTilePosition))
                    {
                        var aroundTileState = GetTileState(gameState, newTilePosition);
                        tilesStatesAround.Add(aroundTileState);
                    }
                }
            }

            return tilesStatesAround;
        }
        
        public static TileState GetTileState(GameState gameState, TilePosition tilePosition)
        {
            return gameState.GetTileStates.Find(x => x.tilePosition.IsEqual(tilePosition));
        }

        public static bool DoesTilesShareRowOrColumn(List<TileState> tileStates, bool isColumn)
        {
            int count = 0;
            var tilePosition = tileStates[0].tilePosition;
            
            for (int i = 1; i < tileStates.Count; i++)
            {
                if (isColumn)
                {
                    if (tileStates[i].tilePosition.column == tilePosition.column)
                    {
                        count++;
                    }
                }
                else
                {
                    if (tileStates[i].tilePosition.row == tilePosition.row)
                    {
                        count++;
                    }
                }
            }

            return count == tileStates.Count - 1;
        }
        
        public static List<TileState> GetSameRowOrColumnTiles(List<TileState> tileStates, int amount, bool isColumn)
        {
            var shareTiles = new List<TileState>();

            foreach (var tileState in tileStates)
            {
                if (isColumn)
                {
                    if (tileState.tilePosition.column == amount)
                    {
                        shareTiles.Add(tileState);
                    }
                }
                else
                {
                    if (tileState.tilePosition.row == amount)
                    {
                        shareTiles.Add(tileState);
                    }
                }
            }

            return shareTiles;
        }
    }
}
