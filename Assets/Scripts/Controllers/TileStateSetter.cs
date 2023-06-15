using Enums;
using Models;
using State;

namespace Controllers
{
    public class TileStateSetter
    {
        public void SetTileStatesType(GameState gameState)
        {
            var nonMinesTiles = gameState.GetTileStates.FindAll(x => x.tileType != TileTypes.Mine);
            foreach (var tileState in nonMinesTiles)
            {
                tileState.tileType = GetTileType(MinesCount(gameState, tileState));
            }
        }
        
        private int MinesCount(GameState gameState, TileState tileState)
        {
            int minRow = tileState.tilePosition.row - 1;
            int maxRow = tileState.tilePosition.row + 1;
            int minColumn = tileState.tilePosition.column - 1;
            int maxColumn = tileState.tilePosition.column + 1;

            int minesCount = 0;
            
            for (int i = minRow; i <= maxRow; i++)
            {
                for (int j = minColumn; j <= maxColumn; j++)
                {
                    var newTilePosition = new TilePosition() { row = i, column = j};
                    if (IsInRange(newTilePosition))
                    {
                        if (gameState.GetTileStates.Find(x => x.tilePosition.IsEqual(newTilePosition)).tileType ==
                            TileTypes.Mine)
                        {
                            minesCount++;
                        }
                    }
                }
            }

            return minesCount;
        }
        
        private bool IsInRange(TilePosition tilePosition)
        {
            return tilePosition.row is >= 0 and < GameState.MaxRows &&
                   tilePosition.column is >= 0 and < GameState.MaxColumns;
        }

        private TileTypes GetTileType(int minesCount)
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
    }
}
