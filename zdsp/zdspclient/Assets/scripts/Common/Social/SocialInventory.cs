using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Zealot.Common.Entities.Social;

namespace Zealot.Common
{
    /// <summary>
    /// 社群好友InventoryData for zdsp
    /// </summary>
    public partial class SocialInventoryData: IJsonInventoryData
    {
        [JsonProperty(PropertyName = "root")]
        public JToken root { get; set; }

        public SocialData data { get;private set; }


        #region removed future
        //[JsonProperty(PropertyName = "friendList")]
        //public IList<string> friendList= new List<string>();
        //[JsonProperty(PropertyName = "friendList")]
        //public IList<string> friendRequestList= new List<string>();
        #endregion
    }

    namespace Entities.Social
    {
        #region removed future
        //public enum SocialReturnCode : byte
        //{
        //    Ret_AlreadyAdded = 0,
        //    Ret_DoesNotExist,
        //    Ret_RecommendedResult,
        //    Ret_OnCooldown,
        //    Ret_LevelNotEnough
        //}
        #endregion
    }
}
