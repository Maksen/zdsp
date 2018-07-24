using Newtonsoft.Json;
using System;

namespace Zealot.Common
{

    public enum OfflineExpRetCode2
    {
        OE_NoRewardChosen,
        OE_RewardChosen,
        OE_RewardReady,

        OE_ChooseRewardSuccess,
        OE_ChooseRewardFailed_InvalidReward,
        OE_ChooseRewardFailed_VipTooLow,

        OE_ClaimRewardSuccess,
        OE_ClaimRewardFailed_NotReady,
        OE_ClaimRewardFailed_NoMoney,
        OE_ClaimRewardFailed_NoGold
    }

    public enum OfflineExpClaimBonusCode
    {
        OEClaimBonus_None,
        OEClaimBonus_Money,
        OEClaimBonus_Gold
    }

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class OfflineExpClientData2
    {
        [JsonProperty(PropertyName = "retcode")]
        public int retcode { get; set; }

        [JsonProperty(PropertyName = "rdLV")]
        public int rewardLv { get; set; }

        [JsonProperty(PropertyName = "rdcNO")]
        public int rewardIndex { get; set; }

        [JsonProperty(PropertyName = "rdST")]
        public long rewardTimeLeft { get; set; }

        public OfflineExpClientData2()
        {
            retcode = -1;
            rewardLv = -1;
            rewardIndex = -1;
            rewardTimeLeft = -1;
        }
    }

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class OfflineExpInventory2
    {
        [JsonProperty(PropertyName = "rdLV")]
        public int rewardLv { get; set; }

        [JsonProperty(PropertyName = "rdIdx")]
        public int rewardIndex { get; set; }

        //TimeSpan
        [JsonProperty(PropertyName = "rdST")]
        public long rewardStartTime { get; set; }

        public OfflineExpInventory2()
        {
            rewardLv = -1;
            rewardIndex = -1;
            rewardStartTime = -1;
        }
    }
}
