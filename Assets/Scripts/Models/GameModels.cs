using System;
using Enums;

namespace Models
{
    public class GameModels 
    {
        [Serializable]
        public struct TilePosition
        {
            public int row;
            public int column;
            
            public bool IsEqual(TilePosition tilePosition)
            {
                return tilePosition.row == row && tilePosition.column == column;
            }
        }

        [Serializable]
        public class TileState
        {
            public TilePosition tilePosition;
            public GameEnums.TileTypes tileType;
            public bool flagged;
        }
    }
}
