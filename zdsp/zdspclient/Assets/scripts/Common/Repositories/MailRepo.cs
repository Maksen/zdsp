using Kopio.JsonContracts;
using System.Collections.Generic;

namespace Zealot.Repository
{
    public static class MailRepo
    {
        public static Dictionary<int, MailContentJson> mIdMap;
        public static Dictionary<string, int> mNameMap;    

        static MailRepo()
        {
            mNameMap = new Dictionary<string, int>();
        }

        public static void Init(GameDBRepo gameData)
        {
            mIdMap = gameData.MailContent;
            mNameMap.Clear();
            
            foreach (KeyValuePair<int, MailContentJson> entry in gameData.MailContent)
            {
                mNameMap.Add(entry.Value.name, entry.Key);
            }
        }

        public static MailContentJson GetInfoByName(string mailName)
        {
            if (mNameMap.ContainsKey(mailName))
                return mIdMap[mNameMap[mailName]];
            return null;
        }
    }
}
