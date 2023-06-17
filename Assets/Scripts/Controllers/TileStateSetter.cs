using Enums;
using Models;
using State;
using Utils;

namespace Controllers
{
    public class TileStateSetter
    {
        public void SetTileStatesType(GameState gameState)
        {
            var nonMinesTiles = gameState.GetTileStates.FindAll(x => x.tileType != TileTypes.Mine);
            foreach (var tileState in nonMinesTiles)
            {
                tileState.tileType = TilesHelper.GetTileType(MinesCount(gameState, tileState));
            }
        }
        
        private int MinesCount(GameState gameState, TileState tileState)
        {
            int minesCount = 0;
            foreach (var aroundTileState in TilesHelper.GetTilesAround(tileState, gameState))
            {
                if (aroundTileState.tileType == TileTypes.Mine)
                {
                    minesCount++;
                }
            }

            return minesCount;
        }
    }
}