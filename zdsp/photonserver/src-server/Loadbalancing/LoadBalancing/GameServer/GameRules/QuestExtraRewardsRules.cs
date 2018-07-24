using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Photon.LoadBalancing.GameServer;


namespace Zealot.Server.Rules
{
    public static class QuestExtraRewardsRules
    {
        public static void LogQERBoxRewardGet(string type, int currentActivePts, int boxNum, GameClientPeer peer)
        {
            string message = string.Format("Type: {0} | Current Active Points: {1} | Box Num: {2}",
                type,
                currentActivePts,
                boxNum);

            Zealot.Logging.Client.LogClasses.QERBoxRewardGet qerRewardBoxGetLog = new Zealot.Logging.Client.LogClasses.QERBoxRewardGet();
            qerRewardBoxGetLog.userId               = peer.mUserId;
            qerRewardBoxGetLog.charId               = peer.GetCharId();
            qerRewardBoxGetLog.message              = message;
            qerRewardBoxGetLog.type                 = type;
            qerRewardBoxGetLog.currActivePts        = currentActivePts;
            qerRewardBoxGetLog.boxNum               = boxNum;
            var ignoreAwait = Zealot.Logging.Client.LoggingAgent.Instance.LogAsync(qerRewardBoxGetLog);
        }

        public static void LogQERTaskFinish(string type, int questId, GameClientPeer peer)
        {
            string message = string.Format("Type: {0} | Quest Id: {0}", type, questId);

            Zealot.Logging.Client.LogClasses.QERTaskFinish qerTaskFinishLog = new Zealot.Logging.Client.LogClasses.QERTaskFinish();
            qerTaskFinishLog.userId     = peer.mUserId;
            qerTaskFinishLog.charId     = peer.GetCharId();
            qerTaskFinishLog.message    = message;
            qerTaskFinishLog.type       = type;
            qerTaskFinishLog.questId    = questId;
            var ignoreAwait = Zealot.Logging.Client.LoggingAgent.Instance.LogAsync(qerTaskFinishLog);
        }

        public static void LogQERTaskFinishAll(string type, string questIds, GameClientPeer peer)
        {
            string message = string.Format("Type: {0} | Quest Ids: {0}", type, questIds);

            Zealot.Logging.Client.LogClasses.QERTaskFinishAll qerTaskFinishAllLog = new Zealot.Logging.Client.LogClasses.QERTaskFinishAll();
            qerTaskFinishAllLog.userId      = peer.mUserId;
            qerTaskFinishAllLog.charId      = peer.GetCharId();
            qerTaskFinishAllLog.message     = message;
            qerTaskFinishAllLog.type        = type;
            qerTaskFinishAllLog.questIds    = questIds;
            var ignoreAwait = Zealot.Logging.Client.LoggingAgent.Instance.LogAsync(qerTaskFinishAllLog);
        }

        public static void LogQERActivePtsGet(string getType, int activePtsAmount, int activePtsAft, GameClientPeer peer)
        {
            string message = string.Format("Get Type: {0} | Active Points Amount: {1} | Active Points After: {2}",
                getType,
                activePtsAmount,
                activePtsAft);

            Zealot.Logging.Client.LogClasses.QERActivePtsGet qerActivePtsGetLog = new Zealot.Logging.Client.LogClasses.QERActivePtsGet();
            qerActivePtsGetLog.userId           = peer.mUserId;
            qerActivePtsGetLog.charId           = peer.GetCharId();
            qerActivePtsGetLog.message          = message;
            qerActivePtsGetLog.getType          = getType;
            qerActivePtsGetLog.activePtsAmount  = activePtsAmount;
            qerActivePtsGetLog.activePtsAft     = activePtsAft;
            var ignoreAwait = Zealot.Logging.Client.LoggingAgent.Instance.LogAsync(qerActivePtsGetLog);
        }

        public static void LogQERActivePtsGetQuestID(string type, int questid, string getType, GameClientPeer peer)
        {
            string message = string.Format("Type: {0} | Quest ID: {1} | Action type: {2}",
                type,
                questid,
                getType);

            Zealot.Logging.Client.LogClasses.QERActivePtsGetQuestID qerActivePtsGetLog = new Zealot.Logging.Client.LogClasses.QERActivePtsGetQuestID();
            qerActivePtsGetLog.userId = peer.mUserId;
            qerActivePtsGetLog.charId = peer.GetCharId();
            qerActivePtsGetLog.message = message;
            qerActivePtsGetLog.type = type;
            qerActivePtsGetLog.getType = getType;
            qerActivePtsGetLog.questId = questid;
            var ignoreAwait = Zealot.Logging.Client.LoggingAgent.Instance.LogAsync(qerActivePtsGetLog);
        }

