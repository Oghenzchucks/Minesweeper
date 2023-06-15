using System.IO;
using Models;
using UnityEngine;

namespace Config
{
    public class ConfigService 
    {
        private const string FileName = "mines.json";
        
        public MinesData LoadFromJson()
        {
            string filePath = Path.Combine (Application.streamingAssetsPath + "/", FileName);

            if (File.Exists(filePath))
            {
                string data = File.ReadAllText(filePath);
                var mineData = JsonUtility.FromJson<MinesData>(data);
                return mineData;
            }
            else
            {
                return null;
            }
        }
    }
}
