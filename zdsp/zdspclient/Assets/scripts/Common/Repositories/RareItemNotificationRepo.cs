using UnityEngine;
using System.Collections.Generic;
using Kopio.JsonContracts;

namespace Zealot.Repository
{
    public class RareItemNotificationRepo
    {
        static List<int> _data = new List<int>();
        public static void Init(GameDBRepo gameData)
        {
            foreach(var data in gameData.RareItemNotification.Values)
            {
                if (data.broadcasttype > 0)
                {
                    if (!_data.Contains(data.itemid))
                        _data.Add(data.itemid);
                }
            }
        }

        public static bool IsRareItem(int itemid)
        {
            return _data.Contains(itemid);
        }
    }
}
