using System;
using System.Collections.Generic;
using System.Linq;
using Kopio.JsonContracts;
using Zealot.Common;
using System.Reflection;

namespace Zealot.Repository
{
    public class PrizeGuaranteeRepository
    {
        private static Dictionary<int, Dictionary<int, PrizeGuaranteeTable>> prizeGuaranteeByItemId = new Dictionary<int, Dictionary<int, PrizeGuaranteeTable>>();
        public const int ThresholdCount=5;

        public static void Init(GameDBRepo gameData)
        {
            foreach (var entry in gameData.PrizeGuarantee.Values)
            {
                int type= (int)entry.type;

                Dictionary<int, PrizeGuaranteeTable> map;
                if (prizeGuaranteeByItemId.TryGetValue(type,out map) ==false)
                {
                    map = new Dictionary<int, PrizeGuaranteeTable>();
                    prizeGuaranteeByItemId.Add(type,map);
                }

                if(map.ContainsKey(entry.itemid)==false)
                {
                    PrizeGuaranteeTable table = new PrizeGuaranteeTable();
                    map.Add(entry.itemid, table);
                    table.AddTable(entry);
                }
            }
        }

        public static PrizeGuaranteeTable GetPrizeGuaranteeByType(int type,int itemId)
        {
            PrizeGuaranteeTable table=null;
            Dictionary<int, PrizeGuaranteeTable> map;
            if (prizeGuaranteeByItemId.TryGetValue(type, out map))
            {
                map.TryGetValue(itemId, out table);
            }
            return table;
        }

        public static int[] GetPrizeGuaranteeIDs()
        {
            return prizeGuaranteeByItemId.Keys.ToArray<int>();
        }
    }

    public class PrizeGuaranteeTable
    {
        public int countId;
        public int type;
        public int itemId;
        public PrizeData[] container;
        public bool reset;

        public void AddTable(PrizeGuaranteeJson data)
        {
            countId = data.id;
            type = (int)data.type;
            itemId = data.itemid;
            reset = data.reset;

            container = new PrizeData[PrizeGuaranteeRepository.ThresholdCount];

            for (int i = 1; i <= container.Length; i++)
            {
                PrizeData temp= new PrizeData();
                int.TryParse(data.GetType().GetProperty(string.Format("prizeid{0}", i)).GetValue(data,null).ToString(), out temp.id);
                int.TryParse(data.GetType().GetProperty(string.Format("prizecount{0}", i)).GetValue(data,null).ToString(), out temp.count);
                int.TryParse(data.GetType().GetProperty(string.Format("threshold{0}", i)).GetValue(data,null).ToString(), out temp.threshold);
                container[i - 1] = temp;
            }
        }
    }
    public class PrizeData
    {
        public int id;
        public int count;
        public int threshold;
    }
}
