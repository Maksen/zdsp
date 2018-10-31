using System.Collections.Generic;

namespace Zealot.Common
{
    public enum SocialReturnCode : byte
    {
        Ret_AlreadyAdded = 0,
        Ret_DoesNotExist,
        Ret_RecommendedResult,
        Ret_OnCooldown,
        Ret_LevelNotEnough
    }
    
    public partial class SocialInventoryData
    {
        public const int MAX_FRIENDS = Entities.SocialStats.MAX_FRIENDS;

        #region serializable properties
        public IList<string> friendList = new List<string>();
        public IList<string> friendRequestList = new List<string>();
        #endregion

        public SocialInventoryData() { }
    }
}
