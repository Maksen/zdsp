using Photon.LoadBalancing.GameServer;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Text;
using Kopio.JsonContracts;
using Zealot.Common;
using Zealot.Repository;

namespace Zealot.Server.Rules
{
    public static class SevenDaysRules
    {
        private static bool isEventRunning;
        private static DateTime eventStartDT;

        //public static void Init()
        //{
        //    int serverid = GameApplication.Instance.GetMyServerId();

        //    // Total Credit
        //    var sevenDaysEventData = GameApplication.dbGM.SevenDays.GetSevenDaysEventData(serverid).Result; // There should be only 1 result

        //    if (sevenDaysEventData != null && sevenDaysEventData.Count > 0)
        //    {
        //        eventStartDT = (DateTime)sevenDaysEventData[0]["start"];
        //    }

        //    if(eventStartDT != DateTime.MinValue && eventStartDT != DateTime.MaxValue)
        //    {
        //        CheckEventRunning();
        //    }
        //    else
        //    {
        //        isEventRunning = false;
        //    }
        //}

        //private static void CheckEventRunning()
        //{
        //    if(isEventRunning && !IsEventPeriod() && !IsCollectionPeriod())
        //    {
        //        isEventRunning = false;
        //    }
        //    else if(!isEventRunning && (IsEventPeriod() || IsCollectionPeriod()))
        //    {
        //        isEventRunning = true;
        //    }
        //}

        //public static bool IsEventRunning()
        //{
        //    return isEventRunning;
        //}

        //public static bool IsEventPeriod()
        //{
        //    int maxDays = (int)Days.NUM_DAYS;
        //    DateTime endDT = eventStartDT.AddDays(maxDays);

        //    return DateTime.Now >= eventStartDT && DateTime.Now < endDT;
        //}

        //public static bool IsCollectionPeriod()
        //{
        //    DateTime endDT = eventStartDT.AddDays(10);

        //    return !IsEventPeriod() && DateTime.Now < endDT;
        //}

        //public static DateTime GetEventStartDate()
        //{
        //    return eventStartDT;
        //}

        //public static string Serialize()
        //{
        //    StringBuilder serializedData = new StringBuilder();
        //    serializedData.Append(IsEventRunning() ? 1 : 0);
        //    serializedData.Append("|");
        //    serializedData.Append(IsEventPeriod() ? 1 : 0);
        //    serializedData.Append("|");
        //    serializedData.Append(IsCollectionPeriod() ? 1 : 0);
        //    serializedData.Append("|");
        //    serializedData.Append(GetEventStartDate().Ticks);

        //    return serializedData.ToString();
        //}

        //public static void OnNewDay()
        //{
        //    CheckEventRunning();
        //}

        public static void LogSevenDaysTaskReward(string system, int newServerActvtyId, int rewardListId, int rewardItemId, int rewardAmount, 
            GameClientPeer peer)
        {
            string message = string.Format("System: {0} | New Server Activity Id: {1} | Reward List Id: {2} | Reward Item Id: {3} | Reward Amount: {4}",
                system,
                newServerActvtyId,
                rewardListId,
                rewardItemId,
                rewardAmount);

            Zealot.Logging.Client.LogClasses.SevenDaysTaskReward sevenDaysTaskRewardLog = new Zealot.Logging.Client.LogClasses.SevenDaysTaskReward();
            sevenDaysTaskRewardLog.userId               = peer.mUserId;
            sevenDaysTaskRewardLog.charId               = peer.GetCharId();
            sevenDaysTaskRewardLog.message              = message;
            sevenDaysTaskRewardLog.system               = system;
            sevenDaysTaskRewardLog.newServerActivityId  = newServerActvtyId;
            sevenDaysTaskRewardLog.rewardListId         = rewardListId;
            sevenDaysTaskRewardLog.rewardItemId         = rewardItemId;
            sevenDaysTaskRewardLog.rewardAmount         = rewardAmount;
            var ignoreAwait = Zealot.Logging.Client.LoggingAgent.Instance.LogAsync(sevenDaysTaskRewardLog);
        }

