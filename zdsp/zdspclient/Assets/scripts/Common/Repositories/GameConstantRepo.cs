using Kopio.JsonContracts;
using System.Collections.Generic;

namespace Zealot.Repository
{
    public class NewCharInfo
    {
        public int levelId = 1;
        public float[] pos;
        public float[] dir;

        public NewCharInfo()
        {
            pos = new float[3];
            dir = new float[3];
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

            foreach (KeyValuePair<int, GameConstantJson> entry in gameData.GameConstant)
            {
                if(mNameMap.ContainsKey(entry.Value.name) == false)
                    mNameMap.Add(entry.Value.name, entry.Value.value);
            }
           
            string newChar_SpawnInfoStr = GetConstant("NewChar_SpawnInfo");
            if (!string.IsNullOrEmpty(newChar_SpawnInfoStr))
            {
                string[] newChar_SpawnInfo = newChar_SpawnInfoStr.Split(';');
                mNewCharInfo.levelId = int.Parse(newChar_SpawnInfo[0]);
                string[] newChar_Pos = newChar_SpawnInfo[1].Split('|');
                string[] newChar_Dir = newChar_SpawnInfo[2].Split('|');
                mNewCharInfo.pos = new float[] { float.Parse(newChar_Pos[0]), float.Parse(newChar_Pos[1]), float.Parse(newChar_Pos[2]) };
                mNewCharInfo.dir = new float[] { float.Parse(newChar_Dir[0]), float.Parse(newChar_Dir[1]), float.Parse(newChar_Dir[2]) };
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
