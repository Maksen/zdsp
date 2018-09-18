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
    
    public class SocialInventoryData
    {
        public static readonly int MAX_FRIENDS = 50;
        
        public IList<string> friendList = new List<string>();
        public IList<string> friendRequestList = new List<string>();

        public SocialInventoryData() { }
    }
}
