using UnityEngine;

namespace Utils
{
    public static class SpriteLoader 
    {
        public static Sprite GetSprite(string spriteName)
        {
            if (string.IsNullOrEmpty(spriteName))
            {
                return null;
            }
            
            return Resources.Load<Sprite>("Sprites/MinesweeperSprites/"+spriteName);
        }
    }
}
