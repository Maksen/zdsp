using System.Text;
using System;
using System.Collections.Generic;
using Zealot.Common.Datablock;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Zealot.DebugTools;
using Zealot.Common.Entities;



namespace Zealot.Common
{
    public partial class SocialInventoryData : IJsonInventoryData
    {
        public SocialInventoryData()
        {

        }

        public void LoadDataFromJsonString(string data)
        {
            JToken _root = null;
            try { _root = JsonConvert.DeserializeObject<JToken>(data); }
            catch { };
            if (_root == null || _root.Type != JTokenType.Object)
                _root = new JObject();
            root = _root;

            this.data = new Entities.Social.SocialData(root);
        }

        public string SaveDataToJsonString()
        {
            string result = null;
            try { result = JsonConvert.SerializeObject(data.Root); }
            catch { }

            if (string.IsNullOrEmpty(result))
            {
                data.OnUpdateNewRoot(new JObject());
                result = JsonConvert.SerializeObject(data.Root);
            }

            return result;
        }

    }
}