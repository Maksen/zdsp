using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Zealot.Common.Entities;
using Zealot.Common.Entities.Social;

namespace Zealot.Common
{
    public partial class SocialInventoryData : IJsonInventoryData
    {
        public SocialInventoryData()
        {

        }

        public void LoadDataFromJsonString(string data)
        {
            GameUtils.CatchException(SocialData.Debug, () =>
            {
                JObject _root = null;
                JsonSerializerSettings settings = new JsonSerializerSettings();
                bool error = false;
                settings.Error = (o, e) =>
                {
                    e.ErrorContext.Handled = true;
                };
                 _root = JsonConvert.DeserializeObject<JToken>(data) as JObject;

                if (error || _root == null || _root.Type != JTokenType.Object)
                    _root = new JObject();

                root = _root;

                this.data = new Entities.Social.SocialData(root,true);

            });
        }


        public string SaveDataToJsonString()
        {
            return GameUtils.CatchException(SocialData.Debug, () =>
            {
                string result = data.BuildRecordsString();

                if (string.IsNullOrEmpty(result))
                {
                    data.OnUpdateNewRoot(new JObject());
                    result = JsonConvert.SerializeObject(data.Root, Formatting.None);
                }

                return result;
            }, () => Entities.Social.SocialData.FinalVersion().ToString(Formatting.None));
        }

    }
}