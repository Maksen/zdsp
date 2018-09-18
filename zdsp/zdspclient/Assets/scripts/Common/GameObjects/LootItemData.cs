using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Zealot.Common
{
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class LimitedItemInventory
    {
        #region serializable properties
        [JsonProperty(PropertyName = "record")]
        public Dictionary<int, LimitedItemData> records = new Dictionary<int, LimitedItemData>();
        #endregion

        public bool IsDirty = false;

        public string SerializeForDB(bool formatIndented = false)
        {
            JsonSerializerSettings jsonSetting = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.None, DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore };
            Formatting formatting = formatIndented ? Formatting.Indented : Formatting.None;
            return JsonConvert.SerializeObject(this, formatting, jsonSetting);
        }

        public static LimitedItemInventory DeserializeFromDB(string data)
        {
            JsonSerializerSettings jsonSetting = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.None, DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore };
            return JsonConvert.DeserializeObject<LimitedItemInventory>(data, jsonSetting);
        }
    }

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class LimitedItemData
    {
        [JsonProperty(PropertyName = "amt")]
        public int amount { get; set; }

        [JsonProperty(PropertyName = "dt")]
        public DateTime lastUpdateDate { get; set; }
    }

    public class LootItemDisplayInventory
    {
        public float[] pos = new float[3];
        public List<LootItemDisplay> records = new List<LootItemDisplay>();

        public void Add(int pid, int itemid)
        {
            records.Add(new LootItemDisplay(){ pid = pid, itemid = itemid });
        }

        public void SetPos(float x, float y, float z)
        {
            pos[0] = x;
            pos[1] = y;
            pos[2] = z;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0}|{1}|{2};", pos[0], pos[1], pos[2]);
            for (int index = 0; index < records.Count; ++index)
                sb.Append(records[index].ToString() + ';');
            return sb.ToString().TrimEnd(';');
        }       

        public static LootItemDisplayInventory ToObject(string data)
        {
            if (string.IsNullOrEmpty(data))
                return null;
            LootItemDisplayInventory ret = new LootItemDisplayInventory();
            string[] itemsArray = data.Split(';');
            int length = itemsArray.Length;
            for (int index = 0; index < length; index++)
            {
                if (index == 0)
                {
                    if (string.IsNullOrEmpty(itemsArray[index]))
                        continue;
                    string[] arr = itemsArray[index].Split('|');
                    if (arr.Length == 3)
                    {
                        ret.pos[0] = float.Parse(arr[0]);
                        ret.pos[1] = float.Parse(arr[1]);
                        ret.pos[2] = float.Parse(arr[2]);
                    }
                }
                else
                {
                    LootItemDisplay itemDisplay = LootItemDisplay.ToObject(itemsArray[index]);
                    if (itemDisplay != null)
                        ret.records.Add(itemDisplay);
                }
            }
            return ret;
        }
    }

    public class LootItemDisplay
    {
        public int pid; //offline owner pid = 0
        public int itemid; //-1 = currency

        public override string ToString()
        {
            return string.Format("{0}|{1}", pid, itemid);
        }

        public static LootItemDisplay ToObject(string data)
        {
            if (string.IsNullOrEmpty(data))
                return null;
            string[] arr = data.Split('|');
            if (arr.Length == 2)
                return new LootItemDisplay() { pid = int.Parse(arr[0]), itemid = int.Parse(arr[1]) };
            return null;
        }
    }
}
