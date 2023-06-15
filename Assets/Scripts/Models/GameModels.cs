using System;
using System.Collections.Generic;
using Enums;

namespace Models
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
        public TileTypes tileType;
        public bool isFlagged;
        public bool isOpen;
    }
    
    [Serializable]
    public class MinesData
    {
        public List<TilePosition> minesPosition;
    }
}
