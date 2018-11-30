using Kopio.JsonContracts;
using System.Collections.Generic;

namespace Zealot.Repository
{
    public class NewCharInfo
    {
        public int LevelId = 1;
        public float[] Pos;
        public float[] Dir;

        public NewCharInfo()
        {
            Pos = new float[3];
            Dir = new float[3];
        }
    }

    public static class GameConstantRepo
    {
        public static Dictionary<string, string> mNameMap;
        public static NewCharInfo mNewCharInfo;
        public static int ItemInvStartingSlotCount = 0;

        static GameConstantRepo()
        {
            mNameMap = new Dictionary<string, string>();
            mNewCharInfo = new NewCharInfo();
        }

        public static void Init(GameDBRepo gameData)
        {
            mNameMap.Clear();

            var gameConstantValues = gameData.GameConstant.Values;
            foreach (GameConstantJson gameConstant in gameConstantValues)
            {
                if(mNameMap.ContainsKey(gameConstant.name) == false)
                    mNameMap.Add(gameConstant.name, gameConstant.value);
            }
           
            string newChar_SpawnInfoStr = GetConstant("NewChar_SpawnInfo");
            if (!string.IsNullOrEmpty(newChar_SpawnInfoStr))
            {
                string[] newChar_SpawnInfo = newChar_SpawnInfoStr.Split(';');
                mNewCharInfo.LevelId = int.Parse(newChar_SpawnInfo[0]);
                string[] newChar_Pos = newChar_SpawnInfo[1].Split('|');
                float.TryParse(newChar_Pos[0], out mNewCharInfo.Pos[0]);
                float.TryParse(newChar_Pos[1], out mNewCharInfo.Pos[1]);
                float.TryParse(newChar_Pos[2], out mNewCharInfo.Pos[2]);
                string[] newChar_Dir = newChar_SpawnInfo[2].Split('|');       
                float.TryParse(newChar_Dir[0], out mNewCharInfo.Dir[0]);
                float.TryParse(newChar_Dir[1], out mNewCharInfo.Dir[1]);
                float.TryParse(newChar_Dir[2], out mNewCharInfo.Dir[2]);
            }

            ItemInvStartingSlotCount = GetConstantInt("NewChar_InventorySlotCount", 30);
        }

        public static string GetConstant(string key)
        {
            string value = "";
            mNameMap.TryGetValue(key, out value);
            return value;
        }

        public static int GetConstantInt(string key, int defaultValue = 0)
        {
            string value = "";
            if (mNameMap.TryGetValue(key, out value))
            {
                int valueInt;
                if (int.TryParse(value, out valueInt))
                    return valueInt;
            }
            return defaultValue;
        }
    }
}
