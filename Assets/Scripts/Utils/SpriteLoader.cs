using UnityEngine;

namespace Utils
{
    public static class SpriteLoader 
    {
        public static Sprite GetSprite(string spriteName)
        {
            return Resources.Load<Sprite>("Sprites/MinesweeperSprites/"+spriteName);
        }
    }
}