        public static void LogQERMoneyGet(int questId, string getType, int moneyAmount, int moneyBef, int moneyAft, GameClientPeer peer)
        {
            string message = string.Format("Quest Id: {0} | Get Type: {1} | Money Amount: {2} | Money Before: {3} | Money After: {4}",
                questId,
                getType,
                moneyAmount,
                moneyBef,
                moneyAft);

            Zealot.Logging.Client.LogClasses.QERMoneyGet qerMoneyGetLog = new Zealot.Logging.Client.LogClasses.QERMoneyGet();
            qerMoneyGetLog.userId       = peer.mUserId;
            qerMoneyGetLog.charId       = peer.GetCharId();
            qerMoneyGetLog.message      = message;
            qerMoneyGetLog.questId      = questId;
            qerMoneyGetLog.getType      = getType;
            qerMoneyGetLog.moneyAmount  = moneyAmount;
            qerMoneyGetLog.moneyBef     = moneyBef;
            qerMoneyGetLog.moneyAft     = moneyAft;
            var ignoreAwait = Zealot.Logging.Client.LoggingAgent.Instance.LogAsync(qerMoneyGetLog);
        }

        public static void LogQERVIPXPGet(string type, int questId, string getType, int vipXPAmount, GameClientPeer peer)
        {
            string message = string.Format("Type: {0} | Quest Id: {1} | Get Type: {2} | VIP XP Amount: {3}",
                type,
                questId,
                getType,
                vipXPAmount);

            Zealot.Logging.Client.LogClasses.QERVIPXPGet qerVIPXPGetLog = new Zealot.Logging.Client.LogClasses.QERVIPXPGet();
            qerVIPXPGetLog.userId       = peer.mUserId;
            qerVIPXPGetLog.charId       = peer.GetCharId();
            qerVIPXPGetLog.message      = message;
            qerVIPXPGetLog.type         = type;
            qerVIPXPGetLog.questId      = questId;
            qerVIPXPGetLog.getType      = getType;
            qerVIPXPGetLog.vipXPAmount  = vipXPAmount;
            var ignoreAwait = Zealot.Logging.Client.LoggingAgent.Instance.LogAsync(qerVIPXPGetLog);
        }

        public static void LogQERLockGoldGet(int questId, string getType, int lockGoldAmount, int lockGoldBef, int lockGoldAft, GameClientPeer peer)
        {
            string message = string.Format("Quest Id: {0} | Get Type: {1} | Lock Gold Amount: {2} | Lock Gold Before: {3} | Lock Gold After: {4}",
                questId,
                getType,
                lockGoldAmount,
                lockGoldBef,
                lockGoldAft);

            Zealot.Logging.Client.LogClasses.QERLockGoldGet qerLockGoldGetLog = new Zealot.Logging.Client.LogClasses.QERLockGoldGet();
            qerLockGoldGetLog.userId           = peer.mUserId;
            qerLockGoldGetLog.charId           = peer.GetCharId();
            qerLockGoldGetLog.message          = message;
            qerLockGoldGetLog.questId          = questId;
            qerLockGoldGetLog.getType          = getType;
            qerLockGoldGetLog.lockGoldAmount   = lockGoldAmount;
            qerLockGoldGetLog.lockGoldBef      = lockGoldBef;
            qerLockGoldGetLog.lockGoldAft      = lockGoldAft;
            var ignoreAwait = Zealot.Logging.Client.LoggingAgent.Instance.LogAsync(qerLockGoldGetLog);
        }

        public static void LogQERItemGet(int questId, string getType, int itemAmount, GameClientPeer peer)
        {
            string message = string.Format("Quest Id: {0} | Get Type: {1} | Item Amount: {2}",
                questId,
                getType,
                itemAmount);

            Zealot.Logging.Client.LogClasses.QERItemGet qerItemGetLog = new Zealot.Logging.Client.LogClasses.QERItemGet();
            qerItemGetLog.userId        = peer.mUserId;
            qerItemGetLog.charId        = peer.GetCharId();
            qerItemGetLog.message       = message;
            qerItemGetLog.questId       = questId;
            qerItemGetLog.getType       = getType;
            qerItemGetLog.itemAmount    = itemAmount;
            var ignoreAwait = Zealot.Logging.Client.LoggingAgent.Instance.LogAsync(qerItemGetLog);
        }
    }
}
