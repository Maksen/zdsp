using Kopio.JsonContracts;
using System.Collections.Generic;

namespace Zealot.Repository
{
    public static class MailRepo
    {
        public static Dictionary<string, int> mNameMap;
        public static Dictionary<int, MailContentJson> mIdMap;

        static MailRepo()
        {
            mNameMap = new Dictionary<string, int>();
            mIdMap = new Dictionary<int, MailContentJson>();
        }

        public static void Init(GameDBRepo gameData)
        {
            mNameMap.Clear();
            mIdMap = gameData.MailContent;

            foreach (KeyValuePair<int, MailContentJson> entry in gameData.MailContent)
            {
                mNameMap.Add(entry.Value.name, entry.Key);
            }
        }

        public static MailContentJson GetInfoByName(string mailName)
        {
            if (mNameMap.ContainsKey(mailName))
            {
                return mIdMap[mNameMap[mailName]];
            }
                
            return null;
        }
    }
}