        public static void LogRestrictionBuyLimit(string system, int restrictionId, int buyLimitBef, int buyLimitAft, GameClientPeer peer)
        {
            string message = string.Format("System: {0} | Restriction Id: {1} | Buy Limit Before: {3} | Buy Limit After: {4}",
                system,
                restrictionId,
                buyLimitBef,
                buyLimitAft);

            Zealot.Logging.Client.LogClasses.RestrictionBuyLimit restrictionBuyLimitLog = new Zealot.Logging.Client.LogClasses.RestrictionBuyLimit();
            restrictionBuyLimitLog.userId           = peer.mUserId;
            restrictionBuyLimitLog.charId           = peer.GetCharId();
            restrictionBuyLimitLog.message          = message;
            restrictionBuyLimitLog.system           = system;
            restrictionBuyLimitLog.restrictionId    = restrictionId;
            restrictionBuyLimitLog.buyLimitBef      = buyLimitBef;
            restrictionBuyLimitLog.buyLimitAft      = buyLimitAft;
            var ignoreAwait = Zealot.Logging.Client.LoggingAgent.Instance.LogAsync(restrictionBuyLimitLog);
        }

        public static void LogRestrictionLockGoldUse(string system, int restrictionId, int useAmount, int lockGoldBef, int lockGoldAft, GameClientPeer peer)
        {
            string message = string.Format("System: {0} | Restriction Id: {1} | Use Amount: {2} | Lock Gold Before: {3} | Lock Gold After: {4}",
                system,
                restrictionId,
                useAmount,
                lockGoldBef,
                lockGoldAft);

            Zealot.Logging.Client.LogClasses.RestrictionLockGoldUse restrictionLockGoldUseLog = new Zealot.Logging.Client.LogClasses.RestrictionLockGoldUse();
            restrictionLockGoldUseLog.userId        = peer.mUserId;
            restrictionLockGoldUseLog.charId        = peer.GetCharId();
            restrictionLockGoldUseLog.message       = message;
            restrictionLockGoldUseLog.system        = system;
            restrictionLockGoldUseLog.restrictionId = restrictionId;
            restrictionLockGoldUseLog.useAmount     = useAmount;
            restrictionLockGoldUseLog.lockGoldBef   = lockGoldBef;
            restrictionLockGoldUseLog.lockGoldAft   = lockGoldAft;
            var ignoreAwait = Zealot.Logging.Client.LoggingAgent.Instance.LogAsync(restrictionLockGoldUseLog);
        }

        public static void LogRestrictionGoldUse(string system, int restrictionId, int useAmount, int goldBef, int goldAft, GameClientPeer peer)
        {
            string message = string.Format("System: {0} | Restriction Id: {1} | Use Amount: {2} | Gold Before: {3} | Gold After: {4}",
                system,
                restrictionId,
                useAmount,
                goldBef,
                goldAft);

            Zealot.Logging.Client.LogClasses.RestrictionGoldUse restrictionGoldUseLog = new Zealot.Logging.Client.LogClasses.RestrictionGoldUse();
            restrictionGoldUseLog.userId        = peer.mUserId;
            restrictionGoldUseLog.charId        = peer.GetCharId();
            restrictionGoldUseLog.message       = message;
            restrictionGoldUseLog.system        = system;
            restrictionGoldUseLog.restrictionId = restrictionId;
            restrictionGoldUseLog.useAmount     = useAmount;
            restrictionGoldUseLog.goldBef       = goldBef;
            restrictionGoldUseLog.goldAft       = goldAft;
            var ignoreAwait = Zealot.Logging.Client.LoggingAgent.Instance.LogAsync(restrictionGoldUseLog);
        }

        public static void LogRestrictionGetItem(string system, int restrictionId, int itemId, int itemAmount, int itemCountBef, int itemCountAft, GameClientPeer peer)
        {
            string message = string.Format("System: {0} | Restriction Id: {1} | Item Id: {2} | Item Amount: {3} | Item Count Before: {4} | Item Count After: {5}",
                system,
                restrictionId,
                itemId,
                itemAmount,
                itemCountBef,
                itemCountAft);

            Zealot.Logging.Client.LogClasses.RestrictionGetItem restrictionGetItemLog = new Zealot.Logging.Client.LogClasses.RestrictionGetItem();
            restrictionGetItemLog.userId        = peer.mUserId;
            restrictionGetItemLog.charId        = peer.GetCharId();
            restrictionGetItemLog.message       = message;
            restrictionGetItemLog.system        = system;
            restrictionGetItemLog.restrictionId = restrictionId;
            restrictionGetItemLog.itemId        = itemId;
            restrictionGetItemLog.itemAmount    = itemAmount;
            restrictionGetItemLog.itemCountBef  = itemCountBef;
            restrictionGetItemLog.itemCountAft  = itemCountAft;
            var ignoreAwait = Zealot.Logging.Client.LoggingAgent.Instance.LogAsync(restrictionGetItemLog);
        }
    }
}
