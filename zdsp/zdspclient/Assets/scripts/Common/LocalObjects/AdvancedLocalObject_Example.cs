#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using Zealot.Common.Datablock;


namespace Zealot.Common.Entities.Examples
{
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class ExampleInventoryData : IInventoryData
    {
        [JsonProperty("root")]
        public JToken root { get; set; }
    }

    /// <summary>
    /// 如果想檢查資料格式是否有殘留的欄位，可以在存取的時候定義schema去check資料
    /// </summary>
    #region Schema
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class Example_Schema
    {
        public const int VERSION = 101;

        [JsonProperty("version")]
        public int version { get; set; }

        [JsonProperty("list")]
        public List<ExampleItem_Schema> list { get; set; }

        [JsonProperty("checkdate")]
        public string checkdate { get; set; }

        public Example_Schema()
        {
            list = new List<ExampleItem_Schema>();
            version = VERSION;
        }
    }

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class ExampleItem_Schema
    {
        [JsonProperty("name")]
        public string name { get; set; }
        [JsonProperty("playerHP")]
        public int playerHP { get; set; }
        public ExampleItem_Schema()
        {
            name = string.Empty;
            playerHP = 0;
        }
    }

    #endregion

    public class ExampleStats : AdvancedLocalObject, IStats // Send only to local client
    {
        [NotSynced]
        public Examples.ExampleData data { get; private set; }

        public ExampleStats(bool isServer) : base((LOTYPE)0, isServer)//0 不是正確的代號，請設定成自己的代號
        {
        }

        public override void OnEvent(int index, string path, string eventName, string param)
        {
            if(eventName=="load_finish")//代表讀取完畢
            {
                //客戶端初始化資料
                data.UpdateRootValue(Root);
            }
        }

        public void LoadFromInventoryData(IInventoryData data)
        {
            ExampleInventoryData invData = (ExampleInventoryData)data;

            var root = invData.root as JObject;
            JToken ver;
            //驗證資料，將不要的舊版資料刪除(可以用版本欄位判斷是否需要驗證，因為驗證會消耗效能)
            if (!root.TryGetValue("version", out ver) || ver.Type != JTokenType.Integer || ver.Value<int>() < Example_Schema.VERSION)
                Root = GameUtils.Validate<Example_Schema>(root);
            else
                Root = root;


            //將資料傳到客戶端
            this.data.PatchToClient();
            this.PatchEvent(string.Empty, "load_finish", string.Empty);
        }

        public void SaveToInventoryData(IInventoryData data)
        {
            ExampleInventoryData invData = (ExampleInventoryData)data;
        }
    }

    public class ExampleData : AdvancedLocalObjectData
    {
        //這段資料會被儲存且server與client都會同步
        public int version { get { return GetValue<int>("version"); } set { SetValue("version", value); } }
        public ExampleList list { get; private set; }

        //這段資料只有server知道
        public string checkdate { get { return GetValue<string>("checkdate"); } set { SetValueNoPatch("checkdate", value); } }

        //這段資料不會被儲存但server與client都會同步
        public int listCount { get { return GetValue<int>("listCount"); } set { SetValue("listCount", value); } }

        protected override void OnSetDataUsage()
        {
            //設定能被client端看到且被server儲存的資訊欄位名稱
            this.m_Records = new string[] { "version", "list" };
            //設定只有server儲存的資訊欄位名稱
            this.m_ServerRecords = new string[] { "checkdate" };
            //設定能被client端看到但不被server儲存的資訊欄位名稱
            this.m_States = new string[] { };
        }

        void initFromRoot(JToken root, AdvancedLocalObject obj)
        {
            NewIfNotExist(root, "version", () => new JValue(-1));
            list = new ExampleList(NewIfNotExist(root, "list", NewArray), "list", obj);

            NewIfNotExist(root, "checkdate", () => new JValue(string.Empty));

            NewIfNotExist(root, "listCount", () => new JValue(list.Count));
        }

        /// <summary>
        /// client端的資料
        /// </summary>
        public ExampleData() : base(null, new JObject())
        {
            OnUpdateRootValue += () => { initFromRoot(m_Root, null); };
        }
        /// <summary>
        /// server端的stats資料，請填入從InventoryData讀取的json資料
        /// </summary>
        public ExampleData(JToken root, AdvancedLocalObject obj) : base(obj, root)
        {
            OnUpdateRootValue += () => { initFromRoot(m_Root, null); };
            UpdateRootValue();//這個function代表初始化資料
        }

    }


    public struct ExampleItem
    {
        public readonly JToken node;

        public string name
        {
            get { return node["name"].Value<string>(); }
        }
        public int playerHP
        {
            get { return node["playerHP"].Value<int>(); }
        }

        public ExampleItem(JToken node)
        {
            this.node = node;
        }
        public ExampleItem( string name,int HP)
        {
            node = new JObject();
            node["name"] = new JValue(name);
            node["playerHP"] = new JValue(playerHP);
        }
    }

    public class ExampleList : JArrayNode<ExampleItem>
    {
        protected override JToken GetToken(ExampleItem node)
        {
            return node.node;
        }
        protected override ExampleItem GetNode(JToken token)
        {
            return new ExampleItem(token);
        }
        public ExampleList(JArray array, string path, AdvancedLocalObject obj) : base(array, path, obj)
        {
        }
        public bool ContainsName(string name)
        {
            int c = Count;
            for (int i = 0; i < c; i++)
                if (this[i].name == name)
                    return true;
            return false;
        }

    }
}

#endif